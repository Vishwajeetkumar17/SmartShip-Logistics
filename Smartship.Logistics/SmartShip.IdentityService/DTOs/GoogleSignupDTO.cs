using System.ComponentModel.DataAnnotations;

namespace SmartShip.IdentityService.DTOs
{
    /// <summary>
    /// Data transfer model for google signup payloads.
    /// </summary>
    public class GoogleSignupDTO
    {
        [Required]
        public string IdToken { get; set; } = string.Empty;
    }
}


