using InventoryScannerCore.Enums;
using InventoryScannerCore.Models;
using InventoryScannerCore.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace InventoryScannerCore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InventoryController(IInventoryRepository inventoryRepository) : Controller
    {
        [HttpGet(Name = "GetAllInventory")]
        public async Task<InventoryControllerResponse> GetAll()
        {
            var response = new InventoryControllerResponse(ControllerResponseStatus.Success, new List<Inventory>());

            try
            {
                var data = (await inventoryRepository.GetAll()).ToList();
                response.Data = data;
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
            var response = new InventoryControllerResponse(ControllerResponseStatus.Success, new List<Inventory>());

            try
            {
                var data = await inventoryRepository.Get(barcode);
                if (data == null)
                {
                    response.Status = ControllerResponseStatus.NotFound;
                    return response;
                }

                response.Data.Add(data);
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
            var response = new InventoryControllerResponse(ControllerResponseStatus.Success, new List<Inventory>());

            try
            {
                await inventoryRepository.Insert(inventory);
                var saved = await inventoryRepository.Get(inventory.Barcode);
                if (saved != null)
                {
                    response.Data.Add(saved);
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
    }
}
