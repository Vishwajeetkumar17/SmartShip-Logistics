/// <summary>
/// Provides backend implementation for PackageService.
/// </summary>

using SmartShip.Shared.Common.Exceptions;
using SmartShip.ShipmentService.DTOs;
using SmartShip.ShipmentService.Helpers;
using SmartShip.ShipmentService.Models;
using SmartShip.ShipmentService.Repositories;

namespace SmartShip.ShipmentService.Services;

/// <summary>
/// Represents PackageService.
/// </summary>
public class PackageService : IPackageService
{
    private readonly IPackageRepository _repository;
    private readonly IShipmentRepository _shipmentRepository;

    public PackageService(IPackageRepository repository, IShipmentRepository shipmentRepository)
    {
        _repository = repository;
        _shipmentRepository = shipmentRepository;
    }

    /// <summary>
    /// Executes AddPackage.
    /// </summary>
    public async Task AddPackage(int shipmentId, PackageDTO dto)
    {
        PackageValidationHelper.ValidatePackage(dto);

        var shipment = await _shipmentRepository.GetByIdAsync(shipmentId)
            ?? throw new NotFoundException("Shipment not found.");

        ShipmentValidationHelper.EnsureShipmentCanBeModified(shipment);

        var package = new Package
        {
            ShipmentId = shipmentId,
            Weight = dto.Weight,
            Length = dto.Length,
            Width = dto.Width,
            Height = dto.Height,
            Description = dto.Description.Trim()
        };

        await _repository.AddAsync(package);

        shipment.TotalWeight += dto.Weight;
        await _shipmentRepository.UpdateAsync(shipment);
    }

    /// <summary>
    /// Executes GetPackages.
    /// </summary>
    public async Task<List<PackageDTO>> GetPackages(int shipmentId)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(shipmentId)
            ?? throw new NotFoundException("Shipment not found.");

        return shipment.Packages.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Executes UpdatePackage.
    /// </summary>
    public async Task UpdatePackage(int shipmentId, int packageId, PackageDTO dto)
    {
        PackageValidationHelper.ValidatePackage(dto);

        var package = await _repository.GetByIdAsync(packageId)
            ?? throw new NotFoundException("Package not found.");

        if (package.ShipmentId != shipmentId)
        {
            throw new RequestValidationException("Package does not belong to the specified shipment.");
        }

        var shipment = await _shipmentRepository.GetByIdAsync(shipmentId)
            ?? throw new NotFoundException("Shipment not found.");

        ShipmentValidationHelper.EnsureShipmentCanBeModified(shipment);

        shipment.TotalWeight = shipment.TotalWeight - package.Weight + dto.Weight;
        package.Weight = dto.Weight;
        package.Length = dto.Length;
        package.Width = dto.Width;
        package.Height = dto.Height;
        package.Description = dto.Description.Trim();

        await _repository.UpdateAsync(package);
        await _shipmentRepository.UpdateAsync(shipment);
    }

    /// <summary>
    /// Executes DeletePackage.
    /// </summary>
    public async Task DeletePackage(int shipmentId, int packageId)
    {
        var package = await _repository.GetByIdAsync(packageId)
            ?? throw new NotFoundException("Package not found.");

        if (package.ShipmentId != shipmentId)
        {
            throw new RequestValidationException("Package does not belong to the specified shipment.");
        }

        var shipment = await _shipmentRepository.GetByIdAsync(shipmentId)
            ?? throw new NotFoundException("Shipment not found.");

        ShipmentValidationHelper.EnsureShipmentCanBeModified(shipment);

        await _repository.DeleteAsync(package);

        shipment.TotalWeight = Math.Max(0, shipment.TotalWeight - package.Weight);
        await _shipmentRepository.UpdateAsync(shipment);
    }

    private static PackageDTO MapToDto(Package package)
    {
        return new PackageDTO
        {
            Id = package.PackageId,
            Weight = package.Weight,
            Length = package.Length,
            Width = package.Width,
            Height = package.Height,
            Description = package.Description
        };
    }
}


