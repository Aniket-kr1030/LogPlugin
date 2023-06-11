using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using LoggerPlugin.Models;
using LoggerPlugin.Data;
using Microsoft.Extensions.Logging;

namespace LoggerPlugin.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoggingController : ControllerBase
    {private readonly ApplicationDbContext _context;
private readonly ILogger<LoggingController> _logger;

public LoggingController(ApplicationDbContext context, ILogger<LoggingController> logger)
{
    _context = context;
    _logger = logger;
}

[HttpPost]
public async Task<IActionResult> PostLogEntry([FromBody] LogEvent logEvent)
{
    try
    {
        // Save to database
        _context.LogEvents.Add(logEvent);
        await _context.SaveChangesAsync();

        // Log to file
        _logger.LogInformation("{Timestamp} {Level} {UserInformation} {SystemInformation} {ProcessInformation} {Message}",
            logEvent.Timestamp, logEvent.Level, logEvent.UserInformation, logEvent.SystemInformation, logEvent.ProcessInformation, logEvent.Message);

        return Ok();
    }
    catch (Exception ex)
    {
        // Log the exception details
        _logger.LogError(ex, "An error occurred while processing the log entry");
        return StatusCode(500, "Internal server error");
    }
}

    }
}
