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
    public int ShipmentId { get; set; }

    public string TrackingNumber { get; set; } = string.Empty;

    public int CustomerId { get; set; }

    public string SenderName { get; set; } = string.Empty;

    public string? SenderPhone { get; set; }

    public string ReceiverName { get; set; } = string.Empty;

    public string? ReceiverPhone { get; set; }

    public Address SenderAddress { get; set; } = new();

    public Address ReceiverAddress { get; set; } = new();

    public decimal TotalWeight { get; set; }

    public decimal EstimatedCost { get; set; }

    public string ServiceType { get; set; } = "Standard";

    public string? BookingHubLocation { get; set; }

    public ShipmentStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public PickupSchedule? PickupSchedule { get; set; }

    public List<Package> Packages { get; set; } = new();
}


