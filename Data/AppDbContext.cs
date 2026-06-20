using Microsoft.EntityFrameworkCore;
using NetworkManager.Models;

namespace NetworkManager.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Device> Devices => Set<Device>();
    public DbSet<IpAssignment> IpAssignments => Set<IpAssignment>();
    public DbSet<MaintenanceLog> MaintenanceLogs => Set<MaintenanceLog>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Device -> IpAssignment (one-to-many, nullable FK)
        modelBuilder.Entity<IpAssignment>()
            .HasOne(i => i.Device)
            .WithMany(d => d.IpAssignments)
            .HasForeignKey(i => i.DeviceId)
            .OnDelete(DeleteBehavior.SetNull);

        // Device -> MaintenanceLog (one-to-many)
        modelBuilder.Entity<MaintenanceLog>()
            .HasOne(m => m.Device)
            .WithMany(d => d.MaintenanceLogs)
            .HasForeignKey(m => m.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed: Admin user (password: Admin@1234)
        modelBuilder.Entity<AppUser>().HasData(new AppUser
        {
            Id = 1,
            Username = "admin",
            // BCrypt hash of "Admin@1234"
            PasswordHash = "$2a$11$goSbhME5UYvrcIAOJdfILOn7O7Y8rb45.Xpw5wmGhEdSl9KTugWjW",
            CreatedAt = new DateTime(2024, 1, 1)
        });

        // Seed: Sample devices
        modelBuilder.Entity<Device>().HasData(
            new Device
            {
                Id = 1,
                Name = "Core Router",
                Type = DeviceType.Router,
                IpAddress = "192.168.1.1",
                MacAddress = "AA:BB:CC:DD:EE:01",
                Location = "Server Room",
                Notes = "Main internet gateway",
                Status = DeviceStatus.Unknown,
                CreatedAt = new DateTime(2024, 1, 1)
            },
            new Device
            {
                Id = 2,
                Name = "Distribution Switch",
                Type = DeviceType.Switch,
                IpAddress = "192.168.1.2",
                MacAddress = "AA:BB:CC:DD:EE:02",
                Location = "Server Room",
                Notes = "24-port managed switch",
                Status = DeviceStatus.Unknown,
                CreatedAt = new DateTime(2024, 1, 1)
            },
            new Device
            {
                Id = 3,
                Name = "Main Server",
                Type = DeviceType.Server,
                IpAddress = "192.168.1.10",
                MacAddress = "AA:BB:CC:DD:EE:03",
                Location = "Server Room",
                Notes = "Primary application server",
                Status = DeviceStatus.Unknown,
                CreatedAt = new DateTime(2024, 1, 1)
            }
        );
    }
}
