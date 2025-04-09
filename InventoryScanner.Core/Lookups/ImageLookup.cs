using InventoryScanner.Core.Settings;

namespace InventoryScanner.Core.Lookups
{
    public class ImageLookup : IImageLookup
    {
        HttpClient client;
        ISettingsService settings;

        public ImageLookup(ISettingsService settings)
        {
            this.settings = settings;
            this.client = new HttpClient();
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
