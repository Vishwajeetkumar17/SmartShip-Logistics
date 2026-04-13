/// <summary>
/// Provides backend implementation for ShipmentExternalDto.
/// </summary>

using SmartShip.Shared.DTOs.Common;

namespace SmartShip.Shared.DTOs.Shipment;

/// <summary>
/// Represents ShipmentExternalDto.
/// </summary>
public class ShipmentExternalDto
{
    public int ShipmentId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string? SenderName { get; set; }
    public string? ReceiverName { get; set; }
    public string? ServiceType { get; set; }
    public ShipmentStatusDto Status { get; set; }
    public decimal TotalWeight { get; set; }
    public DateTime CreatedAt { get; set; }
    public AddressDto SenderAddress { get; set; } = new();
    public AddressDto ReceiverAddress { get; set; } = new();
}


