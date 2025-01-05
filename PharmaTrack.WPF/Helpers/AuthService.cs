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
            _loginUrl = configuration["ApiUrls:Login"];
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

            throw new HttpRequestException($"Error during login: {response.StatusCode}");
        }
    }
}
