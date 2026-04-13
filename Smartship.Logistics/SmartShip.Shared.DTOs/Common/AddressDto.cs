/// <summary>
/// Provides backend implementation for AddressDto.
/// </summary>

namespace SmartShip.Shared.DTOs.Common;

/// <summary>
/// Represents AddressDto.
/// </summary>
public class AddressDto
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
}


