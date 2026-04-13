/// <summary>
/// Provides backend implementation for AddressDto.
/// </summary>

namespace SmartShip.Shared.DTOs.Common;

/// <summary>
/// Represents AddressDto.
/// </summary>
public class AddressDto
{
    /// <summary>
    /// Gets or sets the street.
    /// </summary>
    public string Street { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the city.
    /// </summary>
    public string City { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the state.
    /// </summary>
    public string State { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the country.
    /// </summary>
    public string Country { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the postal code.
    /// </summary>
    public string PostalCode { get; set; } = string.Empty;
}


