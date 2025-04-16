namespace InventoryScanner.Logging
{
    public class LogContext
    {
        public required string Component { get; init; }
        public required string Operation { get; init; }
        public required string Message {  get; init; }
        public string? Barcode { get; init; }
    }
}
