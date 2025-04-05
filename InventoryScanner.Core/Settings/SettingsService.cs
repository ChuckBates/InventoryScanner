using Microsoft.Extensions.Options;

namespace InventoryScannerCore.Settings
{
    public class SettingsService : ISettingsService
    {
        private Settings _settings;

        public SettingsService(IOptions<Settings> settings)
        {
            _settings = settings.Value;
        }

        public string GetPostgresConnectionString()
        {
            return $"Server={_settings.DatabaseServer};Port={_settings.DatabasePort};Database={_settings.DatabaseName};User Id={_settings.DatabaseUser};Password={_settings.DatabasePassword};";
        }

        public string GetRapidApiKey()
        {
            return _settings.RapidApiKey;
        }
        public string GetRapidApiHost()
        {
            return _settings.RapidApiHost;
        }

        public RabbitMqSettings GetRabbitMqSettings()
        {
            return _settings.RabbitMQ;
        }
    }

    public class Settings
    {
        public required string DatabaseServer { get; set; }
        public required int DatabasePort { get; set; }
        public required string DatabaseName { get; set; }
        public required string DatabaseUser { get; set; }
        public required string DatabasePassword { get; set; }
        public required string RapidApiKey { get; set; }
        public required string RapidApiHost { get; set; }
        public RabbitMqSettings RabbitMQ { get; set; }
    }
}
