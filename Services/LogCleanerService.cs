using LoggerPlugin.Data;
using Serilog;
using System.IO;

namespace LogPlugin.Services
{
    public class CleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly Serilog.ILogger _infoLogger;
    private readonly Serilog.ILogger _errorLogger;
    private readonly int _logRetentionDays;

    public CleanupService(IServiceScopeFactory serviceScopeFactory, ILogger<CleanupService> logger, IConfiguration config)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logRetentionDays = config.GetValue<int>("LogRetentionDays");
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
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                try
                {
                    var cutoffDate = DateTime.UtcNow.AddDays(-1*_logRetentionDays);

                    if(context.LogEvents == null){
                        return;
                    }

                    var oldEntries = context.LogEvents
                        .Where(le => le.Timestamp < cutoffDate);

                    context.LogEvents.RemoveRange(oldEntries);

                    await context.SaveChangesAsync(stoppingToken);

                    // Log the successful cleanup
                    _infoLogger.Information($"Deleted old log entries at {DateTime.UtcNow}");
                }
                catch (Exception ex)
                {
                    // Log any errors during cleanup
                    _errorLogger.Error(ex, "An error occurred during cleanup");
                }

                try
                {
                    var logFilesPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
                    var logFiles = Directory.EnumerateFiles(logFilesPath);

                    foreach (var logFile in logFiles)
                    {
                        var creationTime = File.GetCreationTime(logFile);
                        if (creationTime < DateTime.UtcNow.AddDays(-1 * _logRetentionDays))
                        {
                            File.Delete(logFile);
                        }
                    }

                    _infoLogger.Information($"Deleted old log files at {DateTime.UtcNow}");
                }
                catch (Exception ex)
                {
                    _errorLogger.Error(ex, "An error occurred during cleanup");
                }
            }

            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }
}

}
