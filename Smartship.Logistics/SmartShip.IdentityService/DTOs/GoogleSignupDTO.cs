/// <summary>
/// Provides backend implementation for GoogleSignupDTO.
/// </summary>

using System.ComponentModel.DataAnnotations;

namespace SmartShip.IdentityService.DTOs
{
    /// <summary>
    /// Represents GoogleSignupDTO.
    /// </summary>
    public class GoogleSignupDTO
    {
        [Required]
        public string IdToken { get; set; } = string.Empty;
    }
}


