using Microsoft.EntityFrameworkCore;
using SmartShip.IdentityService.Models;

namespace SmartShip.IdentityService.Data
{
    /// <summary>
    /// Entity Framework database context for identity.
    /// </summary>
    public class IdentityDbContext : DbContext
    {
        /// <summary>
        /// Configures Entity Framework context for identity data.
        /// </summary>
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(x => x.Email)
                .IsUnique();

            modelBuilder.Entity<Role>()
                .HasIndex(x => x.RoleName)
                .IsUnique();

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(x => x.TokenHash)
                .IsUnique();

            modelBuilder.Entity<PasswordResetToken>()
                .HasIndex(x => x.TokenHash)
                .IsUnique();
        }
    }
}


