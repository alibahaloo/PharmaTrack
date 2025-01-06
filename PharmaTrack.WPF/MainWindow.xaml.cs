using PharmaTrack.WPF.Controls;
using PharmaTrack.WPF.Helpers;
using System.Windows;

namespace PharmaTrack.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly CalendarControl calendarControl = new();
        private readonly StockTransferControl stockTransferControl = new();
        private readonly LoginControl _loginControl;
        private readonly AuthService _authService;
        private bool _isLoggedIn = false;
        public MainWindow(LoginControl loginControl, AuthService authService)
        {
            InitializeComponent();
            _loginControl = loginControl;
            _authService = authService;

            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            _isLoggedIn = await CheckAuth();

            if (_isLoggedIn)
            {
                LoadMySchedule();
            }
            else
            {
                LoadLogin();
            }
        }

        private async Task<bool> CheckAuth()
        {
            var (accessToken, refreshToken, userName) = TokenStorage.ReadTokens();

            if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(refreshToken) && !string.IsNullOrEmpty(userName))
            {
                // Check refreshToken validity
                try
                {
                    var response = await _authService.RefreshTokenAsync(refreshToken);

                    if (response != null && response.Success)
                    {
                        // Save tokens securely
                        TokenStorage.SaveTokens(response.Content.AccessToken, response.Content.RefreshToken, response.Content.UserName, true);
                        return true;
                    }
                }
                catch
                {
                    // Handle error, e.g., logging
                }
            }

            return false;
        }

        private void LoadMySchedule()
        {
            calendarControl.Mode = CalendarMode.SingleUser;

            // Subscribe to the MonthChanged event
            calendarControl.MonthChanged -= MyCalendar_MonthChanged; // Avoid multiple subscriptions
            calendarControl.MonthChanged += MyCalendar_MonthChanged;

            // Load CalendarControl into the ContentFrame
            ContentFrame.Content = calendarControl;

            // Load initial events for the current month
            MyCalendar_MonthChanged(this, DateTime.Now);
        }

        private void MyScheduleMenuItem_Click(object sender, RoutedEventArgs e)
        {
            LoadMySchedule();
        }

        private void LoadLogin()
        {
            ContentFrame.Content = _loginControl;
        }

        private void LoginMenuItem_Click(object sender, RoutedEventArgs e)
        {
            LoadLogin();
        }

        private void StockTransferMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Load StockTransferControl into the ContentFrame
            ContentFrame.Content = stockTransferControl;
        }

        private void InventoryMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder for Inventory Control
            MessageBox.Show("Inventory feature is not yet implemented.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MyCalendar_MonthChanged(object? sender, DateTime selectedMonth)
        {
            // Define the logic for loading events dynamically
            calendarControl.LoadEventsForMonth(month =>
            {
                // Example: Replace with actual logic to fetch events for the given month
                return new Dictionary<DateTime, string>
                {
                    { new DateTime(month.Year, month.Month, 5), "Meeting" },
                    { new DateTime(month.Year, month.Month, 15), "Birthday" },
                    { new DateTime(month.Year, month.Month, 25), "Holiday" },
                    { new DateTime(month.Year, month.Month, 24), string.Empty }
                };
            });
        }
    }
}