namespace InventoryScanner.Logging
{
    public interface IAppLogger<T>
    {
        void Error(Exception ex, LogContext context);
        void Info(LogContext context);
        void Warning(LogContext context);
    }
}