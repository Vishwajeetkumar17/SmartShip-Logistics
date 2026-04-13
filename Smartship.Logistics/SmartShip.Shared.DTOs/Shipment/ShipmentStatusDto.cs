/// <summary>
/// Provides backend implementation for ShipmentStatusDto.
/// </summary>

namespace SmartShip.Shared.DTOs.Shipment;

public enum ShipmentStatusDto
{
    Draft = 0,
    Booked = 1,
    PickedUp = 2,
    InTransit = 3,
    OutForDelivery = 4,
    Delivered = 5
}


