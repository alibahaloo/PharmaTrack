using PharmaTrack.WPF.Controls;
using PharmaTrack.WPF.Helpers;
using PharmaTrack.WPF.ViewModels;
using System.Windows;

namespace PharmaTrack.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel mainWindowViewModel)
        {
            InitializeComponent();

            // Set the DataContext to the injected ViewModel
            DataContext = mainWindowViewModel;
        }
    }
}
