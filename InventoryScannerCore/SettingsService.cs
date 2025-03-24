using Microsoft.Extensions.Options;

namespace InventoryScannerCore
{
    public class SettingsService
    {
        private Settings _settings;

        public SettingsService(IOptions<Settings> settings)
        {
            _settings = settings.Value;
        }

        public string GetConnectionString()
        {
            return $"Server={_settings.DatabaseServer};Port={_settings.DatabasePort};Database={_settings.DatabaseName};User Id={_settings.DatabaseUser};Password={_settings.DatabasePassword};";
        }
    }

    public class Settings
    {
        public required string DatabaseServer { get; set; }
        public required int DatabasePort { get; set; }
        public required string DatabaseName { get; set; }
        public required string DatabaseUser { get; set; }
        public required string DatabasePassword { get; set; }   
    }
}
