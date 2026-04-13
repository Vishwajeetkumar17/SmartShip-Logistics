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


    }

    /// <summary>
    /// Asynchronously handles the get location process.
    /// </summary>
    [HttpGet("location/{trackingNumber}")]
    /// <summary>
    /// Fetches the last known geo-location or hub scan of the shipment.
    /// </summary>
    /// <param name="trackingNumber">The tracking code.</param>
    /// <returns>The latest location details.</returns>
    public async Task<IActionResult> GetLocation(string trackingNumber)
    {
        var result = await _service.GetLatestLocationAsync(trackingNumber);
        return Ok(result);
    }

    #endregion

    #region Delivery Status Management

    /// <summary>
    /// Asynchronously handles the get status process.
    /// </summary>
    [HttpGet("{trackingNumber}/status")]
    /// <summary>
    /// Quickly resolves the strictly formatted overarching delivery status code.
    /// </summary>
    /// <param name="trackingNumber">The tracking code.</param>
    /// <returns>The enum or string representation of the final status.</returns>
    public async Task<IActionResult> GetStatus(string trackingNumber)
    {
        var result = await _service.GetDeliveryStatusAsync(trackingNumber);
        return Ok(result);
    }

    /// <summary>
    /// Asynchronously handles the update status process.
    /// </summary>
    [HttpPut("{trackingNumber}/status")]
    [Authorize(Roles = "ADMIN")]
    /// <summary>
    /// Updates the global delivery status, which triggers respective final-state logic.
    /// </summary>
    /// <param name="trackingNumber">The tracking code.</param>
    /// <param name="dto">The new status update wrapper.</param>
    /// <returns>Success acknowledgment.</returns>
    public async Task<IActionResult> UpdateStatus(string trackingNumber, [FromBody] StatusUpdateDTO dto)
    {
        dto.TrackingNumber = TrackingValidationHelper.NormalizeTrackingNumber(trackingNumber);
        await _service.UpdateDeliveryStatusAsync(trackingNumber, dto);
        return Ok();
    }

    #endregion
}


