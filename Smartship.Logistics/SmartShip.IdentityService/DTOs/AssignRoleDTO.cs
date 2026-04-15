using System.ComponentModel.DataAnnotations;

namespace SmartShip.IdentityService.DTOs
{
    /// <summary>
    /// Data transfer model for assign role payloads.
    /// </summary>
    public class AssignRoleDTO
    {
        [Range(1, int.MaxValue)]
        public int RoleId { get; set; }
    }
}


