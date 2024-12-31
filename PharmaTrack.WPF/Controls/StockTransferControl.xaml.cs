using System.Windows.Controls;
using PharmaTrack.WPF.ViewModels;

namespace PharmaTrack.WPF.Controls
{
    public partial class StockTransferControl : UserControl
    {
        public StockTransferControl()
        {
            InitializeComponent();
            DataContext = new StockTransferViewModel();
        }
    }
}
