/// <summary>
/// Provides backend implementation for ShipmentDbContext.
/// </summary>

using Microsoft.EntityFrameworkCore;
using SmartShip.ShipmentService.Models;

namespace SmartShip.ShipmentService.Data;

/// <summary>
/// Represents ShipmentDbContext.
/// </summary>
public class ShipmentDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the shipment db context class.
    /// </summary>
    public ShipmentDbContext(DbContextOptions<ShipmentDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the shipments.
    /// </summary>
    public DbSet<Shipment> Shipments { get; set; }

    /// <summary>
    /// Gets or sets the packages.
    /// </summary>
    public DbSet<Package> Packages { get; set; }

    /// <summary>
    /// Gets or sets the addresses.
    /// </summary>
    public DbSet<Address> Addresses { get; set; }

    /// <summary>
    /// Gets or sets the pickup schedules.
    /// </summary>
    public DbSet<PickupSchedule> PickupSchedules { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Shipment>(entity =>
        {
            entity.Property(s => s.TrackingNumber)
                .IsRequired()
                .HasMaxLength(32);

            entity.Property(s => s.SenderName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(s => s.SenderPhone)
                .HasMaxLength(20);

            entity.Property(s => s.ReceiverName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(s => s.ReceiverPhone)
                .HasMaxLength(20);

            entity.HasIndex(s => s.TrackingNumber)
                .IsUnique();

            entity.Property(s => s.TotalWeight)
                .HasPrecision(18, 2);

            entity.Property(s => s.EstimatedCost)
                .HasPrecision(18, 2);

            entity.Property(s => s.ServiceType)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Standard");

            entity.Property(s => s.BookingHubLocation)
                .HasMaxLength(300);

            entity.Property(s => s.CreatedAt)
                .HasColumnType("datetime2")
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(s => s.CreatedAt)
                .IsDescending();

            entity.HasOne(s => s.SenderAddress)
                .WithMany()
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.ReceiverAddress)
                .WithMany()
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.PickupSchedule)
                .WithOne()
                .HasForeignKey<PickupSchedule>(p => p.ShipmentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(s => s.Packages)
                .WithOne(p => p.Shipment)
                .HasForeignKey(p => p.ShipmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Package>(entity =>
        {
            entity.Property(p => p.Weight)
                .HasPrecision(18, 2);

            entity.Property(p => p.Description)
                .HasMaxLength(500);
        });

        modelBuilder.Entity<Address>(entity =>
        {
            entity.Property(a => a.Street).IsRequired().HasMaxLength(200);
            entity.Property(a => a.City).IsRequired().HasMaxLength(100);
            entity.Property(a => a.State).IsRequired().HasMaxLength(100);
            entity.Property(a => a.Country).IsRequired().HasMaxLength(100);
            entity.Property(a => a.PostalCode).IsRequired().HasMaxLength(20);
        });

        modelBuilder.Entity<PickupSchedule>(entity =>
        {
            entity.Property(p => p.Notes)
                .HasMaxLength(1000);
        });
    }
}


