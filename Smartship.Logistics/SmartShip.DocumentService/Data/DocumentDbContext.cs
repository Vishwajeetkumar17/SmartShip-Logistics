/// <summary>
/// Provides backend implementation for DocumentDbContext.
/// </summary>

using Microsoft.EntityFrameworkCore;
using SmartShip.DocumentService.Models;

namespace SmartShip.DocumentService.Data;

/// <summary>
/// Represents DocumentDbContext.
/// </summary>
public class DocumentDbContext : DbContext
{
    public DocumentDbContext(DbContextOptions<DocumentDbContext> options)
        : base(options)
    {
    }

    public DbSet<Document> Documents { get; set; }

    public DbSet<DeliveryProof> DeliveryProofs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(d => d.DocumentId);

            entity.HasIndex(d => new { d.ShipmentId, d.UploadedAt });
            entity.HasIndex(d => new { d.CustomerId, d.UploadedAt });

            entity.Property(d => d.FileName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(d => d.FilePath)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(d => d.DocumentType)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(d => d.ContentType)
                .IsRequired()
                .HasMaxLength(100);
        });

        modelBuilder.Entity<DeliveryProof>(entity =>
        {
            entity.HasKey(p => p.ProofId);

            entity.HasIndex(p => p.ShipmentId)
                .IsUnique();

            entity.Property(p => p.FilePath)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(p => p.SignerName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(p => p.Notes)
                .HasMaxLength(1000);
        });
    }
}


