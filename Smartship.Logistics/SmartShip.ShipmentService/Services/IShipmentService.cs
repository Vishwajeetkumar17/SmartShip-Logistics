using SmartShip.ShipmentService.DTOs;
using SmartShip.ShipmentService.Enums;
using SmartShip.Shared.DTOs;

namespace SmartShip.ShipmentService.Services;

/// <summary>
/// Defines shipment business operations used by the service layer.
/// </summary>
public interface IShipmentService
{
    Task<ShipmentResponseDTO> CreateShipment(CreateShipmentDTO dto);
    Task<PaginatedResponse<ShipmentResponseDTO>> GetShipments(int pageNumber = 1, int pageSize = 5);
    Task<ShipmentResponseDTO?> GetShipment(int id);
    Task<ShipmentResponseDTO?> GetShipmentByTrackingNumber(string trackingNumber);
    Task<List<ShipmentResponseDTO>> GetCustomerShipments(int customerId);
    Task<PaginatedResponse<ShipmentResponseDTO>> GetCustomerShipments(int customerId, int pageNumber = 1, int pageSize = 5);
    Task UpdateShipment(int id, UpdateShipmentDTO dto);
    Task DeleteShipment(int id);
    Task DeleteCustomerShipment(int shipmentId, int customerId);
    Task BookShipment(int shipmentId, BookShipmentDTO dto);
    Task UpdateStatus(int shipmentId, ShipmentStatus nextStatus, string? hubLocation = null);
    Task SchedulePickup(int shipmentId, PickupScheduleDTO dto);
    Task RaiseIssueAsync(int shipmentId, int customerId, ShipmentIssueDTO dto);
}


