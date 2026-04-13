/// <summary>
/// Provides backend implementation for ShipmentStatusDto.
/// </summary>

namespace SmartShip.Shared.DTOs.Shipment;

/// <summary>
    /// Defines the possible values for shipment status dto.
    /// </summary>
    public enum ShipmentStatusDto
{
    /// <summary>
    /// Specifies the draft enumeration value.
    /// </summary>
    Draft = 0,
    /// <summary>
    /// Specifies the booked enumeration value.
    /// </summary>
    Booked = 1,
    /// <summary>
    /// Specifies the picked up enumeration value.
    /// </summary>
    PickedUp = 2,
    /// <summary>
    /// Specifies the in transit enumeration value.
    /// </summary>
    InTransit = 3,
    /// <summary>
    /// Specifies the out for delivery enumeration value.
    /// </summary>
    OutForDelivery = 4,
    /// <summary>
    /// Specifies the delivered enumeration value.
    /// </summary>
    Delivered = 5
}


