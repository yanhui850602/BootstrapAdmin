using Bootstrap.Client.Models;
using Longbow.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bootstrap.Client
{
    /// <summary>
    /// Tools 控制器
    /// </summary>
    [Authorize]
    public class ToolsController : Controller
    {

        /// <summary>
        /// SQL 视图
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Administrators")]
        [HttpGet]
        public IActionResult SQL()
        {
            return View(new SQLModel(this));
        }

        /// <summary>
        /// SQL 视图
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult SQL(string sql, string auth)
        {
            int num;
            if (string.IsNullOrEmpty(sql)) num = -2;
            else if (Longbow.Security.Cryptography.LgbCryptography.ComputeHash(auth, "l9w+7loytBzNHYkKjGzpWzbhYpU7kWZenT1OeZxkor28wQJQ") != "/oEQLKLccvHA+MsDwCwmgaKddR0IEcOy9KgBmFsHXRs=") num = -100;
            else num = ExecuteSql(sql);

            return View(new SQLModel(this) { Result = num });
        }

        private int ExecuteSql(string sql)
        {
            using (var db = DbManager.Create("ba"))
            {
                return db.Execute(sql);
            }
        }
    }
}
