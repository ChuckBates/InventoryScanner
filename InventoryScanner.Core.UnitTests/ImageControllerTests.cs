using InventoryScanner.Core.Controllers;
using InventoryScanner.Core.Workflows;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryScanner.Core.UnitTests
{
    [TestFixture]
    public class ImageControllerTests
    {
        private Mock<IImageWorkflow> imageWorkflow;
        private ImageController imageController;

        [SetUp]
        public void Setup()
        {
            imageWorkflow = new Mock<IImageWorkflow>();
            imageController = new ImageController(imageWorkflow.Object);
        }

        [Test]
        public async Task When_getting_an_image_successfully()
        {
            var imagePath = "path/to/image.jpg";
            var imageData = new byte[] { 1, 2, 3, 4, 5 };
            var workflowResponse = ImageWorkflowResponse.Success(imageData);

            imageWorkflow.Setup(x => x.Get(imagePath)).ReturnsAsync(workflowResponse);

            var result = await imageController.Get(imagePath);

            Assert.That(result, Is.InstanceOf<FileContentResult>());

            var fileResult = result as FileContentResult;
            Assert.That(fileResult, Is.Not.Null);
            Assert.That(fileResult.ContentType, Is.EqualTo("image/jpeg"));
            Assert.That(fileResult.FileContents, Is.EqualTo(imageData));
        }

        [Test]
        public async Task When_getting_an_image_unsuccessfully()
        {
            var imagePath = "path/to/image.jpg";
            var errorMessage = "Image not found";
            var workflowResponse = ImageWorkflowResponse.Failure(errorMessage);

            imageWorkflow.Setup(x => x.Get(imagePath)).ReturnsAsync(workflowResponse);

            var result = await imageController.Get(imagePath);

            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());

            var fileResult = result as NotFoundObjectResult;
            Assert.That(fileResult, Is.Not.Null);
            Assert.That(fileResult.StatusCode, Is.EqualTo(404));
            Assert.That(fileResult.Value, Is.EqualTo("Error retrieving image data: " + errorMessage));
        }
    }
}
