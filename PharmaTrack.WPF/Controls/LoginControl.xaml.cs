using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.Json.Serialization;
using System.IO;
using PharmaTrack.WPF.Helpers;
namespace PharmaTrack.WPF.Controls
{
    

    public class ApiResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("content")]
        public Content Content { get; set; } = default!;
    }

    public class Content
    {
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; } = default!;

        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; } = default!;

        [JsonPropertyName("userName")]
        public string UserName { get; set; } = default!;
    }


    /// <summary>
    /// Interaction logic for LoginControl.xaml
    /// </summary>
    public partial class LoginControl : UserControl
    {
        private readonly HttpClient httpClient = new();

        public LoginControl()
        {
            InitializeComponent();
            UsernameTextBox.Text = "user@email.com";
            PasswordBox.Password = "B4guy#kSDvKJJP+";
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Username and password cannot be empty.");
                return;
            }

            try
            {
                var payload = new { username, password };
                string json = JsonSerializer.Serialize(payload);
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.PostAsync("https://localhost:8082/api/auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    // Read the response content
                    string responseContent = await response.Content.ReadAsStringAsync();

                    // Deserialize the JSON response
                    ApiResponse? apiResponse = JsonSerializer.Deserialize<ApiResponse>(responseContent);

                    if (apiResponse != null && apiResponse.Success)
                    {
                        string accessToken = apiResponse.Content.AccessToken;
                        string refreshToken = apiResponse.Content.RefreshToken;
                        string userName = apiResponse.Content.UserName;

                        // Store securely
                        TokenStorage.SaveTokens(accessToken, refreshToken, userName);
                        // Handle login success
                        //HandleLoginSuccess();
                    }
                    else
                    {
                        ShowError("Invalid response from the server.");
                    }
                }
                else
                {
                    ShowError("Invalid username or password.");
                }
            }
            catch (Exception ex)
            {
                ShowError("An error occurred: " + ex.Message);
            }
        }

        private void ShowError(string message)
        {
            ErrorTextBlock.Text = message;
            ErrorTextBlock.Visibility = Visibility.Visible;
        }

        private void HandleLoginSuccess()
        {
            // Navigate to the main application view or perform post-login actions
            //((MainWindow)Application.Current.MainWindow).ContentFrame.Content = new MainControl();
        }
    }
}
