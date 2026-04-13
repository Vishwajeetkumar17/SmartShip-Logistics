/// <summary>
/// Provides backend implementation for Address.
/// </summary>

using System.ComponentModel.DataAnnotations;

namespace SmartShip.ShipmentService.Models;

/// <summary>
/// Represents Address.
/// </summary>
public class Address
{
    public int AddressId { get; set; }

    [Required]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Street is required")]
    [MaxLength(200)]
    public string Street { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "City is required")]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "State is required")]
    [MaxLength(100)]
    public string State { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Country is required")]
    [MaxLength(100)]
    public string Country { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "PostalCode is required")]
    [MaxLength(20)]
    public string PostalCode { get; set; } = string.Empty;
}


