using PharmaTrack.WPF.ViewModels;
using System.Globalization;
using System.Windows.Data;

namespace PharmaTrack.WPF.Converters
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DataMode enumValue && parameter is string parameterString)
            {
                if (Enum.TryParse(typeof(DataMode), parameterString, out var parsedEnum))
                {
                    return enumValue.Equals(parsedEnum);
                }
            }

            throw new InvalidOperationException("Invalid DataMode value or ConverterParameter.");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked && parameter is string parameterString)
            {
                if (Enum.TryParse(typeof(DataMode), parameterString, out var parsedEnum))
                {
                    return parsedEnum;
                }
            }

            throw new InvalidOperationException("Invalid DataMode value or ConverterParameter.");
        }
    }
}
