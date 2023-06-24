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
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var cutoffDate = DateTime.UtcNow.AddDays(-30);
                    var oldEntries = _context.LogEvents
                        .Where(le => le.Timestamp < cutoffDate);

                    _context.LogEvents.RemoveRange(oldEntries);

                    await _context.SaveChangesAsync();

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


