namespace SmartShip.IdentityService.Models
{
    /// <summary>
    /// Domain model for refresh token.
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


