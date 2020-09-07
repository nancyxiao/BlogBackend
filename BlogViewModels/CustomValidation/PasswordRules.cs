using BlogViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BlogViewModels.CustomValidation
{
    public class PasswordRules: ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var member = (MembersViewModel)validationContext.ObjectInstance;

            if (member.isUpdatePwd == false) return ValidationResult.Success;

            if (string.IsNullOrEmpty(member.UserPwd))
                return new ValidationResult("請填寫會員密碼");

            if (string.IsNullOrEmpty(member.ConfirmUserPwd))
                return new ValidationResult("請填寫確認輸入密碼");

            if (member.UserPwd != member.ConfirmUserPwd)
                return new ValidationResult("會員密碼與確認輸入密碼不一致");

            return ValidationResult.Success;
        }
    }
}
