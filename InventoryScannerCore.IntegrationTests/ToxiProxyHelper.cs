using System.Net;
using System.Text;
using System.Text.Json;

namespace InventoryScannerCore.IntegrationTests
{
    public class ToxiProxyHelper
    {
        private const string toxiProxyUrl = "http://localhost:8474";

        public static async Task CreateProxyAsync(string proxyName, string listenAt, string upstreamAt)
        {
            using (var client = new HttpClient { BaseAddress = new Uri(toxiProxyUrl) })
            {
                var content = new StringContent(JsonSerializer.Serialize(new
                {
                    name = proxyName,
                    listen = listenAt,
                    upstream = upstreamAt,
                }), Encoding.UTF8, "application/json");

                var response = await client.PostAsync("/proxies", content);

                if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.Conflict)
                {
                    throw new Exception($"Failed to create proxy: {await response.Content.ReadAsStringAsync()}");
                }
            }
        }

        public static async Task CutConnectionAsync(string proxyName, string toxicName)
        {
            using (var client = new HttpClient { BaseAddress = new Uri(toxiProxyUrl) })
            {
                var content = new StringContent(JsonSerializer.Serialize(new
                {
                    name = toxicName,
                    type = "limit_data",
                    stream = "upstream",
                    toxicity = 1.0,
                    attributes = new { timeout = 1000 }
                }), Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"/proxies/{proxyName}/toxics", content);
                response.EnsureSuccessStatusCode();
            }
        }

        public static async Task RestoreConnectionAsync(string proxyName, string toxicName)
        {
            using (var client = new HttpClient { BaseAddress = new Uri(toxiProxyUrl) })
            {
                var response = await client.DeleteAsync($"/proxies/{proxyName}/toxics/{toxicName}");

                if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NotFound)
                {
                    throw new Exception("Failed to restore connection.");
                }
            }
        }

        public static async Task RemoveProxyAsync(string proxyName)
        {
            using (var client = new HttpClient { BaseAddress = new Uri(toxiProxyUrl) })
            {
                var response = await client.DeleteAsync($"/proxies/{proxyName}");

                if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.Conflict)
                {
                    throw new Exception($"Failed to create proxy: {await response.Content.ReadAsStringAsync()}");
                }
            }
        }

        public static async Task ClearAllProxiesAsync()
        {
            using (var client = new HttpClient { BaseAddress = new Uri(toxiProxyUrl) })
            {
                var response = await client.GetAsync("/proxies");
                response.EnsureSuccessStatusCode();

                var body = await response.Content.ReadAsStringAsync();
                var proxies = JsonSerializer.Deserialize<Dictionary<string, object>>(body);

                if (proxies != null)
                {
                    foreach (var proxyName in proxies.Keys)
                    {
                        var deleteResponse = await client.DeleteAsync($"/proxies/{proxyName}");
                        if (!deleteResponse.IsSuccessStatusCode && deleteResponse.StatusCode != HttpStatusCode.NotFound)
                        {
                            throw new Exception($"Failed to delete proxy '{proxyName}': {await deleteResponse.Content.ReadAsStringAsync()}");
                        }
                    }
                }
            }
        }

        public static async Task GetAllProxiesAsync()
        {
            using (var client = new HttpClient { BaseAddress = new Uri(toxiProxyUrl) })
            {
                var response = await client.GetAsync("/proxies");
                response.EnsureSuccessStatusCode();

                var body = await response.Content.ReadAsStringAsync();
                var proxies = JsonSerializer.Deserialize<Dictionary<string, object>>(body);

                if (proxies != null)
                {
                    foreach (var proxy in proxies)
                    {
                        Console.WriteLine($"Proxy Name: {proxy.Key}");
                        Console.WriteLine($"Proxy Details: {proxy.Value}");
                    }
                }
            }
        }
    }
}
