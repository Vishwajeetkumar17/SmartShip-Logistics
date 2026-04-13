/// <summary>
/// Provides backend implementation for IShipmentClient.
/// </summary>

using SmartShip.Shared.DTOs.Shipment;

namespace SmartShip.AdminService.Integration;

/// <summary>
/// Represents IShipmentClient.
/// </summary>
public interface IShipmentClient
{
    Task<List<ShipmentExternalDto>> GetAllShipmentsAsync();
    Task<ShipmentExternalDto?> GetShipmentByIdAsync(int shipmentId);
}


