using LoggerPlugin.Models;
using Microsoft.EntityFrameworkCore;

namespace LoggerPlugin.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<LogEvent> LogEvents { get; set; }
    }
}
