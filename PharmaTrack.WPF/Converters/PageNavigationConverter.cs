using System.Globalization;
using System.Windows.Data;

namespace PharmaTrack.WPF.Converters
{
    public class PageNavigationConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is int currentPage && values[1] is int totalPages)
            {
                // Determine if the button should be enabled
                return parameter switch
                {
                    "Previous" => currentPage > 1,
                    "Next" => currentPage < totalPages,
                    _ => false
                };
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

}
