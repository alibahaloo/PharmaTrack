using Microsoft.AspNetCore.Mvc;
using PharmaTrack.Shared.APIModels;
using PharmaTrack.Shared.DBModels;
using PharmaTrack.Shared.Services;
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
        private readonly JwtService _jwtService;

        public InventoryController(IHttpClientFactory httpClientFactory, IConfiguration configuration, JwtService jwtService)
        {
            _jwtService = jwtService;
            _httpClient = httpClientFactory.CreateClient();
            _inventoryApiBaseUrl = configuration["InventoryApi:BaseUrl"] ?? throw new ArgumentNullException("InventoryApi:BaseUrl", "The base URL for the Inventory API is not configured.");
        }

        private (IActionResult? ValidationResult, string? UserId) ValidateAuthorizationHeader()
        {
            // Extract the Authorization header
            if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                return (Unauthorized(new { Success = false, Message = "Authorization header is missing." }), null);
            }

            var token = authHeader.ToString().Replace("Bearer ", string.Empty);
            if (string.IsNullOrEmpty(token))
            {
                return (Unauthorized(new { Success = false, Message = "Token is missing." }), null);
            }

            // Validate the token and extract user ID
            var (isValid, userId) = _jwtService.ValidateAccessToken(token);
            if (!isValid || string.IsNullOrEmpty(userId))
            {
                return (Unauthorized(new { Success = false, Message = "Invalid or expired token." }), null);
            }

            // Return null for validation result if validation succeeds, along with the extracted userId
            return (null, userId);
        }



        [HttpGet]
        public async Task<IActionResult> GetAllProducts(int curPage = 1)
        {
            // Define the Inventory API endpoint URL
            var getAllProductsUrl = $"{_inventoryApiBaseUrl}/api/products?curPage={curPage}";

            try
            {
                // Step 1: Validate Authorization Header
                var (validationResult, userId) = ValidateAuthorizationHeader();
                if (validationResult != null)
                {
                    return validationResult; // Return if validation fails
                }

                // Send GET request to the Inventory API
                var response = await _httpClient.GetAsync(getAllProductsUrl);

                if (!response.IsSuccessStatusCode)
                {
                    // Return the error response from the Inventory API
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                // Deserialize the response JSON into a list of Product objects
                var productsJson = await response.Content.ReadAsStringAsync();

                PagedResponse<Product>? apiResponse;
                try
                {
                    apiResponse = JsonSerializer.Deserialize<PagedResponse<Product>>(productsJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                catch (JsonException jsonEx)
                {
                    return StatusCode(500, $"Error deserializing product data: {jsonEx.Message}");
                }

                return Ok(apiResponse);
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            // Define the Inventory API endpoint URL (update base URL as per your setup)
            var inventoryApiUrl = $"{_inventoryApiBaseUrl}/api/products/{id}"; // Ensure the correct endpoint

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

        [HttpGet("upc/{upc}")]
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductById(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid product ID.");
            }

            // Define the Inventory API endpoint URL
            var deleteProductUrl = $"{_inventoryApiBaseUrl}/api/products/{id}";

            try
            {
                // Send DELETE request to the Inventory API
                var response = await _httpClient.DeleteAsync(deleteProductUrl);

                if (!response.IsSuccessStatusCode)
                {
                    // Return the error response from the Inventory API
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                return Ok($"Product with ID {id} deleted successfully.");
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

        [HttpGet("transactions")]
        public async Task<IActionResult> GetAllTransactions(int curPage = 1)
        {
            // Define the Inventory API endpoint URL
            var getTransactionsUrl = $"{_inventoryApiBaseUrl}/api/transactions?curPage={curPage}";

            try
            {
                // Send GET request to the Inventory API
                var response = await _httpClient.GetAsync(getTransactionsUrl);

                if (!response.IsSuccessStatusCode)
                {
                    // Return the error response from the Inventory API
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                // Deserialize the response JSON into a list of Transaction objects
                var transactionsJson = await response.Content.ReadAsStringAsync();

                PagedResponse<Transaction>? transactions;
                try
                {
                    transactions = JsonSerializer.Deserialize<PagedResponse<Transaction>>(transactionsJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                catch (JsonException jsonEx)
                {
                    return StatusCode(500, $"Error deserializing transaction data: {jsonEx.Message}");
                }

                return Ok(transactions);
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
        [HttpGet("transactions/{id}")]
        public async Task<IActionResult> GetTransactionById(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid transaction ID.");
            }

            // Define the Inventory API endpoint URL
            var getTransactionUrl = $"{_inventoryApiBaseUrl}/api/transactions/{id}";

            try
            {
                // Send GET request to the Inventory API
                var response = await _httpClient.GetAsync(getTransactionUrl);

                if (!response.IsSuccessStatusCode)
                {
                    // Return the error response from the Inventory API
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                // Deserialize the response JSON into a Transaction object
                var transactionJson = await response.Content.ReadAsStringAsync();

                Transaction? transaction;
                try
                {
                    transaction = JsonSerializer.Deserialize<Transaction>(transactionJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                catch (JsonException jsonEx)
                {
                    return StatusCode(500, $"Error deserializing transaction data: {jsonEx.Message}");
                }

                return Ok(transaction);
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
                // Step 1: Validate Authorization Header
                var (validationResult, userId) = ValidateAuthorizationHeader();
                if (validationResult != null)
                {
                    return validationResult; // Return if validation fails
                }

                request.UserId = userId;

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
