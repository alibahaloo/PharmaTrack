using PharmaTrack.WPF.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace PharmaTrack.WPF.Controls
{
    /// <summary>
    /// Interaction logic for InventoryControl.xaml
    /// </summary>
    public partial class InventoryControl : UserControl
    {
        public InventoryControl(InventoryViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += async (_, _) => { 
                await viewModel.LoadProductsAsync();
                UPCInputTextBox.Focus();
            };
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is InventoryViewModel viewModel && viewModel.ViewProductCommand != null)
            {
                // Ensure a product is selected and pass its ID to the command
                if (viewModel.SelectedProduct != null)
                {
                    viewModel.ViewProductCommand.Execute(viewModel.SelectedProduct.Id);
                }
            }
        }

        private void UPCInputTextBox_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            var viewModel = DataContext as InventoryViewModel;
            if (viewModel != null)
            {
                viewModel.ScannerStatusText = "Ready to Scan";
                viewModel.ScannerForeground = System.Windows.Media.Brushes.Green;
            }
        }

        private void UPCInputTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            var viewModel = DataContext as InventoryViewModel;
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
