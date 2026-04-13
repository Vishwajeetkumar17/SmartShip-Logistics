/// <summary>
/// Provides backend implementation for IdentityDbContext.
/// </summary>

using Microsoft.EntityFrameworkCore;
using SmartShip.IdentityService.Models;

namespace SmartShip.IdentityService.Data
{
    /// <summary>
    /// Represents IdentityDbContext.
    /// </summary>
    public class IdentityDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the identity db context class.
        /// </summary>
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the users.
        /// </summary>
        public DbSet<User> Users { get; set; }
        /// <summary>
        /// Gets or sets the roles.
        /// </summary>
        public DbSet<Role> Roles { get; set; }
        /// <summary>
        /// Gets or sets the refresh tokens.
        /// </summary>
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        /// <summary>
        /// Gets or sets the password reset tokens.
        /// </summary>
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


