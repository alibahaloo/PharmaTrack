using Microsoft.Extensions.Configuration;
using PharmaTrack.Shared.APIModels;
using PharmaTrack.Shared.DBModels;
using System.Net.Http;
using System.Text.Json;

namespace PharmaTrack.WPF.Helpers
{
    public class DrugService
    {
        private readonly HttpClient _httpClient;
        private readonly string _drugsUrl;

        public DrugService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            // Safely handle null or empty configuration value
            _drugsUrl = configuration["DrugsUrls:Drugs"]
                        ?? throw new ArgumentException("Drugs URL is not configured in the application settings.", nameof(configuration));
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

            // Handle errors
            throw response.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => new UnauthorizedAccessException($"{response.StatusCode}: Invalid or expired refresh token!"),
                _ => new HttpRequestException($"{await response.Content.ReadAsStringAsync()}"),
            };
        }
    }
}
