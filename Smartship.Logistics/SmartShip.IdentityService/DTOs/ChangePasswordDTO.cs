using System.ComponentModel.DataAnnotations;

namespace SmartShip.IdentityService.DTOs
{
    /// <summary>
    /// Data transfer model for change password payloads.
    /// </summary>
    public class ChangePasswordDTO
    {
        [Required]
        [StringLength(128, MinimumLength = 8)]
        public string OldPassword { get; set; } = string.Empty;
        [Required]
        [StringLength(128, MinimumLength = 8)]
        public string NewPassword { get; set; } = string.Empty;
    }
}


