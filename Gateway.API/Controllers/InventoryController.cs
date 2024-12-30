using Microsoft.AspNetCore.Mvc;
using PharmaTrack.Shared.Models;
using System.Text;
using System.Text.Json;

namespace Gateway.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _inventoryApiBaseUrl;

        public InventoryController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _inventoryApiBaseUrl = configuration["InventoryApi:BaseUrl"] ?? throw new ArgumentNullException("InventoryApi:BaseUrl", "The base URL for the Inventory API is not configured.");
        }

        [HttpGet("{upc}")]
        public async Task<IActionResult> GetProductByUPC(string upc)
        {
            if (string.IsNullOrWhiteSpace(upc))
            {
                return BadRequest("UPC must be provided.");
            }

            // Define the Inventory API endpoint URL (update base URL as per your setup)
            var inventoryApiUrl = $"{_inventoryApiBaseUrl}/api/products/upc/{upc}"; // Ensure the correct endpoint

            try
            {
                // Send request to Inventory API
                var response = await _httpClient.GetAsync(inventoryApiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                // Deserialize the response JSON into a Product object
                var productJson = await response.Content.ReadAsStringAsync();

                Product? product;
                try
                {
                    product = JsonSerializer.Deserialize<Product>(productJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                catch (JsonException jsonEx)
                {
                    return StatusCode(500, $"Error deserializing product data: {jsonEx.Message}");
                }

                return Ok(product);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error calling Inventory API: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProductById(int id, [FromBody] ProductUpdateRequest updateRequest)
        {
            if (updateRequest == null)
            {
                return BadRequest("Invalid product data.");
            }

            // Define the Inventory API endpoint URL
            var updateProductUrl = $"{_inventoryApiBaseUrl}/api/products/{id}";

            try
            {
                // Serialize the update request into JSON
                var content = new StringContent(JsonSerializer.Serialize(updateRequest), Encoding.UTF8, "application/json");

                // Send PUT request to the Inventory API
                var response = await _httpClient.PutAsync(updateProductUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    // Return the error response from the Inventory API
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                return Ok($"Product with ID {id} updated successfully.");
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



        [HttpPost("stock-transfer")]
        public async Task<IActionResult> StockTransfer([FromBody] StockTransferRequest request)
        {
            try
            {
                var stockTransferUrl = $"{_inventoryApiBaseUrl}/api/inventory/stock-transfer";
                var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(stockTransferUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                return Ok("Stock transfer successful.");
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error communicating with Inventory API: {ex.Message}");
            }
        }
    }
}
