using Microsoft.AspNetCore.Mvc;
using PharmaTrack.Shared.APIModels;

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
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, errorContent);
                }

                // Parse the response from Auth API
                var result = await response.Content.ReadFromJsonAsync<object>();
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log error and return 500 status code
                return StatusCode(500, new { Success = false, Message = "An error occurred while logging in.", Details = ex.Message });
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
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, errorContent);
                }

                // Parse the response from Auth API
                var result = await response.Content.ReadFromJsonAsync<object>();
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log error and return 500 status code
                return StatusCode(500, new { Success = false, Message = "An error occurred while registering.", Details = ex.Message });
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
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, errorContent);
                }

                // Parse the response from Auth API
                var result = await response.Content.ReadFromJsonAsync<object>();
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log error and return 500 status code
                return StatusCode(500, new { Success = false, Message = "An error occurred while refreshing the token.", Details = ex.Message });
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
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, errorContent);
                }

                return Ok(new { Success = true, Message = "Logged out successfully." });
            }
            catch (Exception ex)
            {
                // Log error and return 500 status code
                return StatusCode(500, new { Success = false, Message = "An error occurred while logging out.", Details = ex.Message });
            }
        }
    }
}
