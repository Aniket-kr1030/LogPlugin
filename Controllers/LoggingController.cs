using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Events;
using LoggerPlugin.Models;
using LoggerPlugin.Data;
using Serilog.Formatting.Compact;

namespace LoggerPlugin.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoggingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly Serilog.ILogger _infoLogger;
        private readonly Serilog.ILogger _errorLogger;

        public LoggingController(ApplicationDbContext context)
        {
            _context = context;

            _infoLogger = new LoggerConfiguration()
                .WriteTo.File(new CompactJsonFormatter(), "Logs/info.txt", LogEventLevel.Information, rollingInterval: RollingInterval.Day)
                .CreateLogger();

            _errorLogger = new LoggerConfiguration()
                .WriteTo.File(new CompactJsonFormatter(), "Logs/error.txt", LogEventLevel.Error, rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        [HttpPost]
        public async Task<IActionResult> PostLogEntry([FromBody] LoggerPlugin.Models.LogEvent logEvent)
        {
            try
            {
                // Save to database
                _context.LogEvents.Add(logEvent);
                await _context.SaveChangesAsync();

                // Save to file based on the log level
                switch (logEvent.Level.ToLower())
                {
                    case "information":
                        _infoLogger.Information("{Timestamp} {Level} {UserInformation} {SystemInformation} {ProcessInformation} {Message}",
                            logEvent.Timestamp, logEvent.Level, logEvent.UserInformation, logEvent.SystemInformation, logEvent.ProcessInformation, logEvent.Message);
                        break;
                    case "error":
                        _errorLogger.Error("{Timestamp} {Level} {UserInformation} {SystemInformation} {ProcessInformation} {Message}",
                            logEvent.Timestamp, logEvent.Level, logEvent.UserInformation, logEvent.SystemInformation, logEvent.ProcessInformation, logEvent.Message);
                        break;
                    // Add more cases if needed
                    default:
                        break;
                }

                return Ok();
            }
            catch (Exception ex)
            {
                // Log the exception details
                _errorLogger.Error(ex, "An error occurred while processing the log entry");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
