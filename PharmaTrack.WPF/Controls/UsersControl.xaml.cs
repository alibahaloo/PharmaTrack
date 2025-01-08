using PharmaTrack.WPF.ViewModels;
using System.Windows.Controls;

namespace PharmaTrack.WPF.Controls
{
    /// <summary>
    /// Interaction logic for UsersControl.xaml
    /// </summary>
    public partial class UsersControl : UserControl
    {
        public UsersControl(UsersViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += async (_, _) => await viewModel.LoadUsersAsync();
        }
    }
}
