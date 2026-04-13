/// <summary>
/// Service contract for the Admin microservice business operations.
/// Defines hub management, location management, exception handling, shipment integration, and reporting capabilities.
/// </summary>

using SmartShip.AdminService.DTOs;
using SmartShip.EventBus.Contracts;
using SmartShip.Shared.DTOs;
using SmartShip.Shared.DTOs.Shipment;

namespace SmartShip.AdminService.Services;

/// <summary>
/// Defines the contract for admin service operations including hub/location CRUD,
/// exception management, shipment integration queries, and analytics reporting.
/// </summary>
public interface IAdminService
{
    #region Diagnostics

    /// <summary>
    /// Demonstrates all Serilog log levels with scoped metadata for testing and verification.
    /// </summary>
    void LogLevelDemo(string userId, string requestId, bool simulateFailure);

    #endregion

    #region Dashboard & Analytics

    /// <summary>
    /// Retrieves top-level dashboard counters (hubs, locations, exceptions).
    /// </summary>
    Task<DashboardMetricsDTO> GetDashboardMetricsAsync();

    /// <summary>
    /// Builds comprehensive shipment analytics including trends, status distribution, and delivery performance.
    /// </summary>
    Task<ComprehensiveDashboardDTO> GetComprehensiveStatisticsAsync();

    #endregion

    #region Hub Management

    /// <summary>
    /// Retrieves a paginated list of all logistics hubs.
    /// </summary>
    Task<PaginatedResponse<HubResponseDTO>> GetAllHubsAsync(int pageNumber = 1, int pageSize = 5);

    /// <summary>
    /// Retrieves a single hub by its unique identifier.
    /// </summary>
    Task<HubResponseDTO> GetHubByIdAsync(int hubId);

    /// <summary>
    /// Creates a new logistics hub with unique name enforcement.
    /// </summary>
    Task<HubResponseDTO> CreateHubAsync(CreateHubDTO dto);

    /// <summary>
    /// Updates an existing hub's details with validation.
    /// </summary>
    Task<HubResponseDTO> UpdateHubAsync(int hubId, UpdateHubDTO dto);

    /// <summary>
    /// Deletes a hub if it has no assigned service locations.
    /// </summary>
    Task DeleteHubAsync(int hubId);

    #endregion

    #region Location Management

    /// <summary>
    /// Retrieves a paginated list of all service locations.
    /// </summary>
    Task<PaginatedResponse<LocationResponseDTO>> GetAllLocationsAsync(int pageNumber = 1, int pageSize = 5);

    /// <summary>
    /// Creates a new service location tied to an existing hub.
    /// </summary>
    Task<LocationResponseDTO> CreateLocationAsync(CreateLocationDTO dto);

    /// <summary>
    /// Updates an existing service location with hub and zip code validation.
    /// </summary>
    Task<LocationResponseDTO> UpdateLocationAsync(int locationId, UpdateLocationDTO dto);

    /// <summary>
    /// Deletes a service location by its identifier.
    /// </summary>
    Task DeleteLocationAsync(int locationId);

    #endregion

    #region Exception Management

    /// <summary>
    /// Retrieves a paginated list of all open/active shipment exceptions.
    /// </summary>
    Task<PaginatedResponse<ExceptionRecordResponseDTO>> GetActiveExceptionsAsync(int pageNumber = 1, int pageSize = 5);

    /// <summary>
    /// Marks an active exception as resolved with resolution notes.
    /// </summary>
    Task<ExceptionRecordResponseDTO> ResolveExceptionAsync(int shipmentId, ResolveExceptionDTO dto);

    /// <summary>
    /// Records a delay exception for a shipment.
    /// </summary>
    Task<ExceptionRecordResponseDTO> DelayShipmentAsync(int shipmentId, string reason);

    /// <summary>
    /// Records a return exception for a shipment.
    /// </summary>
    Task<ExceptionRecordResponseDTO> ReturnShipmentAsync(int shipmentId, string reason);

    /// <summary>
    /// Processes an incoming shipment exception event from the message bus.
    /// </summary>
    Task CreateExceptionRecordFromEventAsync(ShipmentExceptionEvent @event);

    #endregion

    #region Shipment Integration

    /// <summary>
    /// Retrieves paginated shipments from the ShipmentService via HTTP integration.
    /// </summary>
    Task<PaginatedResponse<ShipmentExternalDto>> GetShipmentsAsync(int pageNumber = 1, int pageSize = 5);

    /// <summary>
    /// Fetches a single shipment by ID from the ShipmentService.
    /// </summary>
    Task<ShipmentExternalDto> GetShipmentByIdAsync(int shipmentId);

    /// <summary>
    /// Returns shipments associated with a hub's service area zip codes.
    /// </summary>
    Task<PaginatedResponse<ShipmentExternalDto>> GetShipmentsByHubAsync(int hubId, int pageNumber = 1, int pageSize = 5);

    #endregion

    #region Reports

    /// <summary>
    /// Generates a high-level operational report (volume, revenue, exceptions).
    /// </summary>
    Task<object> GetReportsOverviewAsync();

    /// <summary>
    /// Calculates shipment delivery performance ratio.
    /// </summary>
    Task<object> GetShipmentPerformanceAsync();

    /// <summary>
    /// Computes delivery SLA compliance percentage.
    /// </summary>
    Task<object> GetDeliverySLAAsync();

    /// <summary>
    /// Aggregates revenue figures based on shipment weight.
    /// </summary>
    Task<object> GetRevenueAsync();

    /// <summary>
    /// Compares throughput and performance metrics across logistics hubs.
    /// </summary>
    Task<object> GetHubPerformanceAsync();

    #endregion
}
