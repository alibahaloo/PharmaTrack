using System.Net.Http.Json;
using System.Text.Json;
using PharmaTrack.Core.DBModels;
namespace PharmaTrack.PWA.Helpers
{
    
    public class ScheduleService
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };
        public ScheduleService(HttpClient http)
        {
            _http = http;
        }
        public async Task<bool> CreateScheduleAsync(ScheduleTask newTask)
        {
            const string url = "schedules";
            try
            {
                // POST the newEvent as JSON, using your case-insensitive options
                var response = await _http.PostAsJsonAsync(url, newTask, _jsonOptions);

                // if the server returns success, deserialize and return the created object
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (HttpRequestException)
            {
                // TODO: log the error, or propagate
                return false;
            }
        }

        public async Task<List<ScheduleTask>> GetMonthlySchedulesAsync(DateTime date, string username = "")
        {
            string url;
            if (username != string.Empty) {
                url = $"schedules/monthly/user/{username}?date={date:yyyy-MM-dd}";
            }
            else {
                url = $"schedules/monthly?date={date:yyyy-MM-dd}";
            }

            try
            {
                var result = await _http.GetFromJsonAsync<List<ScheduleTask>>(url, _jsonOptions);
                return result ?? [];
            }
            catch (HttpRequestException)
            {
                // TODO: log error or handle accordingly
                return [];
            }
        }

        public async Task<List<ScheduleTask>> GetWeeklySchedulesAsync(DateTime weekStart, string username = "")
        {
            string url;
            if (username != string.Empty)
            {
                url = $"schedules/weekly/user/{username}?date={weekStart:yyyy-MM-dd}";
            }
            else
            {
                url = $"schedules/weekly?date={weekStart:yyyy-MM-dd}";
            }

            try
            {
                var result = await _http.GetFromJsonAsync<List<ScheduleTask>>(url);
                return result ?? [];
            }
            catch (HttpRequestException)
            {
                // TODO: log error or handle accordingly
                return [];
            }
        }

        public async Task<List<ScheduleTask>> GetDailySchedulesAsync(DateTime date)
        {
            var url = $"schedules/daily?date={date:yyyy-MM-dd}";
            try
            {
                var result = await _http.GetFromJsonAsync<List<ScheduleTask>>(url, _jsonOptions);
                return result ?? [];
            }
            catch (HttpRequestException)
            {
                // TODO: log error or handle accordingly
                return [];
            }
        }
    }
}
