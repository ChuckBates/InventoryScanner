using System.Drawing;

namespace InventoryScannerCore.Repositories
{
    public class ImageRepository : IImageRepository
    {
        public async Task<byte[]?> Get(string imagePath)
        {
            byte[]? image = null;
            try
            {
                image = await File.ReadAllBytesAsync(imagePath);
            }
            catch (Exception)
            {
                return null;
            }

            return image;
        }

        public async Task<string> Insert(Stream imageStream, string imageSavePath)
        {
            try
            {
                using (var fileStream = new FileStream(imageSavePath, FileMode.Create))
                {
                    await imageStream.CopyToAsync(fileStream);
                }

                return "success";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public bool Delete(string imagePath)
        {
            try
            {
                File.Delete(imagePath);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
