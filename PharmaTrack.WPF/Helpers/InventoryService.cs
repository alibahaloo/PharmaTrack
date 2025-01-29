using Microsoft.Extensions.Configuration;
using PharmaTrack.Shared.APIModels;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using PharmaTrack.Shared.DBModels;

namespace PharmaTrack.WPF.Helpers
{
    public class InventoryService
    {
        private readonly HttpClient _httpClient;
        private readonly string _stockTransferUrl;
        private readonly string _upcUrl;
        private readonly string _productsUrl;
        private readonly string _transactionsUrl;
        public InventoryService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            // Safely handle null or empty configuration value
            _stockTransferUrl = configuration["InventoryUrls:StockTransfer"]
                        ?? throw new ArgumentException("StockTransfer URL is not configured in the application settings.", nameof(configuration));
            _upcUrl = configuration["InventoryUrls:UPC"]
                        ?? throw new ArgumentException("UPC URL is not configured in the application settings.", nameof(configuration));
            _productsUrl = configuration["InventoryUrls:Products"]
                        ?? throw new ArgumentException("Products URL is not configured in the application settings.", nameof(configuration));
            _transactionsUrl = configuration["InventoryUrls:Transactions"]
                        ?? throw new ArgumentException("Transactions URL is not configured in the application settings.", nameof(configuration));
        }

        public async Task<PagedResponse<Transaction>?> GetTransactionsAsync(TransactionsRequest request, int curPage = 1)
        {
            string? accessToken = TokenStorage.AccessToken;
            if (accessToken == null)
            {
                throw new UnauthorizedAccessException("Access token is null or expired.");
            }

            // Add the JWT to the headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            // Convert TransactionsRequest to query parameters
            var queryParameters = new List<string>
            {
                $"curPage={curPage}"
            };

            if (!string.IsNullOrEmpty(request.UPC))
            {
                queryParameters.Add($"upc={Uri.EscapeDataString(request.UPC)}");
            }
            if (!string.IsNullOrEmpty(request.Product))
            {
                queryParameters.Add($"product={Uri.EscapeDataString(request.Product)}");
            }
            if (!string.IsNullOrEmpty(request.Brand))
            {
                queryParameters.Add($"brand={Uri.EscapeDataString(request.Brand)}");
            }
            if (!string.IsNullOrEmpty(request.CreatedBy))
            {
                queryParameters.Add($"createdBy={Uri.EscapeDataString(request.CreatedBy)}");
            }
            if (request.Type.HasValue)
            {
                queryParameters.Add($"type={(int)request.Type.Value}");
            }

            string queryString = string.Join("&", queryParameters);
            string requestUrl = $"{_transactionsUrl}?{queryString}";

            // Send GET request to the API
            var response = await _httpClient.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                // Parse the response (deserialize JSON into PagedResponse<Transaction>)
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PagedResponse<Transaction>>(responseData, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            // Handle errors
            throw response.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => new UnauthorizedAccessException($"{response.StatusCode}: Invalid or expired refresh token!"),
                _ => new HttpRequestException($"{await response.Content.ReadAsStringAsync()}"),
            };
        }

        public async Task<PagedResponse<Product>?> GetProductsAsync(int curPage = 1)
        {
            string? accessToken = TokenStorage.AccessToken;
            if (accessToken == null) { throw new UnauthorizedAccessException(accessToken); }

            // Add the JWT to the headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.GetAsync($"{_productsUrl}?curPage={curPage}");

            if (response.IsSuccessStatusCode)
            {
                // Parse the response (deserialize JSON into Product object)
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PagedResponse<Product>>(responseData, new System.Text.Json.JsonSerializerOptions
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
        public async Task<Product?> GetProductByUPCAsync(string UPC)
        {
            if (string.IsNullOrWhiteSpace(UPC))
            {
                throw new ArgumentException("UPC is required.", nameof(UPC));
            }

            string apiUrl = $"{_upcUrl}/{UPC}";

            string? accessToken = TokenStorage.AccessToken;
            if (accessToken == null) { throw new UnauthorizedAccessException(accessToken); }

            // Add the JWT to the headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                // Parse the response (deserialize JSON into Product object)
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Product>(responseData, new System.Text.Json.JsonSerializerOptions
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
        public async Task<Product?> GetProductByIdAsync(int Id)
        {
            string apiUrl = $"{_productsUrl}{Id}";

            string? accessToken = TokenStorage.AccessToken;
            if (accessToken == null) { throw new UnauthorizedAccessException(accessToken); }

            // Add the JWT to the headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                // Parse the response (deserialize JSON into Product object)
                var responseData = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Product>(responseData, new System.Text.Json.JsonSerializerOptions
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
        public async Task UpdateProductAsync(Product product)
        {
            string apiUrl = $"{_productsUrl}{product.Id}";

            string? accessToken = TokenStorage.AccessToken;
            if (accessToken == null) { throw new UnauthorizedAccessException(accessToken); }

            // Add the JWT to the headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            // Serialize the request body
            var jsonBody = JsonSerializer.Serialize(product);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            try
            {
                // Make the POST request
                var response = await _httpClient.PutAsync(apiUrl, content);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Handle 401 (Unauthorized) error
                throw new UnauthorizedAccessException("Invalid or expired refresh token!", ex);
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"An error occurred: {ex.Message}");
            }
        }
        public async Task StockTransferAsync(StockTransferRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Stock transfer request cannot be null.");
            }

            // Serialize the request body
            var jsonBody = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            string? accessToken = TokenStorage.AccessToken;
            if (accessToken == null) { throw new UnauthorizedAccessException(accessToken); }

            // Add the JWT to the headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            try
            {
                var response = await _httpClient.PostAsync(_stockTransferUrl, content);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Handle 401 (Unauthorized) error
                throw new UnauthorizedAccessException("Invalid or expired refresh token!", ex);
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"An error occurred: {ex.Message}");
            }
        }
    }
}
