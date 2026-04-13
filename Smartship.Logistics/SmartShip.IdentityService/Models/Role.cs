/// <summary>
/// Provides backend implementation for Role.
/// </summary>

namespace SmartShip.IdentityService.Models
{
    /// <summary>
    /// Represents Role.
    /// </summary>
    public class Role
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


