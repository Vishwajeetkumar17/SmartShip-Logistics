/// <summary>
/// Provides backend implementation for RegisterDTO.
/// </summary>

using System.ComponentModel.DataAnnotations;

namespace SmartShip.IdentityService.DTOs
{
    /// <summary>
    /// Represents RegisterDTO.
    /// </summary>
    public class RegisterDTO
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the phone.
        /// </summary>
        [Required]
        [Phone]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        [Required]
        [StringLength(128, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the role id.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int RoleId { get; set; }
    }
}


