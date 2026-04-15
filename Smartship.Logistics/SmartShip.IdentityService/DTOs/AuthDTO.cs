namespace SmartShip.IdentityService.DTOs
{
    /// <summary>
    /// Data transfer model for auth payloads.
    /// </summary>
    public class AuthDTO
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}


