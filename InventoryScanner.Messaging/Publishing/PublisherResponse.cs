using InventoryScanner.Messaging.Enums;
using InventoryScanner.Messaging.Models;

namespace InventoryScanner.Messaging.Publishing
{
    public class PublisherResponse : IPublisherResponse
    {
        public PublisherResponseStatus Status { get; set; }

        public List<string> Errors { get; } = [];
        IReadOnlyList<string> IPublisherResponse.Errors => Errors;

        public List<IRabbitMqMessage> Data { get; } = [];
        IReadOnlyList<IRabbitMqMessage> IPublisherResponse.Data => Data;

        public PublisherResponse()
        {
            Status = PublisherResponseStatus.Success;
        }

        public PublisherResponse(PublisherResponseStatus status, List<IRabbitMqMessage>? data = null, IEnumerable<string>? errors = null)
        {
            Status = status;
            if (data != null)
                Data.AddRange(data);
            if (errors != null)
                Errors.AddRange(errors);
        }

        public static PublisherResponse Success(List<IRabbitMqMessage>? data = null)
        {
            return new(PublisherResponseStatus.Success, data);
        }

        public static PublisherResponse Failed(string[] errors, List<IRabbitMqMessage>? data = null)
        {
            return new PublisherResponse(PublisherResponseStatus.Failure, data, errors);
        }
    }
}
