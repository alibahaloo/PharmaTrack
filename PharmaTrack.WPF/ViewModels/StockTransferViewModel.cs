using PharmaTrack.Shared.DBModels;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;

namespace PharmaTrack.WPF.ViewModels
{
    public class StockTransferViewModel : INotifyPropertyChanged
    {
        private string _upcInput = default!;
        private string _statusText = default!;
        private Brush _statusForeground = default!;
        private string _quantity = default!;
        private string _npn = default!;
        private string _din = default!;
        private string _brand = default!;
        private string _productDescription = default!;
        private bool _isStockIn = true;
        private bool _isStockOut = default!;
        private bool _lookUpBtnEnabled = default!;
        private bool _scanBarcodeBtnEnabled = true;
        private bool _isLoading = false;
        private Product? _selectedProduct = default!;
        private string _lookupStatus = default!;

        public Product? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                OnPropertyChanged(nameof(SelectedProduct));
            }
        }
        public string LookupStatus
        {
            get => _lookupStatus;
            set
            {
                _lookupStatus = value;
                OnPropertyChanged(nameof(LookupStatus));
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(); // Notify UI of changes
            }
        }
        public bool ScanBarcodeBtnEnabled
        {
            get => _scanBarcodeBtnEnabled;
            set
            {
                _scanBarcodeBtnEnabled = value;
                OnPropertyChanged(); // Notify UI of changes
            }
        }
        public bool LookUpBtnEnabled
        {
            get => _lookUpBtnEnabled;
            set
            {
                _lookUpBtnEnabled = value;
                OnPropertyChanged(); // Notify UI of changes
            }
        }

        public string UPCInput
        {
            get => _upcInput;
            set
            {
                _upcInput = value;
                LookUpBtnEnabled = !string.IsNullOrEmpty(value); // Update the button state
                OnPropertyChanged(); // Notify UI that UPCInput has changed
            }
        }

        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        public Brush StatusForeground
        {
            get => _statusForeground;
            set { _statusForeground = value; OnPropertyChanged(); }
        }

        public string Quantity
        {
            get => _quantity;
            set
            {
                if (int.TryParse(value, out int parsedValue))
                {
                    if (parsedValue < 1) value = "1";
                    if (parsedValue > 1000) value = "1000";
                }
                else
                {
                    value = "1"; // Reset to default if invalid
                }

                _quantity = value;
                OnPropertyChanged();
            }
        }

        public string NPN
        {
            get => _npn;
            set { _npn = value; OnPropertyChanged(); }
        }

        public string DIN
        {
            get => _din;
            set { _din = value; OnPropertyChanged(); }
        }

        public string Brand
        {
            get => _brand;
            set { _brand = value; OnPropertyChanged(); }
        }

        public string ProductDescription
        {
            get => _productDescription;
            set { _productDescription = value; OnPropertyChanged(); }
        }

        public bool IsStockIn
        {
            get => _isStockIn;
            set { _isStockIn = value; OnPropertyChanged(); }
        }

        public bool IsStockOut
        {
            get => _isStockOut;
            set { _isStockOut = value; OnPropertyChanged(); }
        }

        public ICommand ScanBarcodeCommand { get; }
        public ICommand SubmitCommand { get; }
        public ICommand LookupCommand { get; }

        public StockTransferViewModel()
        {
            // Initialize defaults
            StatusText = "Ready to Scan";
            StatusForeground = Brushes.Green;
            Quantity = "1";

            // Initialize commands
            ScanBarcodeCommand = new RelayCommand(ExecuteScanBarcodeCommand);
            SubmitCommand = new RelayCommand(ExecuteSubmitCommand);
            LookupCommand = new RelayCommand(ExecuteLookupCommand);
        }
        private async void ExecuteLookupCommand(object? parameter)
        {
            IsLoading = true;
            try
            {
                // Ensure the UPC input is valid
                if (string.IsNullOrWhiteSpace(UPCInput))
                {
                    throw new ArgumentException("UPC input cannot be empty.");
                }

                // Prepare the HttpClient
                using (HttpClient client = new HttpClient())
                {
                    // Construct the API URL with the UPC parameter
                    string apiUrl = $"https://localhost:8082/api/inventory/upc/{UPCInput}";

                    // Make the API request
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        // Parse the response (deserialize JSON into Product object)
                        var responseData = await response.Content.ReadAsStringAsync();
                        Product? product = System.Text.Json.JsonSerializer.Deserialize<Product>(responseData, new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        // Assign product data to a bound property

                        if (product != null)
                        {
                            SelectedProduct = product; // Ensure SelectedProduct is a bindable property in the ViewModel
                            UPCInput = product.UPC;
                            NPN = product.NPN ?? string.Empty;
                            DIN = product.DIN ?? string.Empty;
                            Brand = product.Brand ?? string.Empty;
                            ProductDescription = product.Name;
                        }

                        LookupStatus = "Product Information Found";
                    }
                    else
                    {
                        LookupStatus = $"{response.ReasonPhrase}";
                        // Handle API errors
                        //throw new HttpRequestException($"Error: {response.StatusCode} - {response.ReasonPhrase}");

                    }
                }

            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., show error message)
                LookupStatus = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteScanBarcodeCommand(object? parameter)
        {
            UPCInput = string.Empty;
            StatusText = "Ready to Scan";
            StatusForeground = Brushes.Green;
            ScanBarcodeBtnEnabled = false;
        }

        private void ExecuteSubmitCommand(object? parameter)
        {
            StatusText = "Submitted!";
            StatusForeground = Brushes.Blue;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
