using System.Globalization;
using System.Windows.Controls;

namespace PharmaTrack.WPF.ValidationRules
{
    public class QuantityValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (int.TryParse(value as string, out int quantity))
            {
                if (quantity >= 1 && quantity <= 1000)
                    return ValidationResult.ValidResult;
                else
                    return new ValidationResult(false, "Quantity must be between 1 and 1000.");
            }

            return new ValidationResult(false, "Invalid input. Enter a number.");
        }
    }
}
