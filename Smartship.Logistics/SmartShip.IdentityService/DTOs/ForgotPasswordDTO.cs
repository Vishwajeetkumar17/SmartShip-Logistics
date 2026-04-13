/// <summary>
/// Provides backend implementation for ForgotPasswordDTO.
/// </summary>

using System.ComponentModel.DataAnnotations;

namespace SmartShip.IdentityService.DTOs
{
    /// <summary>
    /// Represents ForgotPasswordDTO.
    /// </summary>
    public class ForgotPasswordDTO
    {
        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;
    }
}


