using Microsoft.Extensions.Configuration;
using PharmaTrack.Shared.APIModels;
using PharmaTrack.Shared.DBModels;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace PharmaTrack.WPF.Helpers
{
    public class ScheduleService
    {
        private readonly HttpClient _httpClient;
        private readonly string _schedulesUrl;
        private readonly string _userScheduleUrl;

        public ScheduleService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            // Safely handle null or empty configuration value
            _schedulesUrl = configuration["SchedulesUrls:Schedules"]
                        ?? throw new ArgumentException("Schedules URL is not configured in the application settings.", nameof(configuration));
            _userScheduleUrl = configuration["SchedulesUrls:UserSchedules"]
                        ?? throw new ArgumentException("UserSchedules URL is not configured in the application settings.", nameof(configuration));
        }
        public async Task<bool> CreateScheduleAsync(ScheduleTaskRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "request cannot be null.");
            }

            // Serialize the request body
            var jsonBody = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            string? accessToken = TokenStorage.AccessToken;
            if (accessToken == null) { throw new UnauthorizedAccessException(accessToken); }

            // Add the JWT to the headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            // Make the POST request
            var response = await _httpClient.PostAsync(_schedulesUrl, content);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            throw response.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => new UnauthorizedAccessException($"{response.StatusCode}: Invalid or expired refresh token!"),
                _ => new HttpRequestException($"An error occurred: {await response.Content.ReadAsStringAsync()}"),
            };
        }

        public async Task<List<ScheduleTask>?> GetScheduleTasksAsync(DateTime month)
        {
            string? accessToken = TokenStorage.AccessToken;
            if (accessToken == null) { throw new UnauthorizedAccessException(accessToken); }

            // Add the JWT to the headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.GetAsync($"{_schedulesUrl}?month={month}");

            if (response.IsSuccessStatusCode)
            {
                // Parse the response (deserialize JSON into Product object)
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<ScheduleTask>>(responseData, new System.Text.Json.JsonSerializerOptions
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

        public async Task<List<ScheduleTask>?> GetUserScheduleTasksAsync(DateTime month, string userName)
        {
            string? accessToken = TokenStorage.AccessToken;
            if (accessToken == null) { throw new UnauthorizedAccessException(accessToken); }

            // Add the JWT to the headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.GetAsync($"{_userScheduleUrl}/{userName}?month={month}");

            if (response.IsSuccessStatusCode)
            {
                // Parse the response (deserialize JSON into Product object)
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<ScheduleTask>>(responseData, new System.Text.Json.JsonSerializerOptions
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
