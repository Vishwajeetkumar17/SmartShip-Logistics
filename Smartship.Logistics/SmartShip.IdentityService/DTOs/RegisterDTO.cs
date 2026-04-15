using System.ComponentModel.DataAnnotations;

namespace SmartShip.IdentityService.DTOs
{
    /// <summary>
    /// Data transfer model for register payloads.
    /// </summary>
    public class RegisterDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;
        [Required]
        [Phone]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;
        [Required]
        [StringLength(128, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;
        [Range(1, int.MaxValue)]
        public int RoleId { get; set; }
    }
}


