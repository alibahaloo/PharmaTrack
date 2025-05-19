using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PharmaTrack.PWA.Helpers
{
    public class DrugListResponse
    {
        public int Id { get; set; }
        public int DrugCode { get; set; }
        public string? BrandName { get; set; }
    }
    public class DrugRequest
    {
        public int? DrugCode { get; set; }
        public string? BrandName { get; set; }
        public string? DIN { get; set; }
    }
    public class DrugResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("hash")]
        public string Hash { get; set; }

        [JsonPropertyName("drugCode")]
        public int DrugCode { get; set; }

        [JsonPropertyName("productCategorization")]
        public string ProductCategorization { get; set; }

        // “class” is a valid property name, but if you'd rather avoid confusion you can rename it and map accordingly.
        [JsonPropertyName("class")]
        public string Class { get; set; }

        [JsonPropertyName("drugIdentificationNumber")]
        public string DrugIdentificationNumber { get; set; }

        [JsonPropertyName("brandName")]
        public string BrandName { get; set; }

        [JsonPropertyName("descriptor")]
        public string Descriptor { get; set; }

        [JsonPropertyName("pediatricFlag")]
        public string PediatricFlag { get; set; }

        [JsonPropertyName("accessionNumber")]
        public string AccessionNumber { get; set; }

        [JsonPropertyName("numberOfAis")]
        public string NumberOfAis { get; set; }

        [JsonPropertyName("lastUpdateDate")]
        public DateTime LastUpdateDate { get; set; }

        [JsonPropertyName("aiGroupNo")]
        public string AiGroupNo { get; set; }
    }
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

        public async Task<List<DrugListResponse>> GetDrugListAsync(string startWith = "")
        {
            string url = $"/Drugs/list?startWith={startWith}";

            try
            {
                var result = await _http.GetFromJsonAsync<List<DrugListResponse>>(url, _jsonOptions);
                return result ?? [];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // TODO: log error or handle accordingly
                return [];
            }
        }

        public async Task<List<DrugResponse>> GetDrugsAsync(DrugRequest request, int curPage = 1)
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
                var result = await _http.GetFromJsonAsync<List<DrugResponse>>(url, _jsonOptions);
                return result ?? [];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // TODO: log error or handle accordingly
                return [];
            }
        }
    }
}
