using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PharmaTrack
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private DateTime currentMonth;
        public MainWindow()
        {
            this.InitializeComponent();
            // Start with today's month
            currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            // Example of highlighted dates
            var highlightedDates = new[]
            {
                new DateTime(DateTime.Today.Year, DateTime.Today.Month, 5),
                new DateTime(DateTime.Today.Year, DateTime.Today.Month, 15),
                new DateTime(DateTime.Today.Year, DateTime.Today.Month, 25)
            };

            GenerateCalendar(currentMonth, highlightedDates);
        }

        private void GenerateCalendar(DateTime month, DateTime[] highlightedDates)
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
                Border dayHeaderBorder = new Border
                {
                    BorderBrush = new SolidColorBrush(Colors.White),
                    BorderThickness = new Thickness(1),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };

                TextBlock dayHeader = new TextBlock
                {
                    Text = daysOfWeek[i],
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Colors.White)
                };

                dayHeaderBorder.Child = dayHeader;
                Grid.SetRow(dayHeaderBorder, 0); // Row 0 is for headers
                Grid.SetColumn(dayHeaderBorder, i);
                CalendarGrid.Children.Add(dayHeaderBorder);
            }

            // Get first day of the month and total days
            DateTime firstDayOfMonth = new DateTime(month.Year, month.Month, 1);
            int daysInMonth = DateTime.DaysInMonth(month.Year, month.Month);

            // Get the day of the week for the first day
            int startDayOfWeek = (int)firstDayOfMonth.DayOfWeek;

            // Adjust to make Monday the first day of the week
            startDayOfWeek = (startDayOfWeek == 0) ? 6 : startDayOfWeek - 1;

            int currentRow = 1; // Start from row 1 because row 0 is for headers
            int currentColumn = startDayOfWeek;

            // Populate the grid with day labels
            for (int day = 1; day <= daysInMonth; day++)
            {
                DateTime currentDate = new DateTime(month.Year, month.Month, day);

                Border dayBorder = new Border
                {
                    BorderBrush = new SolidColorBrush(Colors.White),
                    BorderThickness = new Thickness(1),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Background = highlightedDates.Contains(currentDate)
                        ? new SolidColorBrush(Colors.Green) // Highlighted dates
                        : new SolidColorBrush(Colors.Black) // Default background
                };

                TextBlock dayLabel = new TextBlock
                {
                    Text = day.ToString(),
                    Margin = new Thickness(5),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center,
                    Foreground = new SolidColorBrush(Colors.White) // Default text color
                };

                dayBorder.Child = dayLabel;
                Grid.SetRow(dayBorder, currentRow);
                Grid.SetColumn(dayBorder, currentColumn);
                CalendarGrid.Children.Add(dayBorder);

                // Move to the next cell
                currentColumn++;
                if (currentColumn > 6) // Move to the next row if end of the week
                {
                    currentColumn = 0;
                    currentRow++;
                }
            }
        }

        private void PrevMonthButton_Click(object sender, RoutedEventArgs e)
        {
            currentMonth = currentMonth.AddMonths(-1);
            GenerateCalendar(currentMonth, []);
        }

        private void NextMonthButton_Click(object sender, RoutedEventArgs e)
        {
            currentMonth = currentMonth.AddMonths(1);
            GenerateCalendar(currentMonth, []);
        }

        private void TodayButton_Click(object sender, RoutedEventArgs e)
        {
            // Reset to today's month
            currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            // Example of highlighted dates
            var highlightedDates = new[]
            {
        new DateTime(DateTime.Today.Year, DateTime.Today.Month, 5),
        new DateTime(DateTime.Today.Year, DateTime.Today.Month, 15),
        new DateTime(DateTime.Today.Year, DateTime.Today.Month, 25)
    };

            GenerateCalendar(currentMonth, highlightedDates);
        }


        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            myButton.Content = "Clicked";
        }
    }
}
