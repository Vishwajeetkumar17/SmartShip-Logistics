/// <summary>
/// Provides backend implementation for ShipmentStatus.
/// </summary>

namespace SmartShip.ShipmentService.Enums;

/// <summary>
    /// Defines the possible values for shipment status.
    /// </summary>
    public enum ShipmentStatus
{
    /// <summary>
    /// Specifies the draft enumeration value.
    /// </summary>
    Draft,
    /// <summary>
    /// Specifies the booked enumeration value.
    /// </summary>
    Booked,
    /// <summary>
    /// Specifies the picked up enumeration value.
    /// </summary>
    PickedUp,
    /// <summary>
    /// Specifies the in transit enumeration value.
    /// </summary>
    InTransit,
    /// <summary>
    /// Specifies the out for delivery enumeration value.
    /// </summary>
    OutForDelivery,
    /// <summary>
    /// Specifies the delivered enumeration value.
    /// </summary>
    Delivered
}


