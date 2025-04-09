using InventoryScanner.Core.Enums;
using InventoryScanner.Core.Models;
using InventoryScanner.Core.Publishers;
using InventoryScanner.Core.Repositories;
using InventoryScanner.Core.Workflows;

namespace InventoryScanner.Core.UnitTests
{
    public class InventoryWorkflow : IInventoryWorkflow
    {
        private IInventoryRepository inventoryRepository;
        private IFetchInventoryMetadataRequestPublisher fetchInventoryMetadataRequestPublisher;

        public InventoryWorkflow(IInventoryRepository inventoryRepository, IFetchInventoryMetadataRequestPublisher fetchInventoryMetadataRequestPublisher)
        {
            this.inventoryRepository = inventoryRepository;
            this.fetchInventoryMetadataRequestPublisher = fetchInventoryMetadataRequestPublisher;
        }

        public async Task<InventoryWorkflowResponse> Get(string barcode)
        {
            var response = new InventoryWorkflowResponse(WorkflowResponseStatus.Success, [], []);
            try
            {
                var inventory = await inventoryRepository.Get(barcode);
                if (inventory == null) {
                    response.Status = WorkflowResponseStatus.Failure;
                    response.Errors.Add("Error looking up barcode: Inventory not found.");
                }
                else
                {
                    response.Data.Add(inventory);
                }
            }
            catch (Exception)
            {
                response.Status = WorkflowResponseStatus.Failure;
                response.Errors.Add("Error looking up barcode: Failed to retrieve inventory.");
            }

            return response;
        }

        public async Task<InventoryWorkflowResponse> GetAll()
        {
            var response = new InventoryWorkflowResponse(WorkflowResponseStatus.Success, [], []);
            try
            {
                var inventories = (await inventoryRepository.GetAll())?.ToList();
                if (inventories == null || inventories.Count == 0)
                {
                    response.Status = WorkflowResponseStatus.Failure;
                    response.Errors.Add("Error looking up all inventory: Inventory not found.");
                }
                else
                {
                    response.Data.AddRange(inventories);
                }
            }
            catch (Exception)
            {
                response.Status = WorkflowResponseStatus.Failure;
                response.Errors.Add("Error looking up all inventory: Failed to retrieve inventory.");
            }

            return response;
        }

        public async Task<InventoryWorkflowResponse> Add(Inventory inventory)
        {
            var response = new InventoryWorkflowResponse(WorkflowResponseStatus.Success, [], []);

            var rowsAffected = await inventoryRepository.Insert(inventory);
            if (rowsAffected == 0)
            {
                response.Status = WorkflowResponseStatus.Failure;
                response.Data.Clear();
                response.Errors.Add($"Error saving barcode {inventory.Barcode}: Failed to update inventory.");
            }
            else
            {
                response.Data.Add(inventory);
            }

            if (rowsAffected > 0)
            {
                var publishResponse = await fetchInventoryMetadataRequestPublisher.PublishRequest(inventory.Barcode);
                if (publishResponse.Status == Messaging.Enums.PublisherResponseStatus.Failure)
                {
                    response.Errors.AddRange(publishResponse.Errors);
                }
            }

            return response;
        }

        public async Task<InventoryWorkflowResponse> Update(Inventory inventory, bool refetch = false)
        {
            var response = new InventoryWorkflowResponse(WorkflowResponseStatus.Success, [], []);
            if (refetch)
            {
                var publishResponse = await fetchInventoryMetadataRequestPublisher.PublishRequest(inventory.Barcode);
                if (publishResponse.Status == Messaging.Enums.PublisherResponseStatus.Failure)
                {
                    response.Errors.AddRange(publishResponse.Errors);
                }
            }

            var rowsAffected = await inventoryRepository.Insert(inventory);
            if (rowsAffected == 0)
            {
                response.Status = WorkflowResponseStatus.Failure;
                response.Data.Clear();
                response.Errors.Add($"Error updating barcode {inventory.Barcode}: Failed to update inventory.");
                return response;
            }

            var updatedInventoy = await inventoryRepository.Get(inventory.Barcode);
            if (updatedInventoy == null)
            {
                response.Status = WorkflowResponseStatus.Failure;
                response.Errors.Add("Error looking up barcode: Failed to retrieve updated inventory.");
            }
            else
            {
                response.Data.Add(updatedInventoy);
            }

            return response;
        }
    }
}