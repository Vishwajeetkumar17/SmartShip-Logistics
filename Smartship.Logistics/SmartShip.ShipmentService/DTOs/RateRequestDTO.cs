using System.ComponentModel.DataAnnotations;

namespace SmartShip.ShipmentService.DTOs;

/// <summary>
/// Data transfer model for rate request payloads.
/// </summary>
public class RateRequestDTO
{
    [Required]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "OriginCity is required")]
    [MaxLength(100)]
    public string OriginCity { get; set; } = string.Empty;
    [Required]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "DestinationCity is required")]
    [MaxLength(100)]
    public string DestinationCity { get; set; } = string.Empty;
    [Range(0.1, 10000)]
    public decimal Weight { get; set; }
    [Required]
    [RegularExpression("^(Standard|Express|Economy)$", ErrorMessage = "ServiceType must be one of: Standard, Express, Economy")]
    public string ServiceType { get; set; } = string.Empty;
}


