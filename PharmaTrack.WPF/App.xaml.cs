using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PharmaTrack.WPF.Controls;
using PharmaTrack.WPF.Helpers;
using PharmaTrack.WPF.ViewModels;
using System.IO;
using System.Windows;

namespace PharmaTrack.WPF
{
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize SQLitePCL
            SQLitePCL.Batteries.Init();

            // Configure Services
            var serviceCollection = new ServiceCollection();
            var configuration = BuildConfiguration();
            ConfigureServices(serviceCollection, configuration);

            _serviceProvider = serviceCollection.BuildServiceProvider();

            // Show Main Window
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private IConfiguration BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        private void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Register configuration
            services.AddSingleton(configuration);

            // Register HttpClient and AuthService
            services.AddHttpClient<AuthService>();
            services.AddHttpClient<InventoryService>();
            services.AddHttpClient<UsersService>();
            services.AddHttpClient<ScheduleService>();

            services.AddSingleton<MainWindowViewModel>(); // Register MainWindowViewModel
            // Register Main Window
            services.AddSingleton<MainWindow>();

            // Register ViewModels
            services.AddSingleton<LoginViewModel>();
            services.AddSingleton<StockTransferViewModel>();
            services.AddSingleton<InventoryViewModel>();
            services.AddSingleton<TransactionsViewModel>();
            services.AddSingleton<UsersViewModel>();
            services.AddSingleton<CalendarControlViewModel>();
            services.AddSingleton<ScheduleControlViewModel>();

            // Register Controls
            services.AddSingleton<LoginControl>();
            services.AddSingleton<StockTransferControl>();
            services.AddSingleton<InventoryControl>();
            services.AddSingleton<TransactionsControl>();
            services.AddSingleton<UsersControl>();
            services.AddSingleton<CalendarControl>();
            services.AddSingleton<ScheduleControl>();

            services.AddSingleton<ProductControl>();
        }
    }
}
