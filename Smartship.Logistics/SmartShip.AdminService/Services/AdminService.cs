/// <summary>
/// Provides business operations for SmartShip admin management and reporting.
/// </summary>

using SmartShip.AdminService.DTOs;
using SmartShip.AdminService.Helpers;
using SmartShip.AdminService.Integration;
using SmartShip.AdminService.Models;
using SmartShip.AdminService.Repositories;
using SmartShip.EventBus.Contracts;
using SmartShip.Shared.Common.Exceptions;
using SmartShip.Shared.Common.Extensions;
using SmartShip.Shared.Common.Helpers;
using SmartShip.Shared.DTOs;
using SmartShip.Shared.DTOs.Shipment;
using Microsoft.Extensions.Logging;

namespace SmartShip.AdminService.Services;

/// <summary>
/// Coordinates admin hub, location, exception, shipment, and reporting workflows.
/// </summary>
public class AdminService : IAdminService
{
    private readonly IAdminRepository _repository;
    private readonly IShipmentClient _shipmentClient;
    private readonly ILogger<AdminService> _logger;

    #region AdminService
    /// <summary>
    /// Initializes the admin service with repository, shipment integration, and logging dependencies.
    /// </summary>
    public AdminService(
        IAdminRepository repository,
        IShipmentClient shipmentClient,
        ILogger<AdminService> logger)
    {
        _repository = repository;
        _shipmentClient = shipmentClient;
        _logger = logger;
    }
    #endregion


    #region LogLevelDemo
    /// <summary>
    /// Demonstrates trace, debug, information, warning, error, and critical log levels with scoped metadata.
    /// </summary>
    public void LogLevelDemo(string userId, string requestId, bool simulateFailure)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["UserId"] = userId,
            ["RequestId"] = requestId
        });

        _logger.LogTrace("Verbose-level equivalent: entering {Method}", nameof(LogLevelDemo));
        _logger.LogDebug("Debug-level diagnostics for {Method}", nameof(LogLevelDemo));
        _logger.LogInformation("Business operation started for UserId {UserId}", userId);
        _logger.LogWarning("Demo warning raised for RequestId {RequestId}", requestId);

        try
        {
            if (simulateFailure)
            {
                throw new InvalidOperationException("Simulated failure for logging demo.");
            }

            _logger.LogInformation("Business operation completed successfully for UserId {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error-level event for UserId {UserId}", userId);
            _logger.LogCritical(ex, "Fatal-level equivalent (Critical) for RequestId {RequestId}", requestId);
            throw;
        }
    }
    #endregion


    #region GetDashboardMetricsAsync
    /// <summary>
    /// Returns top-level dashboard counters for active hubs, service locations, and open exceptions.
    /// </summary>
    public async Task<DashboardMetricsDTO> GetDashboardMetricsAsync()
    {
        _logger.LogDebug("Computing dashboard metrics");

        var totalHubs = await _repository.GetTotalActiveHubsAsync();
        var totalLocations = (await _repository.GetAllLocationsAsync()).Count(l => l.IsActive);
        var activeExceptions = await _repository.GetTotalActiveExceptionsAsync();

        if (activeExceptions > 0)
        {
            _logger.LogWarning("Active exceptions detected: {ActiveExceptions}", activeExceptions);
        }

        _logger.LogInformation(
            "Dashboard metrics computed: Hubs={TotalHubs}, Locations={TotalLocations}, Exceptions={ActiveExceptions}",
            totalHubs,
            totalLocations,
            activeExceptions);

        return new DashboardMetricsDTO
        {
            TotalActiveHubs = totalHubs,
            TotalServiceLocations = totalLocations,
            ActiveExceptions = activeExceptions
        };
    }
    #endregion


    #region GetComprehensiveStatisticsAsync
    /// <summary>
    /// Builds comprehensive shipment analytics including trends, status distribution, and delivery performance.
    /// </summary>
    public async Task<ComprehensiveDashboardDTO> GetComprehensiveStatisticsAsync()
    {
        var shipments = await _shipmentClient.GetAllShipmentsAsync();
        var exceptionsCount = await _repository.GetTotalActiveExceptionsAsync();

        var now = TimeZoneHelper.GetCurrentUtcTime();
        var currentPeriodStart = now.AddDays(-30);
        var previousPeriodStart = currentPeriodStart.AddDays(-30);

        var currentPeriodShipments = shipments.Where(s => s.CreatedAt >= currentPeriodStart).ToList();
        var previousPeriodShipments = shipments.Where(s => s.CreatedAt >= previousPeriodStart && s.CreatedAt < currentPeriodStart).ToList();

        static int CalculateChangePercent(int current, int previous)
        {
            if (previous == 0)
            {
                return current > 0 ? 100 : 0;
            }

            return (int)Math.Round(((current - previous) / (double)previous) * 100);
        }

        var total = shipments.Count;
        var inTransit = shipments.Count(s => s.Status == ShipmentStatusDto.InTransit);
        var delivered = shipments.Count(s => s.Status == ShipmentStatusDto.Delivered);

        var currentTotal = currentPeriodShipments.Count;
        var previousTotal = previousPeriodShipments.Count;

        var currentInTransit = currentPeriodShipments.Count(s => s.Status == ShipmentStatusDto.InTransit);
        var previousInTransit = previousPeriodShipments.Count(s => s.Status == ShipmentStatusDto.InTransit);

        var currentDelivered = currentPeriodShipments.Count(s => s.Status == ShipmentStatusDto.Delivered);
        var previousDelivered = previousPeriodShipments.Count(s => s.Status == ShipmentStatusDto.Delivered);

        var last7Days = Enumerable.Range(0, 7)
            .Select(offset => now.Date.AddDays(-6 + offset))
            .ToList();

        var shipmentsByDay = shipments
            .GroupBy(s => s.CreatedAt.Date)
            .ToDictionary(g => g.Key, g => g.Count());

        var dailyShipments = last7Days
            .Select(day => new DailyShipment
            {
                Date = day.ToString("MMM d"),
                Shipments = shipmentsByDay.TryGetValue(day, out var count) ? count : 0
            })
            .ToList();

        var statusDistribution = shipments
            .GroupBy(s => s.Status)
            .Select(g => new ServiceDistribution
            {
                ServiceType = g.Key.ToString(),
                Percentage = total == 0 ? 0 : Math.Round(g.Count() * 100.0 / total, 2)
            })
            .OrderByDescending(x => x.Percentage)
            .ToList();

        var performanceTrend = Enumerable.Range(0, 6)
            .Select(offset => now.AddMonths(-5 + offset))
            .Select(month =>
            {
                var monthStart = new DateTime(month.Year, month.Month, 1);
                var monthEnd = monthStart.AddMonths(1);
                var monthly = shipments.Where(s => s.CreatedAt >= monthStart && s.CreatedAt < monthEnd).ToList();
                var monthlyTotal = monthly.Count;
                var monthlyDelivered = monthly.Count(s => s.Status == ShipmentStatusDto.Delivered);

                var onTime = monthlyTotal == 0 ? 0 : Math.Round(monthlyDelivered * 100.0 / monthlyTotal, 2);

                return new PerformanceTrend
                {
                    Month = monthStart.ToString("MMM"),
                    OnTimePercent = onTime,
                    DelayedPercent = Math.Round(100 - onTime, 2)
                };
            })
            .ToList();

        var deliveredShipments = shipments.Where(s => s.Status == ShipmentStatusDto.Delivered).ToList();
        var onTimeDelivered = deliveredShipments.Count(s => (now - s.CreatedAt).TotalDays <= 5);

        var onTimeDeliveryPercent = deliveredShipments.Count == 0
            ? 0
            : Math.Round(onTimeDelivered * 100.0 / deliveredShipments.Count, 2);

        var avgDeliveryTimeDays = deliveredShipments.Count == 0
            ? 0
            : Math.Round(deliveredShipments.Average(s => (now - s.CreatedAt).TotalDays), 2);

        return new ComprehensiveDashboardDTO
        {
            TotalShipments = new StatMetric { Count = total, PercentageChange = CalculateChangePercent(currentTotal, previousTotal) },
            InTransit = new StatMetric { Count = inTransit, PercentageChange = CalculateChangePercent(currentInTransit, previousInTransit) },
            Delivered = new StatMetric { Count = delivered, PercentageChange = CalculateChangePercent(currentDelivered, previousDelivered) },
            Exceptions = new StatMetric { Count = exceptionsCount, PercentageChange = 0 },
            OnTimeDeliveryPercent = onTimeDeliveryPercent,
            AvgDeliveryTimeDays = avgDeliveryTimeDays,
            DailyShipments = dailyShipments,
            ServiceTypeDistribution = statusDistribution,
            DeliveryPerformanceTrend = performanceTrend
        };
    }
    #endregion


    #region GetAllHubsAsync
    /// <summary>
    /// Returns paginated hub records for admin management screens.
    /// </summary>
    public async Task<PaginatedResponse<HubResponseDTO>> GetAllHubsAsync(int pageNumber = 1, int pageSize = 5)
    {
        var hubs = await _repository.GetAllHubsAsync();
        var hubDtos = hubs.Select(MapToHubDto).ToList();
        var totalCount = hubDtos.Count;

        var pagedHubs = hubDtos
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return pagedHubs.ToPaginatedResponse(pageNumber, pageSize, totalCount);
    }
    #endregion


    #region GetHubByIdAsync
    /// <summary>
    /// Fetches one hub by id and validates that the requested hub exists.
    /// </summary>
    public async Task<HubResponseDTO> GetHubByIdAsync(int hubId)
    {
        if (hubId <= 0)
        {
            throw new RequestValidationException("HubId must be greater than 0.");
        }

        var hub = await _repository.GetHubByIdAsync(hubId)
            ?? throw new NotFoundException($"Hub {hubId} not found");

        return MapToHubDto(hub);
    }
    #endregion


    #region CreateHubAsync
    /// <summary>
    /// Creates a new hub after validating input and enforcing unique hub name rules.
    /// </summary>
    public async Task<HubResponseDTO> CreateHubAsync(CreateHubDTO dto)
    {
        AdminValidationHelper.ValidateHub(dto);
        var normalizedName = AdminValidationHelper.NormalizeReason(dto.Name, nameof(dto.Name));

        if (await _repository.HubNameExistsAsync(normalizedName))
        {
            throw new ConflictException("Hub name already exists.");
        }

        var hub = new Hub
        {
            Name = normalizedName,
            Address = AdminValidationHelper.NormalizeReason(dto.Address, nameof(dto.Address)),
            ContactNumber = dto.ContactNumber?.Trim() ?? string.Empty,
            ManagerName = dto.ManagerName?.Trim() ?? string.Empty,
            IsActive = dto.IsActive,
            CreatedAt = TimeZoneHelper.GetCurrentUtcTime()
        };

        await _repository.AddHubAsync(hub);
        return MapToHubDto(hub);
    }
    #endregion


    #region UpdateHubAsync
    /// <summary>
    /// Updates an existing hub after validation and duplicate-name checks.
    /// </summary>
    public async Task<HubResponseDTO> UpdateHubAsync(int hubId, UpdateHubDTO dto)
    {
        if (hubId <= 0)
        {
            throw new RequestValidationException("HubId must be greater than 0.");
        }

        AdminValidationHelper.ValidateHub(dto);

        var hub = await _repository.GetHubByIdAsync(hubId)
            ?? throw new NotFoundException($"Hub {hubId} not found");

        var normalizedName = AdminValidationHelper.NormalizeReason(dto.Name, nameof(dto.Name));
        if (await _repository.HubNameExistsAsync(normalizedName, hubId))
        {
            throw new ConflictException("Hub name already exists.");
        }

        hub.Name = normalizedName;
        hub.Address = AdminValidationHelper.NormalizeReason(dto.Address, nameof(dto.Address));
        hub.ContactNumber = dto.ContactNumber?.Trim() ?? string.Empty;
        hub.ManagerName = dto.ManagerName?.Trim() ?? string.Empty;
        hub.IsActive = dto.IsActive;

        await _repository.UpdateHubAsync(hub);
        return MapToHubDto(hub);
    }
    #endregion


    #region DeleteHubAsync
    /// <summary>
    /// Deletes a hub when it has no assigned service locations.
    /// </summary>
    public async Task DeleteHubAsync(int hubId)
    {
        if (hubId <= 0)
        {
            throw new RequestValidationException("HubId must be greater than 0.");
        }

        var hub = await _repository.GetHubByIdAsync(hubId)
            ?? throw new NotFoundException($"Hub {hubId} not found");

        if (hub.ServiceLocations.Any())
        {
            throw new RequestValidationException("Cannot delete a hub that has active service locations assigned to it.");
        }

        await _repository.DeleteHubAsync(hub);
    }
    #endregion


    #region GetAllLocationsAsync
    /// <summary>
    /// Returns paginated service locations for admin monitoring.
    /// </summary>
    public async Task<PaginatedResponse<LocationResponseDTO>> GetAllLocationsAsync(int pageNumber = 1, int pageSize = 5)
    {
        var locations = await _repository.GetAllLocationsAsync();
        var locationDtos = locations.Select(MapToLocationDto).ToList();
        var totalCount = locationDtos.Count;

        var pagedLocations = locationDtos
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return pagedLocations.ToPaginatedResponse(pageNumber, pageSize, totalCount);
    }
    #endregion


    #region CreateLocationAsync
    /// <summary>
    /// Creates a service location mapped to an existing hub with unique zip code validation.
    /// </summary>
    public async Task<LocationResponseDTO> CreateLocationAsync(CreateLocationDTO dto)
    {
        AdminValidationHelper.ValidateLocation(dto);

        _ = await _repository.GetHubByIdAsync(dto.HubId)
            ?? throw new NotFoundException($"Hub {dto.HubId} not found");

        var normalizedZipCode = AdminValidationHelper.NormalizeReason(dto.ZipCode, nameof(dto.ZipCode));
        if (await _repository.ZipCodeExistsAsync(normalizedZipCode))
        {
            throw new ConflictException("ZipCode already exists.");
        }

        var location = new ServiceLocation
        {
            HubId = dto.HubId,
            Name = AdminValidationHelper.NormalizeReason(dto.Name, nameof(dto.Name)),
            ZipCode = normalizedZipCode,
            IsActive = dto.IsActive
        };

        await _repository.AddLocationAsync(location);
        return MapToLocationDto(location);
    }
    #endregion


    #region UpdateLocationAsync
    /// <summary>
    /// Updates a service location and validates hub mapping plus zip code uniqueness.
    /// </summary>
    public async Task<LocationResponseDTO> UpdateLocationAsync(int locationId, UpdateLocationDTO dto)
    {
        if (locationId <= 0)
        {
            throw new RequestValidationException("LocationId must be greater than 0.");
        }

        AdminValidationHelper.ValidateLocation(dto);

        var location = await _repository.GetLocationByIdAsync(locationId)
            ?? throw new NotFoundException($"Location {locationId} not found");

        _ = await _repository.GetHubByIdAsync(dto.HubId)
            ?? throw new NotFoundException($"Hub {dto.HubId} not found");

        var normalizedZipCode = AdminValidationHelper.NormalizeReason(dto.ZipCode, nameof(dto.ZipCode));
        if (await _repository.ZipCodeExistsAsync(normalizedZipCode, locationId))
        {
            throw new ConflictException("ZipCode already exists.");
        }

        location.HubId = dto.HubId;
        location.Name = AdminValidationHelper.NormalizeReason(dto.Name, nameof(dto.Name));
        location.ZipCode = normalizedZipCode;
        location.IsActive = dto.IsActive;

        await _repository.UpdateLocationAsync(location);
        return MapToLocationDto(location);
    }
    #endregion


    #region DeleteLocationAsync
    /// <summary>
    /// Deletes a service location by identifier.
    /// </summary>
    public async Task DeleteLocationAsync(int locationId)
    {
        if (locationId <= 0)
        {
            throw new RequestValidationException("LocationId must be greater than 0.");
        }

        var location = await _repository.GetLocationByIdAsync(locationId)
             ?? throw new NotFoundException($"Location {locationId} not found");

        await _repository.DeleteLocationAsync(location);
    }
    #endregion


    #region GetActiveExceptionsAsync
    /// <summary>
    /// Returns paginated open exception records for admin triage.
    /// </summary>
    public async Task<PaginatedResponse<ExceptionRecordResponseDTO>> GetActiveExceptionsAsync(int pageNumber = 1, int pageSize = 5)
    {
        var exceptions = await _repository.GetActiveExceptionsAsync();
        var exceptionDtos = exceptions.Select(MapToExceptionDto).ToList();
        var totalCount = exceptionDtos.Count;

        var pagedException = exceptionDtos
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return pagedException.ToPaginatedResponse(pageNumber, pageSize, totalCount);
    }
    #endregion


    #region ResolveExceptionAsync
    /// <summary>
    /// Marks an active shipment exception as resolved and appends resolution notes.
    /// </summary>
    public async Task<ExceptionRecordResponseDTO> ResolveExceptionAsync(int shipmentId, ResolveExceptionDTO dto)
    {
        if (shipmentId <= 0)
        {
            throw new RequestValidationException("ShipmentId must be greater than 0.");
        }

        var existingException = await _repository.GetExceptionByShipmentIdAsync(shipmentId)
            ?? throw new NotFoundException($"No open exception block found for shipment {shipmentId}");

        existingException.Status = "Resolved";
        existingException.ResolvedAt = TimeZoneHelper.GetCurrentUtcTime();
        existingException.Description += $" | Resolution: {AdminValidationHelper.NormalizeReason(dto.ResolutionNotes, nameof(dto.ResolutionNotes))}";

        await _repository.UpdateExceptionRecordAsync(existingException);
        return MapToExceptionDto(existingException);
    }
    #endregion


    #region DelayShipmentAsync
    /// <summary>
    /// Creates a delay exception record for a shipment when no open delay exists.
    /// </summary>
    public async Task<ExceptionRecordResponseDTO> DelayShipmentAsync(int shipmentId, string reason)
    {
        if (shipmentId <= 0)
        {
            throw new RequestValidationException("ShipmentId must be greater than 0.");
        }

        if (await _repository.GetOpenExceptionByShipmentAndTypeAsync(shipmentId, "Delay") is not null)
        {
            throw new ConflictException("An open delay exception already exists for this shipment.");
        }

        var record = new ExceptionRecord
        {
            ShipmentId = shipmentId,
            ExceptionType = "Delay",
            Description = AdminValidationHelper.NormalizeReason(reason, nameof(reason)),
            Status = "Open",
            CreatedAt = TimeZoneHelper.GetCurrentUtcTime()
        };

        await _repository.AddExceptionRecordAsync(record);
        return MapToExceptionDto(record);
    }
    #endregion


    #region ReturnShipmentAsync
    /// <summary>
    /// Creates a return exception record for a shipment when no open return exists.
    /// </summary>
    public async Task<ExceptionRecordResponseDTO> ReturnShipmentAsync(int shipmentId, string reason)
    {
        if (shipmentId <= 0)
        {
            throw new RequestValidationException("ShipmentId must be greater than 0.");
        }

        if (await _repository.GetOpenExceptionByShipmentAndTypeAsync(shipmentId, "Return") is not null)
        {
            throw new ConflictException("An open return exception already exists for this shipment.");
        }

        var record = new ExceptionRecord
        {
            ShipmentId = shipmentId,
            ExceptionType = "Return",
            Description = AdminValidationHelper.NormalizeReason(reason, nameof(reason)),
            Status = "Open",
            CreatedAt = TimeZoneHelper.GetCurrentUtcTime()
        };

        await _repository.AddExceptionRecordAsync(record);
        return MapToExceptionDto(record);
    }
    #endregion


    #region CreateExceptionRecordFromEventAsync
    /// <summary>
    /// Persists an exception record from shipment exception events while preventing duplicates.
    /// </summary>
    public async Task CreateExceptionRecordFromEventAsync(ShipmentExceptionEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var resolvedType = string.IsNullOrWhiteSpace(@event.ExceptionType) ? "ShipmentException" : @event.ExceptionType.Trim();

        var existing = await _repository.GetOpenExceptionByShipmentAndTypeAsync(@event.ShipmentId, resolvedType);
        if (existing is not null)
        {
            return;
        }

        var description = string.IsNullOrWhiteSpace(@event.Description)
            ? $"Shipment exception received for tracking number '{@event.TrackingNumber}'."
            : @event.Description.Trim();

        var record = new ExceptionRecord
        {
            ShipmentId = @event.ShipmentId,
            ExceptionType = resolvedType,
            Description = description,
            Status = "Open",
            CreatedAt = @event.Timestamp == default ? TimeZoneHelper.GetCurrentUtcTime() : @event.Timestamp
        };

        await _repository.AddExceptionRecordAsync(record);
    }
    #endregion

    #region MapToHubDto
    /// <summary>
    /// Maps a hub entity to the hub response DTO.
    /// </summary>
    private static HubResponseDTO MapToHubDto(Hub hub)
    {
        return new HubResponseDTO
        {
            HubId = hub.HubId,
            Name = hub.Name,
            Address = hub.Address,
            ContactNumber = hub.ContactNumber,
            ManagerName = hub.ManagerName,
            IsActive = hub.IsActive,
            CreatedAt = hub.CreatedAt
        };
    }
    #endregion

    #region MapToLocationDto
    /// <summary>
    /// Maps a service location entity to the location response DTO.
    /// </summary>
    private static LocationResponseDTO MapToLocationDto(ServiceLocation location)
    {
        return new LocationResponseDTO
        {
            LocationId = location.LocationId,
            HubId = location.HubId,
            Name = location.Name,
            ZipCode = location.ZipCode,
            IsActive = location.IsActive
        };
    }
    #endregion

    #region MapToExceptionDto
    /// <summary>
    /// Maps an exception record entity to the exception response DTO.
    /// </summary>
    private static ExceptionRecordResponseDTO MapToExceptionDto(ExceptionRecord record)
    {
        return new ExceptionRecordResponseDTO
        {
            ExceptionId = record.ExceptionId,
            ShipmentId = record.ShipmentId,
            ExceptionType = record.ExceptionType,
            Description = record.Description,
            Status = record.Status,
            CreatedAt = record.CreatedAt,
            ResolvedAt = record.ResolvedAt
        };
    }
    #endregion


    #region GetShipmentsAsync
    /// <summary>
    /// Returns paginated shipments ordered by latest creation timestamp.
    /// </summary>
    public async Task<PaginatedResponse<ShipmentExternalDto>> GetShipmentsAsync(int pageNumber = 1, int pageSize = 5)
    {
        var allShipments = await _shipmentClient.GetAllShipmentsAsync();
        var orderedShipments = allShipments
            .OrderByDescending(s => s.CreatedAt)
            .ToList();
        var totalCount = orderedShipments.Count;

        var pagedShipments = orderedShipments
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return pagedShipments.ToPaginatedResponse(pageNumber, pageSize, totalCount);
    }
    #endregion


    #region GetShipmentByIdAsync
    /// <summary>
    /// Fetches shipment details from the shipment service by shipment id.
    /// </summary>
    public async Task<ShipmentExternalDto> GetShipmentByIdAsync(int shipmentId)
    {
        if (shipmentId <= 0)
        {
            throw new RequestValidationException("ShipmentId must be greater than 0.");
        }

        var shipment = await _shipmentClient.GetShipmentByIdAsync(shipmentId);
        if (shipment == null) throw new NotFoundException($"Shipment {shipmentId} not found");
        return shipment;
    }
    #endregion


    #region GetShipmentsByHubAsync
    /// <summary>
    /// Returns paginated shipments associated with the selected hub service location zip codes.
    /// </summary>
    public async Task<PaginatedResponse<ShipmentExternalDto>> GetShipmentsByHubAsync(int hubId, int pageNumber = 1, int pageSize = 5)
    {
        if (hubId <= 0)
        {
            throw new RequestValidationException("HubId must be greater than 0.");
        }

        var hub = await _repository.GetHubByIdAsync(hubId)
            ?? throw new NotFoundException($"Hub {hubId} not found");

        var zipCodes = hub.ServiceLocations.Select(sl => sl.ZipCode).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var allShipments = await _shipmentClient.GetAllShipmentsAsync();

        var filteredShipments = allShipments.Where(s =>
            (s.SenderAddress is not null && zipCodes.Contains(s.SenderAddress.PostalCode)) ||
            (s.ReceiverAddress is not null && zipCodes.Contains(s.ReceiverAddress.PostalCode))
        )
        .OrderByDescending(s => s.CreatedAt)
        .ToList();

        var totalCount = filteredShipments.Count;
        var pagedShipments = filteredShipments
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return pagedShipments.ToPaginatedResponse(pageNumber, pageSize, totalCount);
    }
    #endregion


    #region GetReportsOverviewAsync
    /// <summary>
    /// Returns summary reporting metrics for total volume, revenue, and active exceptions.
    /// </summary>
    public async Task<object> GetReportsOverviewAsync()
    {
        var shipments = await _shipmentClient.GetAllShipmentsAsync();
        return new
        {
            TotalShipmentsProcessed = shipments.Count,
            TotalRevenue = shipments.Sum(s => s.TotalWeight * 5),
            TotalActiveExceptions = await _repository.GetTotalActiveExceptionsAsync()
        };
    }
    #endregion


    #region GetShipmentPerformanceAsync
    /// <summary>
    /// Calculates delivered shipment ratio to indicate operational performance.
    /// </summary>
    public async Task<object> GetShipmentPerformanceAsync()
    {
        var shipments = await _shipmentClient.GetAllShipmentsAsync();
        var total = shipments.Count;
        if (total == 0) return new { Performance = "N/A - No shipments" };

        var delivered = shipments.Count(s => s.Status == ShipmentStatusDto.Delivered);
        var performance = (double)delivered / total * 100;

        return new
        {
            TotalShipments = total,
            Delivered = delivered,
            PerformanceRatio = $"{performance:F2}% Delivery Rate"
        };
    }
    #endregion


    #region GetDeliverySLAAsync
    /// <summary>
    /// Calculates SLA compliance percentage for shipments delivered within the target window.
    /// </summary>
    public async Task<object> GetDeliverySLAAsync()
    {
        var shipments = await _shipmentClient.GetAllShipmentsAsync();
        var total = shipments.Count;
        if (total == 0) return new { Message = "No shipments to calculate SLA" };

        var metSla = shipments.Count(s => s.Status == ShipmentStatusDto.Delivered && (TimeZoneHelper.GetCurrentUtcTime() - s.CreatedAt).TotalDays <= 5);
        var slaRatio = (double)metSla / total * 100;

        return new
        {
            SLATarget = "95% Delivered under 5 Days",
            CurrentSLA = $"{slaRatio:F2}%"
        };
    }
    #endregion


    #region GetRevenueAsync
    /// <summary>
    /// Calculates revenue for the recent period using shipment weight totals.
    /// </summary>
    public async Task<object> GetRevenueAsync()
    {
        var shipments = await _shipmentClient.GetAllShipmentsAsync();
        var totalWeight = shipments.Sum(s => s.TotalWeight);
        var totalRevenue = totalWeight * 5.25m;

        return new
        {
            Period = $"{TimeZoneHelper.GetCurrentUtcTime().AddDays(-30):MMM dd} - {TimeZoneHelper.GetCurrentUtcTime():MMM dd}",
            TotalWeightProcessed = totalWeight,
            CalculatedRevenueAmount = totalRevenue
        };
    }
    #endregion


    #region GetHubPerformanceAsync
    /// <summary>
    /// Builds per-hub shipment volume rankings for operational comparison.
    /// </summary>
    public async Task<object> GetHubPerformanceAsync()
    {
        var hubs = await _repository.GetAllHubsAsync();
        var shipments = await _shipmentClient.GetAllShipmentsAsync();

        var hubPerformances = hubs
            .Select(hub =>
            {
                var zips = hub.ServiceLocations.Select(sl => sl.ZipCode).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var volume = shipments.Count(s =>
                    (s.SenderAddress is not null && zips.Contains(s.SenderAddress.PostalCode)) ||
                    (s.ReceiverAddress is not null && zips.Contains(s.ReceiverAddress.PostalCode)));

                return new
                {
                    HubName = hub.Name,
                    VolumeHandled = volume
                };
            })
            .OrderByDescending(x => x.VolumeHandled)
            .ToList();

        return new
        {
            ReportGenerator = "System",
            HubPerformance = hubPerformances
        };
    }
    #endregion
}


