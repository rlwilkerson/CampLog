using CampLog.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CampLog.Api.Data;

public class CampLogDbContext(DbContextOptions<CampLogDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<Location> Locations => Set<Location>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.KeycloakId).IsUnique();
            e.Property(u => u.KeycloakId).IsRequired().HasMaxLength(256);
            e.Property(u => u.Email).IsRequired().HasMaxLength(256);
            e.Property(u => u.DisplayName).IsRequired().HasMaxLength(256);
        });

        modelBuilder.Entity<Trip>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Name).IsRequired().HasMaxLength(256);
            e.HasOne(t => t.User).WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(t => t.Locations).WithOne(l => l.Trip).HasForeignKey(l => l.TripId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Location>(e =>
        {
            e.HasKey(l => l.Id);
            e.Property(l => l.Name).IsRequired().HasMaxLength(256);
        });
    }
}
