/// <summary>
/// Provides backend implementation for IShipmentRepository.
/// </summary>

using SmartShip.ShipmentService.Models;

namespace SmartShip.ShipmentService.Repositories;

/// <summary>
/// Represents IShipmentRepository.
/// </summary>
public interface IShipmentRepository
{
    Task<List<Shipment>> GetAllAsync();

    Task<Shipment?> GetByIdAsync(int id);

    Task<Shipment?> GetByTrackingNumberAsync(string trackingNumber);

    Task<List<Shipment>> GetByCustomerAsync(int customerId);

    Task CreateAsync(Shipment shipment);

    Task UpdateAsync(Shipment shipment);

    Task DeleteAsync(Shipment shipment);
}


