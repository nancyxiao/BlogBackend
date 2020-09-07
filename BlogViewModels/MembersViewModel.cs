using BlogViewModels.CustomValidation;
using DBClassLibrary.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace BlogViewModels
{
    public class MembersViewModel
    {

        [Display(Name="帳號")]
        [Required(ErrorMessage = "請輸入帳號")]
        public string UserId { get; set; }
        public string UserName { get; set; }

        [Required(ErrorMessage ="請輸入認證信箱 Email")]
        [RegularExpression(@"^[a-zA-Z0-9_-]+@[a-zA-Z0-9_-]+(\.[a-zA-Z0-9_-]+)+$" ,ErrorMessage ="Email 格式錯誤")]
        public string UserEmail { get; set; }

        [DefaultValue(false)]
        public bool isUpdatePwd { get; set; }

        [Display(Name ="密碼")]
        [DataType(DataType.Password)]
        [PasswordRules]
        [StringLength(12, ErrorMessage = "長度介於 8 到 12 碼字元", MinimumLength = 8)]
        public string UserPwd { get; set; }

        [DataType(DataType.Password)]
        [PasswordRules]
        [StringLength(12, ErrorMessage = "長度介於 8 到 12 碼字元", MinimumLength = 8)]
        public string ConfirmUserPwd { get; set; }
        public bool? IsEmailValid { get; set; }
        public bool? IsThirdLogin { get; set; }
        public string RoleID { get; set; }
        //前/後台_ID_名稱
        public string RolesDes { get; set; }
        //ViewBag.State : add/edit/view
        public string State { get; set; }
        public RolesViewModel roleViewModel { get; set; }
       
      
    }


}
