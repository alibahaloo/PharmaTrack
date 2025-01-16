using PharmaTrack.Shared.APIModels;
using PharmaTrack.WPF.Helpers;
using System;
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

        public ScheduleControlViewModel()
        {
            SubmitCommand = new RelayCommand(Submit, CanSubmit);
        }

        private TimeSpan ClampTime(TimeSpan time)
        {
            if (time < TimeSpan.Zero) return TimeSpan.Zero;
            if (time > new TimeSpan(23, 59, 0)) return new TimeSpan(23, 59, 0);
            return time;
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
            //return !string.IsNullOrEmpty(Description) && EndTime > StartTime;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
