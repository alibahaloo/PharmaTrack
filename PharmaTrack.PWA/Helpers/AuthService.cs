using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Text.Json;
using PharmaTrack.Core.DTOs;

namespace PharmaTrack.PWA.Helpers
{
    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _storage;
        private readonly JwtAuthStateProvider _authState;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };
        public AuthService(HttpClient http,
                           ILocalStorageService storage,
                           AuthenticationStateProvider authState)
        {
            _http = http;
            _storage = storage;
            _authState = (JwtAuthStateProvider)authState;
        }
        public async Task<string> GetAccessTokenAsync() =>
            await _storage.GetItemAsync<string>("accessToken") ?? string.Empty;

        public async Task<bool> AccessTokenExpiresSoonAsync()
        {
            var token = await GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(token)) return true;
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            // if less than 1 minute until expiry, “soon” = true
            return jwt.ValidTo - DateTime.UtcNow < TimeSpan.FromMinutes(1);
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
                           .ReadFromJsonAsync<AuthDto>();
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
            // fire-and-forget logout on server
            var rt = await _storage.GetItemAsync<string>("refreshToken");
            await _http.PostAsJsonAsync("auth/logout", new { refreshToken = rt });

            await _storage.RemoveItemAsync("accessToken");
            await _storage.RemoveItemAsync("refreshToken");
            await _storage.RemoveItemAsync("userName");
            await _storage.RemoveItemAsync("isAdmin");

            _authState.NotifyUserLogout();
        }

        public async Task<bool> TryRefreshTokenAsync(CancellationToken ct = default)
        {
            // 1) do we even have a refresh token?
            var refresh = await _storage.GetItemAsync<string>("refreshToken");
            if (string.IsNullOrWhiteSpace(refresh))
                return false;

            // 2) hit the refresh endpoint
            var resp = await _http.PostAsJsonAsync(
                "auth/refresh",
                new { refreshToken = refresh });

            if (!resp.IsSuccessStatusCode)
                return false;

            // 3) deserialize the new tokens
            var data = await resp.Content
                           .ReadFromJsonAsync<AuthDto>();
            if (data == null || string.IsNullOrWhiteSpace(data.AccessToken))
                return false;

            // 4) store them
            await _storage.SetItemAsync("accessToken", data.AccessToken);
            await _storage.SetItemAsync("refreshToken", data.RefreshToken);
            await _storage.SetItemAsync("userName", data.UserName);
            await _storage.SetItemAsync("isAdmin", data.IsAdmin);

            // 5) notify Blazor auth
            _authState.NotifyUserAuthentication(data.AccessToken);
            return true;
        }

        public async Task<List<UserDto>> GetUsernames()
        {
            var url = "users/usernames";
            try
            {
                // 1) Fetch the raw strings
                var usernames = await _http.GetFromJsonAsync<List<UserDto>>(url, _jsonOptions);

                // If the API gave us nothing, bail out with an empty list
                if (usernames is null || usernames.Count == 0)
                    return [];

                return usernames;
            }
            catch (HttpRequestException)
            {
                // TODO: log error or handle accordingly
                return [];
            }
        }
    }
}
