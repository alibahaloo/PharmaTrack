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
    public class DrugsController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _drugApiBaseUrl;
        private readonly JwtService _jwtService;

        public DrugsController(IHttpClientFactory httpClientFactory, IConfiguration configuration, JwtService jwtService)
        {
            _jwtService = jwtService;
            _httpClient = httpClientFactory.CreateClient();
            _drugApiBaseUrl = configuration["DrugApi:BaseUrl"] ?? throw new ArgumentNullException("DrugApi:BaseUrl", "The base URL for the Drug API is not configured.");
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetDrugList(string startWith = "")
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
                    apiUrl = $"{_drugApiBaseUrl}/api/Drugs/list";
                } else
                {
                    apiUrl = $"{_drugApiBaseUrl}/api/Drugs/list?startWith={startWith}";
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

                List<DrugListDto>? apiResponse;
                try
                {
                    apiResponse = JsonSerializer.Deserialize<List<DrugListDto>>(responseJson, new JsonSerializerOptions
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

        [HttpGet("{drugCode}")]
        public async Task<IActionResult> GetDrugInfoByCode(int drugCode)
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

                var apiUrl = $"{_drugApiBaseUrl}/api/Drugs/{drugCode}";

                // Send GET request to the Inventory API
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    // Return the error response from the Inventory API
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                // Deserialize the response JSON into a list of Product objects
                var responseJson = await response.Content.ReadAsStringAsync();

                DrugInfoDto? apiResponse;
                try
                {
                    apiResponse = JsonSerializer.Deserialize<DrugInfoDto>(responseJson, new JsonSerializerOptions
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

        [HttpGet("DIN/{DIN}")]
        public async Task<IActionResult> GetDrugInfoByDIN(string DIN)
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

                var apiUrl = $"{_drugApiBaseUrl}/api/Drugs/DIN/{DIN}";

                // Send GET request to the Inventory API
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    // Return the error response from the Inventory API
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                // Deserialize the response JSON into a list of Product objects
                var responseJson = await response.Content.ReadAsStringAsync();

                DrugInfoDto? apiResponse;
                try
                {
                    apiResponse = JsonSerializer.Deserialize<DrugInfoDto>(responseJson, new JsonSerializerOptions
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

        [HttpGet("{drugCode}/ingredients")]
        public async Task<IActionResult> GetIngredientsByDrugCode(int drugCode)
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

                var apiUrl = $"{_drugApiBaseUrl}/api/Drugs/{drugCode}/ingredients";

                // Send GET request to the Inventory API
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    // Return the error response from the Inventory API
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                // Deserialize the response JSON into a list of Product objects
                var responseJson = await response.Content.ReadAsStringAsync();

                List<string>? apiResponse;
                try
                {
                    apiResponse = JsonSerializer.Deserialize<List<string>>(responseJson, new JsonSerializerOptions
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
        public async Task<IActionResult> GetDrugs([FromQuery] DrugInfoRequest request, int curPage = 1)
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
                if (!string.IsNullOrEmpty(request.DIN))
                {
                    queryParameters.Add($"DIN={Uri.EscapeDataString(request.DIN)}");
                }
                if (!string.IsNullOrEmpty(request.BrandName))
                {
                    queryParameters.Add($"BrandName={Uri.EscapeDataString(request.BrandName)}");
                }
                

                var queryString = string.Join("&", queryParameters);
                var apiUrl = $"{_drugApiBaseUrl}/api/Drugs?{queryString}";

                // Send GET request to the Inventory API
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    // Return the error response from the Inventory API
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                // Deserialize the response JSON into a list of Product objects
                var responseJson = await response.Content.ReadAsStringAsync();

                PagedResponse<DrugProduct>? apiResponse;
                try
                {
                    apiResponse = JsonSerializer.Deserialize<PagedResponse<DrugProduct>>(responseJson, new JsonSerializerOptions
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
