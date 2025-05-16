using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PharmaTrack.PWA.Helpers
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
    }
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
                           .ReadFromJsonAsync<LoginResponse>();
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

        public async Task<List<User>> GetUsernames()
        {
            var url = "users/usernames";
            try
            {
                // 1) Fetch the raw strings
                var usernames = await _http.GetFromJsonAsync<List<string>>(url, _jsonOptions);

                // If the API gave us nothing, bail out with an empty list
                if (usernames is null || usernames.Count == 0)
                    return [];

                // 2) Map each into a User object
                return [.. usernames
                    .Select((name, idx) => new User
                    {
                        // idx+1 just gives you a simple Id; 
                        // if your back-end knows real IDs, fetch them instead
                        Id = idx + 1,
                        Username = name
                    })];
            }
            catch (HttpRequestException)
            {
                // TODO: log error or handle accordingly
                return [];
            }
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
