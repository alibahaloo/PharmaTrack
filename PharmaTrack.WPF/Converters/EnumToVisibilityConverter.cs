using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PharmaTrack.WPF.Converters
{
    public class EnumToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum enumValue && parameter is string targetValue)
            {
                var targetEnum = Enum.Parse(value.GetType(), targetValue);
                return enumValue.Equals(targetEnum) ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
