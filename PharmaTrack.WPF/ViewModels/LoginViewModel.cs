using PharmaTrack.WPF.Helpers;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace PharmaTrack.WPF.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly AuthService _authService;
        private string username = string.Empty;
        private string password = string.Empty;
        private string errorMessage = string.Empty;
        private bool isLoginEnabled;
        private bool isLoggingIn;
        private bool _rememberMe;

        public event PropertyChangedEventHandler? PropertyChanged;

        // Event to notify login success
        public event Action? LoginSuccessful;
        public string Username
        {
            get => username;
            set
            {
                username = value;
                OnPropertyChanged(nameof(Username));
                UpdateLoginButtonState();
            }
        }

        public string Password
        {
            get => password;
            set
            {
                password = value;
                OnPropertyChanged(nameof(Password));
                UpdateLoginButtonState();
            }
        }

        public string ErrorMessage
        {
            get => errorMessage;
            set
            {
                errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public bool IsLoginEnabled
        {
            get => isLoginEnabled;
            set
            {
                isLoginEnabled = value;
                OnPropertyChanged(nameof(IsLoginEnabled));
            }
        }
        public bool IsLoggingIn
        {
            get => isLoggingIn;
            set
            {
                isLoggingIn = value;
                OnPropertyChanged(nameof(IsLoggingIn));
            }
        }

        public bool RememberMe
        {
            get => _rememberMe;
            set
            {
                if (_rememberMe != value)
                {
                    _rememberMe = value;
                    OnPropertyChanged(nameof(RememberMe));
                }
            }
        }
        public ICommand LoginCommand { get; }

        public LoginViewModel(AuthService authService)
        {
            _authService = authService;
            LoginCommand = new RelayCommand(async _ => await LoginAsync(), _ => IsLoginEnabled);
            Username = "user@email.com";
            Password = "B4guy#kSDvKJJP+";
        }

        private async Task LoginAsync()
        {
            ErrorMessage = string.Empty;
            IsLoggingIn = true;
            try
            {
                var response = await _authService.LoginAsync(Username, Password);

                if (response != null && response.Success)
                {
                    // Save tokens securely
                    TokenStorage.SaveTokens(response.Content.AccessToken, response.Content.RefreshToken, response.Content.UserName, RememberMe);

                    // Notify the parent ViewModel about successful login
                    LoginSuccessful?.Invoke();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsLoggingIn = false;
            }
        }

        private void UpdateLoginButtonState()
        {
            IsLoginEnabled = !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ApiResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("content")]
        public Content Content { get; set; } = new();
    }

    public class Content
    {
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonPropertyName("userName")]
        public string UserName { get; set; } = string.Empty;
    }
}
