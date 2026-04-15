using SmartShip.AdminService.DTOs;
using SmartShip.EventBus.Contracts;
using SmartShip.Shared.DTOs;
using SmartShip.Shared.DTOs.Shipment;

namespace SmartShip.AdminService.Services;

/// <summary>
/// Defines admin business operations used by the service layer.
/// </summary>
public interface IAdminService
{
    #region Diagnostics

    /// <summary>
    /// Writes diagnostic logs across severity levels for observability checks.
    /// </summary>
    void LogLevelDemo(string userId, string requestId, bool simulateFailure);

    #endregion

    #region Dashboard & Analytics

    /// <summary>
    /// Returns dashboard metrics.
    /// </summary>
    Task<DashboardMetricsDTO> GetDashboardMetricsAsync();

    /// <summary>
    /// Returns comprehensive statistics.
    /// </summary>
    Task<ComprehensiveDashboardDTO> GetComprehensiveStatisticsAsync();

    #endregion

    #region Hub Management

    /// <summary>
    /// Returns all hubs.
    /// </summary>
    Task<PaginatedResponse<HubResponseDTO>> GetAllHubsAsync(int pageNumber = 1, int pageSize = 5);

    /// <summary>
    /// Returns hub by id.
    /// </summary>
    Task<HubResponseDTO> GetHubByIdAsync(int hubId);

    /// <summary>
    /// Creates hub.
    /// </summary>
    Task<HubResponseDTO> CreateHubAsync(CreateHubDTO dto);

    /// <summary>
    /// Updates hub.
    /// </summary>
    Task<HubResponseDTO> UpdateHubAsync(int hubId, UpdateHubDTO dto);

    /// <summary>
    /// Deletes hub.
    /// </summary>
    Task DeleteHubAsync(int hubId);

    #endregion

    #region Location Management

    /// <summary>
    /// Returns all locations.
    /// </summary>
    Task<PaginatedResponse<LocationResponseDTO>> GetAllLocationsAsync(int pageNumber = 1, int pageSize = 5);

    /// <summary>
    /// Creates location.
    /// </summary>
    Task<LocationResponseDTO> CreateLocationAsync(CreateLocationDTO dto);

    /// <summary>
    /// Updates location.
    /// </summary>
    Task<LocationResponseDTO> UpdateLocationAsync(int locationId, UpdateLocationDTO dto);

    /// <summary>
    /// Deletes location.
    /// </summary>
    Task DeleteLocationAsync(int locationId);

    #endregion

    #region Exception Management

    /// <summary>
    /// Returns active exceptions.
    /// </summary>
    Task<PaginatedResponse<ExceptionRecordResponseDTO>> GetActiveExceptionsAsync(int pageNumber = 1, int pageSize = 5);

    /// <summary>
    /// Resolves an active shipment exception with resolution notes.
    /// </summary>
    Task<ExceptionRecordResponseDTO> ResolveExceptionAsync(int shipmentId, ResolveExceptionDTO dto);

    /// <summary>
    /// Marks a shipment exception as delayed with the provided reason.
    /// </summary>
    Task<ExceptionRecordResponseDTO> DelayShipmentAsync(int shipmentId, string reason);

    /// <summary>
    /// Marks a shipment exception as returned with the provided reason.
    /// </summary>
    Task<ExceptionRecordResponseDTO> ReturnShipmentAsync(int shipmentId, string reason);

    /// <summary>
    /// Creates exception record from event.
    /// </summary>
    Task CreateExceptionRecordFromEventAsync(ShipmentExceptionEvent @event);

    #endregion

    #region Shipment Integration

    /// <summary>
    /// Returns shipments.
    /// </summary>
    Task<PaginatedResponse<ShipmentExternalDto>> GetShipmentsAsync(int pageNumber = 1, int pageSize = 5);

    /// <summary>
    /// Returns shipment by id.
    /// </summary>
    Task<ShipmentExternalDto> GetShipmentByIdAsync(int shipmentId);

    /// <summary>
    /// Returns shipments by hub.
    /// </summary>
    Task<PaginatedResponse<ShipmentExternalDto>> GetShipmentsByHubAsync(int hubId, int pageNumber = 1, int pageSize = 5);

    #endregion

    #region Reports

    /// <summary>
    /// Returns reports overview.
    /// </summary>
    Task<object> GetReportsOverviewAsync();

    /// <summary>
    /// Returns shipment performance.
    /// </summary>
    Task<object> GetShipmentPerformanceAsync();

    /// <summary>
    /// Returns delivery SLA metrics.
    /// </summary>
    Task<object> GetDeliverySLAAsync();

    /// <summary>
    /// Returns revenue.
    /// </summary>
    Task<object> GetRevenueAsync();

    /// <summary>
    /// Returns hub performance.
    /// </summary>
    Task<object> GetHubPerformanceAsync();

    #endregion
}
