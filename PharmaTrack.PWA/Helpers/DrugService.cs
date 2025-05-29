using PharmaTrack.Core.DBModels;
using PharmaTrack.Core.DTOs;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

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
            string url = $"drugs/list?startWith={startWith}";

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
        public async Task<PagedResponse<DrugIngredient>?> GetIngredientsAsync(DrugIngredientQuery request, int curPage = 1)
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
            if (request.ActiveIngredientCode != null)
            {
                queryParameters.Add($"ActiveIngredientCode={Uri.EscapeDataString(request.ActiveIngredientCode.Value.ToString())}");
            }
            if (!string.IsNullOrEmpty(request.Ingredient))
            {
                queryParameters.Add($"Ingredient={Uri.EscapeDataString(request.Ingredient)}");
            }

            string queryString = string.Join("&", queryParameters);
            string url = $"ingredients?{queryString}";

            try
            {
                var result = await _http.GetFromJsonAsync<PagedResponse<DrugIngredient>>(url, _jsonOptions);
                return result ?? null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // TODO: log error or handle accordingly
                return null;
            }
        }

        public async Task<IngredientInteractionResultDto> GetIngredientInteractionResultAsync(List<string> ingredients)
        {
            string ingredientsString = string.Join(",", ingredients);

            string url = $"interactions/ingredients/{ingredientsString}";

            try
            {
                var result = await _http.GetFromJsonAsync<IngredientInteractionResultDto>(url, _jsonOptions);
                return result ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // TODO: log error or handle accordingly
                return new();
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
            string url = $"drugs?{queryString}";

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

        public async Task<IngredientInfoDto?> GetIngredientInfoAsync(int ingredientCode)
        {
            string url = $"ingredients/{ingredientCode}";
            try
            {
                var response = await _http.GetAsync(url);
                response.EnsureSuccessStatusCode(); // Throws if not 2xx

                var result = await _http.GetFromJsonAsync<IngredientInfoDto>(url, _jsonOptions);
                return result ?? null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // TODO: log error or handle accordingly
                return null;
            }
        }

        public async Task<DrugInfoDto?> GetDrugInfoByDrugCodeAsync(int drugCode)
        {
            string url = $"drugs/{drugCode}";

            var response = await _http.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.NotFound) return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<DrugInfoDto>(_jsonOptions);
        }

        public async Task<DrugInfoDto?> GetDrugInfoByDINAsync(string DIN)
        {
            string url = $"drugs/DIN/{DIN}";

            var response = await _http.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.NotFound) return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<DrugInfoDto>(_jsonOptions);
        }

        public async Task<DrugInteractionResultDto> GetDrugInteractionsAsync(List<int> drugCodes)
        {
            string drugCodesString = string.Join(",", drugCodes);

            string url = $"interactions/drugs/{drugCodesString}";

            try
            {
                var result = await _http.GetFromJsonAsync<DrugInteractionResultDto>(url, _jsonOptions);
                return result ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // TODO: log error or handle accordingly
                return new();
            }
        }
    }
}
