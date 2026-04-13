/// <summary>
/// Provides backend implementation for ITrackingRepository.
/// </summary>

using SmartShip.TrackingService.Models;

namespace SmartShip.TrackingService.Repositories;

/// <summary>
/// Represents ITrackingRepository.
/// </summary>
public interface ITrackingRepository
{
    Task<List<TrackingEvent>> GetEventsAsync(string trackingNumber);
    Task<TrackingEvent?> GetEventByIdAsync(int id);
    Task AddEventAsync(TrackingEvent trackingEvent);
    Task UpdateEventAsync(TrackingEvent trackingEvent);
    Task DeleteEventAsync(TrackingEvent trackingEvent);

    Task<List<ShipmentLocation>> GetLocationsAsync(string trackingNumber);
    Task<ShipmentLocation?> GetLatestLocationAsync(string trackingNumber);
    Task AddLocationAsync(ShipmentLocation location);
}


