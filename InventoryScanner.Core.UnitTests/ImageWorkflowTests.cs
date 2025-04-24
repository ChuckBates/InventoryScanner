using InventoryScanner.Core.Enums;
using InventoryScanner.Core.Repositories;
using InventoryScanner.Core.Workflows;
using InventoryScanner.Logging;
using Moq;

namespace InventoryScanner.Core.UnitTests
{
    [TestFixture]
    public class ImageWorkflowTests
    {
        private Mock<IAppLogger<ImageWorkflow>> mockLogger;
        private Mock<IImageRepository> mockImageRepository;
        private ImageWorkflow imageWorkflow;

        [SetUp]
        public void Setup()
        {
            mockLogger = new Mock<IAppLogger<ImageWorkflow>>();
            mockImageRepository = new Mock<IImageRepository>();
            imageWorkflow = new ImageWorkflow(mockImageRepository.Object, mockLogger.Object);
        }

        [Test]
        public async Task When_getting_an_image_successfully()
        {
            var imagePath = "path/to/image.jpg";
            var imageData = new byte[] { 1, 2, 3, 4, 5 };

            mockImageRepository.Setup(repo => repo.Get(imagePath)).ReturnsAsync(imageData);

            var result = await imageWorkflow.Get(imagePath);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(WorkflowResponseStatus.Success));
            Assert.That(result.Data, Is.EqualTo(imageData));
            Assert.That(result.Errors, Is.Empty);

            mockImageRepository.Verify(repo => repo.Get(imagePath), Times.Once);
            mockLogger.Verify(logger => logger.Warning(It.IsAny<LogContext>()), Times.Never);
        }

        [Test]
        public async Task When_getting_an_image_that_is_not_found()
        {
            var imagePath = "path/to/unknown/image.jpg";

            mockImageRepository.Setup(repo => repo.Get(imagePath)).ReturnsAsync(null as byte[]);
            mockLogger.Setup(logger => logger.Warning(It.IsAny<LogContext>()));

            var result = await imageWorkflow.Get(imagePath);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(WorkflowResponseStatus.Failure));
            Assert.That(result.Data, Is.Empty);
            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.That(result.Errors[0], Is.EqualTo($"Image {imagePath} not found."));

            mockImageRepository.Verify(repo => repo.Get(imagePath), Times.Once);
            mockLogger.Verify(logger => logger.Warning(It.Is<LogContext>(log => log.Message == $"Image {imagePath} not found.")), Times.Once);
        }

        [Test]
        public async Task When_getting_an_image_and_the_repo_throws()
        {
            var imagePath = "path/to/unknown/image.jpg";

            mockImageRepository.Setup(repo => repo.Get(imagePath)).ThrowsAsync(new Exception("Image Repository Error"));
            mockLogger.Setup(logger => logger.Error(It.IsAny<Exception>(), It.IsAny<LogContext>()));

            var result = await imageWorkflow.Get(imagePath);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(WorkflowResponseStatus.Failure));
            Assert.That(result.Data, Is.Empty);
            Assert.That(result.Errors, Has.Count.EqualTo(1));
            Assert.That(result.Errors[0], Is.EqualTo($"Error retrieving image {imagePath}."));

            mockImageRepository.Verify(repo => repo.Get(imagePath), Times.Once);
            mockLogger.Verify(logger => logger.Error(
                It.Is<Exception>(e => e.Message == "Image Repository Error"), 
                It.Is<LogContext>(log => log.Message == $"Error retrieving image {imagePath}.")), Times.Once);
        }
    }
}
