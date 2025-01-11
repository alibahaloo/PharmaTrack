using PharmaTrack.WPF.Controls;
using PharmaTrack.WPF.Helpers;
using System.ComponentModel;
using System.Windows.Input;

namespace PharmaTrack.WPF.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly AuthService _authService;
        private readonly LoginControl _loginControl;
        private readonly CalendarControl _calendarControl;
        private readonly CalendarControlViewModel _calendarViewModel;
        private readonly StockTransferControl _stockTransferControl;
        private readonly InventoryControl _inventoryControl;
        private readonly TransactionsControl _transactionsControl;
        private readonly UsersControl _usersControl;
        private readonly LoadingControl _loadingControl = new();
        private object _currentContent = default!;
        private bool _isLoggedIn;
        private bool _isUserAdmin = false;
        private bool _isLoaded = false;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand LoginCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand ShowMyScheduleCommand { get; }
        public ICommand ShowStockTransferCommand { get; }
        public ICommand ShowInventoryCommand { get; }
        public ICommand ShowTransactionsCommand { get; }
        public ICommand ShowUsersCommand { get; }
        public ICommand RetryCommand { get; }

        public bool IsLoaded
        {
            get => _isLoaded;
            set
            {
                _isLoaded = value;
                OnPropertyChanged(nameof(IsLoaded));
            }
        }

        public object CurrentContent
        {
            get => _currentContent;
            set
            {
                _currentContent = value;
                OnPropertyChanged(nameof(CurrentContent));
            }
        }

        public bool IsUserAdmin
        {
            get => _isUserAdmin;
            set
            {
                _isUserAdmin = value;
                OnPropertyChanged(nameof(IsUserAdmin));
            }
        }

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set
            {
                _isLoggedIn = value;
                OnPropertyChanged(nameof(IsLoggedIn));
            }
        }

        private void OnLoginSuccessful()
        {
            IsLoggedIn = true;
            LoadMySchedule();
        }

        private async void InitializeAsync()
        {
            IsLoggedIn = await CheckAuth();

            if (!IsLoaded) return;

            if (IsLoggedIn)
                LoadMySchedule();
            else
                LoadLogin();
        }

        public MainWindowViewModel(
            AuthService authService,
            LoginControl loginControl,
            CalendarControl calendarControl,
            CalendarControlViewModel calendarViewModel,
            StockTransferControl stockTransferControl,
            InventoryControl inventoryControl,
            TransactionsControl transactionsControl,
            UsersControl usersControl)
        {
            _authService = authService;
            _loginControl = loginControl;
            _calendarControl = calendarControl;
            _calendarViewModel = calendarViewModel;
            _stockTransferControl = stockTransferControl;
            _inventoryControl = inventoryControl;
            _transactionsControl = transactionsControl;
            _usersControl = usersControl;

            // Set the CalendarControl's ViewModel
            _calendarControl.DataContext = _calendarViewModel;

            InitializeAsync();

            // Subscribe to LoginViewModel's LoginSuccessful event
            if (_loginControl.DataContext is LoginViewModel loginViewModel)
            {
                loginViewModel.LoginSuccessful += OnLoginSuccessful;
            }

            LoginCommand = new RelayCommand(_ => LoadLogin());
            LogoutCommand = new RelayCommand(async _ => await LogoutAsync());
            ShowMyScheduleCommand = new RelayCommand(_ => LoadMySchedule());
            ShowStockTransferCommand = new RelayCommand(_ => LoadStockTransfer());
            ShowInventoryCommand = new RelayCommand(_ => LoadInventory());
            ShowTransactionsCommand = new RelayCommand(_ => LoadTransactions());
            ShowUsersCommand = new RelayCommand(_ => LoadUsers());
            RetryCommand = new RelayCommand(_ => Retry());
        }

        private void Retry()
        {
            InitializeAsync();
        }

        private async Task<bool> CheckAuth()
        {
            CurrentContent = _loadingControl;
            bool result = false;
            try
            {
                var (accessToken, refreshToken, userName, isUserAdmin) = TokenStorage.ReadTokens();
                if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(refreshToken))
                {
                    var response = await _authService.RefreshTokenAsync(refreshToken);
                    if (response != null)
                    {
                        if (response.IsAdmin) IsUserAdmin = true;

                        TokenStorage.SaveTokens(response.AccessToken, response.RefreshToken, response.UserName, response.IsAdmin, true);
                        result = true;
                    }
                }

                IsLoaded = true;
            }
            catch (Exception ex)
            {
                _loadingControl.SetErrorMessage(ex.Message);
                IsLoaded = false;
            }

            return result;
        }

        private async Task LogoutAsync()
        {
            var refreshToken = TokenStorage.RefreshToken;
            if (refreshToken != null)
            {
                var response = await _authService.LogoutAsync(refreshToken);
                if (response)
                {
                    TokenStorage.DeleteTokens();
                    IsLoggedIn = false;
                    LoadLogin();
                }
            }
        }

        private void LoadMySchedule()
        {
            CurrentContent = _calendarControl;
        }

        private void LoadUsers()
        {
            CurrentContent = _usersControl;
        }

        private void LoadTransactions()
        {
            CurrentContent = _transactionsControl;
        }

        private void LoadInventory()
        {
            CurrentContent = _inventoryControl;
        }

        private void LoadLogin()
        {
            CurrentContent = _loginControl;
        }

        private void LoadStockTransfer()
        {
            CurrentContent = _stockTransferControl;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
