/// <summary>
/// Provides backend implementation for ShipmentStateValidator.
/// </summary>

using SmartShip.ShipmentService.Enums;

namespace SmartShip.ShipmentService.Helpers;

/// <summary>
/// Represents ShipmentStateValidator.
/// </summary>
public static class ShipmentStateValidator
{
    /// <summary>
    /// Executes IsValidTransition.
    /// </summary>
    public static bool IsValidTransition(ShipmentStatus current, ShipmentStatus next)
    {
        return (current, next) switch
        {
            (ShipmentStatus.Draft, ShipmentStatus.Booked) => true,
            (ShipmentStatus.Booked, ShipmentStatus.PickedUp) => true,
            (ShipmentStatus.PickedUp, ShipmentStatus.InTransit) => true,
            (ShipmentStatus.InTransit, ShipmentStatus.InTransit) => true,
            (ShipmentStatus.InTransit, ShipmentStatus.OutForDelivery) => true,
            (ShipmentStatus.OutForDelivery, ShipmentStatus.Delivered) => true,
            _ => false
        };
    }
}


