using PharmaTrack.WPF.ViewModels;
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
            Loaded += async (_, _) => await viewModel.LoadProductsAsync();
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
    }
}
