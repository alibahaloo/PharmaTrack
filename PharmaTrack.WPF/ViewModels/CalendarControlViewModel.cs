using PharmaTrack.Shared.DBModels;
using PharmaTrack.WPF.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace PharmaTrack.WPF.ViewModels
{
    public enum Mode
    {
        Loading,
        Calendar,
        Details,
    }
    public enum DataMode
    {
        MySchedule,
        TeamSchedule
    }
    public class CalendarControlViewModel : INotifyPropertyChanged
    {
        private DateTime _currentMonth;
        private Dictionary<DateTime, List<string>> _highlightedDates = new();
        private ObservableCollection<CalendarDay> _calendarDays = new();
        public event PropertyChangedEventHandler? PropertyChanged;

        private DataMode _dataMode;
        public DataMode DataMode
        {
            get => _dataMode;
            set
            {
                if (_dataMode != value)
                {
                    _dataMode = value;
                    OnPropertyChanged();
                }
            }
        }

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

        public Dictionary<DateTime, List<string>> HighlightedDates
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
        public ICommand TodayCommand { get; }
        private void ExecuteTodayCommand(object? parameter)
        {
            CurrentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        }
        public ICommand NextMonthCommand { get; }
        private void ExecuteNextMonthCommand(object? parameter)
        {
            CurrentMonth = CurrentMonth.AddMonths(1);
        }
        public ICommand PreviousMonthCommand { get; }
        private void ExecutePreviousMonthCommand(object? parameter)
        {
            CurrentMonth = CurrentMonth.AddMonths(-1);
        }

        public ICommand ChangeDataMode { get; }
        private  void ExecuteChangeDataMode(object? parameter)
        {
            LoadHighlightedDatesAsync();
        }

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
        private readonly ScheduleService _scheduleService;
        public CalendarControlViewModel(ScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
            LoadDetailsCommand = new RelayCommand(param => ExecuteLoadDetailsCommand(param));
            LoadCalendarCommand = new RelayCommand(ExecuteLoadCalendarCommand);
            TodayCommand = new RelayCommand(ExecuteTodayCommand);
            NextMonthCommand = new RelayCommand(ExecuteNextMonthCommand);
            PreviousMonthCommand = new RelayCommand(ExecutePreviousMonthCommand);
            ChangeDataMode = new RelayCommand(ExecuteChangeDataMode);
        }

        public void OnViewModelLoaded()
        {
            // Logic to execute after the view model is fully loaded
            CurrentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DataMode = DataMode.MySchedule;
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
                HighlightedDates = [];  // Clear data on failure
            }
        }

        // Method to simulate fetching events for a specific month
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
                days.Add(new CalendarDay(date, isCurrentMonth: false, highlightedEvents: null, loadDetailsCommand: LoadDetailsCommand));
            }

            // Fill in the current month's days
            for (int day = 1; day <= daysInMonth; day++)
            {
                DateTime date = new DateTime(CurrentMonth.Year, CurrentMonth.Month, day);
                List<string>? highlightedEvents = HighlightedDates.ContainsKey(date) ? HighlightedDates[date] : null;

                days.Add(new CalendarDay(date, isCurrentMonth: true, highlightedEvents: highlightedEvents, loadDetailsCommand: LoadDetailsCommand));
            }

            // Fill in the remaining days of the week after the last day of the month
            int remainingDays = 42 - days.Count; // Ensure a 6-week display
            DateTime nextMonth = CurrentMonth.AddMonths(1);

            for (int i = 1; i <= remainingDays; i++)
            {
                DateTime date = new DateTime(nextMonth.Year, nextMonth.Month, i);
                days.Add(new CalendarDay(date, isCurrentMonth: false, highlightedEvents: null, loadDetailsCommand: LoadDetailsCommand));
            }

            // Update the CalendarDays property
            CalendarDays = days;
        }



        private async Task<Dictionary<DateTime, List<string>>> FetchEventsForMonthAsync(DateTime month)
        {
            DisplayMode = Mode.Loading;
            //await Task.Delay(500); // Simulate API delay

            List<ScheduleTask>? scheduleTasks = null;

            switch (DataMode)
            {
                case DataMode.MySchedule:
                    scheduleTasks = await _scheduleService.GetMyScheduleTasksAsync(month);
                    break;
                case DataMode.TeamSchedule:
                    scheduleTasks = await _scheduleService.GetScheduleTasksAsync(month);
                    break;
                default:
                    throw new InvalidOperationException("Unsupported DataMode");
            }

            Dictionary<DateTime, List<string>>? groupedSchedules;

            groupedSchedules = scheduleTasks?
                .GroupBy(task => task.Start.Date) // Group by the date portion of Start
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(task =>
                        DataMode == DataMode.TeamSchedule
                            ? $"{task.Start:HH:mm} - {task.End:HH:mm} : {task.UserName}"
                            : $"{task.Start:HH:mm} - {task.End:HH:mm} : {task.Description}"
                    ).ToList()
                );

            DisplayMode = Mode.Calendar;
            return groupedSchedules ?? [];
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
        public List<string> HighlightedEvents { get; }
        public ICommand? LoadDetailsCommand { get; }

        public CalendarDay(DateTime date, bool isCurrentMonth, List<string>? highlightedEvents = null, ICommand? loadDetailsCommand = null)
        {
            Date = date;
            IsCurrentMonth = isCurrentMonth;
            HighlightedEvents = highlightedEvents ?? new List<string>();
            LoadDetailsCommand = loadDetailsCommand;
        }
    }
}
