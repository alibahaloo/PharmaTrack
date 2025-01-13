using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PharmaTrack.WPF.ViewModels
{
    public class CalendarControlViewModel : INotifyPropertyChanged
    {
        private DateTime _currentMonth;
        private Dictionary<DateTime, string> _highlightedDates = new();
        private ObservableCollection<CalendarDay> _calendarDays = new();
        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _isLoading = true;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public DateTime CurrentMonth
        {
            get => _currentMonth;
            set
            {
                if (_currentMonth != value)
                {
                    _currentMonth = value;
                    OnPropertyChanged();
                    LoadHighlightedDatesAsync(); // Automatically load data when the month changes
                }
            }
        }

        public Dictionary<DateTime, string> HighlightedDates
        {
            get => _highlightedDates;
            set
            {
                _highlightedDates = value;
                OnPropertyChanged();
                GenerateCalendarDays();
            }
        }

        public ObservableCollection<CalendarDay> CalendarDays
        {
            get => _calendarDays;
            private set
            {
                _calendarDays = value;
                OnPropertyChanged();
            }
        }

        public CalendarControlViewModel()
        {
            CurrentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            LoadHighlightedDatesAsync();
        }

        private async void LoadHighlightedDatesAsync()
        {
            try
            {
                HighlightedDates = await FetchEventsForMonthAsync(CurrentMonth);
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., logging or displaying an error message)
                Console.WriteLine($"Error loading highlighted dates: {ex.Message}");
                HighlightedDates = new Dictionary<DateTime, string>(); // Clear data on failure
            }
        }

        // Method to simulate fetching events for a specific month
        private async Task<Dictionary<DateTime, string>> FetchEventsForMonthAsync(DateTime month)
        {
            IsLoading = true;
            await Task.Delay(500); // Simulate API delay

            var dates = new Dictionary<DateTime, string>
            {
                { new DateTime(month.Year, month.Month, 5), "Meeting" },
                { new DateTime(month.Year, month.Month, 15), "Birthday" },
                { new DateTime(month.Year, month.Month, 25), "Holiday" }
            };
            IsLoading = false;
            return dates;
        }

        private void GenerateCalendarDays()
        {
            var days = new ObservableCollection<CalendarDay>();

            // Get the first day of the month and determine its position in the week
            DateTime firstDayOfMonth = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);
            int daysInMonth = DateTime.DaysInMonth(CurrentMonth.Year, CurrentMonth.Month);
            int startDayOfWeek = (int)firstDayOfMonth.DayOfWeek;

            // Adjust start day to make Monday the first column
            startDayOfWeek = (startDayOfWeek == 0) ? 6 : startDayOfWeek - 1;

            // Fill in the days before the first day of the month
            DateTime previousMonth = CurrentMonth.AddMonths(-1);
            int daysInPreviousMonth = DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month);

            for (int i = startDayOfWeek - 1; i >= 0; i--)
            {
                DateTime date = new DateTime(previousMonth.Year, previousMonth.Month, daysInPreviousMonth - i);
                days.Add(new CalendarDay(date, isCurrentMonth: false));
            }

            // Fill in the current month's days
            for (int day = 1; day <= daysInMonth; day++)
            {
                DateTime date = new DateTime(CurrentMonth.Year, CurrentMonth.Month, day);
                days.Add(new CalendarDay(date, isCurrentMonth: true, HighlightedDates.ContainsKey(date) ? HighlightedDates[date] : null));
            }

            // Fill in the remaining days of the week after the last day of the month
            int remainingDays = 42 - days.Count; // Ensure a 6-week display
            DateTime nextMonth = CurrentMonth.AddMonths(1);

            for (int i = 1; i <= remainingDays; i++)
            {
                DateTime date = new DateTime(nextMonth.Year, nextMonth.Month, i);
                days.Add(new CalendarDay(date, isCurrentMonth: false));
            }

            // Update the CalendarDays property
            CalendarDays = days;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CalendarDay
    {
        public DateTime Date { get; }
        public bool IsCurrentMonth { get; }
        public string? HighlightedEvent { get; }

        public CalendarDay(DateTime date, bool isCurrentMonth, string? highlightedEvent = null)
        {
            Date = date;
            IsCurrentMonth = isCurrentMonth;
            HighlightedEvent = highlightedEvent;
        }
    }
}
