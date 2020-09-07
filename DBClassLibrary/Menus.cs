﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DBClassLibrary
{
    public partial class Menus
    {
        public string MenuId { get; set; }
        [Required(ErrorMessage ="選單名稱必須填寫")]
        [MaxLength(50,ErrorMessage ="最大長度50個中文字")]
        public string MenuName { get; set; }
        public string Platform { get; set; }
        public string ParentId { get; set; }
        public int? Sequence { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Url { get; set; }
    }
}