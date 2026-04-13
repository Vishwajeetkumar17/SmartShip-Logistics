/// <summary>
/// Provides backend implementation for PackageRepository.
/// </summary>

using Microsoft.EntityFrameworkCore;
using SmartShip.ShipmentService.Data;
using SmartShip.ShipmentService.Models;

namespace SmartShip.ShipmentService.Repositories;

/// <summary>
/// Represents PackageRepository.
/// </summary>
public class PackageRepository : IPackageRepository
{
    private readonly ShipmentDbContext _context;

    /// <summary>
    /// Initializes a new instance of the package repository class.
    /// </summary>
    public PackageRepository(ShipmentDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Executes the GetByShipmentIdAsync operation.
    /// </summary>
    public async Task<List<Package>> GetByShipmentIdAsync(int shipmentId)
    {
        return await _context.Packages
            .Where(p => p.ShipmentId == shipmentId)
            .ToListAsync();
    }

    /// <summary>
    /// Executes the GetByIdAsync operation.
    /// </summary>
    public async Task<Package?> GetByIdAsync(int packageId)
    {
        return await _context.Packages.FindAsync(packageId);
    }

    /// <summary>
    /// Executes the AddAsync operation.
    /// </summary>
    public async Task AddAsync(Package package)
    {
        await _context.Packages.AddAsync(package);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Executes the UpdateAsync operation.
    /// </summary>
    public async Task UpdateAsync(Package package)
    {
        _context.Packages.Update(package);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Executes the DeleteAsync operation.
    /// </summary>
    public async Task DeleteAsync(Package package)
    {
        _context.Packages.Remove(package);
        await _context.SaveChangesAsync();
    }
}


