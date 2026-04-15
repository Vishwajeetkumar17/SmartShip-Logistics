using SmartShip.AdminService.Models;

namespace SmartShip.AdminService.Repositories;

/// <summary>
/// Contract for admin persistence operations across hubs, service locations, and exceptions.
/// </summary>
public interface IAdminRepository
{
    #region Hub Operations

    Task<List<Hub>> GetAllHubsAsync();
    Task<Hub?> GetHubByIdAsync(int hubId);
    Task AddHubAsync(Hub hub);
    Task UpdateHubAsync(Hub hub);
    Task DeleteHubAsync(Hub hub);
    Task<bool> HubNameExistsAsync(string name, int? excludingHubId = null);

    #endregion

    #region Service Location Operations

    Task<List<ServiceLocation>> GetAllLocationsAsync();
    Task<ServiceLocation?> GetLocationByIdAsync(int locationId);
    Task AddLocationAsync(ServiceLocation location);
    Task UpdateLocationAsync(ServiceLocation location);
    Task DeleteLocationAsync(ServiceLocation location);
    Task<bool> ZipCodeExistsAsync(string zipCode, int? excludingLocationId = null);

    #endregion

    #region Exception Record Operations

    Task<List<ExceptionRecord>> GetActiveExceptionsAsync();
    Task<ExceptionRecord?> GetExceptionByIdAsync(int exceptionId);
    Task<ExceptionRecord?> GetExceptionByShipmentIdAsync(int shipmentId);
    Task<ExceptionRecord?> GetOpenExceptionByShipmentAndTypeAsync(int shipmentId, string exceptionType);
    Task AddExceptionRecordAsync(ExceptionRecord record);
    Task UpdateExceptionRecordAsync(ExceptionRecord record);

    #endregion

    #region Aggregation Queries

    Task<int> GetTotalActiveHubsAsync();
    Task<int> GetTotalActiveExceptionsAsync();

    #endregion
}


