/// <summary>
/// Provides backend implementation for PickupSchedule.
/// </summary>

namespace SmartShip.ShipmentService.Models;

/// <summary>
/// Represents PickupSchedule.
/// </summary>
public class PickupSchedule
{
    public int PickupScheduleId { get; set; }

    public int ShipmentId { get; set; }

    public DateTime PickupDate { get; set; }

    public string Notes { get; set; } = string.Empty;
}


