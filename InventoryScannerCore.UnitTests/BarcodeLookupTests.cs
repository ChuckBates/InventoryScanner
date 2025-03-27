using InventoryScannerCore.Models;
using Moq;
using Moq.Protected;
using System.Text.Json;

namespace InventoryScannerCore.UnitTests
{
    [TestFixture]
    public class BarcodeLookupTests
    {
        Mock<HttpMessageHandler> mockHttpMessageHandler;
        Mock<ISettingsService> mockSettingsService;
        BarcodeLookup barcodeLookup;

        [SetUp]
        public void Setup()
        {
            mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockSettingsService = new Mock<ISettingsService>();
            barcodeLookup = new BarcodeLookup(mockSettingsService.Object, new HttpClient(mockHttpMessageHandler.Object));
        }

        [Test]
        public async Task When_lookng_up_a_barcode()
        {
            var barcode = "0036800902176";
            var expected = new Barcode
            {
                product = new BarcodeProduct
                {
                    barcode = barcode,
                    title = "Homogenized Evaporated Milk",
                    description = "Homogenized Evaporated Milk. Serving size: 2 Tbsp (30 ml). Country of origin: United States.",
                    images = ["https://images.barcodelookup.com/9054/90542079-1.jpg"]
                }
            };
            var message = new HttpResponseMessage
            {
                Content = new StringContent(JsonSerializer.Serialize(expected))
            };

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(message));
            mockSettingsService
                .Setup(x => x.GetRapidApiHost())
                .Returns("example.com");
            mockSettingsService
                .Setup(x => x.GetRapidApiKey())
                .Returns("key");

            var actual = await barcodeLookup.Get(barcode);

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.product.barcode, Is.EqualTo(expected.product.barcode));
            Assert.That(actual.product.title, Is.EqualTo(expected.product.title));
            Assert.That(actual.product.description, Is.EqualTo(expected.product.description));
            Assert.That(actual.product.images, Is.EqualTo(expected.product.images));
        }

        [Test]
        public void When_looking_up_a_barcode_and_the_http_request_fails()
        {
            var barcode = "0036800902176";

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Throws(new Exception("An error occurred."));
            mockSettingsService
                .Setup(x => x.GetRapidApiHost())
                .Returns("example.com");
            mockSettingsService
                .Setup(x => x.GetRapidApiKey())
                .Returns("key");

            var actual = barcodeLookup.Get(barcode).Result;

            Assert.That(actual.product, Is.Null);
        }

        [Test]
        public void When_looking_up_a_barcode_and_the_deserialization_fails()
        {
            var barcode = "0036800902176";

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    Content = new StringContent("invalid json")
                });
            mockSettingsService
                .Setup(x => x.GetRapidApiHost())
                .Returns("example.com");
            mockSettingsService
                .Setup(x => x.GetRapidApiKey())
                .Returns("key");

            var actual = barcodeLookup.Get(barcode).Result;

            Assert.That(actual.product, Is.Null);
        }
    }
}
