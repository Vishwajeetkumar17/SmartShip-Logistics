using SmartShip.ShipmentService.DTOs;

namespace SmartShip.ShipmentService.Services;

/// <summary>
/// Defines package business operations used by the service layer.
/// </summary>
public interface IPackageService
{
    Task AddPackage(int shipmentId, PackageDTO dto);
    Task<List<PackageDTO>> GetPackages(int shipmentId);
    Task UpdatePackage(int shipmentId, int packageId, PackageDTO dto);
    Task DeletePackage(int shipmentId, int packageId);
}


