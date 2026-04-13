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
    /// <summary>
    /// Gets or sets the address id.
    /// </summary>
    public int AddressId { get; set; }

    /// <summary>
    /// Gets or sets the street.
    /// </summary>
    [Required]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Street is required")]
    [MaxLength(200)]
    public string Street { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the city.
    /// </summary>
    [Required]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "City is required")]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the state.
    /// </summary>
    [Required]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "State is required")]
    [MaxLength(100)]
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the country.
    /// </summary>
    [Required]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Country is required")]
    [MaxLength(100)]
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the postal code.
    /// </summary>
    [Required]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "PostalCode is required")]
    [MaxLength(20)]
    public string PostalCode { get; set; } = string.Empty;
}


