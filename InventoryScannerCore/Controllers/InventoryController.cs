﻿using InventoryScannerCore.Enums;
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
        public InventoryControllerResponse GetAll()
        {
            var response = new InventoryControllerResponse(ControllerResponseStatus.Success, new List<Inventory>());

            try
            {
                var data = inventoryRepository.GetAll().ToList();
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
        public InventoryControllerResponse Get(string barcode)
        {
            var response = new InventoryControllerResponse(ControllerResponseStatus.Success, new List<Inventory>());

            try
            {
                var data = inventoryRepository.Get(barcode);
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
        public InventoryControllerResponse Add(Inventory inventory)
        {
            var response = new InventoryControllerResponse(ControllerResponseStatus.Success, new List<Inventory>());

            try
            {
                inventoryRepository.Insert(inventory);
                var saved = inventoryRepository.Get(inventory.Barcode);
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
