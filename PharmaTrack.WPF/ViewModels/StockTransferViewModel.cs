using PharmaTrack.Shared.APIModels;
using PharmaTrack.Shared.DBModels;
using PharmaTrack.DTOs.Drug;
using PharmaTrack.WPF.Helpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
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
        private string _lookupStatusText = default!;
        private Brush _lookupForeground = default!;
        private string _scannerStatusText = default!;
        private Brush _scannerForeground = default!;
        private bool _submitButtonEnabled;
        public bool SubmitButtonEnabled
        {
            get => _submitButtonEnabled;
            set
            {
                _submitButtonEnabled = value;
                OnPropertyChanged();
            }
        }
        public string ScannerStatusText
        {
            get => _scannerStatusText;
            set
            {
                _scannerStatusText = value;
                OnPropertyChanged();
            }
        }

        public Brush ScannerForeground
        {
            get => _scannerForeground;
            set { _scannerForeground = value; OnPropertyChanged(); }
        }

        public string LookupStatusText
        {
            get => _lookupStatusText;
            set
            {
                _lookupStatusText = value;
                OnPropertyChanged(nameof(LookupStatusText));
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
                UpdateSubmitButtonState();
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

        public Brush LookupForeground
        {
            get => _lookupForeground;
            set { _lookupForeground = value; OnPropertyChanged(); }
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
                UpdateSubmitButtonState();
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
            set
            {
                _din = value;
                IsDrugLookupEnabled = !string.IsNullOrEmpty(value); // Update the button state
                OnPropertyChanged();
            }
        }

        public string Brand
        {
            get => _brand;
            set { _brand = value; OnPropertyChanged(); }
        }

        public string ProductDescription
        {
            get => _productDescription;
            set { _productDescription = value; OnPropertyChanged(); UpdateSubmitButtonState(); }
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

        private bool _isDrugLookupEnabled = default!;
        public bool IsDrugLookupEnabled
        {
            get => _isDrugLookupEnabled;
            set
            {
                _isDrugLookupEnabled = value;
                OnPropertyChanged(nameof(IsDrugLookupEnabled));
            }
        }

        public ICommand ScanBarcodeCommand { get; }
        public ICommand SubmitCommand { get; }
        public ICommand LookupCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand LookupDrugCommand { get; }

        private readonly InventoryService _inventoryService;
        private readonly DrugService _drugService;
        public StockTransferViewModel(InventoryService inventoryService, DrugService drugService)
        {


            _inventoryService = inventoryService;
            _drugService = drugService;

            // Initialize commands
            ScanBarcodeCommand = new RelayCommand(ExecuteScanBarcodeCommand);
            SubmitCommand = new RelayCommand(ExecuteSubmitCommand);
            LookupCommand = new RelayCommand(ExecuteLookupCommand);
            ClearCommand = new RelayCommand(_ => ClearForm());
            LookupDrugCommand = new RelayCommand(ExecuteLookupDrugCommand);
        }
        private void ClearForm()
        {
            IsUPCInputFocused = false;

            UPCInput = string.Empty;
            ProductDescription = string.Empty;
            Quantity = string.Empty;
            NPN = string.Empty;
            DIN = string.Empty;
            Brand = string.Empty;
            LookupStatusText = string.Empty;

            DrugInfo = null;

            DrugInfoIsExpanded = false;

            IsUPCInputFocused = true; // Set focus
        }
        private void UpdateSubmitButtonState()
        {
            // Check if all required fields are valid
            SubmitButtonEnabled = !string.IsNullOrWhiteSpace(UPCInput)
                                  && !string.IsNullOrWhiteSpace(ProductDescription)
                                  && !string.IsNullOrWhiteSpace(Quantity);

            if (!SubmitButtonEnabled)
            {
                StatusText = "UPC, Product and Quantity are needed for submission.";
                StatusForeground = Brushes.Red;
            }
            else
            {
                StatusText = "Ready to submit";
                StatusForeground = Brushes.Green;
            }
        }
        private async void ExecuteLookupDrugCommand(object? parameter)
        {
            if (string.IsNullOrWhiteSpace(DIN))
                return;

            IsLoading = true;
            try
            {
                DrugInfoDto? drugInfo = await _drugService.GetDrugInfoByDINAsync(DIN);
                if (drugInfo != null)
                {
                    DrugInfo = drugInfo;

                    Brand = DrugInfo.Product?.BrandName ?? string.Empty;
                    ProductDescription = Brand;

                    DrugInfoIsExpanded = true;

                    DrugLookupStatusText = "Drug Information Found";
                    DrugLookupForeground = Brushes.Green;
                }
                else
                {
                    DrugLookupStatusText = "Drug Information Not Found!";
                    DrugLookupForeground = Brushes.Red;
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

        private async void ExecuteLookupCommand(object? parameter)
        {
            IsLoading = true;
            LookupStatusText = string.Empty;
            try
            {
                // Ensure the UPC input is valid
                if (string.IsNullOrWhiteSpace(UPCInput))
                {
                    throw new ArgumentException("UPC input cannot be empty.");
                }

                Product? product = await _inventoryService.GetProductByUPCAsync(UPCInput);

                if (product != null)
                {
                    UPCInput = product.UPC;
                    NPN = product.NPN ?? string.Empty;
                    DIN = product.DIN ?? string.Empty;
                    Brand = product.Brand ?? string.Empty;
                    ProductDescription = product.Name;
                }

                LookupStatusText = "Product Information Found";
                LookupForeground = Brushes.Green;

                ExecuteLookupDrugCommand(null);


            }
            catch (Exception ex)
            {
                LookupStatusText = ex.Message;
                LookupForeground = Brushes.Red;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteScanBarcodeCommand(object? parameter)
        {
            UPCInput = string.Empty;
            ScannerStatusText = "Ready to Scan";
            ScannerForeground = Brushes.Green;
            ScanBarcodeBtnEnabled = false;
        }

        private async void ExecuteSubmitCommand(object? parameter)
        {
            IsLoading = true;
            try
            {
                TransactionType transactionType = new();

                if (IsStockIn) { transactionType = TransactionType.In; }
                else
                {
                    transactionType = TransactionType.Out;
                }

                StockTransferRequest stockTransferRequest = new()
                {
                    UPC = UPCInput,
                    DIN = DIN,
                    NPN = NPN,
                    Name = ProductDescription,
                    Brand = Brand,
                    Quantity = Int32.Parse(Quantity),
                    Type = transactionType,
                };

                await _inventoryService.StockTransferAsync(stockTransferRequest);

                StatusText = "Stock Transfer Submitted Successfully!";
                StatusForeground = Brushes.Blue;

                MessageBoxResult result = MessageBox.Show("Stock Transfer Submitted Successfully!", "Confirmation", MessageBoxButton.OK, MessageBoxImage.Information);
                if (result == MessageBoxResult.OK)
                {
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                StatusText = ex.Message;
                StatusForeground = Brushes.Red;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
