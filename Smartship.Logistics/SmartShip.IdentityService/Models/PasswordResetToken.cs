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
        public int PasswordResetTokenId { get; set; }
        public int UserId { get; set; }
        public string TokenHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
    }
}


