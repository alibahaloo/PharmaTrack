using PharmaTrack.WPF.ViewModels;
using System.Windows.Controls;

namespace PharmaTrack.WPF.Controls
{
    public partial class CalendarControl : UserControl
    {
        public CalendarControl(CalendarControlViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += (_, _) => viewModel.OnViewModelLoaded();
        }
    }
}
