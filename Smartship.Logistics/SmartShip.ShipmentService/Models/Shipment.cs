/// <summary>
/// Provides backend implementation for Shipment.
/// </summary>

using SmartShip.ShipmentService.Enums;

namespace SmartShip.ShipmentService.Models;

/// <summary>
/// Represents Shipment.
/// </summary>
public class Shipment
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
    public string SenderName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sender phone.
    /// </summary>
    public string? SenderPhone { get; set; }

    /// <summary>
    /// Gets or sets the receiver name.
    /// </summary>
    public string ReceiverName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the receiver phone.
    /// </summary>
    public string? ReceiverPhone { get; set; }

    /// <summary>
    /// Gets or sets the sender address.
    /// </summary>
    public Address SenderAddress { get; set; } = new();

    /// <summary>
    /// Gets or sets the receiver address.
    /// </summary>
    public Address ReceiverAddress { get; set; } = new();

    /// <summary>
    /// Gets or sets the total weight.
    /// </summary>
    public decimal TotalWeight { get; set; }

    /// <summary>
    /// Gets or sets the estimated cost.
    /// </summary>
    public decimal EstimatedCost { get; set; }

    /// <summary>
    /// Gets or sets the service type.
    /// </summary>
    public string ServiceType { get; set; } = "Standard";

    /// <summary>
    /// Gets or sets the booking hub location.
    /// </summary>
    public string? BookingHubLocation { get; set; }

    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public ShipmentStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the created at.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the pickup schedule.
    /// </summary>
    public PickupSchedule? PickupSchedule { get; set; }

    /// <summary>
    /// Gets or sets the packages.
    /// </summary>
    public List<Package> Packages { get; set; } = new();
}


