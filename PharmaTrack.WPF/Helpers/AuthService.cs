using Microsoft.Extensions.Configuration;
using PharmaTrack.Core.DTOs;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace PharmaTrack.WPF.Helpers
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly string _loginUrl;
        private readonly string _refreshUrl;
        private readonly string _logoutUrl;

        public AuthService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            // Safely handle null or empty configuration value
            _loginUrl = configuration["ApiUrls:Login"]
                        ?? throw new ArgumentException("Login URL is not configured in the application settings.", nameof(configuration));
            _refreshUrl = configuration["ApiUrls:Refresh"]
                        ?? throw new ArgumentException("Refresh URL is not configured in the application settings.", nameof(configuration));
            _logoutUrl = configuration["ApiUrls:Logout"]
                        ?? throw new ArgumentException("Logout URL is not configured in the application settings.", nameof(configuration));
        }

        public async Task<AuthDto?> LoginAsync(string username, string password)
        {
            var payload = new { username, password };
            string json = JsonSerializer.Serialize(payload);
            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(_loginUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                AuthDto? authDto;
                try
                {
                    authDto = JsonSerializer.Deserialize<AuthDto>(result, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return authDto;
                }
                catch (Exception jsonEx)
                {
                    throw new JsonException($"Error deserializing data: {jsonEx.Message}");
                }
            }

            throw response.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => new UnauthorizedAccessException($"{response.StatusCode}: Invalid Username and/or Password!"),
                _ => new HttpRequestException($"An error occurred: {response.StatusCode}"),
            };
        }

        public async Task<AuthDto?> RefreshTokenAsync(string refreshToken)
        {
            var payload = new { refreshToken };
            string json = JsonSerializer.Serialize(payload);
            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(_refreshUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                AuthDto? authDto;
                try
                {
                    authDto = JsonSerializer.Deserialize<AuthDto>(result, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return authDto;
                }
                catch (Exception jsonEx)
                {
                    throw new JsonException($"Error deserializing data: {jsonEx.Message}");
                }
            }

            throw response.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => new UnauthorizedAccessException($"{response.StatusCode}: Invalid or expired refresh token!"),
                _ => new HttpRequestException($"An error occurred: {response.StatusCode}"),
            };
        }

        public async Task<bool> LogoutAsync(string refreshToken)
        {
            var payload = new { refreshToken };
            string json = JsonSerializer.Serialize(payload);
            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(_logoutUrl, content);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            throw response.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => new UnauthorizedAccessException($"{response.StatusCode}: Invalid or expired refresh token!"),
                _ => new HttpRequestException($"An error occurred: {response.StatusCode}"),
            };
        }
    }
}
