using PharmaTrack.Shared.DBModels;
using PharmaTrack.WPF.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace PharmaTrack.WPF.ViewModels
{
    public enum CalendarMode
    {
        Weekly,
        Monthly
    }
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
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private CalendarMode _calendarMode;
        public CalendarMode CalendarMode
        {
            get => _calendarMode;
            set
            {
                if (_calendarMode != value)
                {
                    _calendarMode = value;
                    OnPropertyChanged();
                    LoadHighlightedDatesAsync();
                }
            }
        }

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
                    LoadHighlightedDatesAsync();
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

        private DateTime _currentMonth;
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

        private Dictionary<DateTime, List<string>> _highlightedDates = new();
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

        private ObservableCollection<CalendarDay> _calendarDays = new();
        public ObservableCollection<CalendarDay> CalendarDays
        {
            get => _calendarDays;
            private set
            {
                _calendarDays = value;
                OnPropertyChanged();
            }
        }

        private string _selectedUser = string.Empty;
        private ObservableCollection<string> _filteredUsers = [];

        public ObservableCollection<string> Users { get; } = [];

        public ObservableCollection<string> FilteredUsers
        {
            get => _filteredUsers;
            set
            {
                _filteredUsers = value;
                OnPropertyChanged();
            }
        }

        public string SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (_selectedUser != value)
                {
                    _selectedUser = value;
                    OnPropertyChanged();
                    UpdateFilteredUsers();

                    // Open dropdown when typing
                    IsDropDownOpen = !string.IsNullOrWhiteSpace(_selectedUser);
                }
            }
        }

        private void UpdateFilteredUsers()
        {
            if (string.IsNullOrWhiteSpace(SelectedUser))
            {
                FilteredUsers = new ObservableCollection<string>(Users);
            }
            else
            {
                var filtered = Users
                    .Where(user => user.ToLower().Contains(SelectedUser.ToLower()))
                    .ToList();

                FilteredUsers = new ObservableCollection<string>(filtered);
            }
        }
        private bool _isDropDownOpen;
        public bool IsDropDownOpen
        {
            get => _isDropDownOpen;
            set
            {
                if (_isDropDownOpen != value)
                {
                    _isDropDownOpen = value;
                    OnPropertyChanged();
                }
            }
        }

        public async Task LoadUsersAsync()
        {
            DisplayMode = Mode.Loading;
            //await Task.Delay(500);
            var users = await _usersService.GetUsernamesAsync();

            Users.Clear();
            if (users != null)
            {
                foreach (var user in users)
                {
                    Users.Add(user);
                }

                // Refresh filtered users
                UpdateFilteredUsers();
            }

            DisplayMode = Mode.Calendar;
        }

        public ObservableCollection<ScheduleTask> DailySchedules { get; } = new();

        public ICommand LoadDetailsCommand { get; }
        public ICommand LoadCalendarCommand { get; }
        public ICommand TodayCommand { get; }
        public ICommand NextMonthCommand { get; }
        public ICommand PreviousMonthCommand { get; }
        public ICommand PrevWeekCommand { get; }
        public ICommand NextWeekCommand { get; }
        public ICommand ViewMyScheduleCommand { get; }
        public ICommand ViewTeamScheduleCommand { get; }
        public ICommand ViewMonthlyCommand { get; }
        public ICommand ViewWeeklyCommand { get; }

        private readonly UsersService _usersService;
        private readonly ScheduleService _scheduleService;
        public CalendarControlViewModel(ScheduleService scheduleService, UsersService usersService)
        {
            _scheduleService = scheduleService;
            _usersService = usersService;
            LoadDetailsCommand = new RelayCommand(param => ExecuteLoadDetailsCommand(param));
            LoadCalendarCommand = new RelayCommand(_ => DisplayMode = Mode.Calendar);

            TodayCommand = new RelayCommand(_ => CurrentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1));
            NextMonthCommand = new RelayCommand(_ => CurrentMonth = CurrentMonth.AddMonths(1));
            PreviousMonthCommand = new RelayCommand(_ => CurrentMonth = CurrentMonth.AddMonths(-1));

            PrevWeekCommand = new RelayCommand(_ => CurrentMonth = CurrentMonth.AddDays(-7));
            NextWeekCommand = new RelayCommand(_ => CurrentMonth = CurrentMonth.AddDays(7));

            ViewMyScheduleCommand = new RelayCommand(_ => DataMode = DataMode.MySchedule);
            ViewTeamScheduleCommand = new RelayCommand(_ => DataMode = DataMode.TeamSchedule);

            ViewMonthlyCommand = new RelayCommand(_ => CalendarMode = CalendarMode.Monthly);
            ViewWeeklyCommand = new RelayCommand(_ => CalendarMode = CalendarMode.Weekly);
            
        }

        public async void OnViewModelLoaded()
        {
            // Logic to execute after the view model is fully loaded
            CurrentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DataMode = DataMode.MySchedule;
            CalendarMode = CalendarMode.Weekly;

            await LoadUsersAsync();
        }
        private async void ExecuteLoadDetailsCommand(object? parameter)
        {
            if (parameter is DateTime selectedDate)
            {
                DisplayMode = Mode.Loading;

                SelectedDate = selectedDate;

                // Simulate API delay
                //await Task.Delay(500);

                List<ScheduleTask>? scheduleTasks = DataMode switch
                {
                    DataMode.MySchedule => await _scheduleService.GetMyDailyScheduleTasksAsync(SelectedDate),
                    _ => await _scheduleService.GetDailyScheduleTasksAsync(SelectedDate),
                };

                if (scheduleTasks != null)
                {
                    DailySchedules.Clear();
                    foreach (var schedule in scheduleTasks)
                    {
                        DailySchedules.Add(schedule);
                    }
                }

                DisplayMode = Mode.Details;
            }
        }
        private async Task<Dictionary<DateTime, List<string>>> FetchEventsForMonthAsync(DateTime month)
        {
            DisplayMode = Mode.Loading;
            //await Task.Delay(500); // Simulate API delay

            List<ScheduleTask>? scheduleTasks = null;

            switch (DataMode)
            {
                case DataMode.MySchedule:
                    scheduleTasks = await _scheduleService.GetMyMonthlyScheduleTasksAsync(month);
                    break;
                case DataMode.TeamSchedule:
                    scheduleTasks = await _scheduleService.GetMonthlyScheduleTasksAsync(month);
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

            switch (CalendarMode)
            {
                case CalendarMode.Weekly:
                    // Get the start of the current week (Monday)
                    DateTime startOfWeek = CurrentWeekStart();

                    // Generate 7 days for the current week
                    for (int i = 0; i < 7; i++)
                    {
                        DateTime date = startOfWeek.AddDays(i);
                        List<string>? highlightedEvents = HighlightedDates.ContainsKey(date) ? HighlightedDates[date] : null;

                        days.Add(new CalendarDay(date, isCurrentMonth: date.Month == CurrentMonth.Month, highlightedEvents: highlightedEvents, loadDetailsCommand: LoadDetailsCommand));
                    }
                    break;
                case CalendarMode.Monthly:
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

                    break;
                default:
                    throw new NotImplementedException();
            }
            // Update the CalendarDays property
            CalendarDays = days;
        }

        private DateTime CurrentWeekStart()
        {
            DateTime today = CurrentMonth;
            int daysToSubtract = (int)today.DayOfWeek - 1; // Adjust for Monday as the first day
            daysToSubtract = daysToSubtract < 0 ? 6 : daysToSubtract; // Handle Sunday case
            return today.AddDays(-daysToSubtract);
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
