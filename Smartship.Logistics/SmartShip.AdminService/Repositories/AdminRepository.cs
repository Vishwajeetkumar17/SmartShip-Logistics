using Microsoft.EntityFrameworkCore;
using SmartShip.AdminService.Data;
using SmartShip.AdminService.Models;

namespace SmartShip.AdminService.Repositories;

/// <summary>
/// Repository for admin data access operations.
/// </summary>
public class AdminRepository : IAdminRepository
{
    private readonly AdminDbContext _context;

    /// <summary>
    /// Initializes admin repository dependencies.
    /// </summary>
    /// <param name="context">The injected Admin database context.</param>
    public AdminRepository(AdminDbContext context)
    {
        _context = context;
    }

    #region Hub Operations

    /// <summary>
    /// Returns all hubs.
    /// </summary>
    public async Task<List<Hub>> GetAllHubsAsync()
    {
        return await _context.Hubs
            .AsNoTracking()
            .Include(h => h.ServiceLocations)
            .ToListAsync();
    }

    /// <summary>
    /// Returns hub by id.
    /// </summary>
    public async Task<Hub?> GetHubByIdAsync(int hubId)
    {
        return await _context.Hubs
            .Include(h => h.ServiceLocations)
            .FirstOrDefaultAsync(h => h.HubId == hubId);
    }

    /// <summary>
    /// Adds hub.
    /// </summary>
    public async Task AddHubAsync(Hub hub)
    {
        await _context.Hubs.AddAsync(hub);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Updates hub.
    /// </summary>
    public async Task UpdateHubAsync(Hub hub)
    {
        _context.Hubs.Update(hub);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes hub.
    /// </summary>
    public async Task DeleteHubAsync(Hub hub)
    {
        _context.Hubs.Remove(hub);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Checks whether a hub name is already in use.
    /// </summary>
    public async Task<bool> HubNameExistsAsync(string name, int? excludingHubId = null)
    {
        return await _context.Hubs.AnyAsync(h => h.Name == name && (!excludingHubId.HasValue || h.HubId != excludingHubId.Value));
    }

    #endregion

    #region Service Location Operations

    /// <summary>
    /// Returns all locations.
    /// </summary>
    public async Task<List<ServiceLocation>> GetAllLocationsAsync()
    {
        return await _context.ServiceLocations
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Returns location by id.
    /// </summary>
    public async Task<ServiceLocation?> GetLocationByIdAsync(int locationId)
    {
        return await _context.ServiceLocations.FindAsync(locationId);
    }

    /// <summary>
    /// Adds location.
    /// </summary>
    public async Task AddLocationAsync(ServiceLocation location)
    {
        await _context.ServiceLocations.AddAsync(location);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Updates location.
    /// </summary>
    public async Task UpdateLocationAsync(ServiceLocation location)
    {
        _context.ServiceLocations.Update(location);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes location.
    /// </summary>
    public async Task DeleteLocationAsync(ServiceLocation location)
    {
        _context.ServiceLocations.Remove(location);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Checks whether a ZIP code is already assigned to a location.
    /// </summary>
    public async Task<bool> ZipCodeExistsAsync(string zipCode, int? excludingLocationId = null)
    {
        return await _context.ServiceLocations.AnyAsync(l => l.ZipCode == zipCode && (!excludingLocationId.HasValue || l.LocationId != excludingLocationId.Value));
    }

    #endregion

    #region Exception Record Operations

    /// <summary>
    /// Returns active exceptions.
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
    /// Returns exception by id.
    /// </summary>
    public async Task<ExceptionRecord?> GetExceptionByIdAsync(int exceptionId)
    {
        return await _context.ExceptionRecords.FindAsync(exceptionId);
    }

    /// <summary>
    /// Returns exception by shipment id.
    /// </summary>
    public async Task<ExceptionRecord?> GetExceptionByShipmentIdAsync(int shipmentId)
    {
        return await _context.ExceptionRecords
            .Where(e => e.ShipmentId == shipmentId && e.Status == "Open")
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Returns open exception by shipment and type.
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
    /// Adds exception record.
    /// </summary>
    public async Task AddExceptionRecordAsync(ExceptionRecord record)
    {
        await _context.ExceptionRecords.AddAsync(record);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Updates exception record.
    /// </summary>
    public async Task UpdateExceptionRecordAsync(ExceptionRecord record)
    {
        _context.ExceptionRecords.Update(record);
        await _context.SaveChangesAsync();
    }

    #endregion

    #region Aggregation Queries

    /// <summary>
    /// Returns total active hubs.
    /// </summary>
    public async Task<int> GetTotalActiveHubsAsync()
    {
        return await _context.Hubs.CountAsync(h => h.IsActive);
    }

    /// <summary>
    /// Returns total active exceptions.
    /// </summary>
    public async Task<int> GetTotalActiveExceptionsAsync()
    {
        return await _context.ExceptionRecords.CountAsync(e => e.Status == "Open");
    }

    #endregion
}
