using PharmaTrack.WPF.ViewModels;
using System.Windows.Controls;

namespace PharmaTrack.WPF.Controls
{
    /// <summary>
    /// Interaction logic for ScheduleControl.xaml
    /// </summary>
    public partial class ScheduleControl : UserControl
    {
        public ScheduleControl(ScheduleControlViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

        }
    }
}
