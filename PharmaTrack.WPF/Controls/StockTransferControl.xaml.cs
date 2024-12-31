using PharmaTrack.WPF.ViewModels;
using System.Windows.Controls;

namespace PharmaTrack.WPF.Controls
{
    public partial class StockTransferControl : UserControl
    {
        public StockTransferControl()
        {
            InitializeComponent();
            DataContext = new StockTransferViewModel();
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
                viewModel.StatusText = "Ready to Scan";
                viewModel.StatusForeground = System.Windows.Media.Brushes.Green;
            }
        }

        private void UPCInputTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            var viewModel = DataContext as StockTransferViewModel;
            if (viewModel != null)
            {
                viewModel.StatusText = "Not Ready to Scan";
                viewModel.StatusForeground = System.Windows.Media.Brushes.Red;
            }
        }

        private void ScanButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            UPCInputTextBox.Focus();
        }
    }
}
