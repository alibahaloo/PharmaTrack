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
        private readonly CalendarControl _calendarControl = new();
        private readonly StockTransferControl _stockTransferControl;
        private object _currentContent = default!;
        private bool _isLoggedIn;

        public event PropertyChangedEventHandler? PropertyChanged;
        public ICommand LoginCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand ShowMyScheduleCommand { get; }
        public ICommand ShowStockTransferCommand { get; }

        public object CurrentContent
        {
            get => _currentContent;
            set
            {
                _currentContent = value;
                OnPropertyChanged(nameof(CurrentContent));
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

            if (IsLoggedIn)
                LoadMySchedule();
            else
                LoadLogin();
        }
        public MainWindowViewModel(AuthService authService, LoginControl loginControl, StockTransferControl stockTransferControl)
        {
            _authService = authService;
            _loginControl = loginControl;
            _stockTransferControl = stockTransferControl;
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
        }

        private async Task<bool> CheckAuth()
        {
            var (accessToken, refreshToken, userName) = TokenStorage.ReadTokens();
            if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(refreshToken))
            {
                try
                {
                    var response = await _authService.RefreshTokenAsync(refreshToken);
                    if (response != null && response.Success)
                    {
                        TokenStorage.SaveTokens(response.Content.AccessToken, response.Content.RefreshToken, response.Content.UserName, true);
                        return true;
                    }
                }
                catch
                {
                    // Handle exceptions
                }
            }
            return false;
        }

        private async Task LogoutAsync()
        {
            var refreshToken = TokenStorage.LocalRefreshToken;
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
            _calendarControl.Mode = CalendarMode.SingleUser;
            _calendarControl.MonthChanged -= MyCalendar_MonthChanged;
            _calendarControl.MonthChanged += MyCalendar_MonthChanged;
            CurrentContent = _calendarControl;
            MyCalendar_MonthChanged(this, DateTime.Now);
        }

        private void LoadLogin()
        {
            CurrentContent = _loginControl;
        }

        private void LoadStockTransfer()
        {
            CurrentContent = _stockTransferControl;
        }

        private void MyCalendar_MonthChanged(object? sender, DateTime selectedMonth)
        {
            _calendarControl.LoadEventsForMonth(month =>
            {
                return new Dictionary<DateTime, string>
                {
                    { new DateTime(month.Year, month.Month, 5), "Meeting" },
                    { new DateTime(month.Year, month.Month, 15), "Birthday" },
                    { new DateTime(month.Year, month.Month, 25), "Holiday" },
                    { new DateTime(month.Year, month.Month, 24), string.Empty }
                };
            });
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
