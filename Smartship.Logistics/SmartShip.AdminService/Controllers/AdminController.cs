/// <summary>
/// Provides backend implementation for AdminController.
/// </summary>

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SmartShip.AdminService.DTOs;
using SmartShip.AdminService.Services;

namespace SmartShip.AdminService.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "ADMIN")]
/// <summary>
/// Represents AdminController.
/// </summary>
public class AdminController : ControllerBase
{
    private readonly IAdminService _service;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IAdminService service, ILogger<AdminController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("logging-demo")]
    /// <summary>
    /// Executes LoggingDemo.
    /// </summary>
    public IActionResult LoggingDemo([FromQuery] bool fail = false)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub")
                     ?? "anonymous";
        var requestId = HttpContext.TraceIdentifier;

        _logger.LogInformation(
            "Controller logging demo invoked by UserId {UserId} with RequestId {RequestId}",
            userId,
            requestId);

        _service.LogLevelDemo(userId, requestId, fail);
        return Ok(new
        {
            Message = "Logging demo completed.",
            UserId = userId,
            RequestId = requestId,
            SimulatedFailure = fail
        });
    }

    // Dashboard & Stats
    [HttpGet("dashboard")]
    /// <summary>
    /// Executes GetDashboard.
    /// </summary>
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _service.GetDashboardMetricsAsync();
        return Ok(result);
    }

    [HttpGet("statistics")]
    /// <summary>
    /// Executes GetStatistics.
    /// </summary>
    public async Task<IActionResult> GetStatistics()
    {
        var result = await _service.GetComprehensiveStatisticsAsync();
        return Ok(result);
    }

    // Hubs
    [HttpGet("hubs")]
    /// <summary>
    /// Executes GetAllHubs.
    /// </summary>
    public async Task<IActionResult> GetAllHubs([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
    {
        var result = await _service.GetAllHubsAsync(pageNumber, pageSize);
        return Ok(result);
    }

    [HttpGet("hubs/{id:int}")]
    /// <summary>
    /// Executes GetHub.
    /// </summary>
    public async Task<IActionResult> GetHub(int id)
    {
        var result = await _service.GetHubByIdAsync(id);
        return Ok(result);
    }

    [HttpPost("hubs")]
    /// <summary>
    /// Executes CreateHub.
    /// </summary>
    public async Task<IActionResult> CreateHub([FromBody] CreateHubDTO dto)
    {
        var result = await _service.CreateHubAsync(dto);
        return Ok(result);
    }

    [HttpPut("hubs/{id:int}")]
    /// <summary>
    /// Executes UpdateHub.
    /// </summary>
    public async Task<IActionResult> UpdateHub(int id, [FromBody] UpdateHubDTO dto)
    {
        var result = await _service.UpdateHubAsync(id, dto);
        return Ok(result);
    }

    [HttpDelete("hubs/{id:int}")]
    /// <summary>
    /// Executes DeleteHub.
    /// </summary>
    public async Task<IActionResult> DeleteHub(int id)
    {
        await _service.DeleteHubAsync(id);
        return Ok();
    }

    // Locations
    [HttpGet("locations")]
    /// <summary>
    /// Executes GetAllLocations.
    /// </summary>
    public async Task<IActionResult> GetAllLocations([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
    {
        var result = await _service.GetAllLocationsAsync(pageNumber, pageSize);
        return Ok(result);
    }

    [HttpPost("locations")]
    /// <summary>
    /// Executes CreateLocation.
    /// </summary>
    public async Task<IActionResult> CreateLocation([FromBody] CreateLocationDTO dto)
    {
        var result = await _service.CreateLocationAsync(dto);
        return Ok(result);
    }

    [HttpPut("locations/{id:int}")]
    /// <summary>
    /// Executes UpdateLocation.
    /// </summary>
    public async Task<IActionResult> UpdateLocation(int id, [FromBody] UpdateLocationDTO dto)
    {
        var result = await _service.UpdateLocationAsync(id, dto);
        return Ok(result);
    }

    [HttpDelete("locations/{id:int}")]
    /// <summary>
    /// Executes DeleteLocation.
    /// </summary>
    public async Task<IActionResult> DeleteLocation(int id)
    {
        await _service.DeleteLocationAsync(id);
        return Ok();
    }

    // Exceptions
    [HttpGet("exceptions")]
    /// <summary>
    /// Executes GetExceptions.
    /// </summary>
    public async Task<IActionResult> GetExceptions([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
    {
        var result = await _service.GetActiveExceptionsAsync(pageNumber, pageSize);
        return Ok(result);
    }

    [HttpPut("shipments/{id:int}/resolve")]
    /// <summary>
    /// Executes ResolveException.
    /// </summary>
    public async Task<IActionResult> ResolveException(int id, [FromBody] ResolveExceptionDTO dto)
    {
        dto.ShipmentId = id;
        var result = await _service.ResolveExceptionAsync(id, dto);
        return Ok(result);
    }

    [HttpPut("shipments/{id:int}/delay")]
    /// <summary>
    /// Executes DelayShipment.
    /// </summary>
    public async Task<IActionResult> DelayShipment(int id, [FromBody] ShipmentActionReasonDTO dto)
    {
        var result = await _service.DelayShipmentAsync(id, dto.Reason);
        return Ok(result);
    }

    [HttpPut("shipments/{id:int}/return")]
    /// <summary>
    /// Executes ReturnShipment.
    /// </summary>
    public async Task<IActionResult> ReturnShipment(int id, [FromBody] ShipmentActionReasonDTO dto)
    {
        var result = await _service.ReturnShipmentAsync(id, dto.Reason);
        return Ok(result);
    }

    // Shipment Monitoring Integration
    [HttpGet("shipments")]
    /// <summary>
    /// Executes GetShipments.
    /// </summary>
    public async Task<IActionResult> GetShipments([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
    {
        var result = await _service.GetShipmentsAsync(pageNumber, pageSize);
        return Ok(result);
    }

    [HttpGet("shipments/{id:int}")]
    /// <summary>
    /// Executes GetShipmentById.
    /// </summary>
    public async Task<IActionResult> GetShipmentById(int id)
    {
        var result = await _service.GetShipmentByIdAsync(id);
        return Ok(result);
    }

    [HttpGet("shipments/hub/{hubId:int}")]
    /// <summary>
    /// Executes GetShipmentsByHub.
    /// </summary>
    public async Task<IActionResult> GetShipmentsByHub(int hubId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
    {
        var result = await _service.GetShipmentsByHubAsync(hubId, pageNumber, pageSize);
        return Ok(result);
    }

    // Reports Integration
    [HttpGet("reports")]
    /// <summary>
    /// Executes GetReports.
    /// </summary>
    public async Task<IActionResult> GetReports()
    {
        var result = await _service.GetReportsOverviewAsync();
        return Ok(result);
    }

    [HttpGet("reports/shipment-performance")]
    /// <summary>
    /// Executes GetShipmentPerformance.
    /// </summary>
    public async Task<IActionResult> GetShipmentPerformance()
    {
        var result = await _service.GetShipmentPerformanceAsync();
        return Ok(result);
    }

    [HttpGet("reports/delivery-sla")]
    /// <summary>
    /// Executes GetDeliverySLA.
    /// </summary>
    public async Task<IActionResult> GetDeliverySLA()
    {
        var result = await _service.GetDeliverySLAAsync();
        return Ok(result);
    }

    [HttpGet("reports/revenue")]
    /// <summary>
    /// Executes GetRevenue.
    /// </summary>
    public async Task<IActionResult> GetRevenue()
    {
        var result = await _service.GetRevenueAsync();
        return Ok(result);
    }

    [HttpGet("reports/hub-performance")]
    /// <summary>
    /// Executes GetHubPerformance.
    /// </summary>
    public async Task<IActionResult> GetHubPerformance()
    {
        var result = await _service.GetHubPerformanceAsync();
        return Ok(result);
    }
}


