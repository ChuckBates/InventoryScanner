using Moq;
using Moq.Protected;

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
