/// <summary>
/// Provides backend implementation for AdminDbContext.
/// </summary>

using Microsoft.EntityFrameworkCore;
using SmartShip.AdminService.Models;

namespace SmartShip.AdminService.Data;

/// <summary>
/// Represents AdminDbContext.
/// </summary>
public class AdminDbContext : DbContext
{
    public AdminDbContext(DbContextOptions<AdminDbContext> options)
        : base(options)
    {
    }

    public DbSet<Hub> Hubs { get; set; }
    public DbSet<ServiceLocation> ServiceLocations { get; set; }
    public DbSet<ExceptionRecord> ExceptionRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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

            entity.HasMany(h => h.ServiceLocations)
                .WithOne(sl => sl.Hub)
                .HasForeignKey(sl => sl.HubId)
                .OnDelete(DeleteBehavior.Cascade);
        });

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
}


