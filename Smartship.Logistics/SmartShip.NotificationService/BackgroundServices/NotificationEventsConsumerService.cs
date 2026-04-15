using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartShip.EventBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using SmartShip.EventBus.Constants;
using SmartShip.EventBus.Contracts;
using SmartShip.NotificationService.Configurations;
using SmartShip.NotificationService.Integration;
using SmartShip.NotificationService.Services;
using Serilog.Context;

namespace SmartShip.NotificationService.BackgroundServices;

/// <summary>
/// Implements notification events consumer business workflows for SmartShip logistics operations.
/// </summary>
public sealed class NotificationEventsConsumerService : BackgroundService
{
    #region Static Fields
    private static readonly TimeZoneInfo IndianTimeZone = ResolveIndianTimeZone();
    private static readonly TimeSpan ConsumerStartupRetryDelay = TimeSpan.FromSeconds(5);
    #endregion

    #region Fields
    private readonly IEventConsumer _eventConsumer;
    private readonly IIdentityContactClient _identityContactClient;
    private readonly NotificationSettings _notificationSettings;
    private readonly ILogger<NotificationEventsConsumerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    #endregion

    #region Constructor
    /// <summary>
    /// Implements notification events consumer service workflows.
    /// </summary>
    public NotificationEventsConsumerService(
        IEventConsumer eventConsumer,
        IIdentityContactClient identityContactClient,
        IOptions<NotificationSettings> notificationOptions,
        ILogger<NotificationEventsConsumerService> logger,
        IServiceProvider serviceProvider)
    {
        _eventConsumer = eventConsumer;
        _identityContactClient = identityContactClient;
        _notificationSettings = notificationOptions.Value;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }
    #endregion

    #region Protected API
    /// <summary>
    /// Processes execute asynchronously.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeUserCreatedTask = _eventConsumer.ConsumeAsync<UserCreatedEvent>(
                    RabbitMqQueues.UserCreatedQueue,
                    HandleUserCreatedAsync,
                    stoppingToken);

                var consumeShipmentCreatedTask = _eventConsumer.ConsumeAsync<ShipmentCreatedEvent>(
                    RabbitMqQueues.ShipmentCreatedQueue,
                    HandleShipmentCreatedAsync,
                    stoppingToken);

                var consumeOutForDeliveryTask = _eventConsumer.ConsumeAsync<ShipmentOutForDeliveryEvent>(
                    RabbitMqQueues.ShipmentOutForDeliveryQueue,
                    HandleShipmentOutForDeliveryAsync,
                    stoppingToken);

                var consumeDeliveredTask = _eventConsumer.ConsumeAsync<ShipmentDeliveredEvent>(
                    RabbitMqQueues.ShipmentDeliveredQueue,
                    HandleShipmentDeliveredAsync,
                    stoppingToken);

                _logger.LogInformation("Notification service RabbitMQ consumers started.");

                await Task.WhenAll(
                    consumeUserCreatedTask,
                    consumeShipmentCreatedTask,
                    consumeOutForDeliveryTask,
                    consumeDeliveredTask);

                return;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to initialize notification consumers. Retrying in {DelaySeconds} seconds.", ConsumerStartupRetryDelay.TotalSeconds);

                try
                {
                    await Task.Delay(ConsumerStartupRetryDelay, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
            }
        }
    }
    #endregion

    #region User Event Handling
    /// <summary>
    /// Processes user created asynchronously.
    /// </summary>
    private async Task HandleUserCreatedAsync(UserCreatedEvent @event, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(@event.Email))
        {
            return;
        }

        var createdAtIst = ConvertToIndianTime(@event.Timestamp);

        var subject = "Welcome to SmartShip";

        var body = $"""
        Hi {@event.Name},

        Welcome to SmartShip.

        Your account has been created successfully, and you’re now ready to get started.

        Account details:
        • Email: {@event.Email}
        • Role: {@event.Role}
        • Created At (IST): {createdAtIst:yyyy-MM-dd HH:mm:ss}

        SmartShip helps you manage shipments with ease and reliability. If you need any assistance, our team is always here to help.

        We’re glad to have you with us.

        Best regards,  
        SmartShip Team
        """;

        using var scope = _serviceProvider.CreateScope();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailNotificationService>();
        await emailService.SendEmailAsync([@event.Email], subject, body, cancellationToken);
    }
    #endregion

    #region Timezone Helpers
    /// <summary>
    /// Converts to indian time.
    /// </summary>
    private static DateTime ConvertToIndianTime(DateTime timestamp)
    {
        var utcTimestamp = timestamp.Kind switch
        {
            DateTimeKind.Utc => timestamp,
            DateTimeKind.Local => timestamp.ToUniversalTime(),
            _ => DateTime.SpecifyKind(timestamp, DateTimeKind.Utc)
        };

        return TimeZoneInfo.ConvertTimeFromUtc(utcTimestamp, IndianTimeZone);
    }

    private static TimeZoneInfo ResolveIndianTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        }
    }
    #endregion

    #region Shipment Event Handling
    /// <summary>
    /// Processes shipment created asynchronously.
    /// </summary>
    private async Task HandleShipmentCreatedAsync(ShipmentCreatedEvent @event, CancellationToken cancellationToken)
    {
        // Generate correlation ID for this event processing for distributed tracing
        var correlationId = Guid.NewGuid().ToString();
        using var logContext = LogContext.PushProperty("CorrelationId", correlationId);

        var recipients = await ResolveRecipientsAsync(@event.CustomerId, includeAdminRecipients: true, requireCustomerRecipient: true, cancellationToken, correlationId);
        if (recipients.Count == 0)
        {
            _logger.LogWarning("No recipients found for shipment created notification. ShipmentId: {ShipmentId}, CustomerId: {CustomerId}", @event.ShipmentId, @event.CustomerId);
            return;
        }

        var createdAtIst = ConvertToIndianTime(@event.Timestamp);

        var subject = $"Shipment Created: {@event.TrackingNumber}";
        var body = $"""
            Shipment creation confirmed.

            Shipment ID: {@event.ShipmentId}
            Tracking Number: {@event.TrackingNumber}
            Customer ID: {@event.CustomerId}
            Created At (IST): {createdAtIst:yyyy-MM-dd HH:mm:ss}

            - SmartShip Team
            """;

        using var scope = _serviceProvider.CreateScope();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailNotificationService>();
        await emailService.SendEmailAsync(recipients, subject, body, cancellationToken);

        _logger.LogInformation("Shipment created notification sent. ShipmentId: {ShipmentId}, TrackingNumber: {TrackingNumber}, CorrelationId: {CorrelationId}", @event.ShipmentId, @event.TrackingNumber, correlationId);
    }

    /// <summary>
    /// Processes shipment out for delivery asynchronously.
    /// </summary>
    private async Task HandleShipmentOutForDeliveryAsync(ShipmentOutForDeliveryEvent @event, CancellationToken cancellationToken)
    {
        // Generate correlation ID for this event processing for distributed tracing
        var correlationId = Guid.NewGuid().ToString();
        using var logContext = LogContext.PushProperty("CorrelationId", correlationId);

        var recipients = await ResolveRecipientsAsync(@event.CustomerId, includeAdminRecipients: false, requireCustomerRecipient: true, cancellationToken, correlationId);
        if (recipients.Count == 0)
        {
            _logger.LogWarning("No recipients found for out-for-delivery notification. ShipmentId: {ShipmentId}, CustomerId: {CustomerId}", @event.ShipmentId, @event.CustomerId);
            return;
        }

        var updatedAtIst = ConvertToIndianTime(@event.Timestamp);

        var subject = $"Out for Delivery: {@event.TrackingNumber}";
        var body = $"""
            Your shipment is out for delivery.

            Tracking Number: {@event.TrackingNumber}
            Shipment ID: {@event.ShipmentId}
            Current Hub: {@event.HubLocation}
            Updated At (IST): {updatedAtIst:yyyy-MM-dd HH:mm:ss}

            - SmartShip Team
            """;

        using var scope = _serviceProvider.CreateScope();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailNotificationService>();
        await emailService.SendEmailAsync(recipients, subject, body, cancellationToken);
    }

    /// <summary>
    /// Processes shipment delivered asynchronously.
    /// </summary>
    private async Task HandleShipmentDeliveredAsync(ShipmentDeliveredEvent @event, CancellationToken cancellationToken)
    {
        // Generate correlation ID for this event processing for distributed tracing
        var correlationId = Guid.NewGuid().ToString();
        using var logContext = LogContext.PushProperty("CorrelationId", correlationId);

        var recipients = await ResolveRecipientsAsync(@event.CustomerId, includeAdminRecipients: false, requireCustomerRecipient: true, cancellationToken, correlationId);
        if (recipients.Count == 0)
        {
            _logger.LogWarning("No recipients found for delivered notification. ShipmentId: {ShipmentId}, CustomerId: {CustomerId}", @event.ShipmentId, @event.CustomerId);
            return;
        }

        var deliveredAtIst = ConvertToIndianTime(@event.Timestamp);

        var subject = $"Delivered: {@event.TrackingNumber}";
        var body = $"""
            Your shipment has been delivered successfully.

            Tracking Number: {@event.TrackingNumber}
            Shipment ID: {@event.ShipmentId}
            Delivery Time (IST): {deliveredAtIst:yyyy-MM-dd HH:mm:ss}

            - SmartShip Team
            """;

        using var scope = _serviceProvider.CreateScope();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailNotificationService>();
        await emailService.SendEmailAsync(recipients, subject, body, cancellationToken);
    }

    /// <summary>
    /// Resolves recipients async.
    /// </summary>
    private async Task<List<string>> ResolveRecipientsAsync(int customerId, bool includeAdminRecipients, bool requireCustomerRecipient, CancellationToken cancellationToken, string? correlationId = null)
    {
        var recipients = new List<string>();

        if (includeAdminRecipients)
        {
            recipients.AddRange(_notificationSettings.GetAdminEmails());
        }

        // ✓ Pass correlation ID for distributed tracing
        var customerContact = await _identityContactClient.GetUserContactAsync(customerId, cancellationToken, correlationId);
        var customerEmail = customerContact?.Email?.Trim();

        if (!string.IsNullOrWhiteSpace(customerEmail))
        {
            recipients.Add(customerEmail);
        }
        else if (requireCustomerRecipient)
        {
            throw new InvalidOperationException($"Customer email could not be resolved for customerId {customerId}.");
        }

        return recipients
            .Where(email => !string.IsNullOrWhiteSpace(email))
            .Select(email => email.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
    #endregion
}


