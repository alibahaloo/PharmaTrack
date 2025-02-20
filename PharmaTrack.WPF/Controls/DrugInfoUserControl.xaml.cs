using System.Windows;
using System.Windows.Controls;

namespace PharmaTrack.WPF.Controls
{
    /// <summary>
    /// Interaction logic for DrugInfoUserControl.xaml
    /// </summary>
    public partial class DrugInfoUserControl : UserControl
    {
        public DrugInfoUserControl()
        {
            InitializeComponent();
        }

        // DependencyProperty for the DrugInfo object.
        public object DrugInfo
        {
            get { return GetValue(DrugInfoProperty); }
            set { SetValue(DrugInfoProperty, value); }
        }

        public static readonly DependencyProperty DrugInfoProperty =
            DependencyProperty.Register("DrugInfo", typeof(object), typeof(DrugInfoUserControl), new PropertyMetadata(null));
    }
}
