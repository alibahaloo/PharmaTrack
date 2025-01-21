using System.Globalization;
using System.Windows.Data;

namespace PharmaTrack.WPF.Converters
{
    public class ParentHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double parentHeight)
            {
                // Adjust the height (e.g., subtract space for day text and padding)
                return Math.Max(0, parentHeight - 20); // Subtract 20px for padding/other elements
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
