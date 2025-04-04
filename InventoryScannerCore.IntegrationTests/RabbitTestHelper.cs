using Docker.DotNet.Models;
using Docker.DotNet;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

namespace InventoryScannerCore.IntegrationTests
{
    public static class RabbitTestHelper
    {
        public static async Task<List<T>> WaitForMessagesAsync<T>(
           IModel channel,
           string queueName,
           int expectedCount,
           int maxAttempts = 30,
           int delayMs = 100)
        {
            var messages = new List<T>();
            var attempts = 0;

            while (messages.Count < expectedCount && attempts < maxAttempts)
            {
                while (true)
                {
                    var result = channel.BasicGet(queueName, autoAck: true);
                    if (result == null) break;

                    var body = Encoding.UTF8.GetString(result.Body.ToArray());
                    var message = JsonSerializer.Deserialize<T>(body);
                    if (message != null)
                        messages.Add(message);
                }

                if (messages.Count < expectedCount)
                    await Task.Delay(delayMs);

                attempts++;
            }

            return messages;
        }
    }
}
