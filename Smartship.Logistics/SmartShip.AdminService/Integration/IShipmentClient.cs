using SmartShip.Shared.DTOs.Shipment;

namespace SmartShip.AdminService.Integration;

/// <summary>
/// Contract for shipment client behavior.
/// </summary>
public interface IShipmentClient
{
    Task<List<ShipmentExternalDto>> GetAllShipmentsAsync();
    Task<ShipmentExternalDto?> GetShipmentByIdAsync(int shipmentId);
}


