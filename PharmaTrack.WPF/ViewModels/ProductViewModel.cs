using PharmaTrack.WPF.Helpers;
using PharmaTrack.WPF.ViewModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

public class ProductViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private int _productId;
    public int ProductId
    {
        get => _productId;
        set
        {
            _productId = value;
            OnPropertyChanged();
        }
    }

    public ICommand GoBackCommand { get; }

    public ProductViewModel(int productId)
    {
        ProductId = productId;

        // Command to navigate back to the inventory
        GoBackCommand = new RelayCommand(_ => GoBackToInventory());
    }

    private void GoBackToInventory()
    {
        if (System.Windows.Application.Current.MainWindow.DataContext is MainWindowViewModel mainViewModel)
        {
            mainViewModel.ShowInventoryCommand.Execute(null);
        }
    }
}
