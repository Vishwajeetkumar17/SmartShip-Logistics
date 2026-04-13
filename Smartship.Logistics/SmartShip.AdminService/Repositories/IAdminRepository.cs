/// <summary>
/// Provides backend implementation for IAdminRepository.
/// </summary>

using SmartShip.AdminService.Models;

namespace SmartShip.AdminService.Repositories;

/// <summary>
/// Represents IAdminRepository.
/// </summary>
public interface IAdminRepository
{
    // Hub methods
    Task<List<Hub>> GetAllHubsAsync();
    Task<Hub?> GetHubByIdAsync(int hubId);
    Task AddHubAsync(Hub hub);
    Task UpdateHubAsync(Hub hub);
    Task DeleteHubAsync(Hub hub);
    Task<bool> HubNameExistsAsync(string name, int? excludingHubId = null);

    // Location methods
    Task<List<ServiceLocation>> GetAllLocationsAsync();
    Task<ServiceLocation?> GetLocationByIdAsync(int locationId);
    Task AddLocationAsync(ServiceLocation location);
    Task UpdateLocationAsync(ServiceLocation location);
    Task DeleteLocationAsync(ServiceLocation location);
    Task<bool> ZipCodeExistsAsync(string zipCode, int? excludingLocationId = null);

    // Exception methods
    Task<List<ExceptionRecord>> GetActiveExceptionsAsync();
    Task<ExceptionRecord?> GetExceptionByIdAsync(int exceptionId);
    Task<ExceptionRecord?> GetExceptionByShipmentIdAsync(int shipmentId);
    Task<ExceptionRecord?> GetOpenExceptionByShipmentAndTypeAsync(int shipmentId, string exceptionType);
    Task AddExceptionRecordAsync(ExceptionRecord record);
    Task UpdateExceptionRecordAsync(ExceptionRecord record);

    // Aggregation Methods
    Task<int> GetTotalActiveHubsAsync();
    Task<int> GetTotalActiveExceptionsAsync();
}


