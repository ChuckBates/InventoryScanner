using InventoryScannerCore.Controllers;
using InventoryScannerCore.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InventoryScannerCore.IntegrationTests
{
    public class IntegrationTestDependencyHelper
    {
        public IServiceProvider? Provider { get; private set; }

        private ServiceCollection services = new();

        public async Task SpinUp()
        {
            AddSettingsService();

            Provider = services.BuildServiceProvider();

            await StartServicesAsync();
        }

        private async Task StartServicesAsync()
        {
            if (Provider == null)
            {
                throw new InvalidOperationException("Provider is not initialized.");
            }

            var hostedServices = Provider.GetServices<IHostedService>();
            foreach (var hostedService in hostedServices)
            {
                await hostedService.StartAsync(CancellationToken.None);
            }
        }

        private void AddSettingsService()
        {
            services.AddLogging(builder => builder.AddConsole());

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json");

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                config.AddUserSecrets<InventoryController>();
            }

            var configBuilder = config.Build();
            services.Configure<Settings.Settings>(configBuilder.GetSection("Settings"));
            services.AddScoped<ISettingsService, SettingsService>();
        }
    }
}
