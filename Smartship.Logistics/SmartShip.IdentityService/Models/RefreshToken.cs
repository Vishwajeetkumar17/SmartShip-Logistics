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
        /// <summary>
        /// Gets or sets the refresh token id.
        /// </summary>
        public int RefreshTokenId { get; set; }
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// Gets or sets the token hash.
        /// </summary>
        public string TokenHash { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the expires at.
        /// </summary>
        public DateTime ExpiresAt { get; set; }
        /// <summary>
        /// Gets or sets the is revoked.
        /// </summary>
        public bool IsRevoked { get; set; }
    }
}


