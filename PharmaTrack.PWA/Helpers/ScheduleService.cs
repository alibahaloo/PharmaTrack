using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PharmaTrack.PWA.Helpers
{
    public class ScheduleEvent : IValidatableObject
    {
        public int? Id { get; set; }
        [Required(ErrorMessage = "Username is required")]
        public string UserName { get; set; } = default!;
        [Required(ErrorMessage = "Start date & time is required")]
        public DateTime Start { get; set; }
        [Required(ErrorMessage = "End date & time is required")]
        public DateTime End { get; set; }
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = default!;

        public IEnumerable<ValidationResult> Validate(ValidationContext ctx)
        {
            // 1) Start.Date must be today or later
            if (Start.Date < DateTime.Today)
            {
                yield return new ValidationResult(
                    "Date cannot be in the past.",
                    new[] { nameof(Start) }
                );
            }

            // 2) End must be strictly after Start
            if (End <= Start)
            {
                yield return new ValidationResult(
                    "End time must be later than Start time.",
                    new[] { nameof(End) }
                );
            }
        }
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
        public async Task<ScheduleEvent?> CreateScheduleAsync(ScheduleEvent newEvent)
        {
            const string url = "schedules";
            try
            {
                // POST the newEvent as JSON, using your case-insensitive options
                var response = await _http.PostAsJsonAsync(url, newEvent, _jsonOptions);

                // if the server returns success, deserialize and return the created object
                response.EnsureSuccessStatusCode();
                var created = await response.Content.ReadFromJsonAsync<ScheduleEvent>(_jsonOptions);
                return created;
            }
            catch (HttpRequestException)
            {
                // TODO: log the error, or propagate
                return null;
            }
        }

        public async Task<List<ScheduleEvent>> GetMonthlySchedulesAsync(DateTime date, string username = "")
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
                var result = await _http.GetFromJsonAsync<List<ScheduleEvent>>(url, _jsonOptions);
                return result ?? new List<ScheduleEvent>();
            }
            catch (HttpRequestException)
            {
                // TODO: log error or handle accordingly
                return new List<ScheduleEvent>();
            }
        }

        public async Task<List<ScheduleEvent>> GetWeeklySchedulesAsync(DateTime weekStart, string username = "")
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
                var result = await _http.GetFromJsonAsync<List<ScheduleEvent>>(url);
                return result ?? new List<ScheduleEvent>();
            }
            catch (HttpRequestException)
            {
                // TODO: log error or handle accordingly
                return new List<ScheduleEvent>();
            }
        }

        public async Task<List<ScheduleEvent>> GetDailySchedulesAsync(DateTime date)
        {
            var url = $"schedules/daily?date={date:yyyy-MM-dd}";
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
    }
}
