using PharmaTrack.Shared.APIModels;
using PharmaTrack.WPF.Helpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace PharmaTrack.WPF.ViewModels
{
    public class ScheduleControlViewModel : INotifyPropertyChanged
    {
        private DateTime _selectedDate = DateTime.Today;
        private TimeSpan _startTime;
        private TimeSpan _endTime;
        private string _description = string.Empty;

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
                    _startTime = value;
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
                    _endTime = value;
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
            SubmitCommand = new RelayCommand(Submit, CanSubmit);
        }

        private void Submit(object? parameter)
        {
            // Logic to process the input data
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
            return !string.IsNullOrEmpty(Description) && EndTime > StartTime;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
