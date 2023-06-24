using LoggerPlugin.Data;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog;

namespace LogPlugin.Services
{
    public class CleanupService : BackgroundService
    {
        private readonly ApplicationDbContext _context;
        private readonly Serilog.ILogger _infoLogger;
        private readonly Serilog.ILogger _errorLogger;

        public CleanupService(ApplicationDbContext context, ILogger<CleanupService> logger)
        {
            _context = context;

            _infoLogger = new LoggerConfiguration()
                .WriteTo.File("Logs/info.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            _errorLogger = new LoggerConfiguration()
                .WriteTo.File("Logs/error.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var cutoffDate = DateTime.UtcNow.AddDays(-30);

                    if(_context.LogEvents == null){
                        return;
                    }
                    var oldEntries = _context.LogEvents
                        .Where(le => le.Timestamp < cutoffDate);

                    _context.LogEvents.RemoveRange(oldEntries);

                    await _context.SaveChangesAsync(stoppingToken);

                    // Log the successful cleanup
                    _infoLogger.Information($"Deleted old log entries at {DateTime.UtcNow}");
                }
                catch (Exception ex)
                {
                    // Log any errors during cleanup
                    _errorLogger.Error(ex, "An error occurred during cleanup");
                }

                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}
