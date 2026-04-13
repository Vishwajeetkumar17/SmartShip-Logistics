/// <summary>
/// Provides backend implementation for ChangePasswordDTO.
/// </summary>

using System.ComponentModel.DataAnnotations;

namespace SmartShip.IdentityService.DTOs
{
    /// <summary>
    /// Represents ChangePasswordDTO.
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


