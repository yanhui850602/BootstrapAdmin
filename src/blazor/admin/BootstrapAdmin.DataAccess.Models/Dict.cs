﻿// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace BootstrapAdmin.DataAccess.Models
{
    /// <summary>
    /// 字典配置项
    /// </summary>
    public class Dict
    {
        /// <summary>
        /// 获得/设置 字典主键 数据库自增列
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// 获得/设置 字典标签
        /// </summary>
        [Required(ErrorMessage = "{0}不可为空")]
        [Display(Name = "字典标签")]
        public string Category { get; set; } = "";

        /// <summary>
        /// 获得/设置 字典名称
        /// </summary>
        [Required(ErrorMessage = "{0}不可为空")]
        [Display(Name = "字典名称")]
        public string Name { get; set; } = "";

        /// <summary>
        /// 获得/设置 字典字典值
        /// </summary>
        [Required(ErrorMessage = "{0}不可为空")]
        [Display(Name = "字典代码")]
        public string Code { get; set; } = "";

        /// <summary>
        /// 获得/设置 字典定义值 0 表示系统使用，1 表示用户自定义 默认为 1
        /// </summary>
        [Display(Name = "字典类型")]
        public EnumDictDefine Define { get; set; } = EnumDictDefine.Customer;
    }
}