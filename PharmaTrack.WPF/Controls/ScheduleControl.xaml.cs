using PharmaTrack.WPF.ViewModels;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

            Loaded += async (s, e) => await viewModel.LoadUsersAsync();
        }

        private void TimeInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                // Only allow numeric input
                if (!Regex.IsMatch(e.Text, @"^\d$"))
                {
                    e.Handled = true;
                    return;
                }

                // Get current text and caret position
                string currentText = textBox.Text;
                int caretIndex = textBox.CaretIndex;

                // Insert the new input at the caret position
                string newText = currentText.Insert(caretIndex, e.Text);

                // Remove non-numeric characters and limit to 4 digits
                string numericText = new string(newText.Where(char.IsDigit).ToArray());
                if (numericText.Length > 4)
                {
                    numericText = numericText.Substring(0, 4);
                }

                // Format the numeric text into hh:mm if applicable
                if (numericText.Length >= 3)
                {
                    numericText = numericText.Insert(2, ":");
                }

                // Update the text box
                textBox.Text = numericText;

                // Calculate the new caret position
                int newCaretIndex = caretIndex + (e.Text.Length == 1 ? 1 : 0);
                if (numericText.Length >= 3 && caretIndex >= 2)
                {
                    newCaretIndex++; // Adjust for the colon
                }

                // Set the caret position
                textBox.CaretIndex = newCaretIndex;

                // Prevent the default handling since we handled it
                e.Handled = true;
            }
        }

        private void TimeInput_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                // Use Dispatcher to delay setting the caret index
                textBox.Dispatcher.BeginInvoke(new Action(() =>
                {
                    textBox.CaretIndex = 0; // Set the cursor to the beginning
                }), System.Windows.Threading.DispatcherPriority.Render);
            }
        }


        private void TimeInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (TimeSpan.TryParse(textBox.Text, out var time))
                {
                    // Clamp time to valid range
                    var clampedTime = ValidateAndCorrectTime(time);
                    textBox.Text = clampedTime.ToString(@"hh\:mm");
                }
                else
                {
                    // Reset to default value if invalid
                    textBox.Text = "00:00";
                }
            }
        }

        private TimeSpan ValidateAndCorrectTime(TimeSpan time)
        {
            if (time < TimeSpan.Zero) return TimeSpan.Zero;
            if (time > new TimeSpan(23, 59, 0)) return new TimeSpan(23, 59, 0);
            return time;
        }
    }
}
