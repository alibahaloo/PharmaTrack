using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PharmaTrack.WPF.Converters
{
    public class MonthColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isCurrentMonth)
            {
                return isCurrentMonth ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.LightGray);
            }

            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
