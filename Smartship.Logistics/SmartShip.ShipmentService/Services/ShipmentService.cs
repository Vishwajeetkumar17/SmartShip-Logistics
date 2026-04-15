using SmartShip.EventBus.Abstractions;
using SmartShip.EventBus.Constants;
using SmartShip.EventBus.Contracts;
using SmartShip.Shared.Common.Exceptions;
using SmartShip.Shared.Common.Extensions;
using SmartShip.Shared.Common.Helpers;
using SmartShip.Shared.DTOs;
using SmartShip.ShipmentService.DTOs;
using SmartShip.ShipmentService.Enums;
using SmartShip.ShipmentService.Helpers;
using SmartShip.ShipmentService.Models;
using SmartShip.ShipmentService.Repositories;

namespace SmartShip.ShipmentService.Services;

/// <summary>
/// Implements shipment business workflows for SmartShip logistics operations.
/// </summary>
public class ShipmentService : IShipmentService
{
    private readonly IShipmentRepository _repository;
    private readonly IEventPublisher _eventPublisher;



    #region Constructor
    /// <summary>
    /// Implements shipment service workflows.
    /// </summary>
    public ShipmentService(IShipmentRepository repository, IEventPublisher eventPublisher)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
    }
    #endregion



    #region Public API
    /// <summary>
    /// Creates shipment.
    /// </summary>
    public async Task<ShipmentResponseDTO> CreateShipment(CreateShipmentDTO dto)
    {
        ShipmentValidationHelper.ValidateCreateRequest(dto);

        var shipment = new Shipment
        {
            TrackingNumber = TrackingNumberGenerator.GenerateTrackingNumber(),
            CustomerId = dto.CustomerId,
            SenderName = dto.SenderName.Trim(),
            SenderPhone = dto.SenderPhone?.Trim(),
            ReceiverName = dto.ReceiverName.Trim(),
            ReceiverPhone = dto.ReceiverPhone?.Trim(),
            ServiceType = dto.ServiceType.Trim(),
            Status = ShipmentStatus.Draft,
            CreatedAt = TimeZoneHelper.GetCurrentUtcTime(),
            TotalWeight = ShipmentValidationHelper.CalculateTotalWeight(dto.Packages),
            EstimatedCost = ResolveEstimatedCost(dto),
            SenderAddress = CloneAddress(dto.SenderAddress),
            ReceiverAddress = CloneAddress(dto.ReceiverAddress),
            PickupSchedule = dto.PickupSchedule is null
                ? null
                : new PickupSchedule
                {
                    PickupDate = dto.PickupSchedule.PickupDate,
                    Notes = dto.PickupSchedule.Notes.Trim()
                },
            Packages = dto.Packages.Select(p => new Package
            {
                Weight = p.Weight,
                Length = p.Length,
                Width = p.Width,
                Height = p.Height,
                Description = p.Description.Trim()
            }).ToList()
        };

        await _repository.CreateAsync(shipment);

        await _eventPublisher.PublishAsync(
            RabbitMqQueues.ShipmentCreatedQueue,
            new ShipmentCreatedEvent
            {
                ShipmentId = shipment.ShipmentId,
                TrackingNumber = shipment.TrackingNumber,
                CustomerId = shipment.CustomerId,
                Timestamp = TimeZoneHelper.GetCurrentUtcTime()
            });

        return MapToDto(shipment);
    }
    #endregion



    #region Public API
    /// <summary>
    /// Returns shipments.
    /// </summary>
    public async Task<PaginatedResponse<ShipmentResponseDTO>> GetShipments(int pageNumber = 1, int pageSize = 5)
    {
        var allShipments = await _repository.GetAllAsync();
        // Repository already returns sorted by CreatedAt descending
        var totalCount = allShipments.Count;

        var shipmentDtos = allShipments
            .Select(MapToDto)
            .ToList();

        var pagedShipments = shipmentDtos
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return pagedShipments.ToPaginatedResponse(pageNumber, pageSize, totalCount);
    }
    #endregion



    #region Public API
    /// <summary>
    /// Returns shipment.
    /// </summary>
    public async Task<ShipmentResponseDTO?> GetShipment(int id)
    {
        var shipment = await _repository.GetByIdAsync(id);
        return shipment == null ? null : MapToDto(shipment);
    }
    #endregion



    #region Public API
    /// <summary>
    /// Returns shipment by tracking number.
    /// </summary>
    public async Task<ShipmentResponseDTO?> GetShipmentByTrackingNumber(string trackingNumber)
    {
        var shipment = await _repository.GetByTrackingNumberAsync(trackingNumber);
        return shipment == null ? null : MapToDto(shipment);
    }
    #endregion



    #region Public API
    /// <summary>
    /// Returns customer shipments.
    /// </summary>
    public async Task<List<ShipmentResponseDTO>> GetCustomerShipments(int customerId)
    {
        if (customerId <= 0)
        {
            throw new RequestValidationException("CustomerId must be greater than 0.");
        }

        var shipments = await _repository.GetByCustomerAsync(customerId);
        return shipments.Select(MapToDto).ToList();
    }
    #endregion



    #region Public API
    /// <summary>
    /// Returns customer shipments.
    /// </summary>
    public async Task<PaginatedResponse<ShipmentResponseDTO>> GetCustomerShipments(int customerId, int pageNumber = 1, int pageSize = 5)
    {
        if (customerId <= 0)
        {
            throw new RequestValidationException("CustomerId must be greater than 0.");
        }

        var shipments = await _repository.GetByCustomerAsync(customerId);
        // Repository already returns sorted by CreatedAt descending
        var shipmentDtos = shipments
            .Select(MapToDto)
            .ToList();
        var totalCount = shipmentDtos.Count;

        var pagedShipments = shipmentDtos
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return pagedShipments.ToPaginatedResponse(pageNumber, pageSize, totalCount);
    }
    #endregion



    #region Public API
    /// <summary>
    /// Updates shipment.
    /// </summary>
    public async Task UpdateShipment(int id, UpdateShipmentDTO dto)
    {
        var shipment = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Shipment not found.");

        ShipmentValidationHelper.EnsureShipmentCanBeModified(shipment);
        shipment.TotalWeight = dto.TotalWeight;

        await _repository.UpdateAsync(shipment);
    }
    #endregion



    #region Public API
    /// <summary>
    /// Deletes shipment.
    /// </summary>
    public async Task DeleteShipment(int id)
    {
        var shipment = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Shipment not found.");

        await _repository.DeleteAsync(shipment);
    }
    #endregion



    #region Public API
    /// <summary>
    /// Deletes customer shipment.
    /// </summary>
    public async Task DeleteCustomerShipment(int shipmentId, int customerId)
    {
        if (shipmentId <= 0)
        {
            throw new RequestValidationException("ShipmentId must be greater than 0.");
        }

        if (customerId <= 0)
        {
            throw new RequestValidationException("CustomerId must be greater than 0.");
        }

        var shipment = await _repository.GetByIdAsync(shipmentId)
            ?? throw new NotFoundException("Shipment not found.");

        if (shipment.CustomerId != customerId)
        {
            throw new UnauthorizedAccessException("You can only delete your own shipments.");
        }

        if (shipment.Status != ShipmentStatus.Draft)
        {
            throw new RequestValidationException("Shipment cannot be deleted after admin booking.");
        }

        await _repository.DeleteAsync(shipment);
    }
    #endregion



    #region Public API
    /// <summary>
    /// Books shipment.
    /// </summary>
    public async Task BookShipment(int shipmentId, BookShipmentDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var hubName = dto.HubName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(hubName))
        {
            throw new RequestValidationException("Hub name is required while booking shipment.");
        }

        var hubAddress = dto.HubAddress?.Trim() ?? string.Empty;
        var hubLocation = string.IsNullOrWhiteSpace(hubAddress)
            ? hubName
            : $"{hubName}, {hubAddress}";

        await UpdateStatus(shipmentId, ShipmentStatus.Booked, hubLocation);
    }
    #endregion



    #region Public API
    /// <summary>
    /// Updates status.
    /// </summary>
    public async Task UpdateStatus(int shipmentId, ShipmentStatus nextStatus, string? hubLocation = null)
    {
        var shipment = await _repository.GetByIdAsync(shipmentId)
            ?? throw new NotFoundException("Shipment not found.");

        var effectiveHubLocation = string.IsNullOrWhiteSpace(hubLocation)
            ? null
            : hubLocation.Trim();

        if (nextStatus == ShipmentStatus.Booked && string.IsNullOrWhiteSpace(effectiveHubLocation))
        {
            throw new RequestValidationException("Booking hub is required while booking shipment.");
        }

        if (!ShipmentStateValidator.IsValidTransition(shipment.Status, nextStatus))
        {
            await _eventPublisher.PublishAsync(
                RabbitMqQueues.ShipmentExceptionQueue,
                new ShipmentExceptionEvent
                {
                    ShipmentId = shipment.ShipmentId,
                    TrackingNumber = shipment.TrackingNumber,
                    CustomerId = shipment.CustomerId,
                    Timestamp = TimeZoneHelper.GetCurrentUtcTime(),
                    ExceptionType = "InvalidTransition",
                    Description = $"Invalid shipment status transition from {shipment.Status} to {nextStatus}."
                });

            throw new RequestValidationException($"Invalid shipment status transition from {shipment.Status} to {nextStatus}.");
        }

        shipment.Status = nextStatus;

        await _repository.UpdateAsync(shipment);

        await PublishStatusEventAsync(shipment, nextStatus, effectiveHubLocation);
    }
    #endregion



    #region Public API
    /// <summary>
    /// Schedules pickup.
    /// </summary>
    public async Task SchedulePickup(int shipmentId, PickupScheduleDTO dto)
    {
        ShipmentValidationHelper.ValidatePickupSchedule(dto);

        var shipment = await _repository.GetByIdAsync(shipmentId)
            ?? throw new NotFoundException("Shipment not found.");

        if (shipment.Status == ShipmentStatus.Delivered)
        {
            throw new RequestValidationException("Pickup cannot be scheduled for a delivered shipment.");
        }

        shipment.PickupSchedule ??= new PickupSchedule { ShipmentId = shipmentId };
        shipment.PickupSchedule.PickupDate = dto.PickupDate;
        shipment.PickupSchedule.Notes = dto.Notes.Trim();

        await _repository.UpdateAsync(shipment);
    }
    #endregion



    #region Public API
    /// <summary>
    /// Submits issue async.
    /// </summary>
    public async Task RaiseIssueAsync(int shipmentId, int customerId, ShipmentIssueDTO dto)
    {
        if (shipmentId <= 0)
        {
            throw new RequestValidationException("ShipmentId must be greater than 0.");
        }

        if (customerId <= 0)
        {
            throw new RequestValidationException("CustomerId must be greater than 0.");
        }

        ArgumentNullException.ThrowIfNull(dto);

        var issueType = dto.IssueType?.Trim() ?? string.Empty;
        var description = dto.Description?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(issueType))
        {
            throw new RequestValidationException("Issue type is required.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new RequestValidationException("Issue description is required.");
        }

        var shipment = await _repository.GetByIdAsync(shipmentId)
            ?? throw new NotFoundException("Shipment not found.");

        if (shipment.CustomerId != customerId)
        {
            throw new UnauthorizedAccessException("You can raise issue only for your own shipment.");
        }

        await _eventPublisher.PublishAsync(
            RabbitMqQueues.ShipmentExceptionQueue,
            new ShipmentExceptionEvent
            {
                ShipmentId = shipment.ShipmentId,
                TrackingNumber = shipment.TrackingNumber,
                CustomerId = shipment.CustomerId,
                Timestamp = TimeZoneHelper.GetCurrentUtcTime(),
                ExceptionType = ResolveIssueExceptionType(issueType),
                Description = $"Customer raised issue ({issueType}): {description}",
                Source = "Customer",
                RaisedByUserId = customerId
            });
    }
    #endregion



    #region Private Helpers
    /// <summary>
    /// Publishes status event async.
    /// </summary>
    private async Task PublishStatusEventAsync(Shipment shipment, ShipmentStatus status, string? hubLocation)
    {
        var timestamp = TimeZoneHelper.GetCurrentUtcTime();

        switch (status)
        {
            case ShipmentStatus.Booked:
                await _eventPublisher.PublishAsync(
                    RabbitMqQueues.ShipmentBookedQueue,
                    new ShipmentBookedEvent
                    {
                        ShipmentId = shipment.ShipmentId,
                        TrackingNumber = shipment.TrackingNumber,
                        CustomerId = shipment.CustomerId,
                        Timestamp = timestamp,
                        HubLocation = hubLocation
                    });
                break;

            case ShipmentStatus.PickedUp:
                await _eventPublisher.PublishAsync(
                    RabbitMqQueues.ShipmentPickedUpQueue,
                    new ShipmentPickedUpEvent
                    {
                        ShipmentId = shipment.ShipmentId,
                        TrackingNumber = shipment.TrackingNumber,
                        CustomerId = shipment.CustomerId,
                        Timestamp = timestamp,
                        HubLocation = hubLocation
                    });
                break;

            case ShipmentStatus.InTransit:
                await _eventPublisher.PublishAsync(
                    RabbitMqQueues.ShipmentInTransitQueue,
                    new ShipmentInTransitEvent
                    {
                        ShipmentId = shipment.ShipmentId,
                        TrackingNumber = shipment.TrackingNumber,
                        CustomerId = shipment.CustomerId,
                        Timestamp = timestamp,
                        HubLocation = hubLocation
                    });
                break;

            case ShipmentStatus.OutForDelivery:
                await _eventPublisher.PublishAsync(
                    RabbitMqQueues.ShipmentOutForDeliveryQueue,
                    new ShipmentOutForDeliveryEvent
                    {
                        ShipmentId = shipment.ShipmentId,
                        TrackingNumber = shipment.TrackingNumber,
                        CustomerId = shipment.CustomerId,
                        Timestamp = timestamp,
                        HubLocation = hubLocation
                    });
                break;

            case ShipmentStatus.Delivered:
                await _eventPublisher.PublishAsync(
                    RabbitMqQueues.ShipmentDeliveredQueue,
                    new ShipmentDeliveredEvent
                    {
                        ShipmentId = shipment.ShipmentId,
                        TrackingNumber = shipment.TrackingNumber,
                        CustomerId = shipment.CustomerId,
                        Timestamp = timestamp,
                        HubLocation = hubLocation
                    });
                break;
        }
    }
    #endregion



    #region Private Helpers
    /// <summary>
    /// Processes clone address.
    /// </summary>
    private static Address CloneAddress(Address address)
    {
        return new Address
        {
            Street = address.Street.Trim(),
            City = address.City.Trim(),
            State = address.State.Trim(),
            Country = address.Country.Trim(),
            PostalCode = address.PostalCode.Trim()
        };
    }
    #endregion



    #region Private Helpers
    /// <summary>
    /// Resolves estimated cost.
    /// </summary>
    private static decimal ResolveEstimatedCost(CreateShipmentDTO dto)
    {
        var provided = dto.EstimatedCost ?? 0m;
        if (provided > 0)
        {
            return decimal.Round(provided, 2, MidpointRounding.AwayFromZero);
        }

        const decimal baseRate = 100m;
        const decimal perKgRate = 50m;
        var totalWeight = ShipmentValidationHelper.CalculateTotalWeight(dto.Packages);
        var fallback = baseRate + (totalWeight * perKgRate);
        return decimal.Round(fallback, 2, MidpointRounding.AwayFromZero);
    }
    #endregion



    #region Private Helpers
    /// <summary>
    /// Resolves issue exception type.
    /// </summary>
    private static string ResolveIssueExceptionType(string issueType)
    {
        var normalized = issueType.Trim().ToLowerInvariant();
        if (normalized.Contains("cancel"))
        {
            return "CanceledShipment";
        }

        if (normalized.Contains("delay"))
        {
            return "CustomerDelayConcern";
        }

        return "CustomerIssue";
    }
    #endregion



    #region Private Helpers
    /// <summary>
    /// Maps to dto.
    /// </summary>
    private static ShipmentResponseDTO MapToDto(Shipment shipment)
    {
        return new ShipmentResponseDTO
        {
            ShipmentId = shipment.ShipmentId,
            TrackingNumber = shipment.TrackingNumber,
            CustomerId = shipment.CustomerId,
            SenderName = shipment.SenderName,
            SenderPhone = shipment.SenderPhone,
            ReceiverName = shipment.ReceiverName,
            ReceiverPhone = shipment.ReceiverPhone,
            ServiceType = shipment.ServiceType,
            Status = shipment.Status,
            TotalWeight = shipment.TotalWeight,
            EstimatedCost = shipment.EstimatedCost,
            CreatedAt = shipment.CreatedAt,
            SenderAddress = shipment.SenderAddress ?? new Address(),
            ReceiverAddress = shipment.ReceiverAddress ?? new Address(),
            Packages = shipment.Packages?.Select(p => new PackageDTO
            {
                Id = p.PackageId,
                Weight = p.Weight,
                Length = p.Length,
                Width = p.Width,
                Height = p.Height,
                Description = p.Description
            }).ToList() ?? new List<PackageDTO>(),
            PickupSchedule = shipment.PickupSchedule != null ? new PickupScheduleDTO
            {
                PickupDate = shipment.PickupSchedule.PickupDate,
                Notes = shipment.PickupSchedule.Notes
            } : null
        };
    }
    #endregion
}




