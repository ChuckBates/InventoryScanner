using System.Drawing;

namespace InventoryScannerCore.Repositories
{
    public interface IImageRepository
    {
        bool Delete(string imageUrl);
        Task<byte[]?> Get(string imagePath);
        Task<string> Insert(Stream imageStream, string imageSavePath);
    }
}