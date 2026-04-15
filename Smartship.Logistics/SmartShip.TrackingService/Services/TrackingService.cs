using SmartShip.Shared.Common.Exceptions;
using SmartShip.Shared.Common.Helpers;
using SmartShip.TrackingService.DTOs;
using SmartShip.TrackingService.Helpers;
using SmartShip.TrackingService.Models;
using SmartShip.TrackingService.Repositories;

namespace SmartShip.TrackingService.Services;

/// <summary>
/// Implements tracking business workflows for SmartShip logistics operations.
/// </summary>
public class TrackingService : ITrackingService
{
    private readonly ITrackingRepository _repository;



    #region Constructor
    /// <summary>
    /// Implements tracking service workflows.
    /// </summary>
    public TrackingService(ITrackingRepository repository)
    {
        _repository = repository;
    }
    #endregion



    #region Public API
    /// <summary>
    /// Returns tracking info async.
    /// </summary>
    public async Task<TrackingResponseDTO> GetTrackingInfoAsync(string trackingNumber)
    {
        trackingNumber = TrackingValidationHelper.NormalizeTrackingNumber(trackingNumber);

        var events = await _repository.GetEventsAsync(trackingNumber);
        if (!events.Any())
        {
            throw new NotFoundException($"No tracking information found for {trackingNumber}");
        }

        var latestEvent = events.First();
        var initialEvent = events.Last();
        var latestLocation = await _repository.GetLatestLocationAsync(trackingNumber);

        return new TrackingResponseDTO
        {
            TrackingNumber = trackingNumber,
            CurrentStatus = latestEvent.Status,
            CurrentLocation = latestLocation != null
                ? $"{latestLocation.Latitude}, {latestLocation.Longitude}"
                : latestEvent.Location,
            InitialEventTimestamp = initialEvent.Timestamp,
            LatestEventTimestamp = latestEvent.Timestamp,
            Timeline = events.Select(MapToDto).ToList()
        };
    }
    #endregion



    #region Public API
    /// <summary>
    /// Returns timeline async.
    /// </summary>
    public async Task<List<TrackingEventDTO>> GetTimelineAsync(string trackingNumber)
    {
        trackingNumber = TrackingValidationHelper.NormalizeTrackingNumber(trackingNumber);

        var events = await _repository.GetEventsAsync(trackingNumber);
        if (!events.Any())
        {
            throw new NotFoundException($"No timeline events found for {trackingNumber}");
        }

        return events.Select(MapToDto).ToList();
    }
    #endregion



    #region Public API
    /// <summary>
    /// Returns events async.
    /// </summary>
    public async Task<List<TrackingEventDTO>> GetEventsAsync(string trackingNumber)
    {
        trackingNumber = TrackingValidationHelper.NormalizeTrackingNumber(trackingNumber);

        var events = await _repository.GetEventsAsync(trackingNumber);
        return events.Select(MapToDto).ToList();
    }
    #endregion



    #region Public API
    /// <summary>
    /// Adds tracking event async.
    /// </summary>
    public async Task<TrackingEventDTO> AddTrackingEventAsync(TrackingEventDTO dto)
    {
        TrackingValidationHelper.ValidateEvent(dto);

        var trackingEvent = new TrackingEvent
        {
            TrackingNumber = TrackingValidationHelper.NormalizeTrackingNumber(dto.TrackingNumber),
            Status = dto.Status.Trim(),
            Location = dto.Location.Trim(),
            Description = dto.Description.Trim(),
            Timestamp = dto.Timestamp == default ? TimeZoneHelper.GetCurrentUtcTime() : dto.Timestamp
        };

        await _repository.AddEventAsync(trackingEvent);
        return MapToDto(trackingEvent);
    }
    #endregion



    #region Public API
    /// <summary>
    /// Updates tracking event async.
    /// </summary>
    public async Task UpdateTrackingEventAsync(int eventId, TrackingEventDTO dto)
    {
        TrackingValidationHelper.ValidateEvent(dto);

        var existingEvent = await _repository.GetEventByIdAsync(eventId)
            ?? throw new NotFoundException($"Tracking event {eventId} not found.");

        existingEvent.TrackingNumber = TrackingValidationHelper.NormalizeTrackingNumber(dto.TrackingNumber);
        existingEvent.Status = dto.Status.Trim();
        existingEvent.Location = dto.Location.Trim();
        existingEvent.Description = dto.Description.Trim();
        existingEvent.Timestamp = dto.Timestamp == default ? existingEvent.Timestamp : dto.Timestamp;

        await _repository.UpdateEventAsync(existingEvent);
    }
    #endregion



    #region Public API
    /// <summary>
    /// Deletes tracking event async.
    /// </summary>
    public async Task DeleteTrackingEventAsync(int eventId)
    {
        var existingEvent = await _repository.GetEventByIdAsync(eventId)
            ?? throw new NotFoundException($"Tracking event {eventId} not found.");

        await _repository.DeleteEventAsync(existingEvent);
    }
    #endregion



    #region Public API
    /// <summary>
    /// Adds location update async.
    /// </summary>
    public async Task AddLocationUpdateAsync(LocationUpdateDTO dto)
    {
        TrackingValidationHelper.ValidateLocation(dto);

        var location = new ShipmentLocation
        {
            TrackingNumber = TrackingValidationHelper.NormalizeTrackingNumber(dto.TrackingNumber),
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Timestamp = dto.Timestamp == default ? TimeZoneHelper.GetCurrentUtcTime() : dto.Timestamp
        };

        await _repository.AddLocationAsync(location);
    }
    #endregion



    #region Public API
    /// <summary>
    /// Returns latest location async.
    /// </summary>
    public async Task<LocationUpdateDTO?> GetLatestLocationAsync(string trackingNumber)
    {
        trackingNumber = TrackingValidationHelper.NormalizeTrackingNumber(trackingNumber);

        var location = await _repository.GetLatestLocationAsync(trackingNumber);
        if (location == null)
        {
            throw new NotFoundException($"No location history found for {trackingNumber}");
        }

        return new LocationUpdateDTO
        {
            TrackingNumber = location.TrackingNumber,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            Timestamp = location.Timestamp
        };
    }
    #endregion



    #region Public API
    /// <summary>
    /// Returns delivery status async.
    /// </summary>
    public async Task<StatusUpdateDTO> GetDeliveryStatusAsync(string trackingNumber)
    {
        trackingNumber = TrackingValidationHelper.NormalizeTrackingNumber(trackingNumber);

        var events = await _repository.GetEventsAsync(trackingNumber);
        var latestEvent = events.FirstOrDefault()
            ?? throw new NotFoundException($"No status found for {trackingNumber}");

        return new StatusUpdateDTO
        {
            TrackingNumber = latestEvent.TrackingNumber,
            Status = latestEvent.Status,
            Location = latestEvent.Location,
            Description = latestEvent.Description
        };
    }
    #endregion



    #region Public API
    /// <summary>
    /// Updates delivery status async.
    /// </summary>
    public async Task UpdateDeliveryStatusAsync(string trackingNumber, StatusUpdateDTO dto)
    {
        TrackingValidationHelper.ValidateStatus(dto, trackingNumber);
        trackingNumber = TrackingValidationHelper.NormalizeTrackingNumber(trackingNumber);

        var trackingEvent = new TrackingEvent
        {
            TrackingNumber = trackingNumber,
            Status = dto.Status.Trim(),
            Location = dto.Location.Trim(),
            Description = dto.Description.Trim(),
            Timestamp = TimeZoneHelper.GetCurrentUtcTime()
        };

        await _repository.AddEventAsync(trackingEvent);
    }
    #endregion



    #region Private Helpers
    /// <summary>
    /// Maps to dto.
    /// </summary>
    private static TrackingEventDTO MapToDto(TrackingEvent trackingEvent)
    {
        return new TrackingEventDTO
        {
            EventId = trackingEvent.EventId,
            TrackingNumber = trackingEvent.TrackingNumber,
            Status = trackingEvent.Status,
            Location = trackingEvent.Location,
            Description = trackingEvent.Description,
            Timestamp = trackingEvent.Timestamp
        };
    }
    #endregion
}




