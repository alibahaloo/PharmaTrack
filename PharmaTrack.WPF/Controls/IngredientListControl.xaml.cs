using PharmaTrack.WPF.ViewModels;
using System.Windows.Controls;

namespace PharmaTrack.WPF.Controls
{
    /// <summary>
    /// Interaction logic for IngredientListControl.xaml
    /// </summary>
    public partial class IngredientListControl : UserControl
    {
        public IngredientListControl(IngredientListViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += async (_, _) => await viewModel.LoadIngredientsAsync();
        }
    }
}
