using Microsoft.AspNetCore.Mvc;
using PharmaTrack.Shared.DTOs;
using PharmaTrack.Shared.Services;
using System.Text.Json;

namespace Gateway.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InteractionsController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _drugApiBaseUrl;
        private readonly JwtService _jwtService;

        public InteractionsController(IHttpClientFactory httpClientFactory, IConfiguration configuration, JwtService jwtService)
        {
            _jwtService = jwtService;
            _httpClient = httpClientFactory.CreateClient();
            _drugApiBaseUrl = configuration["DrugApi:BaseUrl"] ?? throw new ArgumentNullException("DrugApi:BaseUrl", "The base URL for the Drug API is not configured.");
        }

        [HttpGet("drugs/{drugCodes}")]
        public async Task<IActionResult> GetInteractionsByDrugCode(string drugCodes)
        {
            try
            {
                // Step 1: Validate Authorization Header
                /*
                var (validationResult, username, isAdmin) = _jwtService.ValidateAuthorizationHeader(Request);
                if (validationResult != null)
                {
                    return validationResult; // Return if validation fails
                }
                */

                var apiUrl = $"{_drugApiBaseUrl}/api/Interactions/drugs/{drugCodes}";

                // Send GET request to the Inventory API
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    // Return the error response from the Inventory API
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                // Deserialize the response JSON into a list of Product objects
                var responseJson = await response.Content.ReadAsStringAsync();

                DrugInteractionResultDto? apiResponse;
                try
                {
                    apiResponse = JsonSerializer.Deserialize<DrugInteractionResultDto>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                catch (JsonException jsonEx)
                {
                    return StatusCode(500, $"Error deserializing data: {jsonEx.Message}");
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

        [HttpGet("ingredients/{ingredientCodes}")]
        public async Task<IActionResult> GetInteractionsByIngredientCode(string ingredientCodes)
        {
            try
            {
                // Step 1: Validate Authorization Header
                /*
                var (validationResult, username, isAdmin) = _jwtService.ValidateAuthorizationHeader(Request);
                if (validationResult != null)
                {
                    return validationResult; // Return if validation fails
                }
                */

                var apiUrl = $"{_drugApiBaseUrl}/api/Interactions/ingredients/{ingredientCodes}";

                // Send GET request to the Inventory API
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    // Return the error response from the Inventory API
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                // Deserialize the response JSON into a list of Product objects
                var responseJson = await response.Content.ReadAsStringAsync();

                DrugInteractionResultDto? apiResponse;
                try
                {
                    apiResponse = JsonSerializer.Deserialize<DrugInteractionResultDto>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                catch (JsonException jsonEx)
                {
                    return StatusCode(500, $"Error deserializing data: {jsonEx.Message}");
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
    }
}
