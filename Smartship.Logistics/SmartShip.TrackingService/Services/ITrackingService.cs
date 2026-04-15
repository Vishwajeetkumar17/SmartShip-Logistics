using SmartShip.TrackingService.DTOs;

namespace SmartShip.TrackingService.Services;

/// <summary>
/// Defines tracking business operations used by the service layer.
/// </summary>
public interface ITrackingService
{
    Task<TrackingResponseDTO> GetTrackingInfoAsync(string trackingNumber);
    Task<List<TrackingEventDTO>> GetTimelineAsync(string trackingNumber);
    Task<List<TrackingEventDTO>> GetEventsAsync(string trackingNumber);
    
    Task<TrackingEventDTO> AddTrackingEventAsync(TrackingEventDTO dto);
    Task UpdateTrackingEventAsync(int eventId, TrackingEventDTO dto);
    Task DeleteTrackingEventAsync(int eventId);
    
    Task AddLocationUpdateAsync(LocationUpdateDTO dto);
    Task<LocationUpdateDTO?> GetLatestLocationAsync(string trackingNumber);
    
    Task<StatusUpdateDTO> GetDeliveryStatusAsync(string trackingNumber);
    Task UpdateDeliveryStatusAsync(string trackingNumber, StatusUpdateDTO dto);
}


