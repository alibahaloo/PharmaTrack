using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PharmaTrack.WPF.Converters
{
    public class TodayDateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime date)
            {
                // Compare the date with today's date
                return date.Date == DateTime.Today ? Brushes.Blue : Brushes.Gray;
            }

            return Brushes.Gray; // Default color for non-DateTime values
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
