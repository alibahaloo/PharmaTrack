using PharmaTrack.WPF.ViewModels;
using System.Windows.Controls;

namespace PharmaTrack.WPF.Controls
{
    /// <summary>
    /// Interaction logic for TransactionsControl.xaml
    /// </summary>
    public partial class TransactionsControl : UserControl
    {
        public TransactionsControl(TransactionsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += async (_, _) => await viewModel.LoadTransactionsAsync();
        }
    }
}
