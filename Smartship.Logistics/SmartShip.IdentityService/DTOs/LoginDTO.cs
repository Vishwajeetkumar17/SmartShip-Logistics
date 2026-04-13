/// <summary>
/// Provides backend implementation for LoginDTO.
/// </summary>

using System.ComponentModel.DataAnnotations;

namespace SmartShip.IdentityService.DTOs
{
    /// <summary>
    /// Represents LoginDTO.
    /// </summary>
    public class LoginDTO
    {
        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        [Required]
        [StringLength(128, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;
    }
}


