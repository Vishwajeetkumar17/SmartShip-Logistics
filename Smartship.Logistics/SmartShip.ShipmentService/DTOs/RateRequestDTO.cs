/// <summary>
/// Provides backend implementation for RateRequestDTO.
/// </summary>

using System.ComponentModel.DataAnnotations;

namespace SmartShip.ShipmentService.DTOs;

/// <summary>
/// Represents RateRequestDTO.
/// </summary>
public class RateRequestDTO
{
    /// <summary>
    /// Gets or sets the origin city.
    /// </summary>
    [Required]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "OriginCity is required")]
    [MaxLength(100)]
    public string OriginCity { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the destination city.
    /// </summary>
    [Required]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "DestinationCity is required")]
    [MaxLength(100)]
    public string DestinationCity { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the weight.
    /// </summary>
    [Range(0.1, 10000)]
    public decimal Weight { get; set; }

    /// <summary>
    /// Gets or sets the service type.
    /// </summary>
    [Required]
    [RegularExpression("^(Standard|Express|Economy)$", ErrorMessage = "ServiceType must be one of: Standard, Express, Economy")]
    public string ServiceType { get; set; } = string.Empty;
}


