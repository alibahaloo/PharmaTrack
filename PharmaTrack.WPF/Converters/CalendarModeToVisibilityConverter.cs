using PharmaTrack.WPF.ViewModels;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PharmaTrack.WPF.Converters
{
    public class CalendarModeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ViewMode currentMode && parameter is string targetModeString)
            {
                if (Enum.TryParse(typeof(ViewMode), targetModeString, out var targetMode) && targetMode is ViewMode target)
                {
                    // Hide the button if the current mode matches the target mode
                    return currentMode == target ? Visibility.Collapsed : Visibility.Visible;
                }
            }

            return Visibility.Visible; // Default to visible if the conversion fails
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("CalendarModeToVisibilityConverter does not support ConvertBack.");
        }
    }
}
