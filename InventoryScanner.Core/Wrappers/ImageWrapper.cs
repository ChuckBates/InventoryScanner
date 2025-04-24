using InventoryScanner.Core.Settings;

namespace InventoryScanner.Core.Wrappers
{
    public class ImageWrapper : IImageWrapper
    {
        HttpClient client;
        ISettingsService settings;

        public ImageWrapper(ISettingsService settings)
        {
            this.settings = settings;
            client = new HttpClient();
        }

        public async Task<Stream?> Get(string imageUrl)
        {
            var newClient = new HttpClient();
            try
            {
                var response = await newClient.GetAsync(imageUrl);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var stream = await response.Content.ReadAsStreamAsync();
                var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                return memoryStream;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
