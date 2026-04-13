/// <summary>
/// Entity Framework Core database context for the Admin microservice.
/// Manages persistence for Hubs, Service Locations, and Exception Records.
/// </summary>

using Microsoft.EntityFrameworkCore;
using SmartShip.AdminService.Models;

namespace SmartShip.AdminService.Data;

/// <summary>
/// Database context for the Admin microservice, providing access to
/// logistics hub management, service location mapping, and shipment exception tracking.
/// </summary>
public class AdminDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AdminDbContext"/> class
    /// with the specified database context options.
    /// </summary>
    /// <param name="options">The configuration options for this context.</param>
    public AdminDbContext(DbContextOptions<AdminDbContext> options)
        : base(options)
    {
    }

    #region DbSet Properties

    /// <summary>
    /// Gets or sets the collection of logistics hubs (warehouses, sorting centres).
    /// </summary>
    public DbSet<Hub> Hubs { get; set; }

    /// <summary>
    /// Gets or sets the collection of service delivery locations tied to hubs.
    /// </summary>
    public DbSet<ServiceLocation> ServiceLocations { get; set; }

    /// <summary>
    /// Gets or sets the collection of shipment exception/anomaly records.
    /// </summary>
    public DbSet<ExceptionRecord> ExceptionRecords { get; set; }

    #endregion

    #region Entity Configuration

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- Hub Entity Configuration ---
        modelBuilder.Entity<Hub>(entity =>
        {
            entity.HasKey(h => h.HubId);
            entity.HasIndex(h => h.Name).IsUnique();

            entity.Property(h => h.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(h => h.Address)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(h => h.ContactNumber)
                .HasMaxLength(20);

            entity.Property(h => h.ManagerName)
                .HasMaxLength(100);

            // One Hub can serve many Service Locations
            entity.HasMany(h => h.ServiceLocations)
                .WithOne(sl => sl.Hub)
                .HasForeignKey(sl => sl.HubId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // --- Service Location Entity Configuration ---
        modelBuilder.Entity<ServiceLocation>(entity =>
        {
            entity.HasKey(sl => sl.LocationId);
            entity.HasIndex(sl => sl.ZipCode).IsUnique();

            entity.Property(sl => sl.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(sl => sl.ZipCode)
                .IsRequired()
                .HasMaxLength(20);
        });

        // --- Exception Record Entity Configuration ---
        modelBuilder.Entity<ExceptionRecord>(entity =>
        {
            entity.HasKey(e => e.ExceptionId);
            entity.HasIndex(e => new { e.ShipmentId, e.Status });

            entity.Property(e => e.ExceptionType)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50);
        });
    }

    #endregion
}


