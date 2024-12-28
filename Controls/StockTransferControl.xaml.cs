using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PharmaTrack.Controls
{
    public sealed partial class StockTransferControl : UserControl
    {
        public StockTransferControl()
        {
            this.InitializeComponent();
            this.Loaded += StockTransferControl_Loaded;
        }
        private void UPCInputBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Disable the button and update the status
            ScanBarcodeButton.IsEnabled = false;
            StatusText.Text = "Ready to Scan";
            StatusText.Foreground = new SolidColorBrush(Colors.Green);
        }

        private void UPCInputBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Enable the button and update the status
            ScanBarcodeButton.IsEnabled = true;
            StatusText.Text = "Not Ready for Scanning";
            StatusText.Foreground = new SolidColorBrush(Colors.Red);
        }
        private void StockTransferControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Set focus to the AutoSuggestBox
            UPCInputBox.Focus(FocusState.Programmatic);
        }
        private void ScanBarcodeButton_Click(object sender, RoutedEventArgs e)
        {
            UPCInputBox.Text = string.Empty;
            // Set focus to the AutoSuggestBox
            UPCInputBox.Focus(FocusState.Programmatic);
        }
        private void UPC_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            /*
            ContentDialog dialog = new ContentDialog();

            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = this.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "UPC";
            dialog.PrimaryButtonText = "OK";
            dialog.DefaultButton = ContentDialogButton.Close;
            await dialog.ShowAsync();
            */
            SubmitButton.Focus(FocusState.Programmatic);
        }
    }
}
