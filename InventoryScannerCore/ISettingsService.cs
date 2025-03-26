namespace InventoryScannerCore
{
    public interface ISettingsService
    {
        string GetPostgresConnectionString();
        string GetRapidApiHost();
        string GetRapidApiKey();
    }
}