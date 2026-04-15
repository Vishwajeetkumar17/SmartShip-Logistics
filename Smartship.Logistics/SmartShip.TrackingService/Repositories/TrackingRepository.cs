using Microsoft.EntityFrameworkCore;
using SmartShip.TrackingService.Data;
using SmartShip.TrackingService.Models;

namespace SmartShip.TrackingService.Repositories;

/// <summary>
/// Repository for tracking data access operations.
/// </summary>
public class TrackingRepository : ITrackingRepository
{
    #region Fields
    private readonly TrackingDbContext _context;
    #endregion

    #region Constructor
    /// <summary>
    /// Provides persistence operations for tracking data.
    /// </summary>
    public TrackingRepository(TrackingDbContext context)
    {
        _context = context;
    }
    #endregion

    #region Public API
    /// <summary>
    /// Returns events async.
    /// </summary>
    public async Task<List<TrackingEvent>> GetEventsAsync(string trackingNumber)
    {
        return await _context.TrackingEvents
            .AsNoTracking()
            .Where(e => e.TrackingNumber == trackingNumber)
            .OrderByDescending(e => e.Timestamp)
            .ThenByDescending(e => e.EventId)
            .ToListAsync();
    }

    /// <summary>
    /// Returns event by id async.
    /// </summary>
    public async Task<TrackingEvent?> GetEventByIdAsync(int id)
    {
        return await _context.TrackingEvents.FindAsync(id);
    }
    #endregion

    #region Public API
    /// <summary>
    /// Adds event async.
    /// </summary>
    public async Task AddEventAsync(TrackingEvent trackingEvent)
    {
        var duplicateWindowStart = trackingEvent.Timestamp.AddSeconds(-1);
        var duplicateWindowEnd = trackingEvent.Timestamp.AddSeconds(1);

        // Protects against duplicate broker deliveries that differ only by milliseconds.
        var hasNearDuplicate = await _context.TrackingEvents
            .AsNoTracking()
            .AnyAsync(e =>
                e.TrackingNumber == trackingEvent.TrackingNumber &&
                e.Status == trackingEvent.Status &&
                e.Location == trackingEvent.Location &&
                e.Description == trackingEvent.Description &&
                e.Timestamp >= duplicateWindowStart &&
                e.Timestamp <= duplicateWindowEnd);

        if (hasNearDuplicate)
        {
            return;
        }

        await _context.TrackingEvents.AddAsync(trackingEvent);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Updates event async.
    /// </summary>
    public async Task UpdateEventAsync(TrackingEvent trackingEvent)
    {
        _context.TrackingEvents.Update(trackingEvent);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes event async.
    /// </summary>
    public async Task DeleteEventAsync(TrackingEvent trackingEvent)
    {
        _context.TrackingEvents.Remove(trackingEvent);
        await _context.SaveChangesAsync();
    }
    #endregion

    #region Public API
    /// <summary>
    /// Returns locations async.
    /// </summary>
    public async Task<List<ShipmentLocation>> GetLocationsAsync(string trackingNumber)
    {
        return await _context.ShipmentLocations
            .AsNoTracking()
            .Where(l => l.TrackingNumber == trackingNumber)
            .OrderByDescending(l => l.Timestamp)
            .ThenByDescending(l => l.LocationId)
            .ToListAsync();
    }

    /// <summary>
    /// Returns latest location async.
    /// </summary>
    public async Task<ShipmentLocation?> GetLatestLocationAsync(string trackingNumber)
    {
        return await _context.ShipmentLocations
            .AsNoTracking()
            .Where(l => l.TrackingNumber == trackingNumber)
            .OrderByDescending(l => l.Timestamp)
            .ThenByDescending(l => l.LocationId)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Adds location async.
    /// </summary>
    public async Task AddLocationAsync(ShipmentLocation location)
    {
        await _context.ShipmentLocations.AddAsync(location);
        await _context.SaveChangesAsync();
    }
    #endregion
}


