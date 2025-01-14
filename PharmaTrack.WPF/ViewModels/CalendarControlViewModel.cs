using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PharmaTrack.WPF.Helpers;

namespace PharmaTrack.WPF.ViewModels
{
    public enum Mode
    {
        Loading,
        Calendar,
        Details,
    }
    public class CalendarControlViewModel : INotifyPropertyChanged
    {
        private DateTime _currentMonth;
        private Dictionary<DateTime, string> _highlightedDates = new();
        private ObservableCollection<CalendarDay> _calendarDays = new();
        public event PropertyChangedEventHandler? PropertyChanged;

        private DateTime _selectedDate;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (_selectedDate != value)
                {
                    _selectedDate = value;
                    OnPropertyChanged();
                }
            }
        }

        private Mode _displayMode = Mode.Loading;
        public Mode DisplayMode
        {
            get => _displayMode;
            set
            {
                _displayMode = value;
                OnPropertyChanged(nameof(DisplayMode));
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

        public ICommand LoadDetailsCommand { get; }
        public ICommand LoadCalendarCommand { get; }

        private async void ExecuteLoadDetailsCommand(object? parameter)
        {
            if (parameter is DateTime selectedDate)
            {
                DisplayMode = Mode.Loading;

                SelectedDate = selectedDate;

                // Simulate API delay
                await Task.Delay(500);

                // Use the selected date for your logic
                Console.WriteLine($"Selected Date: {selectedDate}");

                DisplayMode = Mode.Details;
            }
        }

        private void ExecuteLoadCalendarCommand(object? parameter)
        {
            DisplayMode = Mode.Calendar;
        }

        public CalendarControlViewModel()
        {
            CurrentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            LoadHighlightedDatesAsync();
            LoadDetailsCommand = new RelayCommand(param => ExecuteLoadDetailsCommand(param));
            LoadCalendarCommand = new RelayCommand(ExecuteLoadCalendarCommand);

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
            //IsLoading = true;
            DisplayMode = Mode.Loading;
            await Task.Delay(500); // Simulate API delay

            var dates = new Dictionary<DateTime, string>
            {
                { new DateTime(month.Year, month.Month, 5), "Meeting" },
                { new DateTime(month.Year, month.Month, 15), "Birthday" },
                { new DateTime(month.Year, month.Month, 25), "Holiday" }
            };
            DisplayMode = Mode.Calendar;
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
                days.Add(new CalendarDay(date, isCurrentMonth: false, loadDetailsCommand: LoadDetailsCommand));
            }

            // Fill in the current month's days
            for (int day = 1; day <= daysInMonth; day++)
            {
                DateTime date = new DateTime(CurrentMonth.Year, CurrentMonth.Month, day);
                days.Add(new CalendarDay(date, isCurrentMonth: true,
                    HighlightedDates.ContainsKey(date) ? HighlightedDates[date] : null,
                    LoadDetailsCommand));
            }

            // Fill in the remaining days of the week after the last day of the month
            int remainingDays = 42 - days.Count; // Ensure a 6-week display
            DateTime nextMonth = CurrentMonth.AddMonths(1);

            for (int i = 1; i <= remainingDays; i++)
            {
                DateTime date = new DateTime(nextMonth.Year, nextMonth.Month, i);
                days.Add(new CalendarDay(date, isCurrentMonth: false, loadDetailsCommand: LoadDetailsCommand));
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
        public ICommand? LoadDetailsCommand { get; }

        public CalendarDay(DateTime date, bool isCurrentMonth, string? highlightedEvent = null, ICommand? loadDetailsCommand = null)
        {
            Date = date;
            IsCurrentMonth = isCurrentMonth;
            HighlightedEvent = highlightedEvent;
            LoadDetailsCommand = loadDetailsCommand;
        }
    }

}
