/// <summary>
/// Provides backend implementation for RoleDTO.
/// </summary>

namespace SmartShip.IdentityService.DTOs
{
    /// <summary>
    /// Represents RoleDTO.
    /// </summary>
    public class RoleDTO
    {
        /// <summary>
        /// Gets or sets the role id.
        /// </summary>
        public int RoleId { get; set; }
        /// <summary>
        /// Gets or sets the role name.
        /// </summary>
        public string RoleName { get; set; } = string.Empty;
    }
}


