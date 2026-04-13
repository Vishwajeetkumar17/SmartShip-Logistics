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

    public PackageRepository(ShipmentDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Executes GetByShipmentIdAsync.
    /// </summary>
    public async Task<List<Package>> GetByShipmentIdAsync(int shipmentId)
    {
        return await _context.Packages
            .Where(p => p.ShipmentId == shipmentId)
            .ToListAsync();
    }

    /// <summary>
    /// Executes GetByIdAsync.
    /// </summary>
    public async Task<Package?> GetByIdAsync(int packageId)
    {
        return await _context.Packages.FindAsync(packageId);
    }

    /// <summary>
    /// Executes AddAsync.
    /// </summary>
    public async Task AddAsync(Package package)
    {
        await _context.Packages.AddAsync(package);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Executes UpdateAsync.
    /// </summary>
    public async Task UpdateAsync(Package package)
    {
        _context.Packages.Update(package);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Executes DeleteAsync.
    /// </summary>
    public async Task DeleteAsync(Package package)
    {
        _context.Packages.Remove(package);
        await _context.SaveChangesAsync();
    }
}


