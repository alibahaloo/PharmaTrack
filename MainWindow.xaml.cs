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
using PharmaTrack.Shared;
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
        private readonly CalendarControl calendarControl = new();
        public MainWindow()
        {
            this.InitializeComponent();
        }
        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer == null)
                return;

            var selectedItem = args.SelectedItemContainer as NavigationViewItem;

            if (selectedItem == null || selectedItem.Tag == null)
                return;

            string? tag = selectedItem.Tag.ToString();
            switch (tag)
            {
                case "MySchedule":
                    calendarControl.Mode = CalendarMode.SingleUser;
                    // Subscribe to the MonthChanged event
                    calendarControl.MonthChanged += MyCalendar_MonthChanged;
                    // Load your CalendarControl UserControl
                    ContentFrame.Content = calendarControl; 
                    break;
                case "StockTransfer":
                    //ContentFrame.Content = new StockTransferControl(); // Load your StockTransferControl UserControl
                    break;
                case "Inventory":
                    //ContentFrame.Content = new InventoryControl(); // Load your InventoryControl UserControl
                    break;
                default:
                    break;
            }
        }
        private void MyCalendar_MonthChanged(object? sender, DateTime selectedMonth)
        {
            // Handle the month change event
            // For now, just display the selected month in the console
            Console.WriteLine($"Selected Month: {selectedMonth:MMMM yyyy}");

            // You can also update your UI or perform other actions here
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
