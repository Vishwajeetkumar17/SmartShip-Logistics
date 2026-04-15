namespace SmartShip.Shared.DTOs.Shipment;

/// <summary>
/// Domain model for shipment status dto.
/// </summary>
    public enum ShipmentStatusDto
{
    /// <summary>
    /// Code summary.
    /// </summary>
    Draft = 0,
    /// <summary>
    /// Code summary.
    /// </summary>
    Booked = 1,
    /// <summary>
    /// Code summary.
    /// </summary>
    PickedUp = 2,
    /// <summary>
    /// Code summary.
    /// </summary>
    InTransit = 3,
    /// <summary>
    /// Code summary.
    /// </summary>
    OutForDelivery = 4,
    /// <summary>
    /// Code summary.
    /// </summary>
    Delivered = 5
}


