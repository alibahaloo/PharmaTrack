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
        private readonly HttpClient httpClient = new();
        private string username = string.Empty;
        private string password = string.Empty;
        private string errorMessage = string.Empty;
        private bool isLoginEnabled;
        private bool isLoggingIn;

        public event PropertyChangedEventHandler? PropertyChanged;

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
        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
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
                var payload = new { username, password };
                string json = JsonSerializer.Serialize(payload);
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.PostAsync("https://localhost:8082/api/auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse>(responseContent);

                    if (apiResponse != null && apiResponse.Success)
                    {
                        // Save tokens securely
                        TokenStorage.SaveTokens(apiResponse.Content.AccessToken, apiResponse.Content.RefreshToken, apiResponse.Content.UserName);
                        // Handle login success logic here
                    }
                    else
                    {
                        ErrorMessage = "Invalid response from the server.";
                    }
                }
                else
                {
                    ErrorMessage = "Invalid username or password.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred: " + ex.Message;
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
