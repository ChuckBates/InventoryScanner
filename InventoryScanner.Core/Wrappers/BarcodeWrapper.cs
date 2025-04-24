using InventoryScanner.Core.Models;
using InventoryScanner.Core.Settings;
using System.Runtime.CompilerServices;
using System.Text.Json;

[assembly: InternalsVisibleTo("InventoryScanner.Core.UnitTests")]

namespace InventoryScanner.Core.Wrappers
{
    public class BarcodeWrapper : IBarcodeWrapper
    {
        internal HttpClient client;
        private readonly ISettingsService settings;

        public BarcodeWrapper(ISettingsService settings)
        {
            this.settings = settings;
            client = new HttpClient();
        }

        public async Task<Barcode> Get(string barcode)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://{settings.GetRapidApiHost()}/?query={barcode}"),
                Headers =
                {
                    { "x-rapidapi-key", $"{settings.GetRapidApiKey()}" },
                    { "x-rapidapi-host", $"{settings.GetRapidApiHost()}" }
                }
            };

            try
            {
                using (var response = await client.SendAsync(request))
                {
                    var body = await response.Content.ReadAsStringAsync();

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        ReadCommentHandling = JsonCommentHandling.Skip,
                        AllowTrailingCommas = true
                    };
                    var retrievedResult = JsonSerializer.Deserialize<Barcode>(body, options);

                    if (retrievedResult != null && retrievedResult.product != null)
                    {
                        retrievedResult.product.barcode = barcode.ToString();
                        return retrievedResult;
                    }
                    else if (retrievedResult != null && retrievedResult.results.Length > 0)
                    {
                        retrievedResult.product = new BarcodeProduct
                        {
                            barcode = barcode.ToString(),
                            title = retrievedResult.results[0].title,
                            description = string.Empty,
                            images = [retrievedResult.results[0].image]
                        };
                        retrievedResult.results = [];
                        return retrievedResult;
                    }

                    return new Barcode();
                }
            }
            catch (Exception)
            {
                return new Barcode();
            }
        }
    }
}
