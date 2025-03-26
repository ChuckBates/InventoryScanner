using InventoryScannerCore.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryScannerCore.IntegrationTests
{
    public class IntegrationTestHelper
    {
        public static SettingsService? GetSettingsService()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddOptions();

            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json");

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                configurationBuilder.AddUserSecrets<InventoryController>();
            }
            else
            {
                configurationBuilder.AddEnvironmentVariables();
            }

            var configuration = configurationBuilder.Build();

            serviceCollection.Configure<Settings>(configuration.GetSection("Settings"));
            serviceCollection.AddSingleton<SettingsService>();

            var serviceProvider = serviceCollection.BuildServiceProvider();
            
            return serviceProvider.GetService<SettingsService>();
        }
    }
}
