using InventoryScanner.Core.Models;
using InventoryScanner.Core.Settings;
using System.Text.Json;

namespace InventoryScanner.Core.Lookups
{
    public class BarcodeLookup : IBarcodeLookup
    {
        HttpClient client;
        ISettingsService settings;

        public BarcodeLookup(ISettingsService settings, HttpClient client)
        {
            this.settings = settings;
            this.client = client;
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
