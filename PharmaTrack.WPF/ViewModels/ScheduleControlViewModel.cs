using PharmaTrack.Core.DBModels;
using PharmaTrack.WPF.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;

namespace PharmaTrack.WPF.ViewModels
{
    public class ScheduleControlViewModel : INotifyPropertyChanged
    {
        private DateTime _selectedDate = DateTime.Today;
        private TimeSpan _startTime;
        private TimeSpan _endTime;
        private string _description = string.Empty;
        private string _statusText = default!;
        private Brush _statusForeground = default!;
        private bool _isLoading = false;

        private string _selectedUser = string.Empty;

        public ObservableCollection<string> Users { get; } = [];


        public string SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (_selectedUser != value)
                {
                    _selectedUser = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(); // Notify UI of changes
            }
        }
        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        public Brush StatusForeground
        {
            get => _statusForeground;
            set { _statusForeground = value; OnPropertyChanged(); }
        }

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
        public TimeSpan StartTime
        {
            get => _startTime;
            set
            {
                if (_startTime != value)
                {
                    _startTime = ValidateAndCorrectTime(value);
                    OnPropertyChanged();
                }
            }
        }

        public TimeSpan EndTime
        {
            get => _endTime;
            set
            {
                if (_endTime != value)
                {
                    _endTime = ValidateAndCorrectTime(value);
                    OnPropertyChanged();
                }
            }
        }
        private TimeSpan ValidateAndCorrectTime(TimeSpan time)
        {
            if (time < TimeSpan.Zero)
                return TimeSpan.Zero;

            if (time > new TimeSpan(23, 59, 0))
                return new TimeSpan(23, 59, 0);

            return time;
        }

        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand SubmitCommand { get; }
        private readonly UsersService _usersService;
        private readonly ScheduleService _scheduleService;
        public ScheduleControlViewModel(UsersService usersService, ScheduleService scheduleService)
        {
            _usersService = usersService;
            _scheduleService = scheduleService;
            SubmitCommand = new RelayCommand(Submit, CanSubmit);
        }

        public async Task LoadUsersAsync()
        {
            IsLoading = true;
            //await Task.Delay(500);
            var users = await _usersService.GetUsernamesAsync();

            Users.Clear();
            if (users != null) {
                foreach (var user in users)
                {
                    Users.Add(user);
                }

            }
            
            IsLoading = false;
        }
        private async void Submit(object? parameter)
        {
            IsLoading = true;
            try
            {
                var scheduleTask = new ScheduleTask
                {
                    UserName = SelectedUser,
                    Start = SelectedDate.Add(StartTime),
                    End = SelectedDate.Add(EndTime),
                    Description = Description
                };

                if (await _scheduleService.CreateScheduleAsync(scheduleTask)) {
                    StatusText = "Schedule Task saved successfully!";
                    StatusForeground = Brushes.Green;
                }
            }
            catch (Exception ex)
            {
                StatusText = ex.Message;
                StatusForeground = Brushes.Red;
            }
            finally
            {
                IsLoading = false;
            }            
        }

        private bool CanSubmit(object? parameter)
        {
            //Check if a user is selected
            if (string.IsNullOrEmpty(SelectedUser) || !Users.Contains(SelectedUser))
            {
                StatusText = "Please select a valid user!";
                StatusForeground = Brushes.Red;
                return false;
            }

            if (EndTime <= StartTime)
            {
                StatusText = "End time cannot be before or at start time!";
                StatusForeground = Brushes.Red;
                return false;
            }

            if (string.IsNullOrEmpty(Description))
            {
                StatusText = "Description cannot be empty!";
                StatusForeground = Brushes.Red;
                return false;
            }

            StatusText = "Ready to save schedule.";
            StatusForeground = Brushes.Green;
            return true;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
