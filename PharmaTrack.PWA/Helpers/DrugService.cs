using System.Net.Http.Json;
using System.Text.Json;
using PharmaTrack.Core.DTOs;
using PharmaTrack.Core.DBModels;

namespace PharmaTrack.PWA.Helpers
{
    public class DrugService
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };
        public DrugService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<DrugListDto>> GetDrugListAsync(string startWith = "")
        {
            string url = $"/Drugs/list?startWith={startWith}";

            try
            {
                var result = await _http.GetFromJsonAsync<List<DrugListDto>>(url, _jsonOptions);
                return result ?? [];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // TODO: log error or handle accordingly
                return [];
            }
        }

        public async Task<PagedResponse<DrugProduct>?> GetDrugsAsync(DrugQuery request, int curPage = 1)
        {
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
            string url = $"/Drugs?{queryString}";

            try
            {
                var result = await _http.GetFromJsonAsync<PagedResponse<DrugProduct>>(url, _jsonOptions);
                return result ?? null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // TODO: log error or handle accordingly
                return null;
            }
        }
    }
}
