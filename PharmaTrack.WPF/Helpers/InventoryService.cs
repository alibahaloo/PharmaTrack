using Microsoft.Extensions.Configuration;
using PharmaTrack.Shared.APIModels;
using System.Net.Http;
using System.Text.Json;
using System.Text;

namespace PharmaTrack.WPF.Helpers
{
    public class InventoryService
    {
        private readonly HttpClient _httpClient;
        private readonly string _stockTransferUrl;
        public InventoryService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            // Safely handle null or empty configuration value
            _stockTransferUrl = configuration["InventoryUrls:StockTransfer"]
                        ?? throw new ArgumentException("StockTransfer URL is not configured in the application settings.", nameof(configuration));
        }

        public async Task<bool> StockTransferAsync(StockTransferRequest request, string jwtToken)
        {
            if (string.IsNullOrWhiteSpace(jwtToken))
            {
                throw new ArgumentException("JWT token is required.", nameof(jwtToken));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Stock transfer request cannot be null.");
            }

            // Serialize the request body
            var jsonBody = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            // Add the JWT to the headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwtToken}");

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
