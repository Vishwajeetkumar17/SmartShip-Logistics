/// <summary>
/// Provides backend implementation for IPackageRepository.
/// </summary>

using SmartShip.ShipmentService.Models;

namespace SmartShip.ShipmentService.Repositories;

/// <summary>
/// Represents IPackageRepository.
/// </summary>
public interface IPackageRepository
{
    Task<List<Package>> GetByShipmentIdAsync(int shipmentId);
    Task<Package?> GetByIdAsync(int packageId);
    Task AddAsync(Package package);
    Task UpdateAsync(Package package);
    Task DeleteAsync(Package package);
}


