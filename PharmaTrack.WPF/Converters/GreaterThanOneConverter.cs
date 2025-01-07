using System.Globalization;
using System.Windows.Data;

namespace PharmaTrack.WPF.Converters
{
    public class GreaterThanOneConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int page && page > 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
