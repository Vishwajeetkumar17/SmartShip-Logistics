using Microsoft.EntityFrameworkCore;
using SmartShip.ShipmentService.Data;
using SmartShip.ShipmentService.Models;

namespace SmartShip.ShipmentService.Repositories;

/// <summary>
/// Repository for package data access operations.
/// </summary>
public class PackageRepository : IPackageRepository
{
    private readonly ShipmentDbContext _context;

    /// <summary>
    /// Provides persistence operations for package data.
    /// </summary>
    public PackageRepository(ShipmentDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Returns packages for a shipment identifier.
    /// </summary>
    public async Task<List<Package>> GetByShipmentIdAsync(int shipmentId)
    {
        return await _context.Packages
            .Where(p => p.ShipmentId == shipmentId)
            .ToListAsync();
    }

    /// <summary>
    /// Returns a record by identifier.
    /// </summary>
    public async Task<Package?> GetByIdAsync(int packageId)
    {
        return await _context.Packages.FindAsync(packageId);
    }

    /// <summary>
    /// Adds async.
    /// </summary>
    public async Task AddAsync(Package package)
    {
        await _context.Packages.AddAsync(package);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Updates async.
    /// </summary>
    public async Task UpdateAsync(Package package)
    {
        _context.Packages.Update(package);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes async.
    /// </summary>
    public async Task DeleteAsync(Package package)
    {
        _context.Packages.Remove(package);
        await _context.SaveChangesAsync();
    }
}


