using Microsoft.Extensions.DependencyInjection;
using SmartShip.EventBus.Abstractions;
using SmartShip.EventBus.Constants;
using SmartShip.EventBus.Contracts;
using SmartShip.TrackingService.DTOs;
using SmartShip.TrackingService.Services;
using Serilog.Context;

namespace SmartShip.TrackingService.BackgroundServices;

/// <summary>
/// Implements tracking shipment events consumer business workflows for SmartShip logistics operations.
/// </summary>
public sealed class TrackingShipmentEventsConsumerService : BackgroundService
{
    private readonly IEventConsumer _eventConsumer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TrackingShipmentEventsConsumerService> _logger;

    public TrackingShipmentEventsConsumerService(
        IEventConsumer eventConsumer,
        IServiceScopeFactory scopeFactory,
        ILogger<TrackingShipmentEventsConsumerService> logger)
    {
        _eventConsumer = eventConsumer;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumeCreatedTask = _eventConsumer.ConsumeAsync<ShipmentCreatedEvent>(
            RabbitMqQueues.ShipmentCreatedQueue,
            HandleShipmentCreatedAsync,
            stoppingToken);

        var consumeBookedTask = _eventConsumer.ConsumeAsync<ShipmentBookedEvent>(
            RabbitMqQueues.ShipmentBookedQueue,
            HandleShipmentBookedAsync,
            stoppingToken);

        var consumePickedUpTask = _eventConsumer.ConsumeAsync<ShipmentPickedUpEvent>(
            RabbitMqQueues.ShipmentPickedUpQueue,
            HandleShipmentPickedUpAsync,
            stoppingToken);

        var consumeInTransitTask = _eventConsumer.ConsumeAsync<ShipmentInTransitEvent>(
            RabbitMqQueues.ShipmentInTransitQueue,
            HandleShipmentInTransitAsync,
            stoppingToken);

        var consumeOutForDeliveryTask = _eventConsumer.ConsumeAsync<ShipmentOutForDeliveryEvent>(
            RabbitMqQueues.ShipmentOutForDeliveryQueue,
            HandleShipmentOutForDeliveryAsync,
            stoppingToken);

        var consumeDeliveredTask = _eventConsumer.ConsumeAsync<ShipmentDeliveredEvent>(
            RabbitMqQueues.ShipmentDeliveredQueue,
            HandleShipmentDeliveredAsync,
            stoppingToken);

        _logger.LogInformation("Tracking service RabbitMQ consumers started.");

        await Task.WhenAll(
            consumeCreatedTask,
            consumeBookedTask,
            consumePickedUpTask,
            consumeInTransitTask,
            consumeOutForDeliveryTask,
            consumeDeliveredTask);
    }

    /// <summary>
    /// Processes shipment created asynchronously.
    /// </summary>
    private async Task HandleShipmentCreatedAsync(ShipmentCreatedEvent @event, CancellationToken cancellationToken)
    {
        // ✓ Generate correlation ID for event processing
        var correlationId = Guid.NewGuid().ToString();
        using var logContext = LogContext.PushProperty("CorrelationId", correlationId);

        _logger.LogInformation("Processing shipment created event. ShipmentId: {ShipmentId}, TrackingNumber: {TrackingNumber}", @event.ShipmentId, @event.TrackingNumber);
        await AddTrackingEventAsync(@event, "Created", "Shipment created in system");
    }

    /// <summary>
    /// Processes shipment booked asynchronously.
    /// </summary>
    private async Task HandleShipmentBookedAsync(ShipmentBookedEvent @event, CancellationToken cancellationToken)
    {
        // ✓ Generate correlation ID for event processing
        var correlationId = Guid.NewGuid().ToString();
        using var logContext = LogContext.PushProperty("CorrelationId", correlationId);

        _logger.LogInformation("Processing shipment booked event. ShipmentId: {ShipmentId}", @event.ShipmentId);
        await AddTrackingEventAsync(@event, "Booked", "Shipment booked by admin");
    }

    /// <summary>
    /// Processes shipment picked up asynchronously.
    /// </summary>
    private async Task HandleShipmentPickedUpAsync(ShipmentPickedUpEvent @event, CancellationToken cancellationToken)
    {
        // ✓ Generate correlation ID for event processing
        var correlationId = Guid.NewGuid().ToString();
        using var logContext = LogContext.PushProperty("CorrelationId", correlationId);

        _logger.LogInformation("Processing shipment picked up event. ShipmentId: {ShipmentId}", @event.ShipmentId);
        await AddTrackingEventAsync(@event, "PickedUp", "Shipment picked up from sender");
    }

    /// <summary>
    /// Processes shipment in transit asynchronously.
    /// </summary>
    private async Task HandleShipmentInTransitAsync(ShipmentInTransitEvent @event, CancellationToken cancellationToken)
    {
        // ✓ Generate correlation ID for event processing
        var correlationId = Guid.NewGuid().ToString();
        using var logContext = LogContext.PushProperty("CorrelationId", correlationId);

        _logger.LogInformation("Processing shipment in transit event. ShipmentId: {ShipmentId}", @event.ShipmentId);
        await AddTrackingEventAsync(@event, "InTransit", "Shipment moved through transit hub");
    }

    /// <summary>
    /// Processes shipment out for delivery asynchronously.
    /// </summary>
    private async Task HandleShipmentOutForDeliveryAsync(ShipmentOutForDeliveryEvent @event, CancellationToken cancellationToken)
    {
        // ✓ Generate correlation ID for event processing
        var correlationId = Guid.NewGuid().ToString();
        using var logContext = LogContext.PushProperty("CorrelationId", correlationId);

        _logger.LogInformation("Processing shipment out for delivery event. ShipmentId: {ShipmentId}", @event.ShipmentId);
        await AddTrackingEventAsync(@event, "OutForDelivery", "Shipment is out for delivery");
    }

    /// <summary>
    /// Processes shipment delivered asynchronously.
    /// </summary>
    private async Task HandleShipmentDeliveredAsync(ShipmentDeliveredEvent @event, CancellationToken cancellationToken)
    {
        // ✓ Generate correlation ID for event processing
        var correlationId = Guid.NewGuid().ToString();
        using var logContext = LogContext.PushProperty("CorrelationId", correlationId);

        _logger.LogInformation("Processing shipment delivered event. ShipmentId: {ShipmentId}", @event.ShipmentId);
        await AddTrackingEventAsync(@event, "Delivered", "Shipment delivered to receiver");
    }

    /// <summary>
    /// Adds tracking event async.
    /// </summary>
    private async Task AddTrackingEventAsync(ShipmentEventBase @event, string status, string description)
    {
        using var scope = _scopeFactory.CreateScope();
        var trackingService = scope.ServiceProvider.GetRequiredService<ITrackingService>();
        var resolvedLocation = await ResolveLocationAsync(@event, trackingService);

        await trackingService.AddTrackingEventAsync(new TrackingEventDTO
        {
            TrackingNumber = @event.TrackingNumber,
            Status = status,
            Location = resolvedLocation,
            Description = description,
            Timestamp = @event.Timestamp
        });
    }

    /// <summary>
    /// Resolves location async.
    /// </summary>
    private static async Task<string> ResolveLocationAsync(ShipmentEventBase @event, ITrackingService trackingService)
    {
        if (!string.IsNullOrWhiteSpace(@event.HubLocation))
        {
            return @event.HubLocation.Trim();
        }

        try
        {
            var trackingInfo = await trackingService.GetTrackingInfoAsync(@event.TrackingNumber);
            var currentLocation = trackingInfo.CurrentLocation?.Trim();
            if (!string.IsNullOrWhiteSpace(currentLocation))
            {
                return currentLocation;
            }
        }
        catch
        {
            // Ignore and fallback to default.
        }

        return "SmartShip Hub";
    }
}


