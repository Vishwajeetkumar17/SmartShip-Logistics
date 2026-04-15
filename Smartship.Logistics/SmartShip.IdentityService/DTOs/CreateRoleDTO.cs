using System.ComponentModel.DataAnnotations;

namespace SmartShip.IdentityService.DTOs
{
    /// <summary>
    /// Data transfer model for create role payloads.
    /// </summary>
    public class CreateRoleDTO
    {
        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string RoleName { get; set; } = string.Empty;
    }
}


