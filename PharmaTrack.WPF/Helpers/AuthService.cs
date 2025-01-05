using Microsoft.Extensions.Configuration;
using PharmaTrack.WPF.ViewModels;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace PharmaTrack.WPF.Helpers
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly string _loginUrl;

        public AuthService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            // Safely handle null or empty configuration value
            _loginUrl = configuration["ApiUrls:Login"]
                        ?? throw new ArgumentException("Login URL is not configured in the application settings.", nameof(configuration));
        }

        public async Task<ApiResponse?> LoginAsync(string username, string password)
        {
            var payload = new { username, password };
            string json = JsonSerializer.Serialize(payload);
            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(_loginUrl, content);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ApiResponse>(responseContent);
            }

            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.Unauthorized:
                    throw new UnauthorizedAccessException($"{response.StatusCode}: Invalid Username and/or Password!");
                default:
                    throw new HttpRequestException($"An error occurred: {response.StatusCode}");
            }
        }
    }
}
