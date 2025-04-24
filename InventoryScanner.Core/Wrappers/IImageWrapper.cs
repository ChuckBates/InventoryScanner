namespace InventoryScanner.Core.Wrappers
{
    public interface IImageWrapper
    {
        Task<Stream?> Get(string imageUrl);
    }
}