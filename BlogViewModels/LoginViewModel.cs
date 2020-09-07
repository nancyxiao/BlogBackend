using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace BlogViewModels
{
    public class LoginViewModel
    {
        [Display(Name="帳號")]
        [Required(ErrorMessage = "請輸入帳號")]
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        [Display(Name ="密碼")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "請輸入密碼")]
        [StringLength(12, ErrorMessage = "長度介於 8 到 12 碼字元", MinimumLength = 8)]
        public string UserPwd { get; set; }

        public bool? IsEmailValid { get; set; }
        public bool? IsThirdLogin { get; set; }

        public string ReturnUrl { get; set; }
    }


}
