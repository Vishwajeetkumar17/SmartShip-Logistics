/// <summary>
/// Provides backend implementation for AssignRoleDTO.
/// </summary>

using System.ComponentModel.DataAnnotations;

namespace SmartShip.IdentityService.DTOs
{
    /// <summary>
    /// Represents AssignRoleDTO.
    /// </summary>
    public class AssignRoleDTO
    {
        [Range(1, int.MaxValue)]
        public int RoleId { get; set; }
    }
}


