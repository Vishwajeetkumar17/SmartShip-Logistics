/// <summary>
/// Entity Framework Core implementation of the Admin repository.
/// Provides data access for Hubs, Service Locations, and Exception Records
/// using the AdminDbContext.
/// </summary>

using Microsoft.EntityFrameworkCore;
using SmartShip.AdminService.Data;
using SmartShip.AdminService.Models;

namespace SmartShip.AdminService.Repositories;

/// <summary>
/// Concrete EF Core repository for admin data persistence operations.
/// Handles CRUD for hubs, service locations, and shipment exception records.
/// </summary>
public class AdminRepository : IAdminRepository
{
    private readonly AdminDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminRepository"/> class.
    /// </summary>
    /// <param name="context">The injected Admin database context.</param>
    public AdminRepository(AdminDbContext context)
    {
        _context = context;
    }

    #region Hub Operations

    /// <summary>
    /// Retrieves all hubs with their associated service locations (read-only).
    /// </summary>
    public async Task<List<Hub>> GetAllHubsAsync()
    {
        return await _context.Hubs
            .AsNoTracking()
            .Include(h => h.ServiceLocations)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves a single hub by ID, including its service locations (tracked for updates).
    /// </summary>
    public async Task<Hub?> GetHubByIdAsync(int hubId)
    {
        return await _context.Hubs
            .Include(h => h.ServiceLocations)
            .FirstOrDefaultAsync(h => h.HubId == hubId);
    }

    /// <summary>
    /// Persists a new hub entity and commits the transaction.
    /// </summary>
    public async Task AddHubAsync(Hub hub)
    {
        await _context.Hubs.AddAsync(hub);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Updates an existing hub entity and commits the transaction.
    /// </summary>
    public async Task UpdateHubAsync(Hub hub)
    {
        _context.Hubs.Update(hub);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Removes a hub entity from the database and commits the transaction.
    /// </summary>
    public async Task DeleteHubAsync(Hub hub)
    {
        _context.Hubs.Remove(hub);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Checks if a hub name already exists, optionally excluding a specific hub ID (for updates).
    /// </summary>
    public async Task<bool> HubNameExistsAsync(string name, int? excludingHubId = null)
    {
        return await _context.Hubs.AnyAsync(h => h.Name == name && (!excludingHubId.HasValue || h.HubId != excludingHubId.Value));
    }

    #endregion

    #region Service Location Operations

    /// <summary>
    /// Retrieves all service locations (read-only, no tracking).
    /// </summary>
    public async Task<List<ServiceLocation>> GetAllLocationsAsync()
    {
        return await _context.ServiceLocations
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves a single service location by its primary key.
    /// </summary>
    public async Task<ServiceLocation?> GetLocationByIdAsync(int locationId)
    {
        return await _context.ServiceLocations.FindAsync(locationId);
    }

    /// <summary>
    /// Persists a new service location entity and commits the transaction.
    /// </summary>
    public async Task AddLocationAsync(ServiceLocation location)
    {
        await _context.ServiceLocations.AddAsync(location);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Updates an existing service location entity and commits the transaction.
    /// </summary>
    public async Task UpdateLocationAsync(ServiceLocation location)
    {
        _context.ServiceLocations.Update(location);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Removes a service location from the database and commits the transaction.
    /// </summary>
    public async Task DeleteLocationAsync(ServiceLocation location)
    {
        _context.ServiceLocations.Remove(location);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Checks if a ZIP code already exists, optionally excluding a specific location ID (for updates).
    /// </summary>
    public async Task<bool> ZipCodeExistsAsync(string zipCode, int? excludingLocationId = null)
    {
        return await _context.ServiceLocations.AnyAsync(l => l.ZipCode == zipCode && (!excludingLocationId.HasValue || l.LocationId != excludingLocationId.Value));
    }

    #endregion

    #region Exception Record Operations

    /// <summary>
    /// Retrieves all open/active exception records, ordered by most recent first (read-only).
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
    /// Retrieves a single exception record by its primary key.
    /// </summary>
    public async Task<ExceptionRecord?> GetExceptionByIdAsync(int exceptionId)
    {
        return await _context.ExceptionRecords.FindAsync(exceptionId);
    }

    /// <summary>
    /// Finds the most recent open exception for a specific shipment.
    /// </summary>
    public async Task<ExceptionRecord?> GetExceptionByShipmentIdAsync(int shipmentId)
    {
        return await _context.ExceptionRecords
            .Where(e => e.ShipmentId == shipmentId && e.Status == "Open")
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Finds an open exception of a specific type for a shipment (prevents duplicates).
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
    /// Persists a new exception record and commits the transaction.
    /// </summary>
    public async Task AddExceptionRecordAsync(ExceptionRecord record)
    {
        await _context.ExceptionRecords.AddAsync(record);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Updates an existing exception record (e.g., to mark as resolved) and commits.
    /// </summary>
    public async Task UpdateExceptionRecordAsync(ExceptionRecord record)
    {
        _context.ExceptionRecords.Update(record);
        await _context.SaveChangesAsync();
    }

    #endregion

    #region Aggregation Queries

    /// <summary>
    /// Counts all hubs currently marked as active.
    /// </summary>
    public async Task<int> GetTotalActiveHubsAsync()
    {
        return await _context.Hubs.CountAsync(h => h.IsActive);
    }

    /// <summary>
    /// Counts all exception records with an "Open" status.
    /// </summary>
    public async Task<int> GetTotalActiveExceptionsAsync()
    {
        return await _context.ExceptionRecords.CountAsync(e => e.Status == "Open");
    }

    #endregion
}
