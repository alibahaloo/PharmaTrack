using Microsoft.AspNetCore.Mvc;
using PharmaTrack.Shared.APIModels;
using PharmaTrack.Shared.DBModels;
using PharmaTrack.Shared.Services;
using System.Text;
using System.Text.Json;

namespace Gateway.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SchedulesController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _scheduleApiBaseUrl;
        private readonly JwtService _jwtService;
        public SchedulesController(IHttpClientFactory httpClientFactory, IConfiguration configuration, JwtService jwtService)
        {
            _jwtService = jwtService;
            _httpClient = httpClientFactory.CreateClient();
            _scheduleApiBaseUrl = configuration["ScheduleApi:BaseUrl"] ?? throw new ArgumentNullException("ScheduleApi:BaseUrl", "The base URL for the Schedule API is not configured.");
        }

        [HttpGet("daily")]
        public async Task<IActionResult> GetDailySchedules(DateTime date)
        {
            var scheduleApiUrl = $"{_scheduleApiBaseUrl}/api/schedules/daily?date={date}";

            try
            {
                // Step 1: Validate Authorization Header

                // Send GET request to the Inventory API
                var response = await _httpClient.GetAsync(scheduleApiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    // Return the error response from the Inventory API
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                // Deserialize the response JSON into a list of Product objects
                var productsJson = await response.Content.ReadAsStringAsync();

                List<ScheduleTask>? apiResponse;
                try
                {
                    apiResponse = JsonSerializer.Deserialize<List<ScheduleTask>>(productsJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                catch (JsonException jsonEx)
                {
                    return StatusCode(500, $"Error deserializing: {jsonEx.Message}");
                }

                return Ok(apiResponse);
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP request errors
                return StatusCode(500, $"Error communicating with API: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        [HttpGet("daily/user/{userName}")]
        public async Task<IActionResult> GetDailySchedulesForUser(DateTime date, string userName)
        {
            var scheduleApiUrl = $"{_scheduleApiBaseUrl}/api/schedules/daily/user/{userName}?date={date}";
            try
            {
                // Step 1: Validate Authorization Header

                // Send GET request to the Inventory API
                var response = await _httpClient.GetAsync(scheduleApiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    // Return the error response from the Inventory API
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                // Deserialize the response JSON into a list of Product objects
                var productsJson = await response.Content.ReadAsStringAsync();

                List<ScheduleTask>? apiResponse;
                try
                {
                    apiResponse = JsonSerializer.Deserialize<List<ScheduleTask>>(productsJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                catch (JsonException jsonEx)
                {
                    return StatusCode(500, $"Error deserializing: {jsonEx.Message}");
                }

                return Ok(apiResponse);
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP request errors
                return StatusCode(500, $"Error communicating with API: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        [HttpGet("monthly")]
        public async Task<IActionResult> GetMonthlySchedules(DateTime month)
        {
            var scheduleApiUrl = $"{_scheduleApiBaseUrl}/api/schedules/monthly?month={month}";

            try
            {
                // Step 1: Validate Authorization Header

                // Send GET request to the Inventory API
                var response = await _httpClient.GetAsync(scheduleApiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    // Return the error response from the Inventory API
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                // Deserialize the response JSON into a list of Product objects
                var productsJson = await response.Content.ReadAsStringAsync();

                List<ScheduleTask>? apiResponse;
                try
                {
                    apiResponse = JsonSerializer.Deserialize<List<ScheduleTask>>(productsJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                catch (JsonException jsonEx)
                {
                    return StatusCode(500, $"Error deserializing: {jsonEx.Message}");
                }

                return Ok(apiResponse);
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP request errors
                return StatusCode(500, $"Error communicating with API: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        [HttpGet("monthly/user/{userName}")]
        public async Task<IActionResult> GetMonthlySchedulesForUser(DateTime month, string userName)
        {
            var scheduleApiUrl = $"{_scheduleApiBaseUrl}/api/schedules/monthly/user/{userName}?month={month}";

            try
            {
                // Step 1: Validate Authorization Header

                // Send GET request to the Inventory API
                var response = await _httpClient.GetAsync(scheduleApiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    // Return the error response from the Inventory API
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                // Deserialize the response JSON into a list of Product objects
                var productsJson = await response.Content.ReadAsStringAsync();

                List<ScheduleTask>? apiResponse;
                try
                {
                    apiResponse = JsonSerializer.Deserialize<List<ScheduleTask>>(productsJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                catch (JsonException jsonEx)
                {
                    return StatusCode(500, $"Error deserializing: {jsonEx.Message}");
                }

                return Ok(apiResponse);
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP request errors
                return StatusCode(500, $"Error communicating with API: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateSchedule([FromBody] ScheduleTaskRequest request)
        {
            try
            {
                // Step 1: Validate Authorization Header, only admins can do this!

                var apiUrl = $"{_scheduleApiBaseUrl}/api/schedules"; // Ensure the correct endpoint

                var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(apiUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error communicating with API: {ex.Message}");

            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSchedule(int id, [FromBody] ScheduleTaskRequest request)
        {
            var apiUrl = $"{_scheduleApiBaseUrl}/api/schedules/{id}";
            try
            {
                // Serialize the update request into JSON
                var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

                // Send PUT request to the Inventory API
                var response = await _httpClient.PutAsync(apiUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    // Return the error response from the Inventory API
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                return Ok();
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP request errors
                return StatusCode(500, $"Error communicating with API: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var apiUrl = $"{_scheduleApiBaseUrl}/api/schedules/{id}";
            try
            {
                // Send DELETE request to the Inventory API
                var response = await _httpClient.DeleteAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    // Return the error response from the Inventory API
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                return Ok();
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP request errors
                return StatusCode(500, $"Error communicating with API: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }
    }
}
