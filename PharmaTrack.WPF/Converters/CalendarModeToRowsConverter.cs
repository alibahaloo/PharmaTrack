using PharmaTrack.WPF.ViewModels;
using System.Globalization;
using System.Windows.Data;

namespace PharmaTrack.WPF.Converters
{
    public class CalendarModeToRowsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ViewMode calendarMode)
            {
                return calendarMode == ViewMode.Weekly ? 1 : 6;
            }

            return 6; // Default to 6 (Monthly)
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("CalendarModeToRowsConverter does not support ConvertBack.");
        }
    }
}
