using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PharmaTrack.PWA.Helpers
{
    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _storage;
        private readonly JwtAuthStateProvider _authState;

        public AuthService(HttpClient http,
                           ILocalStorageService storage,
                           AuthenticationStateProvider authState)
        {
            _http = http;
            _storage = storage;
            _authState = (JwtAuthStateProvider)authState;
        }

        public async Task<bool> LoginAsync(
            string username,
            string password,
            bool rememberMe)
        {
            var response = await _http.PostAsJsonAsync(
                "auth/login",
                new { username, password });

            if (!response.IsSuccessStatusCode)
                return false;

            var data = await response.Content
                           .ReadFromJsonAsync<LoginResponse>();
            if (data is null || string.IsNullOrWhiteSpace(data.AccessToken))
                return false;

            // 1) store in localStorage
            await _storage.SetItemAsync("accessToken", data.AccessToken);
            await _storage.SetItemAsync("refreshToken", data.RefreshToken);
            await _storage.SetItemAsync("userName", data.UserName);
            await _storage.SetItemAsync("isAdmin", data.IsAdmin);

            // 2) notify Blazor of the new JWT
            _authState.NotifyUserAuthentication(data.AccessToken);

            return true;
        }

        public async Task LogoutAsync()
        {
            await _storage.RemoveItemAsync("accessToken");
            await _storage.RemoveItemAsync("refreshToken");
            await _storage.RemoveItemAsync("userName");
            await _storage.RemoveItemAsync("isAdmin");

            _authState.NotifyUserLogout();
        }

        private class LoginResponse
        {
            [JsonPropertyName("accessToken")]
            public string AccessToken { get; set; }

            [JsonPropertyName("refreshToken")]
            public string RefreshToken { get; set; }

            [JsonPropertyName("userName")]
            public string UserName { get; set; }

            [JsonPropertyName("isAdmin")]
            public bool IsAdmin { get; set; }
        }
    }
}
