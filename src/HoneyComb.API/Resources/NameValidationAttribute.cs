using System;
using System.ComponentModel.DataAnnotations;

namespace HoneyComb.API.Resources.CronReminders.Models
{

    public class NameAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value,
            ValidationContext validationContext)
        {
            var name = value as string;
            if (string.IsNullOrWhiteSpace(name))
            {
                return new ValidationResult("名称不能为空");
            }

            if (Uri.CheckHostName(name) != UriHostNameType.Dns)
            {
                return new ValidationResult("名称不合法，只能包含字母、数字、短线");
            }

            return ValidationResult.Success;
        }
    }
}
