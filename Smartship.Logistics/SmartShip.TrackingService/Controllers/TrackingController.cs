using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartShip.TrackingService.DTOs;
using SmartShip.TrackingService.Helpers;
using SmartShip.TrackingService.Services;

namespace SmartShip.TrackingService.Controllers;

[ApiController]
[Route("api/tracking")]
/// <summary>
/// Public and admin tracking API: consolidated status, timeline, events, and last-known location by tracking number.
/// </summary>
public class TrackingController : ControllerBase
{
    private readonly ITrackingService _service;

    /// <summary>
    /// Initializes the controller with tracking read and update workflows.
    /// </summary>
    public TrackingController(ITrackingService service)
    {
        _service = service;
    }

    [HttpGet("{trackingNumber}")]
    /// <summary>
    /// Returns a consolidated tracking view: latest status, location snapshot, and summary fields.
    /// </summary>
    public async Task<IActionResult> GetTrackingInfo(string trackingNumber)
    {
        var result = await _service.GetTrackingInfoAsync(trackingNumber);
        return Ok(result);
    }

    [HttpGet("{trackingNumber}/timeline")]
    /// <summary>
    /// Returns the ordered milestone timeline for the shipment (status history).
    /// </summary>
    public async Task<IActionResult> GetTimeline(string trackingNumber)
    {
        var result = await _service.GetTimelineAsync(trackingNumber);
        return Ok(result);
    }

    [HttpGet("{trackingNumber}/events")]
    /// <summary>
    /// Returns raw tracking events for diagnostics or detailed audit views.
    /// </summary>
    public async Task<IActionResult> GetEvents(string trackingNumber)
    {
        var result = await _service.GetEventsAsync(trackingNumber);
        return Ok(result);
    }

    [HttpPost("events")]
    [Authorize(Roles = "ADMIN")]
    /// <summary>
    /// Appends a tracking event for a shipment (admin integration and operations).
    /// </summary>
    public async Task<IActionResult> AddEvent([FromBody] TrackingEventDTO dto)
    {
        var result = await _service.AddTrackingEventAsync(dto);
        return Ok(result);
    }

    [HttpPut("events/{id:int}")]
    [Authorize(Roles = "ADMIN")]
    /// <summary>
    /// Corrects or enriches an existing tracking event by id (admin).
    /// </summary>
    public async Task<IActionResult> UpdateEvent(int id, [FromBody] TrackingEventDTO dto)
    {
        await _service.UpdateTrackingEventAsync(id, dto);
        return Ok();
    }

    [HttpDelete("events/{id:int}")]
    [Authorize(Roles = "ADMIN")]
    /// <summary>
    /// Removes a tracking event by id (admin; use sparingly for data correction).
    /// </summary>
    public async Task<IActionResult> DeleteEvent(int id)
    {
        await _service.DeleteTrackingEventAsync(id);
        return Ok();
    }

    [HttpPost("location")]
    [Authorize(Roles = "ADMIN")]
    /// <summary>
    /// Records a GPS or hub-reported location update for a shipment (admin).
    /// </summary>
    public async Task<IActionResult> AddLocationUpdate([FromBody] LocationUpdateDTO dto)
    {
        await _service.AddLocationUpdateAsync(dto);
        return Ok();
    }

    [HttpGet("location/{trackingNumber}")]
    /// <summary>
    /// Returns the latest known location for the shipment, if any.
    /// </summary>
    public async Task<IActionResult> GetLocation(string trackingNumber)
    {
        var result = await _service.GetLatestLocationAsync(trackingNumber);
        return Ok(result);
    }

    [HttpGet("{trackingNumber}/status")]
    /// <summary>
    /// Returns the current delivery status code and label for the shipment.
    /// </summary>
    public async Task<IActionResult> GetStatus(string trackingNumber)
    {
        var result = await _service.GetDeliveryStatusAsync(trackingNumber);
        return Ok(result);
    }

    [HttpPut("{trackingNumber}/status")]
    [Authorize(Roles = "ADMIN")]
    /// <summary>
    /// Updates delivery status from an admin or driver workflow (authorized roles).
    /// </summary>
    public async Task<IActionResult> UpdateStatus(string trackingNumber, [FromBody] StatusUpdateDTO dto)
    {
        dto.TrackingNumber = TrackingValidationHelper.NormalizeTrackingNumber(trackingNumber);
        await _service.UpdateDeliveryStatusAsync(trackingNumber, dto);
        return Ok();
    }
}


