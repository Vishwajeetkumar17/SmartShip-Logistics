using System.ComponentModel.DataAnnotations;

namespace SmartShip.IdentityService.DTOs
{
    /// <summary>
    /// Data transfer model for reset password payloads.
    /// </summary>
    public class ResetPasswordDTO
    {
        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;
        [Required]
        [RegularExpression("^\\d{6}$", ErrorMessage = "OTP must be 6 digits.")]
        public string Token { get; set; } = string.Empty;
        [Required]
        [StringLength(128, MinimumLength = 8)]
        public string NewPassword { get; set; } = string.Empty;
    }
}


