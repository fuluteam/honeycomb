using System.ComponentModel.DataAnnotations;

namespace HoneyComb.API.Resources.CronReminders.Models
{

    public class CronAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value,
            ValidationContext validationContext)
        {
            var cron = value as string;
            if (string.IsNullOrWhiteSpace(cron))
            {
                return new ValidationResult("Cron表达式不合法");
            }

            try
            {
                var exp = Cronos.CronExpression.Parse(cron, Cronos.CronFormat.IncludeSeconds);
            }
            catch (Cronos.CronFormatException)
            {
                return new ValidationResult("Cron表达式不合法");
            }

            return ValidationResult.Success;
        }
    }
}
