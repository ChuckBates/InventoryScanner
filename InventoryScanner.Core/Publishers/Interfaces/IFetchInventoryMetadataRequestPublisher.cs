﻿using InventoryScanner.Messaging.Publishing;

namespace InventoryScanner.Core.Publishers.Interfaces
{
    public interface IFetchInventoryMetadataRequestPublisher
    {
        Task<PublisherResponse> Publish(string barcode);
    }
}
