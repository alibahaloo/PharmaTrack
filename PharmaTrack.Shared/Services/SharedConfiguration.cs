using Microsoft.Extensions.Configuration;

namespace PharmaTrack.Shared.Services
{
    public static class SharedConfiguration
    {
        public static IConfiguration GetSharedConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("shared.appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }
    }
}
