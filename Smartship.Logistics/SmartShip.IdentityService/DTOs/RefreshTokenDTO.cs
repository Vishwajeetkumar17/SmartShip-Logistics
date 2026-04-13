/// <summary>
/// Provides backend implementation for RefreshTokenDTO.
/// </summary>

using System.ComponentModel.DataAnnotations;

namespace SmartShip.IdentityService.DTOs
{
    /// <summary>
    /// Represents RefreshTokenDTO.
    /// </summary>
    public class RefreshTokenDTO
    {
        /// <summary>
        /// Gets or sets the refresh token.
        /// </summary>
        [Required]
        [StringLength(256, MinimumLength = 32)]
        public string RefreshToken { get; set; } = string.Empty;
    }
}


