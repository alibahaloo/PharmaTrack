using PharmaTrack.WPF.ViewModels;
using System.Windows.Controls;

namespace PharmaTrack.WPF.Controls
{
    /// <summary>
    /// Interaction logic for IngredientInteractionControl.xaml
    /// </summary>
    public partial class IngredientInteractionControl : UserControl
    {
        public IngredientInteractionControl(IngredientInteractionViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += (_, _) => {
                viewModel.IsCombBoxFocused = true;
            };
        }
        private void ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.IsEditable)
            {
                if (comboBox.Template.FindName("PART_EditableTextBox", comboBox) is TextBox textBox)
                {
                    // Delay clearing the selection until after the ComboBox has done its own selection.
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        textBox.SelectionLength = 0;
                        textBox.SelectionStart = textBox.Text.Length;
                    }), System.Windows.Threading.DispatcherPriority.ContextIdle);
                }
            }
        }
    }
}
