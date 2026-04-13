/// <summary>
/// Provides backend implementation for TrackingRepository.
/// </summary>

using Microsoft.EntityFrameworkCore;
using SmartShip.TrackingService.Data;
using SmartShip.TrackingService.Models;

namespace SmartShip.TrackingService.Repositories;

/// <summary>
/// Represents TrackingRepository.
/// </summary>
public class TrackingRepository : ITrackingRepository
{
    private readonly TrackingDbContext _context;

    public TrackingRepository(TrackingDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Executes GetEventsAsync.
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
    /// Executes GetEventByIdAsync.
    /// </summary>
    public async Task<TrackingEvent?> GetEventByIdAsync(int id)
    {
        return await _context.TrackingEvents.FindAsync(id);
    }

    /// <summary>
    /// Executes AddEventAsync.
    /// </summary>
    public async Task AddEventAsync(TrackingEvent trackingEvent)
    {
        var duplicateWindowStart = trackingEvent.Timestamp.AddSeconds(-1);
        var duplicateWindowEnd = trackingEvent.Timestamp.AddSeconds(1);

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
    /// Executes UpdateEventAsync.
    /// </summary>
    public async Task UpdateEventAsync(TrackingEvent trackingEvent)
    {
        _context.TrackingEvents.Update(trackingEvent);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Executes DeleteEventAsync.
    /// </summary>
    public async Task DeleteEventAsync(TrackingEvent trackingEvent)
    {
        _context.TrackingEvents.Remove(trackingEvent);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Executes GetLocationsAsync.
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
    /// Executes GetLatestLocationAsync.
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
    /// Executes AddLocationAsync.
    /// </summary>
    public async Task AddLocationAsync(ShipmentLocation location)
    {
        await _context.ShipmentLocations.AddAsync(location);
        await _context.SaveChangesAsync();
    }
}


