using InventoryScanner.Core.Enums;
using InventoryScanner.Core.Repositories;
using InventoryScanner.Logging;

namespace InventoryScanner.Core.Workflows
{
    public class ImageWorkflow : IImageWorkflow
    {
		private readonly IImageRepository imageRepository;
        private readonly IAppLogger<ImageWorkflow> logger;

        public ImageWorkflow(IImageRepository imageRepository, IAppLogger<ImageWorkflow> logger)
        {
            this.imageRepository = imageRepository;
            this.logger = logger;
        }

        public async Task<ImageWorkflowResponse> Get(string imagePath)
        {
            var response = ImageWorkflowResponse.Success([]);
            try
			{
				var imageData = await imageRepository.Get(imagePath);
                if (imageData == null)
                {
                    var message = $"Image {imagePath} not found.";
                    logger.Warning(new LogContext
                    { 
                        Component = nameof(ImageWorkflow),
                        Message = message,
                        Operation = "Get"
                    });
                    response.Status = WorkflowResponseStatus.Failure;
                    response.Data = [];
                    response.Errors.Add(message);
                    return response;
                }
                
                response.Data = imageData;
                return response;
            }
			catch (Exception e)
            {
                var message = $"Error retrieving image {imagePath}.";
                logger.Error(e, new LogContext
                {
                    Component = nameof(ImageWorkflow),
                    Message = message,
                    Operation = "Get"
                });

                response.Status = WorkflowResponseStatus.Failure;
                response.Data = [];
                response.Errors.Add(message);
                return response;
            }
        }
    }
}
