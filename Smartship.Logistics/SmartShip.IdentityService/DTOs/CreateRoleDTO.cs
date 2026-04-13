/// <summary>
/// Provides backend implementation for CreateRoleDTO.
/// </summary>

using System.ComponentModel.DataAnnotations;

namespace SmartShip.IdentityService.DTOs
{
    /// <summary>
    /// Represents CreateRoleDTO.
    /// </summary>
    public class CreateRoleDTO
    {
        /// <summary>
        /// Gets or sets the role name.
        /// </summary>
        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string RoleName { get; set; } = string.Empty;
    }
}


