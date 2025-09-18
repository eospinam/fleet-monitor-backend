using Microsoft.EntityFrameworkCore;

namespace FleetMonitor.Core.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Telemetry> Telemetries { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
