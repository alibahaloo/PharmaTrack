using PharmaTrack.Core.DBModels;
using PharmaTrack.Core.DTOs;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace PharmaTrack.PWA.Helpers
{
    public class InventoryService
    {
        private readonly HttpClient _http;
        private readonly DrugService _drugService;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };
        public InventoryService(HttpClient http, DrugService drugService)
        {
            _http = http;
            _drugService = drugService;
        }

        public async Task<Product?> GetProductByUPCAsync(string UPC)
        {
            string url = $"products/upc/{UPC}";

            var response = await _http.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Product>(_jsonOptions);
        }

        public async Task<Product?> GetProductByIdAsync(int Id)
        {
            string url = $"products/{Id}";

            var response = await _http.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Product>(_jsonOptions);
        }

        public async Task UpdateProductAsync(Product product)
        {
            //Check if DIN is valid, if there's one
            if (!string.IsNullOrEmpty(product.DIN))
            {
                _ = await _drugService.GetDrugInfoByDINAsync(product.DIN) ?? throw new InvalidOperationException("Invalid DIN. No drug found with the given DIN");
            }

            string url = $"products/{product.Id}";

            var response = await _http.PutAsJsonAsync(url, product, _jsonOptions);
            response.EnsureSuccessStatusCode();
        }

        public async Task StockTransferAsync(StockTransferRequest request)
        {
            //Check if DIN is valid, if there's one
            if (!string.IsNullOrEmpty(request.DIN)) {
                _ = await _drugService.GetDrugInfoByDINAsync(request.DIN) ?? throw new InvalidOperationException("Invalid DIN. No drug found with the given DIN");
            }

            string url = "inventory/stock-transfer";

            var response = await _http.PostAsJsonAsync(url, request, _jsonOptions);
            response.EnsureSuccessStatusCode();
        }

        public async Task<PagedResponse<Product>?> GetProductsAsync(string? searchPhrase, int curPage = 1)
        {
            // Convert TransactionsRequest to query parameters
            var queryParameters = new List<string>
            {
                $"curPage={curPage}"
            };

            if (!string.IsNullOrEmpty(searchPhrase))
            {
                queryParameters.Add($"searchPhrase={Uri.EscapeDataString(searchPhrase)}");
            }

            string queryString = string.Join("&", queryParameters);
            string url = $"products?{queryString}";

            var result = await _http.GetFromJsonAsync<PagedResponse<Product>>(url, _jsonOptions);
            return result ?? null;
        }

        public async Task<PagedResponse<Transaction>?> GetTransactionsAsync(string? searchPhrase, TransactionType transactionType = TransactionType.In, int curPage = 1)
        {
            // Convert TransactionsRequest to query parameters
            var queryParameters = new List<string>
            {
                $"curPage={curPage}"
            };

            if (!string.IsNullOrEmpty(searchPhrase))
            {
                queryParameters.Add($"searchPhrase={Uri.EscapeDataString(searchPhrase)}");
            }

            queryParameters.Add($"transactionType={transactionType}");

            string queryString = string.Join("&", queryParameters);
            string url = $"transactions?{queryString}";

            var result = await _http.GetFromJsonAsync<PagedResponse<Transaction>>(url, _jsonOptions);
            return result ?? null;
        }
    }
}
