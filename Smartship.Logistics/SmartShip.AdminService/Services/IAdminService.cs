/// <summary>
/// Provides backend implementation for IAdminService.
/// </summary>

using SmartShip.AdminService.DTOs;
using SmartShip.EventBus.Contracts;
using SmartShip.Shared.DTOs;
using SmartShip.Shared.DTOs.Shipment;

namespace SmartShip.AdminService.Services;

/// <summary>
/// Represents IAdminService.
/// </summary>
public interface IAdminService
{
    void LogLevelDemo(string userId, string requestId, bool simulateFailure);

    // Dashboards
    Task<DashboardMetricsDTO> GetDashboardMetricsAsync();
    Task<ComprehensiveDashboardDTO> GetComprehensiveStatisticsAsync();

    // Hubs
    Task<PaginatedResponse<HubResponseDTO>> GetAllHubsAsync(int pageNumber = 1, int pageSize = 5);
    Task<HubResponseDTO> GetHubByIdAsync(int hubId);
    Task<HubResponseDTO> CreateHubAsync(CreateHubDTO dto);
    Task<HubResponseDTO> UpdateHubAsync(int hubId, UpdateHubDTO dto);
    Task DeleteHubAsync(int hubId);

    // Locations
    Task<PaginatedResponse<LocationResponseDTO>> GetAllLocationsAsync(int pageNumber = 1, int pageSize = 5);
    Task<LocationResponseDTO> CreateLocationAsync(CreateLocationDTO dto);
    Task<LocationResponseDTO> UpdateLocationAsync(int locationId, UpdateLocationDTO dto);
    Task DeleteLocationAsync(int locationId);

    // Exceptions
    Task<PaginatedResponse<ExceptionRecordResponseDTO>> GetActiveExceptionsAsync(int pageNumber = 1, int pageSize = 5);
    Task<ExceptionRecordResponseDTO> ResolveExceptionAsync(int shipmentId, ResolveExceptionDTO dto);
    Task<ExceptionRecordResponseDTO> DelayShipmentAsync(int shipmentId, string reason);
    Task<ExceptionRecordResponseDTO> ReturnShipmentAsync(int shipmentId, string reason);
    Task CreateExceptionRecordFromEventAsync(ShipmentExceptionEvent @event);

    // Shipments (Integration)
    Task<PaginatedResponse<ShipmentExternalDto>> GetShipmentsAsync(int pageNumber = 1, int pageSize = 5);
    Task<ShipmentExternalDto> GetShipmentByIdAsync(int shipmentId);
    Task<PaginatedResponse<ShipmentExternalDto>> GetShipmentsByHubAsync(int hubId, int pageNumber = 1, int pageSize = 5);

    // Reports (Integration)
    Task<object> GetReportsOverviewAsync();
    Task<object> GetShipmentPerformanceAsync();
    Task<object> GetDeliverySLAAsync();
    Task<object> GetRevenueAsync();
    Task<object> GetHubPerformanceAsync();
}


