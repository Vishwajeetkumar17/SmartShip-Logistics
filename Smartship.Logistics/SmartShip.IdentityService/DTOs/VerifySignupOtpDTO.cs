using System.ComponentModel.DataAnnotations;

namespace SmartShip.IdentityService.DTOs
{
    /// <summary>
    /// Data transfer model for verify signup otp payloads.
    /// </summary>
    public class VerifySignupOtpDTO
    {
        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;
        [Required]
        [RegularExpression("^[0-9]{6}$")]
        public string Otp { get; set; } = string.Empty;
    }
}


