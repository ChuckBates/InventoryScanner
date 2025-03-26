using System.Drawing;

namespace InventoryScannerCore.Repositories
{
    public interface IImageRepository
    {
        bool Delete(string imageUrl);
        Image? Get(string imageUrl);
        bool Insert(Image testImage, string imageUrl);
    }
}