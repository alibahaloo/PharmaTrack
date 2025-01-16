using PharmaTrack.Shared.APIModels;
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


        private bool _isLoading = false;
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

        public ScheduleControlViewModel()
        {
            // Initialize with all users
            FilteredUsers = new ObservableCollection<string>(Users);
            SubmitCommand = new RelayCommand(Submit, CanSubmit);
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

        public async Task LoadUsersAsync()
        {
            // Simulate an API call with a delay
            await Task.Delay(500);

            // Replace this with API call logic
            var dummyUsers = new[] { "Alice", "Bob", "Charlie", "David", "Eva", "Frank", "Grace", "Helen" };

            Users.Clear();
            foreach (var user in dummyUsers)
            {
                Users.Add(user);
            }

            // Refresh filtered users
            UpdateFilteredUsers();
        }
        private void Submit(object? parameter)
        {
            var scheduleTask = new ScheduleTaskRequest
            {
                UserName = Environment.UserName,
                Start = SelectedDate.Add(StartTime),
                End = SelectedDate.Add(EndTime),
                Description = Description
            };
            // Submit scheduleTask to a service or further processing
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

            StatusText = "Ready to save schedule";
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
