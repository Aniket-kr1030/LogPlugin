using LoggerPlugin.Data;

namespace LogPlugin.Services
{
    public class DatabaseMaintenanceService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DatabaseMaintenanceService> _logger;

        public DatabaseMaintenanceService(ApplicationDbContext context, ILogger<DatabaseMaintenanceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void DeleteOldLogs()
        {
            try
            {
                var oldLogs = _context.LogEvents
                    .Where(log => log.Timestamp < DateTime.Now.AddMonths(-1));

                _context.LogEvents.RemoveRange(oldLogs);
                _context.SaveChanges();
                DeleteOldFiles();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting old logs and files.");
                throw;
            }
        }

        private void DeleteOldFiles()
        {
            

            try
            {
               
                var logFiles = Directory.EnumerateFiles("Logs", "*.txt");

                foreach (var logFile in logFiles)
                {
                    
                    var creationTime = File.GetCreationTime(logFile);

                    
                    if (creationTime < DateTime.Now.AddDays(-30))
                    {
                        
                        File.Delete(logFile);
                    }
                }
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"An error occurred while deleting old log files: {ex.Message}");
            }
        }
    }

}
