using Microsoft.AspNetCore.Mvc;
using PharmaTrack.Shared.APIModels;
using PharmaTrack.Shared.DBModels;
using System.Text.Json;

namespace Gateway.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _authApiBaseUrl;
        public UsersController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _authApiBaseUrl = configuration["AuthApi:BaseUrl"] ?? throw new ArgumentNullException("AuthApi:BaseUrl", "The base URL for the Auth API is not configured.");
        }
        [HttpGet]
        public async Task<IActionResult> GetUsers(int curPage = 1)
        {
            var getAllUsersUrl = $"{_authApiBaseUrl}/api/Users?curPage={curPage}";
            try
            {
                /*
                // Step 1: Validate Authorization Header
                var (validationResult, userId) = ValidateAuthorizationHeader();
                if (validationResult != null)
                {
                    return validationResult; // Return if validation fails
                }
                */
                // Send GET request to the Inventory API
                var response = await _httpClient.GetAsync(getAllUsersUrl);

                if (!response.IsSuccessStatusCode)
                {
                    // Return the error response from the Inventory API
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                // Deserialize the response JSON into a list of Product objects
                var productsJson = await response.Content.ReadAsStringAsync();

                PagedResponse<UserDto>? apiResponse;
                try
                {
                    apiResponse = JsonSerializer.Deserialize<PagedResponse<UserDto>>(productsJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                catch (JsonException jsonEx)
                {
                    return StatusCode(500, $"Error deserializing product data: {jsonEx.Message}");
                }

                return Ok(apiResponse);
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP request errors
                return StatusCode(500, $"Error communicating with Inventory API: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }
     }
}
