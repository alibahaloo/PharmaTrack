using Microsoft.AspNetCore.Mvc;
using PharmaTrack.Shared.Models;
using System.Text;
using System.Text.Json;

namespace Gateway.API.Controllers
{
    public class StockTransferRequest
    {
        public TransactionType Type { get; set; }
        public Product Product { get; set; } = default!;
        public int Quantity { get; set; }
    }

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
            var inventoryApiUrl = $"{_inventoryApiBaseUrl}/api/products/{upc}"; // Ensure the correct endpoint

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

        [HttpPost("stock-transfer")]
        public async Task<IActionResult> StockTransfer([FromBody] StockTransferRequest request)
        {
            if (request.Product == null || string.IsNullOrWhiteSpace(request.Product.UPC))
            {
                return BadRequest("Product UPC is required.");
            }

            var type = request.Type;
            var product = request.Product;
            var quantity = request.Quantity;

            try
            {
                // Check if the product exists
                var inventoryApiUrl = $"{_inventoryApiBaseUrl}/api/products/upc/{product.UPC}";
                var response = await _httpClient.GetAsync(inventoryApiUrl);

                Product? existingProduct = null;

                if (response.IsSuccessStatusCode)
                {
                    var productJson = await response.Content.ReadAsStringAsync();
                    existingProduct = JsonSerializer.Deserialize<Product>(productJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // Product does not exist, create it
                    var createProductUrl = $"{_inventoryApiBaseUrl}/api/products";
                    var productContent = new StringContent(JsonSerializer.Serialize(product), Encoding.UTF8, "application/json");

                    var createResponse = await _httpClient.PostAsync(createProductUrl, productContent);
                    if (!createResponse.IsSuccessStatusCode)
                    {
                        return StatusCode((int)createResponse.StatusCode, await createResponse.Content.ReadAsStringAsync());
                    }

                    var createdProductJson = await createResponse.Content.ReadAsStringAsync();
                    existingProduct = JsonSerializer.Deserialize<Product>(createdProductJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                if (existingProduct == null)
                {
                    return StatusCode(500, "Failed to retrieve or create the product.");
                }

                // Create a new transaction
                var transaction = new
                {
                    ProductId = existingProduct.Id,
                    Type = type,
                    Quantity = quantity
                };

                var transactionUrl = $"{_inventoryApiBaseUrl}/api/transactions";
                var transactionContent = new StringContent(JsonSerializer.Serialize(transaction), Encoding.UTF8, "application/json");

                var transactionResponse = await _httpClient.PostAsync(transactionUrl, transactionContent);
                if (!transactionResponse.IsSuccessStatusCode)
                {
                    return StatusCode((int)transactionResponse.StatusCode, await transactionResponse.Content.ReadAsStringAsync());
                }

                return Ok("Stock transfer successful.");
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error communicating with Inventory API: {ex.Message}");
            }
            catch (JsonException jsonEx)
            {
                return StatusCode(500, $"Error processing JSON data: {jsonEx.Message}");
            }
        }
    }
}
