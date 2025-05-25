using Microsoft.Extensions.Configuration;
using PharmaTrack.Core.DBModels;
using PharmaTrack.Core.DTOs;
using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace PharmaTrack.WPF.Helpers
{
    public class DrugService
    {
        private readonly HttpClient _httpClient;
        private readonly string _drugsUrl;
        private readonly string _ingredientsUrl;
        private readonly string _interactionsUrl;

        public DrugService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            // Safely handle null or empty configuration value
            _drugsUrl = configuration["DrugsUrls:Drugs"]
                        ?? throw new ArgumentException("Drugs URL is not configured in the application settings.", nameof(configuration));
            _ingredientsUrl = configuration["DrugsUrls:Ingredients"]
                        ?? throw new ArgumentException("Ingredients URL is not configured in the application settings.", nameof(configuration));
            _interactionsUrl = configuration["DrugsUrls:Interactions"]
                        ?? throw new ArgumentException("Interactions URL is not configured in the application settings.", nameof(configuration));
        }

        public async Task<DrugInteractionResultDto?> GetDrugInteractions(List<int> drugCodes)
        {
            string? accessToken = TokenStorage.AccessToken ?? throw new UnauthorizedAccessException("Access token is null or expired.");

            // Add the JWT to the headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            string drugCodesString = string.Join(",", drugCodes);

            string requestUrl = $"{_interactionsUrl}/drugs/{drugCodesString}";

            // Send GET request to the API
            var response = await _httpClient.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                // Deserialize JSON into a DrugInfoDto object
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<DrugInteractionResultDto>(
                    responseData,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
            }

            // For other error statuses, throw an exception
            throw response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => new UnauthorizedAccessException($"{response.StatusCode}: Invalid or expired refresh token!"),
                _ => new HttpRequestException($"{response.StatusCode}: {await response.Content.ReadAsStringAsync()}"),
            };
        }
        public async Task<IngredientInteractionResultDto?> GetIngredientInteractions(List<string> ingredientNames)
        {
            string? accessToken = TokenStorage.AccessToken ?? throw new UnauthorizedAccessException("Access token is null or expired.");

            // Add the JWT to the headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            string ingredientCodesString = string.Join(",", ingredientNames);

            string requestUrl = $"{_interactionsUrl}/ingredients/{ingredientCodesString}";

            // Send GET request to the API
            var response = await _httpClient.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                // Deserialize JSON into a DrugInfoDto object
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IngredientInteractionResultDto>(
                    responseData,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
            }

            // For other error statuses, throw an exception
            throw response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => new UnauthorizedAccessException($"{response.StatusCode}: Invalid or expired refresh token!"),
                _ => new HttpRequestException($"{response.StatusCode}: {await response.Content.ReadAsStringAsync()}"),
            };
        }
        public async Task<List<string>?> GetIngredientLists(string startWith = "")
        {
            string? accessToken = TokenStorage.AccessToken ?? throw new UnauthorizedAccessException("Access token is null or expired.");

            // Add the JWT to the headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");


            string requestUrl;
            if (string.IsNullOrEmpty(startWith))
            {
                requestUrl = $"{_ingredientsUrl}/list";
            }
            else
            {
                requestUrl = $"{_ingredientsUrl}/list?startWith={startWith}";
            }

            // Send GET request to the API
            var response = await _httpClient.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                // Deserialize JSON into a DrugInfoDto object
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<string>>(
                    responseData,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
            }

            // For other error statuses, throw an exception
            throw response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => new UnauthorizedAccessException($"{response.StatusCode}: Invalid or expired refresh token!"),
                _ => new HttpRequestException($"{response.StatusCode}: {await response.Content.ReadAsStringAsync()}"),
            };
        }
        public async Task<List<DrugListDto>?> GetDrugList(string startWith = "")
        {
            string? accessToken = TokenStorage.AccessToken ?? throw new UnauthorizedAccessException("Access token is null or expired.");

            // Add the JWT to the headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");


            string requestUrl;
            if (string.IsNullOrEmpty(startWith))
            {
                requestUrl = $"{_drugsUrl}/list";
            } else
            {
                requestUrl = $"{_drugsUrl}/list?startWith={startWith}";
            }

            // Send GET request to the API
            var response = await _httpClient.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                // Deserialize JSON into a DrugInfoDto object
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<DrugListDto>>(
                    responseData,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
            }

            // For other error statuses, throw an exception
            throw response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => new UnauthorizedAccessException($"{response.StatusCode}: Invalid or expired refresh token!"),
                _ => new HttpRequestException($"{response.StatusCode}: {await response.Content.ReadAsStringAsync()}"),
            };
        }
        public async Task<DrugInfoDto?> GetDrugInfoByDINAsync(string DIN)
        {
            string? accessToken = TokenStorage.AccessToken ?? throw new UnauthorizedAccessException("Access token is null or expired.");

            // Add the JWT to the headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            string requestUrl = $"{_drugsUrl}/DIN/{DIN}";

            // Send GET request to the API
            var response = await _httpClient.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                // Deserialize JSON into a DrugInfoDto object
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<DrugInfoDto>(
                    responseData,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
            }

            // If the resource was not found, return null
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            // For other error statuses, throw an exception
            throw response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => new UnauthorizedAccessException($"{response.StatusCode}: Invalid or expired refresh token!"),
                _ => new HttpRequestException($"{response.StatusCode}: {await response.Content.ReadAsStringAsync()}"),
            };
        }
        public async Task<DrugInfoDto?> GetDrugInfoByCodeAsync(int drugCode)
        {
            string? accessToken = TokenStorage.AccessToken ?? throw new UnauthorizedAccessException("Access token is null or expired.");

            // Add the JWT to the headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            string requestUrl = $"{_drugsUrl}/{drugCode}";

            // Send GET request to the API
            var response = await _httpClient.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                // Deserialize JSON into a DrugInfoDto object
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<DrugInfoDto>(
                    responseData,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
            }

            // If the resource was not found, return null
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            // For other error statuses, throw an exception
            throw response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => new UnauthorizedAccessException($"{response.StatusCode}: Invalid or expired refresh token!"),
                _ => new HttpRequestException($"{response.StatusCode}: {await response.Content.ReadAsStringAsync()}"),
            };
        }
        public async Task<DrugIngredient?> GetIngredientByIngredientCode(int activeIngredientCode)
        {
            string? accessToken = TokenStorage.AccessToken ?? throw new UnauthorizedAccessException("Access token is null or expired.");

            // Add the JWT to the headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            string requestUrl = $"{_ingredientsUrl}/{activeIngredientCode}";

            // Send GET request to the API
            var response = await _httpClient.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                // Deserialize JSON into a DrugInfoDto object
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<DrugIngredient>(
                    responseData,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
            }

            // If the resource was not found, return null
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            // For other error statuses, throw an exception
            throw response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => new UnauthorizedAccessException($"{response.StatusCode}: Invalid or expired refresh token!"),
                _ => new HttpRequestException($"{response.StatusCode}: {await response.Content.ReadAsStringAsync()}"),
            };
        }
        public async Task<PagedResponse<DrugProduct>?> GetDrugsAsync(DrugInfoRequest request, int curPage = 1)
        {
            string? accessToken = TokenStorage.AccessToken ?? throw new UnauthorizedAccessException("Access token is null or expired.");

            // Add the JWT to the headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            // Convert TransactionsRequest to query parameters
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

            string queryString = string.Join("&", queryParameters);
            string requestUrl = $"{_drugsUrl}?{queryString}";

            // Send GET request to the API
            var response = await _httpClient.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                // Parse the response (deserialize JSON into PagedResponse<Transaction>)
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PagedResponse<DrugProduct>>(responseData, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            // If the resource was not found, return null
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            // Handle errors
            throw response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => new UnauthorizedAccessException($"{response.StatusCode}: Invalid or expired refresh token!"),
                _ => new HttpRequestException($"{await response.Content.ReadAsStringAsync()}"),
            };
        }
        public async Task<PagedResponse<DrugIngredient>?> GetIngredientsAsync(DrugIngredientQuery request, int curPage = 1)
        {
            string? accessToken = TokenStorage.AccessToken ?? throw new UnauthorizedAccessException("Access token is null or expired.");

            // Add the JWT to the headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            // Convert TransactionsRequest to query parameters
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

            string queryString = string.Join("&", queryParameters);
            string requestUrl = $"{_ingredientsUrl}?{queryString}";

            // Send GET request to the API
            var response = await _httpClient.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                // Parse the response (deserialize JSON into PagedResponse<Transaction>)
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PagedResponse<DrugIngredient>>(responseData, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            // If the resource was not found, return null
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            // Handle errors
            throw response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => new UnauthorizedAccessException($"{response.StatusCode}: Invalid or expired refresh token!"),
                _ => new HttpRequestException($"{response.StatusCode}: {await response.Content.ReadAsStringAsync()}"),
            };
        }
    }
}
