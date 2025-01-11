using System.Windows;
using System.Windows.Controls;

namespace PharmaTrack.WPF.Controls
{
    /// <summary>
    /// Interaction logic for LoadingControl.xaml
    /// </summary>
    public partial class LoadingControl : UserControl
    {
        public LoadingControl()
        {
            InitializeComponent();
        }

        public void SetErrorMessage(string message)
        {
            ProgressBar.IsIndeterminate = false;
            ErrorMessage.Text = message;
            ErrorMessage.Visibility = Visibility.Visible;
            RetryButton.Visibility = Visibility.Visible;
        }

        private void RetryButton_Click(object sender, RoutedEventArgs e)
        {
            RetryButton.Visibility = Visibility.Hidden;
            ProgressBar.IsIndeterminate = true;
            ErrorMessage.Text = string.Empty;
        }
    }
}
