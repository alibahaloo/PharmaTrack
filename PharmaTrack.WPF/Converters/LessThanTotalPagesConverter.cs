using System.Globalization;
using System.Windows.Data;

namespace PharmaTrack.WPF.Converters
{
    public class LessThanTotalPagesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int currentPage && parameter is int totalPages)
            {
                return currentPage < totalPages;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
