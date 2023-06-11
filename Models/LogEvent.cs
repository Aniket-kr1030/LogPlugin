using System;

namespace LoggerPlugin.Models
{
    public class LogEvent
    {
        public int Id { get; set; }
        public string? Message { get; set; }
        public string? Level { get; set; }
        public DateTime Timestamp { get; set; }
        public string? UserInformation { get; set; }
        public string? SystemInformation { get; set; }
        public string? ProcessInformation { get; set; }
    }
}
