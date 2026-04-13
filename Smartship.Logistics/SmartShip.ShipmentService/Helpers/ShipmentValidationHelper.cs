/// <summary>
/// Provides backend implementation for ShipmentValidationHelper.
/// </summary>

using SmartShip.Shared.Common.Exceptions;
using SmartShip.Shared.Common.Helpers;
using SmartShip.ShipmentService.DTOs;
using SmartShip.ShipmentService.Enums;
using SmartShip.ShipmentService.Models;

namespace SmartShip.ShipmentService.Helpers;

/// <summary>
/// Represents ShipmentValidationHelper.
/// </summary>
public static class ShipmentValidationHelper
{
    /// <summary>
    /// Executes ValidateCreateRequest.
    /// </summary>
    public static void ValidateCreateRequest(CreateShipmentDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (dto.CustomerId <= 0)
        {
            throw new RequestValidationException("CustomerId must be greater than 0.");
        }

        ValidateAddress(dto.SenderAddress, "SenderAddress");
        ValidateAddress(dto.ReceiverAddress, "ReceiverAddress");

        if (dto.Packages.Count == 0)
        {
            throw new RequestValidationException("At least one package is required.");
        }

        if (dto.PickupSchedule is not null)
        {
            ValidatePickupSchedule(dto.PickupSchedule);
        }
    }

    /// <summary>
    /// Executes ValidatePickupSchedule.
    /// </summary>
    public static void ValidatePickupSchedule(PickupScheduleDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (dto.PickupDate <= TimeZoneHelper.GetCurrentUtcTime())
        {
            throw new RequestValidationException("PickupDate must be in the future.");
        }
    }

    /// <summary>
    /// Executes EnsureShipmentCanBeModified.
    /// </summary>
    public static void EnsureShipmentCanBeModified(Shipment shipment)
    {
        if (shipment.Status is ShipmentStatus.PickedUp or ShipmentStatus.InTransit or ShipmentStatus.OutForDelivery or ShipmentStatus.Delivered)
        {
            throw new RequestValidationException("Shipment can no longer be modified after pickup.");
        }
    }

    /// <summary>
    /// Executes CalculateTotalWeight.
    /// </summary>
    public static decimal CalculateTotalWeight(IEnumerable<PackageDTO> packages)
    {
        return packages.Sum(package => package.Weight);
    }

    private static void ValidateAddress(Address address, string fieldName)
    {
        ArgumentNullException.ThrowIfNull(address);

        if (string.IsNullOrWhiteSpace(address.Street) ||
            string.IsNullOrWhiteSpace(address.City) ||
            string.IsNullOrWhiteSpace(address.State) ||
            string.IsNullOrWhiteSpace(address.Country) ||
            string.IsNullOrWhiteSpace(address.PostalCode))
        {
            throw new RequestValidationException($"{fieldName} is incomplete.");
        }
    }
}


