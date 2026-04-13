/// <summary>
/// Provides backend implementation for TrackingController.
/// </summary>

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartShip.TrackingService.DTOs;
using SmartShip.TrackingService.Helpers;
using SmartShip.TrackingService.Services;

namespace SmartShip.TrackingService.Controllers;

[ApiController]
[Route("api/tracking")]
/// <summary>
/// Represents TrackingController.
/// </summary>
public class TrackingController : ControllerBase
{
    private readonly ITrackingService _service;

    public TrackingController(ITrackingService service)
    {
        _service = service;
    }

    [HttpGet("{trackingNumber}")]
    /// <summary>
    /// Executes GetTrackingInfo.
    /// </summary>
    public async Task<IActionResult> GetTrackingInfo(string trackingNumber)
    {
        var result = await _service.GetTrackingInfoAsync(trackingNumber);
        return Ok(result);
    }

    [HttpGet("{trackingNumber}/timeline")]
    /// <summary>
    /// Executes GetTimeline.
    /// </summary>
    public async Task<IActionResult> GetTimeline(string trackingNumber)
    {
        var result = await _service.GetTimelineAsync(trackingNumber);
        return Ok(result);
    }

    [HttpGet("{trackingNumber}/events")]
    /// <summary>
    /// Executes GetEvents.
    /// </summary>
    public async Task<IActionResult> GetEvents(string trackingNumber)
    {
        var result = await _service.GetEventsAsync(trackingNumber);
        return Ok(result);
    }

    [HttpPost("events")]
    [Authorize(Roles = "ADMIN")]
    /// <summary>
    /// Executes AddEvent.
    /// </summary>
    public async Task<IActionResult> AddEvent([FromBody] TrackingEventDTO dto)
    {
        var result = await _service.AddTrackingEventAsync(dto);
        return Ok(result);
    }

    [HttpPut("events/{id:int}")]
    [Authorize(Roles = "ADMIN")]
    /// <summary>
    /// Executes UpdateEvent.
    /// </summary>
    public async Task<IActionResult> UpdateEvent(int id, [FromBody] TrackingEventDTO dto)
    {
        await _service.UpdateTrackingEventAsync(id, dto);
        return Ok();
    }

    [HttpDelete("events/{id:int}")]
    [Authorize(Roles = "ADMIN")]
    /// <summary>
    /// Executes DeleteEvent.
    /// </summary>
    public async Task<IActionResult> DeleteEvent(int id)
    {
        await _service.DeleteTrackingEventAsync(id);
        return Ok();
    }

    [HttpPost("location")]
    [Authorize(Roles = "ADMIN")]
    /// <summary>
    /// Executes AddLocationUpdate.
    /// </summary>
    public async Task<IActionResult> AddLocationUpdate([FromBody] LocationUpdateDTO dto)
    {
        await _service.AddLocationUpdateAsync(dto);
        return Ok();
    }

    [HttpGet("location/{trackingNumber}")]
    /// <summary>
    /// Executes GetLocation.
    /// </summary>
    public async Task<IActionResult> GetLocation(string trackingNumber)
    {
        var result = await _service.GetLatestLocationAsync(trackingNumber);
        return Ok(result);
    }

    [HttpGet("{trackingNumber}/status")]
    /// <summary>
    /// Executes GetStatus.
    /// </summary>
    public async Task<IActionResult> GetStatus(string trackingNumber)
    {
        var result = await _service.GetDeliveryStatusAsync(trackingNumber);
        return Ok(result);
    }

    [HttpPut("{trackingNumber}/status")]
    [Authorize(Roles = "ADMIN")]
    /// <summary>
    /// Executes UpdateStatus.
    /// </summary>
    public async Task<IActionResult> UpdateStatus(string trackingNumber, [FromBody] StatusUpdateDTO dto)
    {
        dto.TrackingNumber = TrackingValidationHelper.NormalizeTrackingNumber(trackingNumber);
        await _service.UpdateDeliveryStatusAsync(trackingNumber, dto);
        return Ok();
    }
}


