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
        private IServiceProvider _serviceProvider = default!;
        private SplashWindow _splash = default!;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1) Show splash immediately
            _splash = new SplashWindow();
            _splash.Show();

            // 2) Init SQLitePCL
            SQLitePCL.Batteries.Init();

            // 3) Configure & build DI container
            var serviceCollection = new ServiceCollection();
            var configuration = BuildConfiguration();
            ConfigureServices(serviceCollection, configuration);
            _serviceProvider = serviceCollection.BuildServiceProvider();

            // 4) Run your long‑running VM init BEFORE showing MainWindow
            var mainVm = _serviceProvider.GetRequiredService<MainWindowViewModel>();
            await mainVm.InitializeAsync();

            // 5) Create & show MainWindow, then close splash
            var mainWindow = new MainWindow(mainVm);
            mainWindow.Show();

            _splash.Close();
        }

        private IConfiguration BuildConfiguration()
            => new ConfigurationBuilder()
                  .SetBasePath(Directory.GetCurrentDirectory())
                  .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                  .Build();

        private void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Register configuration
            services.AddSingleton(configuration);

            // Register HttpClient and AuthService
            services.AddHttpClient<AuthService>();
            services.AddHttpClient<InventoryService>();
            services.AddHttpClient<UsersService>();
            services.AddHttpClient<ScheduleService>();
            services.AddHttpClient<DrugService>();

            // ViewModels
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<LoginViewModel>();
            services.AddSingleton<StockTransferViewModel>();
            services.AddSingleton<InventoryViewModel>();
            services.AddSingleton<TransactionsViewModel>();
            services.AddSingleton<UsersViewModel>();
            services.AddSingleton<CalendarControlViewModel>();
            services.AddSingleton<ScheduleControlViewModel>();
            services.AddSingleton<DrugListViewModel>();
            services.AddSingleton<IngredientListViewModel>();
            services.AddSingleton<DrugInteractionViewModel>();
            services.AddSingleton<IngredientInteractionViewModel>();

            // Views / Controls
            services.AddSingleton<MainWindow>();
            services.AddSingleton<LoginControl>();
            services.AddSingleton<StockTransferControl>();
            services.AddSingleton<InventoryControl>();
            services.AddSingleton<TransactionsControl>();
            services.AddSingleton<UsersControl>();
            services.AddSingleton<CalendarControl>();
            services.AddSingleton<ScheduleControl>();
            services.AddSingleton<DrugListControl>();
            services.AddSingleton<IngredientListControl>();
            services.AddSingleton<DrugInteractionControl>();
            services.AddSingleton<IngredientInteractionControl>();
            services.AddSingleton<ProductControl>();
        }
    }
}
