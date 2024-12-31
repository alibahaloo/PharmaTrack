using PharmaTrack.WPF.Controls;
using System.Windows;

namespace PharmaTrack.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly CalendarControl calendarControl = new();
        private readonly StockTransferControl stockTransferControl = new();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MyScheduleMenuItem_Click(object sender, RoutedEventArgs e)
        {
            calendarControl.Mode = CalendarMode.SingleUser;

            // Subscribe to the MonthChanged event
            calendarControl.MonthChanged -= MyCalendar_MonthChanged; // Avoid multiple subscriptions
            calendarControl.MonthChanged += MyCalendar_MonthChanged;

            // Load CalendarControl into the ContentFrame
            ContentFrame.Content = calendarControl;

            // Load initial events for the current month
            MyCalendar_MonthChanged(this, DateTime.Now);
        }

        private void StockTransferMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Load StockTransferControl into the ContentFrame
            ContentFrame.Content = stockTransferControl;
        }

        private void InventoryMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder for Inventory Control
            MessageBox.Show("Inventory feature is not yet implemented.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MyCalendar_MonthChanged(object? sender, DateTime selectedMonth)
        {
            // Define the logic for loading events dynamically
            calendarControl.LoadEventsForMonth(month =>
            {
                // Example: Replace with actual logic to fetch events for the given month
                return new Dictionary<DateTime, string>
                {
                    { new DateTime(month.Year, month.Month, 5), "Meeting" },
                    { new DateTime(month.Year, month.Month, 15), "Birthday" },
                    { new DateTime(month.Year, month.Month, 25), "Holiday" },
                    { new DateTime(month.Year, month.Month, 24), string.Empty }
                };
            });
        }
    }
}