namespace InventoryScanner.Messaging.Interfaces
{
    public interface IMessagingStartupService
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
    }
}