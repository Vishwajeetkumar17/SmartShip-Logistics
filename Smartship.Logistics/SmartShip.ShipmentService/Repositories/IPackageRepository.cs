using SmartShip.ShipmentService.Models;

namespace SmartShip.ShipmentService.Repositories;

/// <summary>
/// Contract for ipackage persistence operations.
/// </summary>
public interface IPackageRepository
{
    Task<List<Package>> GetByShipmentIdAsync(int shipmentId);
    Task<Package?> GetByIdAsync(int packageId);
    Task AddAsync(Package package);
    Task UpdateAsync(Package package);
    Task DeleteAsync(Package package);
}


