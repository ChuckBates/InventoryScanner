using InventoryScannerCore.Models;
using System.Text.Json;

namespace InventoryScannerCore.Lookups
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
                    var retrivedBarcode = JsonSerializer.Deserialize<Barcode>(body, options);

                    if (retrivedBarcode != null && retrivedBarcode.product != null)
                    {
                        retrivedBarcode.product.barcode = barcode.ToString();
                        return retrivedBarcode;
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
