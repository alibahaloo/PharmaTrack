using PharmaTrack.WPF.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace PharmaTrack.WPF.Controls
{
    public partial class CalendarControl : UserControl
    {
        private CalendarControlViewModel ViewModel => DataContext as CalendarControlViewModel
            ?? throw new InvalidOperationException("CalendarControlViewModel is not set as DataContext.");

        public CalendarControl(CalendarControlViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.CurrentMonth = DateTime.Today;
        }

        private void PrevMonthButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CurrentMonth = ViewModel.CurrentMonth.AddMonths(-1);
        }

        private void NextMonthButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CurrentMonth = ViewModel.CurrentMonth.AddMonths(1);
        }

        private void TodayButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CurrentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        }
    }
}
