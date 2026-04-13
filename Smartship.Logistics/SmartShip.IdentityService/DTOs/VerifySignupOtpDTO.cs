/// <summary>
/// Provides backend implementation for VerifySignupOtpDTO.
/// </summary>

using System.ComponentModel.DataAnnotations;

namespace SmartShip.IdentityService.DTOs
{
    /// <summary>
    /// Represents VerifySignupOtpDTO.
    /// </summary>
    public class VerifySignupOtpDTO
    {
        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the otp.
        /// </summary>
        [Required]
        [RegularExpression("^[0-9]{6}$")]
        public string Otp { get; set; } = string.Empty;
    }
}


