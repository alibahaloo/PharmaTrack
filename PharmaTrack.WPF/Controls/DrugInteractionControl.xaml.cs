using PharmaTrack.WPF.ViewModels;
using System.Windows.Controls;

namespace PharmaTrack.WPF.Controls
{
    /// <summary>
    /// Interaction logic for DrugInteractionControl.xaml
    /// </summary>
    public partial class DrugInteractionControl : UserControl
    {
        public DrugInteractionControl(DrugInteractionViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
        private void ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.IsEditable)
            {
                // Find the internal editable TextBox
                if (comboBox.Template.FindName("PART_EditableTextBox", comboBox) is TextBox textBox)
                {
                    // Clear any selection and move caret to the end of the text.
                    textBox.SelectionLength = 0;
                    textBox.SelectionStart = textBox.Text.Length;
                }
            }
        }
    }
}
