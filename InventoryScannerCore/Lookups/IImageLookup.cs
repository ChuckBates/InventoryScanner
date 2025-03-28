
namespace InventoryScannerCore.Lookups
{
    public interface IImageLookup
    {
        Task<Stream?> Get(string imageUrl);
    }
}