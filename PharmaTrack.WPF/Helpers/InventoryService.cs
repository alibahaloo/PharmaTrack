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
        private readonly string _UpcUrl;
        public InventoryService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            // Safely handle null or empty configuration value
            _stockTransferUrl = configuration["InventoryUrls:StockTransfer"]
                        ?? throw new ArgumentException("StockTransfer URL is not configured in the application settings.", nameof(configuration));
            _UpcUrl = configuration["InventoryUrls:UPC"]
                        ?? throw new ArgumentException("UPC URL is not configured in the application settings.", nameof(configuration));
        }

        public async Task<Product?> GetProductByUPCAsync(string UPC)
        {
            if (string.IsNullOrWhiteSpace(UPC))
            {
                throw new ArgumentException("UPC is required.", nameof(UPC));
            }

            string apiUrl = $"{_UpcUrl}/{UPC}";

            string? accessToken = TokenStorage.LocalAccessToken;
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
                System.Net.HttpStatusCode.NotFound => new UnauthorizedAccessException($"Product Not Found!"),
                _ => new HttpRequestException($"An error occurred: {response.StatusCode}"),
            };
        }

        public async Task<bool> StockTransferAsync(StockTransferRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Stock transfer request cannot be null.");
            }

            // Serialize the request body
            var jsonBody = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            string? accessToken = TokenStorage.LocalAccessToken;
            if (accessToken == null) { throw new UnauthorizedAccessException(accessToken); }

            // Add the JWT to the headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            // Make the POST request
            var response = await _httpClient.PostAsync(_stockTransferUrl, content);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            throw response.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => new UnauthorizedAccessException($"{response.StatusCode}: Invalid or expired refresh token!"),
                _ => new HttpRequestException($"An error occurred: {response.StatusCode}"),
            };
        }
    }
}
