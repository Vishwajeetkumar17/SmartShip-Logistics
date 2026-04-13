/// <summary>
/// Provides backend implementation for DocumentShipmentEventsConsumerService.
/// </summary>

using Microsoft.Extensions.DependencyInjection;
using SmartShip.DocumentService.Services;
using SmartShip.EventBus.Abstractions;
using SmartShip.EventBus.Constants;
using SmartShip.EventBus.Contracts;
using Serilog.Context;

namespace SmartShip.DocumentService.BackgroundServices;

/// <summary>
/// Represents DocumentShipmentEventsConsumerService.
/// </summary>
public sealed class DocumentShipmentEventsConsumerService : BackgroundService
{
    private readonly IEventConsumer _eventConsumer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DocumentShipmentEventsConsumerService> _logger;

    public DocumentShipmentEventsConsumerService(
        IEventConsumer eventConsumer,
        IServiceScopeFactory scopeFactory,
        ILogger<DocumentShipmentEventsConsumerService> logger)
    {
        _eventConsumer = eventConsumer;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Document service RabbitMQ consumer started.");

        await _eventConsumer.ConsumeAsync<ShipmentDeliveredEvent>(
            RabbitMqQueues.ShipmentDeliveredQueue,
            HandleShipmentDeliveredAsync,
            stoppingToken);
    }

    private async Task HandleShipmentDeliveredAsync(ShipmentDeliveredEvent @event, CancellationToken cancellationToken)
    {
        // ✓ Generate correlation ID for event processing
        var correlationId = Guid.NewGuid().ToString();
        using var logContext = LogContext.PushProperty("CorrelationId", correlationId);

        _logger.LogInformation("Processing shipment delivered event. ShipmentId: {ShipmentId}", @event.ShipmentId);

        using var scope = _scopeFactory.CreateScope();
        var documentService = scope.ServiceProvider.GetRequiredService<IDocumentService>();

        await documentService.CreateDeliveryConfirmationDocumentAsync(@event);
    }
}


