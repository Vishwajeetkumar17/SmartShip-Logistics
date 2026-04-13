/// <summary>
/// Provides backend implementation for UpdateUserDTO.
/// </summary>

using System.ComponentModel.DataAnnotations;

namespace SmartShip.IdentityService.DTOs
{
    /// <summary>
    /// Represents UpdateUserDTO.
    /// </summary>
    public class UpdateUserDTO
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
    }
}


