using Microsoft.Extensions.Configuration;
using PharmaTrack.Core.DBModels;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace PharmaTrack.WPF.Helpers
{
    public class ScheduleService
    {
        private readonly HttpClient _httpClient;
        private readonly string _monthlyForTeamURL;
        private readonly string _monthlyForUserURL;
        private readonly string _dailyForTeamURL;
        private readonly string _dailyForUserURL;

        public ScheduleService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            // Safely handle null or empty configuration value
            _monthlyForTeamURL = configuration["SchedulesUrls:Monthly"]
                        ?? throw new ArgumentException("Monthly URL is not configured in the application settings.", nameof(configuration));
            _monthlyForUserURL = configuration["SchedulesUrls:MonthlyUser"]
                        ?? throw new ArgumentException("MonthlyUser URL is not configured in the application settings.", nameof(configuration));
            _dailyForTeamURL = configuration["SchedulesUrls:Daily"]
                        ?? throw new ArgumentException("Daily URL is not configured in the application settings.", nameof(configuration));
            _dailyForUserURL = configuration["SchedulesUrls:DailyUser"]
                        ?? throw new ArgumentException("DailyUser URL is not configured in the application settings.", nameof(configuration));
        }
        public async Task<bool> CreateScheduleAsync(ScheduleTask request)
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
            var response = await _httpClient.PostAsync(_monthlyForTeamURL, content);
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

        public async Task<List<ScheduleTask>?> GetMyMonthlyScheduleTasksAsync(DateTime month)
        {
            string? accessToken = !string.IsNullOrEmpty(TokenStorage.AccessToken) ? TokenStorage.AccessToken : throw new UnauthorizedAccessException(TokenStorage.AccessToken);
            string? userName = !string.IsNullOrEmpty(TokenStorage.UserName) ? TokenStorage.UserName : throw new UnauthorizedAccessException(TokenStorage.UserName);

            // Add the JWT to the headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.GetAsync($"{_monthlyForUserURL}/{userName}?month={month}");

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

        public async Task<List<ScheduleTask>?> GetMyDailyScheduleTasksAsync(DateTime date)
        {
            string? accessToken = !string.IsNullOrEmpty(TokenStorage.AccessToken) ? TokenStorage.AccessToken : throw new UnauthorizedAccessException(TokenStorage.AccessToken);
            string? userName = !string.IsNullOrEmpty(TokenStorage.UserName) ? TokenStorage.UserName : throw new UnauthorizedAccessException(TokenStorage.UserName);

            // Add the JWT to the headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.GetAsync($"{_dailyForUserURL}/{userName}?date={date}");

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

        public async Task<List<ScheduleTask>?> GetMonthlyScheduleTasksAsync(DateTime month)
        {
            string? accessToken = !string.IsNullOrEmpty(TokenStorage.AccessToken) ? TokenStorage.AccessToken : throw new UnauthorizedAccessException(TokenStorage.AccessToken);

            // Add the JWT to the headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.GetAsync($"{_monthlyForTeamURL}?month={month}");

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

        public async Task<List<ScheduleTask>?> GetDailyScheduleTasksAsync(DateTime date)
        {
            string? accessToken = !string.IsNullOrEmpty(TokenStorage.AccessToken) ? TokenStorage.AccessToken : throw new UnauthorizedAccessException(TokenStorage.AccessToken);
            string? userName = !string.IsNullOrEmpty(TokenStorage.UserName) ? TokenStorage.UserName : throw new UnauthorizedAccessException(TokenStorage.UserName);

            // Add the JWT to the headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.GetAsync($"{_dailyForTeamURL}?date={date}");

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
            string? accessToken = !string.IsNullOrEmpty(TokenStorage.AccessToken) ? TokenStorage.AccessToken : throw new UnauthorizedAccessException(TokenStorage.AccessToken);

            // Add the JWT to the headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.GetAsync($"{_monthlyForUserURL}/{userName}?month={month}");

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
