﻿using InventoryScanner.Core.Messages;
using InventoryScanner.Messaging.Publishing;

namespace InventoryScanner.Core.Publishers.Interfaces
{
    public interface IFetchInventoryMetadataRequestDeadLetterPublisher
    {
        Task<PublisherResponse> Publish(FetchInventoryMetadataMessage message);
    }
}
