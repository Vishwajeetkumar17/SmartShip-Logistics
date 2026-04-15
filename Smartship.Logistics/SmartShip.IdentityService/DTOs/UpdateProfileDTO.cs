using System.ComponentModel.DataAnnotations;

namespace SmartShip.IdentityService.DTOs
{
    /// <summary>
    /// Data transfer model for update profile payloads.
    /// </summary>
    public class UpdateProfileDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;
        [Required]
        [Phone]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;
    }
}


