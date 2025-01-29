using PharmaTrack.Shared.APIModels;
using PharmaTrack.Shared.DBModels;
using PharmaTrack.WPF.Controls;
using PharmaTrack.WPF.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace PharmaTrack.WPF.ViewModels
{
    public class InventoryViewModel : INotifyPropertyChanged
    {
        private readonly InventoryService _inventoryService;

        public ObservableCollection<Product> Products { get; set; } = [];

        private string _statusMessage = default!;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        private int _currentPage = 1;
        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
            }
        }
        private int _totalPages = 1;
        public int TotalPages
        {
            get => _totalPages;
            set
            {
                _totalPages = value;
                OnPropertyChanged(nameof(TotalPages));
            }
        }
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged(nameof(IsLoading)); // Notify the UI
                }
            }
        }
        private Brush _statusForeground = default!;
        public Brush StatusForeground
        {
            get => _statusForeground;
            set { _statusForeground = value; OnPropertyChanged(nameof(StatusForeground)); }
        }
        private string? _productName;
        public string? ProductName
        {
            get => _productName;
            set { _productName = value; OnPropertyChanged(nameof(ProductName)); }
        }
        private string? _brand;
        public string? Brand
        {
            get => _brand;
            set { _brand = value; OnPropertyChanged(nameof(Brand)); }
        }
        private string? _upc;
        public string? UPC
        {
            get => _upc;
            set { _upc = value; OnPropertyChanged(nameof(UPC)); }
        }
        private string? _din;
        public string? DIN
        {
            get => _din;
            set { _din = value; OnPropertyChanged(nameof(DIN)); }
        }
        private string? _npn;
        public string? NPN
        {
            get => _npn;
            set { _npn = value; OnPropertyChanged(nameof(NPN)); }
        }
        private Product _selectedProduct = default!;
        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                OnPropertyChanged(nameof(SelectedProduct));
            }
        }

        private string _scannerStatusText = default!;
        public string ScannerStatusText
        {
            get => _scannerStatusText;
            set
            {
                _scannerStatusText = value;
                OnPropertyChanged(nameof(ScannerStatusText));
            }
        }
        private Brush _scannerForeground = default!;
        public Brush ScannerForeground
        {
            get => _scannerForeground;
            set 
            { 
                _scannerForeground = value; 
                OnPropertyChanged(nameof(ScannerForeground)); 
            }
        }
        private bool _isUPCInputFocused;
        public bool IsUPCInputFocused
        {
            get => _isUPCInputFocused;
            set
            {
                _isUPCInputFocused = value;
                OnPropertyChanged(nameof(IsUPCInputFocused));
            }
        }
        public ICommand ViewProductCommand { get; }
        public ICommand LoadProductsCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand ApplyFiltersCommand { get; }
        public ICommand ResetFiltersCommand { get; }
        public ICommand ScanBarcodeCommand { get; }
        public InventoryViewModel(InventoryService inventoryService)
        {
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            LoadProductsCommand = new AsyncRelayCommand(async _ => await LoadProductsAsync());
            NextPageCommand = new AsyncRelayCommand(async _ => await ChangePageAsync(1));
            PreviousPageCommand = new AsyncRelayCommand(async _ => await ChangePageAsync(-1));

            ApplyFiltersCommand = new AsyncRelayCommand(async _ => await LoadProductsAsync());
            ResetFiltersCommand = new RelayCommand(_ => ResetFilters());

            ScanBarcodeCommand = new RelayCommand(ExecuteScanBarcodeCommand);

            ViewProductCommand = new RelayCommand(
                param =>
                {
                    if (SelectedProduct != null)
                    {
                        if (Application.Current.MainWindow.DataContext is MainWindowViewModel mainViewModel)
                        {
                            var productControl = new ProductControl(SelectedProduct.Id, inventoryService);
                            mainViewModel.CurrentContent = productControl;
                        }
                    }
                },
                param => SelectedProduct != null);
        }
        private void ExecuteScanBarcodeCommand(object? parameter)
        {
            UPC = string.Empty;
            ScannerStatusText = "Ready to Scan";
            ScannerForeground = Brushes.Green;
        }
        private async void ResetFilters()
        {
            ProductName = null;
            Brand = null;
            UPC = null;
            NPN = null;
            DIN = null;
            await LoadProductsAsync();
        }
        public async Task LoadProductsAsync()
        {
            IsLoading = true;
            try
            {
                // Create a InventoryRequest with filters from ViewModel properties
                var request = new InventoryRequest
                {
                    UPC = UPC,
                    Name = ProductName,
                    Brand = Brand,
                    DIN = DIN,
                    NPN = NPN
                };

                var response = await _inventoryService.GetProductsAsync(request, CurrentPage);
                if (response != null)
                {
                    Products.Clear();
                    foreach (var product in response.Data)
                    {
                        Products.Add(product);
                    }

                    CurrentPage = response.CurrentPage;
                    TotalPages = response.TotalPageCount;
                }
                StatusMessage = "Products loaded successfully.";
                StatusForeground = Brushes.Green;
            }
            catch (UnauthorizedAccessException ex)
            {
                StatusMessage = $"Authorization error: {ex.Message}";
                StatusForeground = Brushes.Red;
            }
            catch (HttpRequestException ex)
            {
                StatusMessage = $"Network error: {ex.Message}";
                StatusForeground = Brushes.Red;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ChangePageAsync(int direction)
        {
            if ((direction == -1 && CurrentPage > 1) || (direction == 1 && CurrentPage < TotalPages))
            {
                CurrentPage += direction;
                await LoadProductsAsync();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
