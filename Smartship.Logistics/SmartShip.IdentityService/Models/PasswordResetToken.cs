/// <summary>
/// Provides backend implementation for PasswordResetToken.
/// </summary>

namespace SmartShip.IdentityService.Models
{
    /// <summary>
    /// Represents PasswordResetToken.
    /// </summary>
    public class PasswordResetToken
    {
        /// <summary>
        /// Gets or sets the password reset token id.
        /// </summary>
        public int PasswordResetTokenId { get; set; }
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// Gets or sets the token hash.
        /// </summary>
        public string TokenHash { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the created at.
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// Gets or sets the expires at.
        /// </summary>
        public DateTime ExpiresAt { get; set; }
        /// <summary>
        /// Gets or sets the is used.
        /// </summary>
        public bool IsUsed { get; set; }
    }
}


