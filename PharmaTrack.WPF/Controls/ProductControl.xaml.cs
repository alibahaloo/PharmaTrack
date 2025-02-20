using PharmaTrack.WPF.Helpers;
using PharmaTrack.WPF.ViewModels;
using System.Windows.Controls;

namespace PharmaTrack.WPF.Controls
{
    /// <summary>
    /// Interaction logic for ProductControl.xaml
    /// </summary>
    public partial class ProductControl : UserControl
    {
        public ProductControl(int productId, InventoryService inventoryService, DrugService drugService)
        {
            InitializeComponent();
            DataContext = new ProductViewModel(productId, inventoryService, drugService);
        }

        private void UPCInputTextBox_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            var viewModel = DataContext as ProductViewModel;
            if (viewModel != null)
            {
                viewModel.ScannerStatusText = "Ready to Scan";
                viewModel.ScannerForeground = System.Windows.Media.Brushes.Green;
            }
        }

        private void UPCInputTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            var viewModel = DataContext as ProductViewModel;
            if (viewModel != null)
            {
                viewModel.ScannerStatusText = "Not Ready to Scan";
                viewModel.ScannerForeground = System.Windows.Media.Brushes.Red;
            }
        }

        private void ScanButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            UPCInputTextBox.Focus();
        }
    }
}
