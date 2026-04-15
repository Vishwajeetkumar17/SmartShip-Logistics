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
/// Admin-only API: hubs and service locations, exceptions, cross-service shipment views, and operational reports.
/// </summary>
public class AdminController : ControllerBase
{
    private readonly IAdminService _service;
    private readonly ILogger<AdminController> _logger;

    /// <summary>
    /// Initializes the controller with admin business services and structured logging.
    /// </summary>
    public AdminController(IAdminService service, ILogger<AdminController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("logging-demo")]
    /// <summary>
    /// Emits sample logs to verify structured logging and correlation IDs.
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
    /// Returns high-level KPIs: active hubs, locations, and open shipment exceptions.
    /// </summary>
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _service.GetDashboardMetricsAsync();
        return Ok(result);
    }

    [HttpGet("statistics")]
    /// <summary>
    /// Returns the comprehensive analytics dashboard (trends, distributions, delivery performance).
    /// </summary>
    public async Task<IActionResult> GetStatistics()
    {
        var result = await _service.GetComprehensiveStatisticsAsync();
        return Ok(result);
    }

    // Hubs
    [HttpGet("hubs")]
    /// <summary>
    /// Returns a paginated list of hubs with their service locations.
    /// </summary>
    public async Task<IActionResult> GetAllHubs([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
    {
        var result = await _service.GetAllHubsAsync(pageNumber, pageSize);
        return Ok(result);
    }

    [HttpGet("hubs/{id:int}")]
    /// <summary>
    /// Returns a single hub by identifier, including nested service locations.
    /// </summary>
    public async Task<IActionResult> GetHub(int id)
    {
        var result = await _service.GetHubByIdAsync(id);
        return Ok(result);
    }

    [HttpPost("hubs")]
    /// <summary>
    /// Creates a new logistics hub (name must be unique).
    /// </summary>
    public async Task<IActionResult> CreateHub([FromBody] CreateHubDTO dto)
    {
        var result = await _service.CreateHubAsync(dto);
        return Ok(result);
    }

    [HttpPut("hubs/{id:int}")]
    /// <summary>
    /// Updates hub metadata and active status.
    /// </summary>
    public async Task<IActionResult> UpdateHub(int id, [FromBody] UpdateHubDTO dto)
    {
        var result = await _service.UpdateHubAsync(id, dto);
        return Ok(result);
    }

    [HttpDelete("hubs/{id:int}")]
    /// <summary>
    /// Deletes a hub when it has no assigned service locations.
    /// </summary>
    public async Task<IActionResult> DeleteHub(int id)
    {
        await _service.DeleteHubAsync(id);
        return Ok();
    }

    // Locations
    [HttpGet("locations")]
    /// <summary>
    /// Returns a paginated list of service locations (ZIP coverage per hub).
    /// </summary>
    public async Task<IActionResult> GetAllLocations([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
    {
        var result = await _service.GetAllLocationsAsync(pageNumber, pageSize);
        return Ok(result);
    }

    [HttpPost("locations")]
    /// <summary>
    /// Adds a service location to a hub (ZIP code must be unique).
    /// </summary>
    public async Task<IActionResult> CreateLocation([FromBody] CreateLocationDTO dto)
    {
        var result = await _service.CreateLocationAsync(dto);
        return Ok(result);
    }

    [HttpPut("locations/{id:int}")]
    /// <summary>
    /// Updates a service location (hub assignment, name, ZIP, active flag).
    /// </summary>
    public async Task<IActionResult> UpdateLocation(int id, [FromBody] UpdateLocationDTO dto)
    {
        var result = await _service.UpdateLocationAsync(id, dto);
        return Ok(result);
    }

    [HttpDelete("locations/{id:int}")]
    /// <summary>
    /// Removes a service location from the network.
    /// </summary>
    public async Task<IActionResult> DeleteLocation(int id)
    {
        await _service.DeleteLocationAsync(id);
        return Ok();
    }

    // Exceptions
    [HttpGet("exceptions")]
    /// <summary>
    /// Returns a paginated list of open shipment exceptions for operations triage.
    /// </summary>
    public async Task<IActionResult> GetExceptions([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
    {
        var result = await _service.GetActiveExceptionsAsync(pageNumber, pageSize);
        return Ok(result);
    }

    [HttpPut("shipments/{id:int}/resolve")]
    /// <summary>
    /// Closes an open exception for the shipment with resolution notes.
    /// </summary>
    public async Task<IActionResult> ResolveException(int id, [FromBody] ResolveExceptionDTO dto)
    {
        dto.ShipmentId = id;
        var result = await _service.ResolveExceptionAsync(id, dto);
        return Ok(result);
    }

    [HttpPut("shipments/{id:int}/delay")]
    /// <summary>
    /// Marks an active shipment exception as delayed.
    /// </summary>
    public async Task<IActionResult> DelayShipment(int id, [FromBody] ShipmentActionReasonDTO dto)
    {
        var result = await _service.DelayShipmentAsync(id, dto.Reason);
        return Ok(result);
    }

    [HttpPut("shipments/{id:int}/return")]
    /// <summary>
    /// Marks an active shipment exception as returned to sender.
    /// </summary>
    public async Task<IActionResult> ReturnShipment(int id, [FromBody] ShipmentActionReasonDTO dto)
    {
        var result = await _service.ReturnShipmentAsync(id, dto.Reason);
        return Ok(result);
    }

    // Shipment Monitoring Integration
    [HttpGet("shipments")]
    /// <summary>
    /// Returns a paginated view of shipments from ShipmentService (aggregated for admin monitoring).
    /// </summary>
    public async Task<IActionResult> GetShipments([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
    {
        var result = await _service.GetShipmentsAsync(pageNumber, pageSize);
        return Ok(result);
    }

    [HttpGet("shipments/{id:int}")]
    /// <summary>
    /// Returns shipment details by identifier via ShipmentService.
    /// </summary>
    public async Task<IActionResult> GetShipmentById(int id)
    {
        var result = await _service.GetShipmentByIdAsync(id);
        return Ok(result);
    }

    [HttpGet("shipments/hub/{hubId:int}")]
    /// <summary>
    /// Returns shipments whose sender or receiver ZIP matches the hub's service locations.
    /// </summary>
    public async Task<IActionResult> GetShipmentsByHub(int hubId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
    {
        var result = await _service.GetShipmentsByHubAsync(hubId, pageNumber, pageSize);
        return Ok(result);
    }

    // Reports Integration
    [HttpGet("reports")]
    /// <summary>
    /// Returns a summary overview for reporting (counts, revenue proxy, active exceptions).
    /// </summary>
    public async Task<IActionResult> GetReports()
    {
        var result = await _service.GetReportsOverviewAsync();
        return Ok(result);
    }

    [HttpGet("reports/shipment-performance")]
    /// <summary>
    /// Returns delivery performance metrics (delivered vs total shipments).
    /// </summary>
    public async Task<IActionResult> GetShipmentPerformance()
    {
        var result = await _service.GetShipmentPerformanceAsync();
        return Ok(result);
    }

    [HttpGet("reports/delivery-sla")]
    /// <summary>
    /// Returns delivery SLA metrics against the configured on-time window.
    /// </summary>
    public async Task<IActionResult> GetDeliverySLA()
    {
        var result = await _service.GetDeliverySLAAsync();
        return Ok(result);
    }

    [HttpGet("reports/revenue")]
    /// <summary>
    /// Returns a revenue estimate derived from shipped weight over the reporting period.
    /// </summary>
    public async Task<IActionResult> GetRevenue()
    {
        var result = await _service.GetRevenueAsync();
        return Ok(result);
    }

    [HttpGet("reports/hub-performance")]
    /// <summary>
    /// Returns shipment volume attributed to each hub via matching ZIP codes.
    /// </summary>
    public async Task<IActionResult> GetHubPerformance()
    {
        var result = await _service.GetHubPerformanceAsync();
        return Ok(result);
    }
}


