using PharmaTrack.WPF.ViewModels;
using System.Windows.Controls;

namespace PharmaTrack.WPF.Controls
{
    /// <summary>
    /// Interaction logic for TransactionsControl.xaml
    /// </summary>
    public partial class TransactionsControl : UserControl
    {
        public TransactionsControl(TransactionsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += async (_, _) =>
            {
                await viewModel.ViewModelLoaded();
                UPCInputTextBox.Focus();
            };
        }

        private void UPCInputTextBox_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            var viewModel = DataContext as TransactionsViewModel;
            if (viewModel != null)
            {
                viewModel.ScannerStatusText = "Ready to Scan";
                viewModel.ScannerForeground = System.Windows.Media.Brushes.Green;
            }
        }

        private void UPCInputTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            var viewModel = DataContext as TransactionsViewModel;
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
