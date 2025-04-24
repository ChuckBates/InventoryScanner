using InventoryScanner.Core.Enums;
using InventoryScanner.Core.Workflows;
using Microsoft.AspNetCore.Mvc;

namespace InventoryScanner.Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImageController(IImageWorkflow imageWorkflow) : ControllerBase
    {
        [HttpGet(Name = "GetImage")]
        public async Task<IActionResult> Get(string imagePath)
        {
            var workflowResponse = await imageWorkflow.Get(imagePath);
            if (workflowResponse.Status == WorkflowResponseStatus.Failure)
            {
                var error = "Error retrieving image data: " + string.Join(", ", workflowResponse.Errors);
                return NotFound(error);
            }

            var contentType = GetContentType(imagePath);
            return File(workflowResponse.Data, contentType);
        }

        private string GetContentType(string imagePath)
        {
            var extension = Path.GetExtension(imagePath).ToLowerInvariant();
            return extension switch
            {
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream"
            };
        }
    }
}
