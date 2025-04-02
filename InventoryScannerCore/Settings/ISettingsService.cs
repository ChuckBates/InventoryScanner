namespace InventoryScannerCore.Settings
{
    public interface ISettingsService
    {
        string GetPostgresConnectionString();
        string GetRapidApiHost();
        string GetRapidApiKey();
    }
}