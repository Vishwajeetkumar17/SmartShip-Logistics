/// <summary>
/// Provides backend implementation for ShipmentDelayExceptionMonitorService.
/// </summary>

using Microsoft.Extensions.Options;
using SmartShip.EventBus.Abstractions;
using SmartShip.EventBus.Constants;
using SmartShip.EventBus.Contracts;
using SmartShip.ShipmentService.Configuration;
using SmartShip.ShipmentService.Enums;
using SmartShip.ShipmentService.Repositories;
using SmartShip.Shared.Common.Helpers;

namespace SmartShip.ShipmentService.BackgroundServices;

/// <summary>
/// Represents ShipmentDelayExceptionMonitorService.
/// </summary>
public sealed class ShipmentDelayExceptionMonitorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly DelayExceptionMonitoringOptions _options;
    private readonly ILogger<ShipmentDelayExceptionMonitorService> _logger;

    public ShipmentDelayExceptionMonitorService(
        IServiceScopeFactory scopeFactory,
        IOptions<DelayExceptionMonitoringOptions> options,
        ILogger<ShipmentDelayExceptionMonitorService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalMinutes = _options.CheckIntervalMinutes <= 0 ? 10 : _options.CheckIntervalMinutes;
        var delay = TimeSpan.FromMinutes(intervalMinutes);

        _logger.LogInformation("Shipment delay monitor started with interval {IntervalMinutes} minutes.", intervalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckDelayedShipmentsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while checking delayed shipments.");
            }

            await Task.Delay(delay, stoppingToken);
        }
    }

    private async Task CheckDelayedShipmentsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IShipmentRepository>();
        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

        var shipments = await repository.GetAllAsync();
        var now = TimeZoneHelper.GetCurrentUtcTime();

        foreach (var shipment in shipments)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (shipment.Status is ShipmentStatus.Draft or ShipmentStatus.Delivered)
            {
                continue;
            }

            var thresholdHours = ResolveThresholdHours(shipment.ServiceType);
            var elapsedHours = (now - shipment.CreatedAt).TotalHours;

            if (elapsedHours < thresholdHours)
            {
                continue;
            }

            var exceptionDescription = $"Shipment delayed for {elapsedHours:F1} hours (threshold: {thresholdHours} hours) for service type '{shipment.ServiceType}'.";

            await publisher.PublishAsync(
                RabbitMqQueues.ShipmentExceptionQueue,
                new ShipmentExceptionEvent
                {
                    ShipmentId = shipment.ShipmentId,
                    TrackingNumber = shipment.TrackingNumber,
                    CustomerId = shipment.CustomerId,
                    Timestamp = now,
                    ExceptionType = "DelayedShipment",
                    Description = exceptionDescription,
                    Source = "System"
                },
                cancellationToken);
        }
    }

    private int ResolveThresholdHours(string? serviceType)
    {
        var normalized = (serviceType ?? string.Empty).Trim().ToLowerInvariant();
        if (normalized == "express")
        {
            return _options.ExpressThresholdHours;
        }

        if (normalized == "economy")
        {
            return _options.EconomyThresholdHours;
        }

        return _options.StandardThresholdHours;
    }
}



    #endregion
}


