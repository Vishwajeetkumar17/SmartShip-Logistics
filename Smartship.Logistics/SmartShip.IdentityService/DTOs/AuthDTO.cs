/// <summary>
/// Provides backend implementation for AuthDTO.
/// </summary>

namespace SmartShip.IdentityService.DTOs
{
    /// <summary>
    /// Represents AuthDTO.
    /// </summary>
    public class AuthDTO
    {
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        public string Email { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the phone.
        /// </summary>
        public string Phone { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        public string Token { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the refresh token.
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the role.
        /// </summary>
        public string Role { get; set; } = string.Empty;
    }
}


