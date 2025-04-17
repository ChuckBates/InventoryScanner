using InventoryScanner.Core.Enums;
using InventoryScanner.Core.Models;
using InventoryScanner.Core.Workflows;
using Microsoft.AspNetCore.Mvc;

namespace InventoryScanner.Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InventoryController(IInventoryWorkflow inventoryWorkflow) : Controller
    {
        [HttpGet(Name = "GetAllInventory")]
        public async Task<InventoryControllerPaginatedResponse> GetAll(DateTime? since = null, int page = 1, int pageSize = 50)
        {
            var response = new InventoryControllerPaginatedResponse(ControllerResponseStatus.Success, []);
            var beginning = since ?? DateTime.MinValue;

            try
            {
                var workflowResponse = await inventoryWorkflow.GetAll(beginning, page, pageSize);
                if (workflowResponse.Status == WorkflowResponseStatus.Failure)
                {
                    response.Status = ControllerResponseStatus.Error;
                    response.Error = "Error retrieving inventory data: " + string.Join(", ", workflowResponse.Errors);
                    return response;
                }

                response.Page = page;
                response.PageSize = pageSize;

                if (workflowResponse.Data.Count > pageSize)
                {
                    response.HasMore = true;
                    response.Data = workflowResponse.Data.Take(pageSize).ToList();
                }
                else
                {
                    response.HasMore = false;
                    response.Data = workflowResponse.Data;
                }
            }
            catch (Exception e)
            {
                response.Status = ControllerResponseStatus.Error;
                response.Error = "Error retrieving inventory data: " + e.Message;
                return response;
            }

            return response;
        }

        [HttpGet("{barcode}", Name = "GetInventory")]
        public async Task<InventoryControllerResponse> Get(string barcode)
        {
            var response = new InventoryControllerResponse(ControllerResponseStatus.Success, []);

            try
            {
                var workflowResponse = await inventoryWorkflow.Get(barcode);
                if (workflowResponse.Status == WorkflowResponseStatus.Failure)
                {
                    response.Status = ControllerResponseStatus.Error;
                    response.Error = "Error retrieving inventory data: " + string.Join(", ", workflowResponse.Errors);
                    return response;
                }

                if (workflowResponse.Data.Count == 0)
                {
                    response.Status = ControllerResponseStatus.NotFound;
                    return response;
                }

                response.Data.AddRange(workflowResponse.Data);
            }
            catch (Exception e)
            {
                response.Status = ControllerResponseStatus.Error;
                response.Error = "Error retrieving inventory data: " + e.Message;
                return response;
            }

            return response;
        }

        [HttpPost(Name = "AddInventory")]
        public async Task<InventoryControllerResponse> Add(Inventory inventory)
        {
            var response = new InventoryControllerResponse(ControllerResponseStatus.Success, []);

            try
            {
                var workflowResponse = await inventoryWorkflow.Add(inventory);
                if (workflowResponse.Status == WorkflowResponseStatus.Success)
                {
                    response.Data.AddRange(workflowResponse.Data);
                }
                else
                {
                    response.Status = ControllerResponseStatus.Error;
                    response.Error = "Error adding inventory data: " + string.Join(", ", workflowResponse.Errors);
                }
            }
            catch (Exception e)
            {
                response.Status = ControllerResponseStatus.Error;
                response.Error = "Error adding inventory data: " + e.Message;
                return response;
            }

            return response;
        }

        [HttpPut(Name = "UpdateInventory")]
        public async Task<InventoryControllerResponse> Update(Inventory inventory, bool refetch)
        {
            var response = new InventoryControllerResponse(ControllerResponseStatus.Success, []);

            try
            {
                var workflowResponse = await inventoryWorkflow.Update(inventory, refetch);
                if (workflowResponse.Status == WorkflowResponseStatus.Success)
                {
                    if (workflowResponse.Data.Count == 0)
                    {
                        response.Status = ControllerResponseStatus.NotFound;
                        return response;
                    }
                    response.Data.AddRange(workflowResponse.Data);
                }
                else
                {
                    response.Status = ControllerResponseStatus.Error;
                    response.Error = "Error updating inventory data: " + string.Join(", ", workflowResponse.Errors);
                }
            }
            catch (Exception e)
            {
                response.Status = ControllerResponseStatus.Error;
                response.Error = "Error updating inventory data: " + e.Message;
                return response;
            }

            return response;
        }
    }
}
