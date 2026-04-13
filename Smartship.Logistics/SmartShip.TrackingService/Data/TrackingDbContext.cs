/// <summary>
/// Provides backend implementation for TrackingDbContext.
/// </summary>

using Microsoft.EntityFrameworkCore;
using SmartShip.TrackingService.Models;

namespace SmartShip.TrackingService.Data;

/// <summary>
/// Represents TrackingDbContext.
/// </summary>
public class TrackingDbContext : DbContext
{
    public TrackingDbContext(DbContextOptions<TrackingDbContext> options)
        : base(options)
    {
    }

    public DbSet<TrackingEvent> TrackingEvents { get; set; }
    
    public DbSet<ShipmentLocation> ShipmentLocations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TrackingEvent>(entity =>
        {
            entity.HasKey(e => e.EventId);
            
            entity.Property(e => e.TrackingNumber)
                .IsRequired()
                .HasMaxLength(32);

            entity.HasIndex(e => new { e.TrackingNumber, e.Timestamp });

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Location)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .HasMaxLength(1000);
        });

        modelBuilder.Entity<ShipmentLocation>(entity =>
        {
            entity.HasKey(e => e.LocationId);

            entity.Property(e => e.TrackingNumber)
                .IsRequired()
                .HasMaxLength(32);

            entity.HasIndex(e => new { e.TrackingNumber, e.Timestamp });

            entity.Property(e => e.Latitude)
                .HasPrecision(10, 8);

            entity.Property(e => e.Longitude)
                .HasPrecision(11, 8);
        });
    }
}


