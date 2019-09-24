﻿using Microsoft.AspNetCore.Mvc;

namespace Bootstrap.Admin.Models
{
    /// <summary>
    /// 系统锁屏数据模型
    /// </summary>
    public class LockModel : HeaderBarModel
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="controller"></param>
        public LockModel(ControllerBase controller) : base(controller.User.Identity)
        {

        }

        /// <summary>
        /// 获得/设置 返回路径
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        /// 获得/设置 认证方式 Cookie Mobile Gitee GitHub
        /// </summary>
        public string AuthenticationType { get; set; }
    }
}