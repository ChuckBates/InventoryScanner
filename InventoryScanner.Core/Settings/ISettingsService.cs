namespace InventoryScanner.Core.Settings
{
    public interface ISettingsService
    {
        string GetPostgresConnectionString();
        RabbitMqSettings GetRabbitMqSettings();
        string GetRapidApiHost();
        string GetRapidApiKey();
    }
}