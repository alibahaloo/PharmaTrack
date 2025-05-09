using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace PharmaTrack.PWA.Helpers
{
    public class ScheduleEvent
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Description { get; set; }
    }
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

        public async Task<List<ScheduleEvent>> GetMonthlySchedulesAsync(DateTime month)
        {
            var url = $"schedules/monthly?month={month:yyyy-MM-dd}";
            try
            {
                var result = await _http.GetFromJsonAsync<List<ScheduleEvent>>(url, _jsonOptions);
                return result ?? new List<ScheduleEvent>();
            }
            catch (HttpRequestException)
            {
                // TODO: log error or handle accordingly
                return new List<ScheduleEvent>();
            }
        }

        public async Task<List<ScheduleEvent>> GetWeeklySchedulesAsync(DateTime weekStart)
        {
            var url = $"schedules/weekly?start={weekStart:yyyy-MM-dd}";
            try
            {
                var result = await _http.GetFromJsonAsync<List<ScheduleEvent>>(url);
                return result ?? new List<ScheduleEvent>();
            }
            catch (HttpRequestException)
            {
                // TODO: log error or handle accordingly
                return new List<ScheduleEvent>();
            }
        }
    }
}
