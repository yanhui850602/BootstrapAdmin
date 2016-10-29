﻿using Longbow;
using Longbow.Caching;
using Longbow.Caching.Configuration;
using Longbow.Data;
using Longbow.ExceptionManagement;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

namespace Bootstrap.DataAccess
{
    /// <summary>
    /// 
    /// </summary>
    public class RoleHelper
    {
        private const string RoleDataKey = "RoleData-CodeRoleHelper";
        private const string RoleUserIDDataKey = "RoleData-CodeRoleHelper-";
        private const string RoleNavigationIDDataKey = "RoleData-CodeRoleHelper-Navigation-";
        /// <summary>
        /// 查询所有角色
        /// </summary>
        /// <param name="tId"></param>
        /// <returns></returns>
        public static IEnumerable<Role> RetrieveRoles(string tId = null)
        {
            string sql = "select * from Roles";
            var ret = CacheManager.GetOrAdd(RoleDataKey, CacheSection.RetrieveIntervalByKey(RoleDataKey), key =>
            {
                List<Role> roles = new List<Role>();
                DbCommand cmd = DBAccessManager.SqlDBAccess.CreateCommand(CommandType.Text, sql);
                try
                {
                    using (DbDataReader reader = DBAccessManager.SqlDBAccess.ExecuteReader(cmd))
                    {
                        while (reader.Read())
                        {
                            roles.Add(new Role()
                            {
                                ID = (int)reader[0],
                                RoleName = LgbConvert.ReadValue(reader[1], string.Empty),
                                Description = LgbConvert.ReadValue(reader[2], string.Empty)
                            });
                        }
                    }
                }
                catch (Exception ex) { ExceptionManager.Publish(ex); }
                return roles;
            }, CacheSection.RetrieveDescByKey(RoleDataKey));
            return string.IsNullOrEmpty(tId) ? ret : ret.Where(t => tId.Equals(t.ID.ToString(), StringComparison.OrdinalIgnoreCase));
        }
        /// <summary>
        /// 保存用户角色关系
        /// </summary>
        /// <param name="id"></param>
        /// <param name="roleIds"></param>
        /// <returns></returns>
        public static bool SaveRolesByUserId(int id, string roleIds)
        {
            var ret = false;
            DataTable dt = new DataTable();
            dt.Columns.Add("UserID", typeof(int));
            dt.Columns.Add("RoleID", typeof(int));
            //判断用户是否选定角色
            if (!string.IsNullOrEmpty(roleIds))
            {
                roleIds.Split(',').ToList().ForEach(roleId =>
                {
                    DataRow row = dt.NewRow();
                    dt.Rows.Add(id, roleId);
                });
            }
            using (TransactionPackage transaction = DBAccessManager.SqlDBAccess.BeginTransaction())
            {
                try
                {
                    // delete user from config table
                    string sql = "delete from UserRole where UserID = @UserID;";
                    using (DbCommand cmd = DBAccessManager.SqlDBAccess.CreateCommand(CommandType.Text, sql))
                    {
                        cmd.Parameters.Add(DBAccessManager.SqlDBAccess.CreateParameter("@UserID", id, ParameterDirection.Input));
                        DBAccessManager.SqlDBAccess.ExecuteNonQuery(cmd, transaction);

                        // insert batch data into config table
                        using (SqlBulkCopy bulk = new SqlBulkCopy((SqlConnection)transaction.Transaction.Connection, SqlBulkCopyOptions.Default, (SqlTransaction)transaction.Transaction))
                        {
                            bulk.BatchSize = 1000;
                            bulk.DestinationTableName = "UserRole";
                            bulk.ColumnMappings.Add("UserID", "UserID");
                            bulk.ColumnMappings.Add("RoleID", "RoleID");
                            bulk.WriteToServer(dt);
                            transaction.CommitTransaction();
                        }
                    }
                    ret = true;
                    ClearCache();
                }
                catch (Exception ex)
                {
                    ExceptionManager.Publish(ex);
                    transaction.RollbackTransaction();
                }
            }
            return ret;
        }

        /// <summary>
        /// 查询某个用户所拥有的角色
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Role> RetrieveRolesByUserId(int userId)
        {
            string k = string.Format("{0}{1}", RoleUserIDDataKey, userId);
            return CacheManager.GetOrAdd(k, CacheSection.RetrieveIntervalByKey(RoleUserIDDataKey), key =>
            {
                List<Role> Roles = new List<Role>();
                string sql = "select r.ID, r.RoleName, r.[Description], case ur.RoleID when r.ID then 'checked' else '' end [status] from Roles r left join UserRole ur on r.ID = ur.RoleID and UserID = @UserID";
                try
                {
                    DbCommand cmd = DBAccessManager.SqlDBAccess.CreateCommand(CommandType.Text, sql);
                    cmd.Parameters.Add(DBAccessManager.SqlDBAccess.CreateParameter("@UserID", userId, ParameterDirection.Input));
                    using (DbDataReader reader = DBAccessManager.SqlDBAccess.ExecuteReader(cmd))
                    {
                        while (reader.Read())
                        {
                            Roles.Add(new Role()
                            {
                                ID = (int)reader[0],
                                RoleName = (string)reader[1],
                                Description = (string)reader[2],
                                Checked = (string)reader[3]
                            });
                        }
                    }
                }
                catch (Exception ex) { ExceptionManager.Publish(ex); }
                return Roles;
            }, CacheSection.RetrieveDescByKey(RoleUserIDDataKey));
        }

        /// <summary>
        /// 删除角色表
        /// </summary>
        /// <param name="IDs"></param>
        public static bool DeleteRole(string IDs)
        {
            bool ret = false;
            if (string.IsNullOrEmpty(IDs) || IDs.Contains("'")) return ret;
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, "Delete from Roles where ID in ({0})", IDs);
                using (DbCommand cmd = DBAccessManager.SqlDBAccess.CreateCommand(CommandType.Text, sql))
                {
                    DBAccessManager.SqlDBAccess.ExecuteNonQuery(cmd);
                    ClearCache();
                    ret = true;
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.Publish(ex);
            }
            return ret;
        }
        /// <summary>
        /// 保存新建/更新的角色信息
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool SaveRole(Role p)
        {
            if (p == null) throw new ArgumentNullException("p");
            bool ret = false;
            if (!string.IsNullOrEmpty(p.RoleName) && p.RoleName.Length > 50) p.RoleName = p.RoleName.Substring(0, 50);
            if (!string.IsNullOrEmpty(p.Description) && p.Description.Length > 50) p.Description = p.Description.Substring(0, 500);
            string sql = p.ID == 0 ?
                "Insert Into Roles (RoleName, Description) Values (@RoleName, @Description)" :
                "Update Roles set RoleName = @RoleName, Description = @Description where ID = @ID";
            try
            {
                using (DbCommand cmd = DBAccessManager.SqlDBAccess.CreateCommand(CommandType.Text, sql))
                {
                    cmd.Parameters.Add(DBAccessManager.SqlDBAccess.CreateParameter("@ID", p.ID, ParameterDirection.Input));
                    cmd.Parameters.Add(DBAccessManager.SqlDBAccess.CreateParameter("@RoleName", p.RoleName, ParameterDirection.Input));
                    cmd.Parameters.Add(DBAccessManager.SqlDBAccess.CreateParameter("@Description", p.Description, ParameterDirection.Input));
                    DBAccessManager.SqlDBAccess.ExecuteNonQuery(cmd);
                }
                ret = true;
                ClearCache();
            }
            catch (DbException ex)
            {
                ExceptionManager.Publish(ex);
            }
            return ret;
        }

        /// <summary>
        /// 查询某个菜单所拥有的角色
        /// </summary>
        /// <param name="menuId"></param>
        /// <returns></returns>
        public static IEnumerable<Role> RetrieveRolesByMenuId(int menuId)
        {
            string k = string.Format("{0}{1}", RoleNavigationIDDataKey, menuId);
            return CacheManager.GetOrAdd(k, CacheSection.RetrieveIntervalByKey(RoleUserIDDataKey), key =>
            {
                List<Role> Roles = new List<Role>();
                string sql = "select r.ID, r.RoleName, r.[Description], case ur.RoleID when r.ID then 'checked' else '' end [status] from Roles r left join NavigationRole ur on r.ID = ur.RoleID and NavigationID = @NavigationID";
                try
                {
                    DbCommand cmd = DBAccessManager.SqlDBAccess.CreateCommand(CommandType.Text, sql);
                    cmd.Parameters.Add(DBAccessManager.SqlDBAccess.CreateParameter("@NavigationID", menuId, ParameterDirection.Input));
                    using (DbDataReader reader = DBAccessManager.SqlDBAccess.ExecuteReader(cmd))
                    {
                        while (reader.Read())
                        {
                            Roles.Add(new Role()
                            {
                                ID = (int)reader[0],
                                RoleName = (string)reader[1],
                                Description = (string)reader[2],
                                Checked = (string)reader[3]
                            });
                        }
                    }
                }
                catch (Exception ex) { ExceptionManager.Publish(ex); }
                return Roles;
            }, CacheSection.RetrieveDescByKey(RoleUserIDDataKey));
        }

        /// <summary>
        /// 保存菜单角色关系
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool SavaRolesByMenuId(int id, string value)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("NavigationID", typeof(int));
            dt.Columns.Add("RoleID", typeof(int));
            //判断用户是否选定角色
            if (!string.IsNullOrEmpty(value))
            {
                string[] roleIDs = value.Split(',');
                foreach (string roleID in roleIDs)
                {
                    DataRow row = dt.NewRow();
                    row["NavigationID"] = id;
                    row["RoleID"] = roleID;
                    dt.Rows.Add(row);
                }
            }

            string sql = "delete from NavigationRole where NavigationID=@NavigationID;";
            using (DbCommand cmd = DBAccessManager.SqlDBAccess.CreateCommand(CommandType.Text, sql))
            {
                cmd.Parameters.Add(DBAccessManager.SqlDBAccess.CreateParameter("@NavigationID", id, ParameterDirection.Input));
                using (TransactionPackage transaction = DBAccessManager.SqlDBAccess.BeginTransaction())
                {
                    using (SqlBulkCopy bulk = new SqlBulkCopy((SqlConnection)transaction.Transaction.Connection, SqlBulkCopyOptions.Default, (SqlTransaction)transaction.Transaction))
                    {
                        bulk.BatchSize = 1000;
                        bulk.DestinationTableName = "NavigationRole";
                        bulk.ColumnMappings.Add("NavigationID", "NavigationID");
                        bulk.ColumnMappings.Add("RoleID", "RoleID");

                        bool ret = true;
                        try
                        {
                            DBAccessManager.SqlDBAccess.ExecuteNonQuery(cmd, transaction);
                            bulk.WriteToServer(dt);
                            transaction.CommitTransaction();
                            ClearCache();
                        }
                        catch (Exception ex)
                        {
                            ret = false;
                            transaction.RollbackTransaction();
                        }
                        return ret;
                    }
                }
            }
        }

        // 更新缓存
        private static void ClearCache(string cacheKey = null)
        {
            CacheManager.Clear(key => string.IsNullOrEmpty(cacheKey) || key == cacheKey);
        }
        /// <summary>
        /// 查询某个部门所拥有的角色
        /// </summary>
        /// <param name="menuId"></param>
        /// <returns></returns>
        public static IEnumerable<Role> RetrieveRolesByGroupId(int groupId)
        {
            return null;
        }
        /// <summary>
        /// 保存部门角色关系
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool SaveRolesByGroupId(int id, string value)
        {
            return false;
        }
    }
}