using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PharmaTrack.WPF.Converters
{
    public class InteractionToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool hasInteraction)
            {
                return hasInteraction ? Brushes.Red : Brushes.Green;
            }
            return Brushes.Green; // default value if not a bool
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
