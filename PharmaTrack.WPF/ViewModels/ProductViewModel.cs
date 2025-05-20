using PharmaTrack.DTOs.Drug;
using PharmaTrack.Shared.DBModels;
using PharmaTrack.WPF.Helpers;
using PharmaTrack.WPF.ViewModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

public class ProductViewModel : INotifyPropertyChanged
{
    private readonly InventoryService _inventoryService;
    private readonly DrugService _drugService;
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private string _scannerStatusText = default!;
    public string ScannerStatusText
    {
        get => _scannerStatusText;
        set
        {
            _scannerStatusText = value;
            OnPropertyChanged();
        }
    }
    private Brush _scannerForeground = default!;
    public Brush ScannerForeground
    {
        get => _scannerForeground;
        set { _scannerForeground = value; OnPropertyChanged(); }
    }
    private bool _isLoading = false;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged(); // Notify UI of changes
        }
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
    private string _statusText = default!;
    public string StatusText
    {
        get => _statusText;
        set { _statusText = value; OnPropertyChanged(); }
    }
    private Brush _statusForeground = default!;
    public Brush StatusForeground
    {
        get => _statusForeground;
        set { _statusForeground = value; OnPropertyChanged(); }
    }
    private DrugInfoDto? _drugInfo;
    public DrugInfoDto? DrugInfo
    {
        get => _drugInfo;
        set
        {
            _drugInfo = value;
            OnPropertyChanged(nameof(DrugInfo));
        }
    }
    private bool _drugInfoIsExpanded;
    public bool DrugInfoIsExpanded
    {
        get => _drugInfoIsExpanded;
        set
        {
            _drugInfoIsExpanded = value;
            OnPropertyChanged(nameof(DrugInfoIsExpanded));
        }
    }
    private string _drugLookupStatusText = default!;
    public string DrugLookupStatusText
    {
        get => _drugLookupStatusText;
        set
        {
            _drugLookupStatusText = value;
            OnPropertyChanged(nameof(DrugLookupStatusText));
        }
    }

    private Brush _drugLookupForeground = default!;
    public Brush DrugLookupForeground
    {
        get => _drugLookupForeground;
        set
        {
            _drugLookupForeground = value;
            OnPropertyChanged(nameof(DrugLookupForeground));
        }
    }
    public ICommand GoBackCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand ScanBarcodeCommand { get; }
    public ICommand LookupDrugCommand { get; }

    public ProductViewModel(int productId, InventoryService inventoryService, DrugService drugService)
    {
        _inventoryService = inventoryService;
        _drugService = drugService;
        GoBackCommand = new RelayCommand(_ => GoBackToInventory());
        SaveCommand = new AsyncRelayCommand(async _ => await SaveProductAsync());
        ScanBarcodeCommand = new RelayCommand(ExecuteScanBarcodeCommand);
        LookupDrugCommand = new RelayCommand(ExecuteLookupDrugCommand);
        LoadProductAsync(productId);
    }

    private void ExecuteScanBarcodeCommand(object? parameter)
    {
        //Product.UPC = string.Empty;
        ScannerStatusText = "Ready to Scan";
        ScannerForeground = Brushes.Green;
        //ScanBarcodeBtnEnabled = false;
    }

    private async void ExecuteLookupDrugCommand(object? parameter = null)
    {
        if (string.IsNullOrWhiteSpace(Product.DIN))
            return;

        IsLoading = true;
        try
        {
            DrugInfo = await _drugService.GetDrugInfoByDINAsync(Product.DIN);
            if (DrugInfo != null)
            {
                Product.Brand = DrugInfo.Product?.BrandName ?? string.Empty;

                DrugLookupStatusText = "Drug Information Found";
                DrugLookupForeground = Brushes.Green;

                DrugInfoIsExpanded = true;
            }
            else
            {
                DrugLookupStatusText = "Drug Information Not Found!";
                DrugLookupForeground = Brushes.Red;

                DrugInfoIsExpanded = false;
            }
        }
        catch (Exception ex)
        {
            // Handle error
            DrugLookupStatusText = ex.Message;
            DrugLookupForeground = Brushes.Red;
            DrugInfoIsExpanded = false;
        }
        finally
        {
            IsLoading = false;
        }
    }


    private async void LoadProductAsync(int productId)
    {
        IsLoading = true;
        DrugInfoIsExpanded = false;
        try
        {
            Product = await _inventoryService.GetProductByIdAsync(productId) ?? new Product();

            ExecuteLookupDrugCommand();

            StatusText = "Product Loaded Successfully!";
            StatusForeground = Brushes.Green;
        }
        catch (Exception ex)
        {
            // Handle or log errors
            //Product = new Product { Name = "Error loading product", Id = productId };
            StatusText = $"Error Loading Product: {ex.Message}";
            StatusForeground = Brushes.Red;
        }
        finally
        {
            IsLoading = false;
        }
    }
    private bool ValidateInputs()
    {
        // Check if all required fields are valid
        bool valid = !string.IsNullOrWhiteSpace(Product.UPC)
                              && !string.IsNullOrWhiteSpace(Product.Name);

        if (!valid)
        {
            StatusText = "UPC, Product are needed for submission.";
            StatusForeground = Brushes.Red;
        }
        else
        {
            StatusText = string.Empty;
            StatusForeground = Brushes.Green;
        }

        return valid;
    }
    private async Task SaveProductAsync()
    {
        if (!ValidateInputs()) { return; }

        try
        {
            await _inventoryService.UpdateProductAsync(Product);
            MessageBoxResult result = MessageBox.Show("Product Updated Successfully.", "Confirmation", MessageBoxButton.OK, MessageBoxImage.Information);
            if (result == MessageBoxResult.OK) {
                GoBackToInventory();
            }            
        }
        catch (Exception ex)
        {
            // Handle errors, e.g., show a message to the user
            StatusText = $"Error Saving Product: {ex.Message}";
            StatusForeground = Brushes.Red;
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
