using PharmaTrack.Shared.APIModels;
using PharmaTrack.Shared.DBModels;
using PharmaTrack.WPF.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Windows.Input;
using System.Windows.Media;

namespace PharmaTrack.WPF.ViewModels
{
    public class TransactionsViewModel : INotifyPropertyChanged
    {
        private readonly InventoryService _inventoryService;
        public ObservableCollection<Transaction> Transactions { get; set; } = [];
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

        private bool _isStockBoth = true;
        public bool IsStockBoth
        {
            get => _isStockBoth;
            set { _isStockBoth = value; OnPropertyChanged(nameof(IsStockBoth)); }
        }
        private bool _isStockIn = false;
        public bool IsStockIn
        {
            get => _isStockIn;
            set { _isStockIn = value; OnPropertyChanged(nameof(IsStockIn)); }
        }
        private bool _isStockOut = false!;
        public bool IsStockOut
        {
            get => _isStockOut;
            set { _isStockOut = value; OnPropertyChanged(nameof(IsStockOut)); }
        }

        public ObservableCollection<string> Users { get; } = [];
        private string _selectedUser = string.Empty;
        public string SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (_selectedUser != value)
                {
                    _selectedUser = value;
                    OnPropertyChanged(nameof(SelectedUser));
                }
            }
        }

        // Computed Transaction Type Filter
        public TransactionType? SelectedTransactionType
        {
            get
            {
                if (IsStockIn) return TransactionType.In;
                if (IsStockOut) return TransactionType.Out;
                return null; // Both selected
            }
        }

        public async Task LoadUsersAsync()
        {
            var users = await _usersService.GetUsernamesAsync();

            Users.Clear();
            if (users != null)
            {
                foreach (var user in users)
                {
                    Users.Add(user);
                }
            }
        }

        public ICommand LoadTransactionsCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand ApplyFiltersCommand { get; }
        public ICommand ResetFiltersCommand { get; }
        private readonly UsersService _usersService;

        public TransactionsViewModel(InventoryService inventoryService, UsersService usersService)
        {
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _usersService = usersService;
            LoadTransactionsCommand = new AsyncRelayCommand(async _ => await LoadTransactionsAsync());
            NextPageCommand = new AsyncRelayCommand(async _ => await ChangePageAsync(1));
            PreviousPageCommand = new AsyncRelayCommand(async _ => await ChangePageAsync(-1));

            ApplyFiltersCommand = new AsyncRelayCommand(async _ => await LoadTransactionsAsync());
            ResetFiltersCommand = new RelayCommand(_ => ResetFilters());
        }
        public async Task ViewModelLoaded()
        {
            await LoadUsersAsync(); // Ensure users are loaded before filtering
            await LoadTransactionsAsync();
        }
        private async void ResetFilters()
        {
            ProductName = null;
            Brand = null;
            UPC = null;
            SelectedUser = string.Empty;
            IsStockBoth = true;
            IsStockIn = false;
            IsStockOut = false;
            await LoadTransactionsAsync();
        }
        public async Task LoadTransactionsAsync()
        {
            IsLoading = true;
           
            try
            {
                // Create a TransactionsRequest with filters from ViewModel properties
                var request = new TransactionsRequest
                {
                    UPC = UPC,
                    Product = ProductName,
                    Brand = Brand,
                    CreatedBy = SelectedUser,
                    Type = SelectedTransactionType
                };

                // Call API with filters
                var response = await _inventoryService.GetTransactionsAsync(request, CurrentPage);

                if (response != null)
                {
                    Transactions.Clear();
                    foreach (var transaction in response.Data)
                    {
                        Transactions.Add(transaction);
                    }

                    CurrentPage = response.CurrentPage;
                    TotalPages = response.TotalPageCount;
                }

                StatusMessage = "Transactions loaded successfully.";
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
                await LoadTransactionsAsync();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
