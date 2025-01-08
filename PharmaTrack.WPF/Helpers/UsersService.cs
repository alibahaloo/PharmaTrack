using Microsoft.Extensions.Configuration;
using PharmaTrack.Shared.APIModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PharmaTrack.WPF.Helpers
{    
    public class UsersService
    {
        private readonly HttpClient _httpClient;
        private readonly string _usersUrl;
        public UsersService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            // Safely handle null or empty configuration value
            _usersUrl = configuration["UsersUrls:Users"]
                        ?? throw new ArgumentException("Users URL is not configured in the application settings.", nameof(configuration));
        }

        public async Task<PagedResponse<UserDto>?> GetUsersAsync(int curPage = 1)
        {
            string? accessToken = TokenStorage.AccessToken;
            if (accessToken == null) { throw new UnauthorizedAccessException(accessToken); }

            // Add the JWT to the headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.GetAsync($"{_usersUrl}?curPage={curPage}");
            if (response.IsSuccessStatusCode)
            {
                // Parse the response (deserialize JSON into Product object)
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PagedResponse<UserDto>>(responseData, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            throw response.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => new UnauthorizedAccessException($"{response.StatusCode}: Invalid or expired refresh token!"),
                _ => new HttpRequestException($"{await response.Content.ReadAsStringAsync()}"),
            };
        }
    }
}
