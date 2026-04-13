/// <summary>
/// Provides backend implementation for RefreshToken.
/// </summary>

namespace SmartShip.IdentityService.Models
{
    /// <summary>
    /// Represents RefreshToken.
    /// </summary>
    public class RefreshToken
    {
        public int RefreshTokenId { get; set; }
        public int UserId { get; set; }
        public string TokenHash { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
    }
}


