using PharmaTrack.WPF.ViewModels;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PharmaTrack.WPF.Converters
{
    public class DataModeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DataMode currentMode && parameter is string parameterString)
            {
                if (Enum.TryParse(typeof(DataMode), parameterString, out var parsedEnum) && parsedEnum is DataMode targetMode)
                {
                    // If current mode matches the target, hide the button; otherwise, show it.
                    return currentMode == targetMode ? Visibility.Collapsed : Visibility.Visible;
                }
            }

            return Visibility.Visible; // Default to Visible
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("DataModeToVisibilityConverter does not support ConvertBack.");
        }
    }
}
