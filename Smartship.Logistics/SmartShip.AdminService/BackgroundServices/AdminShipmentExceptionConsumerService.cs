/// <summary>
/// Provides backend implementation for AdminShipmentExceptionConsumerService.
/// </summary>

using Microsoft.Extensions.DependencyInjection;
using SmartShip.AdminService.Services;
using SmartShip.EventBus.Abstractions;
using SmartShip.EventBus.Constants;
using SmartShip.EventBus.Contracts;
using Serilog.Context;

namespace SmartShip.AdminService.BackgroundServices;

/// <summary>
/// Represents AdminShipmentExceptionConsumerService.
/// </summary>
public sealed class AdminShipmentExceptionConsumerService : BackgroundService
{
    private readonly IEventConsumer _eventConsumer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AdminShipmentExceptionConsumerService> _logger;

    public AdminShipmentExceptionConsumerService(
        IEventConsumer eventConsumer,
        IServiceScopeFactory scopeFactory,
        ILogger<AdminShipmentExceptionConsumerService> logger)
    {
        _eventConsumer = eventConsumer;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Admin service RabbitMQ exception consumer started.");

        await _eventConsumer.ConsumeAsync<ShipmentExceptionEvent>(
            RabbitMqQueues.ShipmentExceptionQueue,
            HandleShipmentExceptionAsync,
            stoppingToken);
    }

    private async Task HandleShipmentExceptionAsync(ShipmentExceptionEvent @event, CancellationToken cancellationToken)
    {
        // ✓ Generate correlation ID for event processing
        var correlationId = Guid.NewGuid().ToString();
        using var logContext = LogContext.PushProperty("CorrelationId", correlationId);

        _logger.LogInformation("Processing shipment exception event. ShipmentId: {ShipmentId}", @event.ShipmentId);

        using var scope = _scopeFactory.CreateScope();
        var adminService = scope.ServiceProvider.GetRequiredService<IAdminService>();

        await adminService.CreateExceptionRecordFromEventAsync(@event);
    }
}


