using System.Drawing;

namespace InventoryScanner.Core.Repositories
{
    public interface IImageRepository
    {
        bool Delete(string imageUrl);
        Task<byte[]?> Get(string imagePath);
        Task<string> Insert(Stream imageStream, string imageSavePath);
    }
}