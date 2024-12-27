using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI;
using Microsoft.UI.Text;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PharmaTrack.Shared
{
    public enum CalendarMode
    {
        SingleUser,
        MultipleUsers
    }
    public sealed partial class CalendarControl : UserControl
    {
        private DateTime currentMonth;

        // Event to notify the parent of month changes
        public event EventHandler<DateTime>? MonthChanged;

        public CalendarMode Mode
        {
            get => (CalendarMode)GetValue(ModeProperty);
            set => SetValue(ModeProperty, value);
        }

        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register(
                nameof(Mode),
                typeof(CalendarMode),
                typeof(CalendarControl),
                new PropertyMetadata(CalendarMode.SingleUser));

        public static readonly DependencyProperty HighlightedDatesProperty = DependencyProperty.Register(
            nameof(HighlightedDates),
            typeof(Dictionary<DateTime, string>),
            typeof(CalendarControl),
            new PropertyMetadata(new Dictionary<DateTime, string>(), OnHighlightedDatesChanged));

        public Dictionary<DateTime, string> HighlightedDates
        {
            get => (Dictionary<DateTime, string>)GetValue(HighlightedDatesProperty);
            set => SetValue(HighlightedDatesProperty, value);
        }

        private static void OnHighlightedDatesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CalendarControl calendarControl)
            {
                calendarControl.GenerateCalendar(calendarControl.currentMonth, (Dictionary<DateTime, string>)e.NewValue);
            }
        }

        public CalendarControl()
        {
            this.InitializeComponent();
            currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            // Handle the Loaded event to ensure the event is fired after the control is initialized
            this.Loaded += CalendarControl_Loaded;
        }

        private void CalendarControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Notify parent of the initial month after the control has loaded
            NotifyMonthChanged();
            GenerateCalendar(currentMonth, HighlightedDates);
        }

        private void NotifyMonthChanged()
        {
            MonthChanged?.Invoke(this, currentMonth);
        }
        private void GenerateCalendar(DateTime month, Dictionary<DateTime, string> highlightedDates)
        {
            CalendarGrid.Children.Clear();
            CalendarGrid.RowDefinitions.Clear();
            CalendarGrid.ColumnDefinitions.Clear();

            // Add 7 columns for weekdays
            for (int i = 0; i < 7; i++)
            {
                CalendarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            // Add rows: one for the headers and six for the calendar days
            CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header row
            for (int i = 0; i < 6; i++)
            {
                CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            }

            // Display Month and Year
            MonthYearHeader.Text = month.ToString("MMMM yyyy");

            // Add day headers
            string[] daysOfWeek = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

            for (int i = 0; i < 7; i++)
            {
                var dayHeader = new TextBlock
                {
                    Text = daysOfWeek[i],
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Colors.White)
                };

                Border border = new Border
                {
                    BorderBrush = new SolidColorBrush(Colors.White),
                    BorderThickness = new Thickness(1),
                    Child = dayHeader
                };

                Grid.SetRow(border, 0);
                Grid.SetColumn(border, i);
                CalendarGrid.Children.Add(border);
            }

            // Populate calendar days
            DateTime firstDayOfMonth = new DateTime(month.Year, month.Month, 1);
            int daysInMonth = DateTime.DaysInMonth(month.Year, month.Month);
            int startDayOfWeek = (int)firstDayOfMonth.DayOfWeek;
            startDayOfWeek = (startDayOfWeek == 0) ? 6 : startDayOfWeek - 1;

            int currentRow = 1;
            int currentColumn = startDayOfWeek;

            for (int day = 1; day <= daysInMonth; day++)
            {
                DateTime currentDate = new DateTime(month.Year, month.Month, day);

                // Check if the date is highlighted
                bool isHighlighted = highlightedDates.ContainsKey(currentDate);

                // Create the day label
                var dayStackPanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                TextBlock dayLabel = new TextBlock
                {
                    Text = day.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Colors.White)
                };
                dayStackPanel.Children.Add(dayLabel);

                // Add the event string if highlighted and not null or empty
                if (isHighlighted && !string.IsNullOrEmpty(highlightedDates[currentDate]))
                {
                    TextBlock eventLabel = new TextBlock
                    {
                        Text = highlightedDates[currentDate],
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 12,
                        FontWeight = FontWeights.Normal,
                        Foreground = new SolidColorBrush(Colors.LightYellow)
                    };
                    dayStackPanel.Children.Add(eventLabel);
                }

                // Create the border
                Border dayBorder = new Border
                {
                    BorderBrush = new SolidColorBrush(Colors.White),
                    BorderThickness = new Thickness(1),
                    Background = isHighlighted
                        ? new SolidColorBrush(Colors.Green)
                        : new SolidColorBrush(Colors.Black),
                    Child = dayStackPanel
                };

                // Handle clicks on highlighted dates
                if (isHighlighted)
                {
                    dayBorder.Tapped += (s, e) =>
                    {
                        EventDetails.Text = $"You clicked on {currentDate:MMMM dd, yyyy}" +
                                            (string.IsNullOrEmpty(highlightedDates[currentDate])
                                                ? ""
                                                : $": {highlightedDates[currentDate]}");
                    };
                }

                Grid.SetRow(dayBorder, currentRow);
                Grid.SetColumn(dayBorder, currentColumn);
                CalendarGrid.Children.Add(dayBorder);

                currentColumn++;
                if (currentColumn > 6)
                {
                    currentColumn = 0;
                    currentRow++;
                }
            }
        }

        private void PrevMonthButton_Click(object sender, RoutedEventArgs e)
        {
            currentMonth = currentMonth.AddMonths(-1);
            NotifyMonthChanged();
            GenerateCalendar(currentMonth, HighlightedDates);
        }

        private void NextMonthButton_Click(object sender, RoutedEventArgs e)
        {
            currentMonth = currentMonth.AddMonths(1);
            NotifyMonthChanged();
            GenerateCalendar(currentMonth, HighlightedDates);
        }

        private void TodayButton_Click(object sender, RoutedEventArgs e)
        {
            currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            NotifyMonthChanged();
            GenerateCalendar(currentMonth, HighlightedDates);
        }

        public void LoadEventsForMonth(Func<DateTime, Dictionary<DateTime, string>> fetchEvents)
        {
            ArgumentNullException.ThrowIfNull(fetchEvents);

            HighlightedDates = fetchEvents(currentMonth);
            GenerateCalendar(currentMonth, HighlightedDates);
        }
    }
}
