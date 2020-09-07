using DBClassLibrary;
using DBClassLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace BlogViewModels.CustomValidation
{
    public class EmailRules: ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var member = (MembersViewModel)validationContext.ObjectInstance;


            //if (emailExists)
            //    return new ValidationResult("此 Email 已經被註冊過");

            return ValidationResult.Success;
        }
    }
}
