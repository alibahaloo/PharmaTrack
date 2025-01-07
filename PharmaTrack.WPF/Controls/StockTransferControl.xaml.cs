using PharmaTrack.WPF.ViewModels;
using System.Windows.Controls;

namespace PharmaTrack.WPF.Controls
{
    public partial class StockTransferControl : UserControl
    {
        public StockTransferControl(StockTransferViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += StockTransferControl_Loaded;
        }

        private void StockTransferControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            UPCInputTextBox.Focus();
        }

        private void UPCInputTextBox_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            var viewModel = DataContext as StockTransferViewModel;
            if (viewModel != null)
            {
                viewModel.ScannerStatusText = "Ready to Scan";
                viewModel.ScannerForeground = System.Windows.Media.Brushes.Green;
                viewModel.ScanBarcodeBtnEnabled = false;
            }
        }

        private void UPCInputTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            var viewModel = DataContext as StockTransferViewModel;
            if (viewModel != null)
            {
                viewModel.ScannerStatusText = "Not Ready to Scan";
                viewModel.ScannerForeground = System.Windows.Media.Brushes.Red;
                viewModel.ScanBarcodeBtnEnabled = true;
            }
        }

        private void ScanButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            UPCInputTextBox.Focus();
        }
    }
}
