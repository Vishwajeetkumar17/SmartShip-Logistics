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
    /// <summary>
    /// Gets or sets the shipment id.
    /// </summary>
    public int ShipmentId { get; set; }
    /// <summary>
    /// Gets or sets the tracking number.
    /// </summary>
    public string TrackingNumber { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the customer id.
    /// </summary>
    public int CustomerId { get; set; }
    /// <summary>
    /// Gets or sets the sender name.
    /// </summary>
    public string? SenderName { get; set; }
    /// <summary>
    /// Gets or sets the receiver name.
    /// </summary>
    public string? ReceiverName { get; set; }
    /// <summary>
    /// Gets or sets the service type.
    /// </summary>
    public string? ServiceType { get; set; }
    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public ShipmentStatusDto Status { get; set; }
    /// <summary>
    /// Gets or sets the total weight.
    /// </summary>
    public decimal TotalWeight { get; set; }
    /// <summary>
    /// Gets or sets the created at.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>
    /// Gets or sets the sender address.
    /// </summary>
    public AddressDto SenderAddress { get; set; } = new();
    /// <summary>
    /// Gets or sets the receiver address.
    /// </summary>
    public AddressDto ReceiverAddress { get; set; } = new();
}


