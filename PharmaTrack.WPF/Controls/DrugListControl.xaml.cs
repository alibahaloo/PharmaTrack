using PharmaTrack.WPF.ViewModels;
using System.Windows.Controls;

namespace PharmaTrack.WPF.Controls
{
    /// <summary>
    /// Interaction logic for DrugControl.xaml
    /// </summary>
    public partial class DrugListControl : UserControl
    {
        public DrugListControl(DrugListViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += async (_, _) => await viewModel.LoadDrugsAsync();
        }
    }
}
