/// <summary>
/// Provides backend implementation for PickupSchedule.
/// </summary>

namespace SmartShip.ShipmentService.Models;

/// <summary>
/// Represents PickupSchedule.
/// </summary>
public class PickupSchedule
{
    /// <summary>
    /// Gets or sets the pickup schedule id.
    /// </summary>
    public int PickupScheduleId { get; set; }

    /// <summary>
    /// Gets or sets the shipment id.
    /// </summary>
    public int ShipmentId { get; set; }

    /// <summary>
    /// Gets or sets the pickup date.
    /// </summary>
    public DateTime PickupDate { get; set; }

    /// <summary>
    /// Gets or sets the notes.
    /// </summary>
    public string Notes { get; set; } = string.Empty;
}


