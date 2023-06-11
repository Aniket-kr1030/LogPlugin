using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using LoggerPlugin.Models;
using LoggerPlugin.Data;

namespace LoggerPlugin.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoggingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LoggingController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> PostLogEntry([FromBody] LogEvent logEvent)
        {
            try
            {
                // Save to database
                _context.LogEvents.Add(logEvent);
                await _context.SaveChangesAsync();

                // Save to file
                var log = new LoggerConfiguration()
                    .WriteTo.File("Logs/logs.txt", rollingInterval: RollingInterval.Day)
                    .CreateLogger();
                log.Information("{Timestamp} {Level} {UserInformation} {SystemInformation} {ProcessInformation} {Message}",
                    logEvent.Timestamp, logEvent.Level, logEvent.UserInformation, logEvent.SystemInformation, logEvent.ProcessInformation, logEvent.Message);

                return Ok();
            }
            catch (Exception ex)
            {
                // Log the exception details
                Log.Error(ex, "An error occurred while processing the log entry");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
