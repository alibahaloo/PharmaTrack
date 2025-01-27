using System.Windows.Controls;

namespace PharmaTrack.WPF.Controls
{
    /// <summary>
    /// Interaction logic for ProductControl.xaml
    /// </summary>
    public partial class ProductControl : UserControl
    {
        public ProductControl(int productId)
        {
            InitializeComponent();
            DataContext = new ProductViewModel(productId);
        }
    }
}
