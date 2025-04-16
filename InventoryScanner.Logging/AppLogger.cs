using Microsoft.Extensions.Logging;

namespace InventoryScanner.Logging
{
    public class AppLogger<T> : IAppLogger<T>
    {
        private readonly ILogger<T> logger;

        public AppLogger(ILogger<T> logger)
        {
            this.logger = logger;
        }

        public void Info(LogContext context)
        {
            logger.LogInformation("{@Context}", context);
        }

        public void Warning(LogContext context)
        {
            logger.LogWarning("{@Context}", context);
        }

        public void Error(Exception ex, LogContext context)
        {
            logger.LogError(ex, "{@Context}", context);
        }
    }
}
