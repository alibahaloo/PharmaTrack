using Microsoft.AspNetCore.Mvc;
using PharmaTrack.Shared.APIModels;
using PharmaTrack.Shared.DBModels;
using PharmaTrack.Shared.DTOs;
using PharmaTrack.Shared.Services;
using System.Text.Json;

namespace Gateway.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IngredientsController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _drugApiBaseUrl;
        private readonly JwtService _jwtService;
        public IngredientsController(IHttpClientFactory httpClientFactory, IConfiguration configuration, JwtService jwtService)
        {
            _jwtService = jwtService;
            _httpClient = httpClientFactory.CreateClient();
            _drugApiBaseUrl = configuration["DrugApi:BaseUrl"] ?? throw new ArgumentNullException("DrugApi:BaseUrl", "The base URL for the Drug API is not configured.");
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetIngredientList(string startWith = "")
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
                string apiUrl;
                if (string.IsNullOrEmpty(startWith))
                {
                    apiUrl = $"{_drugApiBaseUrl}/api/Ingredients/list";
                }
                else
                {
                    apiUrl = $"{_drugApiBaseUrl}/api/Ingredients/list?startWith={startWith}";
                }

                // Send GET request to the Inventory API
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    // Return the error response from the Inventory API
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                // Deserialize the response JSON into a list of Product objects
                var responseJson = await response.Content.ReadAsStringAsync();

                List<IngredientListDto>? apiResponse;
                try
                {
                    apiResponse = JsonSerializer.Deserialize<List<IngredientListDto>>(responseJson, new JsonSerializerOptions
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
        
        [HttpGet("{ingredientCode}")]
        public async Task<IActionResult> GetIngredientByIngredientCode(int ingredientCode)
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

                var apiUrl = $"{_drugApiBaseUrl}/api/Ingredients/{ingredientCode}";

                // Send GET request to the Inventory API
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    // Return the error response from the Inventory API
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                // Deserialize the response JSON into a list of Product objects
                var responseJson = await response.Content.ReadAsStringAsync();

                List<DrugIngredient>? apiResponse;
                try
                {
                    apiResponse = JsonSerializer.Deserialize<List<DrugIngredient>>(responseJson, new JsonSerializerOptions
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

        [HttpGet]
        public async Task<IActionResult> GetIngredients([FromQuery] DrugIngredientRequest request, int curPage = 1)
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
                // Serialize TransactionsRequest into query parameters
                var queryParameters = new List<string>
                {
                    $"curPage={curPage}"
                };
                if (request.DrugCode != null)
                {
                    queryParameters.Add($"DrugCode={Uri.EscapeDataString(request.DrugCode.Value.ToString())}");
                }
                if (request.ActiveIngredientCode != null)
                {
                    queryParameters.Add($"ActiveIngredientCode={Uri.EscapeDataString(request.ActiveIngredientCode.Value.ToString())}");
                }
                if (!string.IsNullOrEmpty(request.Ingredient))
                {
                    queryParameters.Add($"Ingredient={Uri.EscapeDataString(request.Ingredient)}");
                }

                var queryString = string.Join("&", queryParameters);
                var apiUrl = $"{_drugApiBaseUrl}/api/Ingredients?{queryString}";

                // Send GET request to the Inventory API
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    // Return the error response from the Inventory API
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                // Deserialize the response JSON into a list of Product objects
                var responseJson = await response.Content.ReadAsStringAsync();

                PagedResponse<DrugIngredient>? apiResponse;
                try
                {
                    apiResponse = JsonSerializer.Deserialize<PagedResponse<DrugIngredient>>(responseJson, new JsonSerializerOptions
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
