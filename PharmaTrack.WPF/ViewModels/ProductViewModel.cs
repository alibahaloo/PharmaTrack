using PharmaTrack.Shared.DBModels;
using PharmaTrack.WPF.Helpers;
using PharmaTrack.WPF.ViewModels;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

public class ProductViewModel : INotifyPropertyChanged
{
    private readonly InventoryService _inventoryService;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private Product _product = default!;
    public Product Product
    {
        get => _product;
        set
        {
            _product = value;
            OnPropertyChanged();
        }
    }

    public ICommand GoBackCommand { get; }
    public ICommand SaveCommand { get; }

    public ProductViewModel(int productId, InventoryService inventoryService)
    {
        _inventoryService = inventoryService;
        GoBackCommand = new RelayCommand(_ => GoBackToInventory());
        SaveCommand = new AsyncRelayCommand(async _ => await SaveProductAsync());

        LoadProductAsync(productId);
    }

    private async void LoadProductAsync(int productId)
    {
        try
        {
            Product = await _inventoryService.GetProductByIdAsync(productId) ?? new Product();
        }
        catch (Exception ex)
        {
            // Handle or log errors
            Product = new Product { Name = "Error loading product", Id = productId };
        }
    }

    private async Task SaveProductAsync()
    {
        try
        {
            // Logic to save the updated product (implement API call here)
            // await _inventoryService.UpdateProductAsync(Product);

            // Simulate save
            Product.UpdatedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            // Handle errors, e.g., show a message to the user
        }
    }

    private void GoBackToInventory()
    {
        if (System.Windows.Application.Current.MainWindow.DataContext is MainWindowViewModel mainViewModel)
        {
            mainViewModel.ShowInventoryCommand.Execute(null);
        }
    }
}
