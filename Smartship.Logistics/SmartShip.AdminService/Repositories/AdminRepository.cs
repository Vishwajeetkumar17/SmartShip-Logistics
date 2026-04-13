/// <summary>
/// Provides backend implementation for AdminRepository.
/// </summary>

using Microsoft.EntityFrameworkCore;
using SmartShip.AdminService.Data;
using SmartShip.AdminService.Models;

namespace SmartShip.AdminService.Repositories;

/// <summary>
/// Represents AdminRepository.
/// </summary>
public class AdminRepository : IAdminRepository
{
    private readonly AdminDbContext _context;

    public AdminRepository(AdminDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Executes GetAllHubsAsync.
    /// </summary>
    public async Task<List<Hub>> GetAllHubsAsync()
    {
        return await _context.Hubs
            .AsNoTracking()
            .Include(h => h.ServiceLocations)
            .ToListAsync();
    }

    /// <summary>
    /// Executes GetHubByIdAsync.
    /// </summary>
    public async Task<Hub?> GetHubByIdAsync(int hubId)
    {
        return await _context.Hubs
            .Include(h => h.ServiceLocations)
            .FirstOrDefaultAsync(h => h.HubId == hubId);
    }

    /// <summary>
    /// Executes AddHubAsync.
    /// </summary>
    public async Task AddHubAsync(Hub hub)
    {
        await _context.Hubs.AddAsync(hub);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Executes UpdateHubAsync.
    /// </summary>
    public async Task UpdateHubAsync(Hub hub)
    {
        _context.Hubs.Update(hub);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Executes DeleteHubAsync.
    /// </summary>
    public async Task DeleteHubAsync(Hub hub)
    {
        _context.Hubs.Remove(hub);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Executes HubNameExistsAsync.
    /// </summary>
    public async Task<bool> HubNameExistsAsync(string name, int? excludingHubId = null)
    {
        return await _context.Hubs.AnyAsync(h => h.Name == name && (!excludingHubId.HasValue || h.HubId != excludingHubId.Value));
    }

    /// <summary>
    /// Executes GetAllLocationsAsync.
    /// </summary>
    public async Task<List<ServiceLocation>> GetAllLocationsAsync()
    {
        return await _context.ServiceLocations
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Executes GetLocationByIdAsync.
    /// </summary>
    public async Task<ServiceLocation?> GetLocationByIdAsync(int locationId)
    {
        return await _context.ServiceLocations.FindAsync(locationId);
    }

    /// <summary>
    /// Executes AddLocationAsync.
    /// </summary>
    public async Task AddLocationAsync(ServiceLocation location)
    {
        await _context.ServiceLocations.AddAsync(location);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Executes UpdateLocationAsync.
    /// </summary>
    public async Task UpdateLocationAsync(ServiceLocation location)
    {
        _context.ServiceLocations.Update(location);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Executes DeleteLocationAsync.
    /// </summary>
    public async Task DeleteLocationAsync(ServiceLocation location)
    {
        _context.ServiceLocations.Remove(location);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Executes ZipCodeExistsAsync.
    /// </summary>
    public async Task<bool> ZipCodeExistsAsync(string zipCode, int? excludingLocationId = null)
    {
        return await _context.ServiceLocations.AnyAsync(l => l.ZipCode == zipCode && (!excludingLocationId.HasValue || l.LocationId != excludingLocationId.Value));
    }

    /// <summary>
    /// Executes GetActiveExceptionsAsync.
    /// </summary>
    public async Task<List<ExceptionRecord>> GetActiveExceptionsAsync()
    {
        return await _context.ExceptionRecords
            .AsNoTracking()
            .Where(e => e.Status == "Open")
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Executes GetExceptionByIdAsync.
    /// </summary>
    public async Task<ExceptionRecord?> GetExceptionByIdAsync(int exceptionId)
    {
        return await _context.ExceptionRecords.FindAsync(exceptionId);
    }

    /// <summary>
    /// Executes GetExceptionByShipmentIdAsync.
    /// </summary>
    public async Task<ExceptionRecord?> GetExceptionByShipmentIdAsync(int shipmentId)
    {
        return await _context.ExceptionRecords
            .Where(e => e.ShipmentId == shipmentId && e.Status == "Open")
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Executes GetOpenExceptionByShipmentAndTypeAsync.
    /// </summary>
    public async Task<ExceptionRecord?> GetOpenExceptionByShipmentAndTypeAsync(int shipmentId, string exceptionType)
    {
        var normalizedType = exceptionType.Trim();
        return await _context.ExceptionRecords
            .Where(e => e.ShipmentId == shipmentId && e.Status == "Open" && e.ExceptionType == normalizedType)
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Executes AddExceptionRecordAsync.
    /// </summary>
    public async Task AddExceptionRecordAsync(ExceptionRecord record)
    {
        await _context.ExceptionRecords.AddAsync(record);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Executes UpdateExceptionRecordAsync.
    /// </summary>
    public async Task UpdateExceptionRecordAsync(ExceptionRecord record)
    {
        _context.ExceptionRecords.Update(record);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Executes GetTotalActiveHubsAsync.
    /// </summary>
    public async Task<int> GetTotalActiveHubsAsync()
    {
        return await _context.Hubs.CountAsync(h => h.IsActive);
    }

    /// <summary>
    /// Executes GetTotalActiveExceptionsAsync.
    /// </summary>
    public async Task<int> GetTotalActiveExceptionsAsync()
    {
        return await _context.ExceptionRecords.CountAsync(e => e.Status == "Open");
    }
}


