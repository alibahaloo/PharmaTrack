using System;
using System.Windows;
using SQLitePCL;

namespace PharmaTrack.WPF
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize SQLitePCL
            Batteries.Init();
        }
    }
}
