using Microsoft.AspNetCore.Mvc;
using PharmaTrack.Shared.APIModels;
using PharmaTrack.Shared.DBModels;
using System.Text.Json;

namespace Gateway.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _authApiBaseUrl;
        public AuthController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _authApiBaseUrl = configuration["AuthApi:BaseUrl"] ?? throw new ArgumentNullException("AuthApi:BaseUrl", "The base URL for the Auth API is not configured.");
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            try
            {
                // Forward the login request to the Auth API
                var response = await _httpClient.PostAsJsonAsync($"{_authApiBaseUrl}/auth/login", model);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                // Parse the response from Auth API
                var result = await response.Content.ReadAsStringAsync();

                AuthDto? authDto;
                try
                {
                    authDto = JsonSerializer.Deserialize<AuthDto>(result, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                catch (JsonException jsonEx)
                {
                    return StatusCode(500, $"Error deserializing data: {jsonEx.Message}");
                }

                return Ok(authDto);
            }
            catch (Exception ex)
            {
                // Log error and return 500 status code
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] LoginRequest model)
        {
            try
            {
                // Forward the registration request to the Auth API
                var response = await _httpClient.PostAsJsonAsync($"{_authApiBaseUrl}/auth/register", model);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                // Parse the response from Auth API
                var result = await response.Content.ReadAsStringAsync();

                AuthDto? authDto;
                try
                {
                    authDto = JsonSerializer.Deserialize<AuthDto>(result, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                catch (JsonException jsonEx)
                {
                    return StatusCode(500, $"Error deserializing data: {jsonEx.Message}");
                }

                return Ok(authDto);
            }
            catch (Exception ex)
            {
                // Log error and return 500 status code
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }


        [HttpPost]
        [Route("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest model)
        {
            try
            {
                // Forward the refresh token request to the Auth API
                var response = await _httpClient.PostAsJsonAsync($"{_authApiBaseUrl}/auth/refresh", model);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                // Parse the response from Auth API
                var result = await response.Content.ReadAsStringAsync();

                AuthDto? authDto;
                try
                {
                    authDto = JsonSerializer.Deserialize<AuthDto>(result, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                catch (JsonException jsonEx)
                {
                    return StatusCode(500, $"Error deserializing data: {jsonEx.Message}");
                }

                return Ok(authDto);
            }
            catch (Exception ex)
            {
                // Log error and return 500 status code
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest model)
        {
            try
            {
                // Forward the logout request to the Auth API
                var response = await _httpClient.PostAsJsonAsync($"{_authApiBaseUrl}/auth/logout", model);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                return Ok();
            }
            catch (Exception ex)
            {
                // Log error and return 500 status code
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }
    }
}
