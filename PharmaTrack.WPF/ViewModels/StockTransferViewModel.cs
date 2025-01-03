using System.ComponentModel;
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

        public StockTransferViewModel()
        {
            // Initialize defaults
            StatusText = "Ready to Scan";
            StatusForeground = Brushes.Green;
            Quantity = "1";

            // Initialize commands
            ScanBarcodeCommand = new RelayCommand(ExecuteScanBarcodeCommand);
            SubmitCommand = new RelayCommand(ExecuteSubmitCommand);
        }

        private void ExecuteScanBarcodeCommand(object? parameter)
        {
            UPCInput = string.Empty;
            StatusText = "Ready to Scan";
            StatusForeground = Brushes.Green;
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
