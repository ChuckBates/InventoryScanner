namespace InventoryScanner.Messaging.Enums
{
    public enum PublisherResponseStatus
    {
        Success,
        Failure,
        Timeout,
        InvalidMessage,
        ConnectionError,
        UnknownError
    }
}
