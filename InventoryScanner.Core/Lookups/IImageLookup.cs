
namespace InventoryScanner.Core.Lookups
{
    public interface IImageLookup
    {
        Task<Stream?> Get(string imageUrl);
    }
}