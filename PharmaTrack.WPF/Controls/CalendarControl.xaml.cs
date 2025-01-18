using PharmaTrack.WPF.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace PharmaTrack.WPF.Controls
{
    public partial class CalendarControl : UserControl
    {
        public CalendarControl(CalendarControlViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += OnLoaded;
        }

        // Event handler for Loaded
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Ensure the Loaded event is handled only once
            Loaded -= OnLoaded;

            // Call the method after the view model is fully loaded
            if (DataContext is CalendarControlViewModel viewModel)
            {
                viewModel.OnViewModelLoaded();
            }
        }
    }
}
