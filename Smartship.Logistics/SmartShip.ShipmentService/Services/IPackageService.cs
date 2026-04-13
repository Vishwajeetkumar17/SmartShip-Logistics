/// <summary>
/// Provides backend implementation for IPackageService.
/// </summary>

using SmartShip.ShipmentService.DTOs;

namespace SmartShip.ShipmentService.Services;

/// <summary>
/// Represents IPackageService.
/// </summary>
public interface IPackageService
{
    Task AddPackage(int shipmentId, PackageDTO dto);
    Task<List<PackageDTO>> GetPackages(int shipmentId);
    Task UpdatePackage(int shipmentId, int packageId, PackageDTO dto);
    Task DeletePackage(int shipmentId, int packageId);
}


