/// <summary>
/// Provides backend implementation for UpdateProfileDTO.
/// </summary>

using System.ComponentModel.DataAnnotations;

namespace SmartShip.IdentityService.DTOs
{
    /// <summary>
    /// Represents UpdateProfileDTO.
    /// </summary>
    public class UpdateProfileDTO
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the phone.
        /// </summary>
        [Required]
        [Phone]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;
    }
}


