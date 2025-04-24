using InventoryScanner.Core.Messages;
using InventoryScanner.Messaging.Publishing;

namespace InventoryScanner.Core.Publishers.Interfaces
{
    public interface IInventoryUpdatedDeadLetterPublisher
    {
        Task<PublisherResponse> Publish(InventoryUpdatedMessage message);
    }
}