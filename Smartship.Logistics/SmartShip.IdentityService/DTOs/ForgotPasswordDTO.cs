using System.ComponentModel.DataAnnotations;

namespace SmartShip.IdentityService.DTOs
{
    /// <summary>
    /// Data transfer model for forgot password payloads.
    /// </summary>
    public class ForgotPasswordDTO
    {
        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;
    }
}


