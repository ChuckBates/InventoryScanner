using System.Drawing;

namespace InventoryScannerCore.Repositories
{
    public class ImageRepository : IImageRepository
    {
        public Image? Get(string imageUrl)
        {
            Image? image = null;
            try
            {
                image = Image.FromFile(imageUrl);
            }
            catch (Exception)
            {
                return null;
            }

            return image;
        }

        public bool Insert(Image testImage, string imageUrl)
        {
            try
            {
                testImage.Save(imageUrl);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public bool Delete(string imageUrl)
        {
            try
            {
                File.Delete(imageUrl);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
