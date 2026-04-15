using System.Text;
using System.Text.Json;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SmartShip.EventBus.Abstractions;
using SmartShip.EventBus.Configuration;

namespace SmartShip.EventBus.Infrastructure;

/// <summary>
/// Domain model for rabbit mqconsumer.
/// </summary>
public sealed class RabbitMQConsumer : IEventConsumer
{
    #region Constants
    private const string RetryHeader = "x-retry-count";
    private const string DeadLetterRoutingKey = "dead";
    #endregion

    #region Fields
    private readonly RabbitMQConnectionManager _connectionManager;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMQConsumer> _logger;
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);
    private readonly string _consumerId = (Assembly.GetEntryAssembly()?.GetName().Name ?? "service").ToLowerInvariant();
    #endregion

    #region Construction
    /// <summary>
    /// Processes rabbit mqconsumer.
    /// </summary>
    public RabbitMQConsumer(
        RabbitMQConnectionManager connectionManager,
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMQConsumer> logger)
    {
        _connectionManager = connectionManager;
        _options = options.Value;
        _logger = logger;
    }
    #endregion

    #region Public API
    /// <summary>
    /// Code summary.
    /// </summary>
    public async Task ConsumeAsync<T>(string queueName, Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName);
        ArgumentNullException.ThrowIfNull(handler);

        var connection = await _connectionManager.GetConnectionAsync(cancellationToken);
        using var channel = connection.CreateModel();

        var consumerQueueName = BuildConsumerQueueName(queueName, _consumerId);
        var retryQueueName = $"{consumerQueueName}.retry";
        var dlxExchangeName = $"{consumerQueueName}.dlx";
        var dlqQueueName = $"{consumerQueueName}.dlq";

        // Pub/Sub stream per event type (fanout exchange)
        channel.ExchangeDeclare(exchange: queueName, type: ExchangeType.Fanout, durable: true, autoDelete: false, arguments: null);

        // Production-grade: Dead-letter exchange + DLQ per consumer queue
        // When a message is rejected/nacked (requeue=false) it is routed to DLQ instead of being dropped.
        channel.ExchangeDeclare(exchange: dlxExchangeName, type: ExchangeType.Direct, durable: true, autoDelete: false, arguments: null);
        channel.QueueDeclare(queue: dlqQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        channel.QueueBind(queue: dlqQueueName, exchange: dlxExchangeName, routingKey: DeadLetterRoutingKey);

        // Main consumer queue (durable) with dead-lettering enabled
        var consumerQueueArgs = new Dictionary<string, object>
        {
            ["x-dead-letter-exchange"] = dlxExchangeName,
            ["x-dead-letter-routing-key"] = DeadLetterRoutingKey
        };

        channel.QueueDeclare(queue: consumerQueueName, durable: true, exclusive: false, autoDelete: false, arguments: consumerQueueArgs);
        channel.QueueBind(queue: consumerQueueName, exchange: queueName, routingKey: string.Empty);

        // Retry queue with TTL-per-message. When message expires it dead-letters back to the main consumer queue (default exchange routing).
        // This provides delayed retries without blocking the consumer.
        var retryQueueArgs = new Dictionary<string, object>
        {
            ["x-dead-letter-exchange"] = string.Empty,
            ["x-dead-letter-routing-key"] = consumerQueueName
        };
        channel.QueueDeclare(queue: retryQueueName, durable: true, exclusive: false, autoDelete: false, arguments: retryQueueArgs);

        channel.BasicQos(prefetchSize: 0, prefetchCount: _options.PrefetchCount, global: false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (_, eventArgs) =>
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                var @event = JsonSerializer.Deserialize<T>(eventArgs.Body.Span, _serializerOptions);
                if (@event is null)
                {
                    _logger.LogError("Failed to deserialize message from queue {QueueName} as {EventType}", queueName, typeof(T).Name);
                    channel.BasicReject(eventArgs.DeliveryTag, requeue: false);
                    return;
                }

                await handler(@event, cancellationToken);
                channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                var retryCount = ReadRetryCount(eventArgs.BasicProperties?.Headers);
                var maxRetries = Math.Max(0, _options.ConsumerMaxRetryAttempts);

                if (retryCount < maxRetries)
                {
                    var nextRetry = retryCount + 1;
                    var delay = TimeSpan.FromSeconds(_options.BaseRetryDelaySeconds * Math.Pow(2, retryCount));
                    var delayMs = Math.Max(1, (int)delay.TotalMilliseconds);

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.ContentType = eventArgs.BasicProperties?.ContentType ?? "application/json";
                    properties.Type = eventArgs.BasicProperties?.Type ?? typeof(T).Name;
                    properties.MessageId = eventArgs.BasicProperties?.MessageId ?? Guid.NewGuid().ToString("n");
                    properties.CorrelationId = eventArgs.BasicProperties?.CorrelationId;
                    properties.Headers = eventArgs.BasicProperties?.Headers is null
                        ? new Dictionary<string, object>()
                        : new Dictionary<string, object>(eventArgs.BasicProperties.Headers);
                    properties.Headers[RetryHeader] = nextRetry;
                    properties.Expiration = delayMs.ToString();

                    // Send to retry queue (delayed via per-message TTL), then it dead-letters back to main consumer queue.
                    channel.BasicPublish(exchange: string.Empty, routingKey: retryQueueName, basicProperties: properties, body: eventArgs.Body);
                    channel.BasicAck(eventArgs.DeliveryTag, multiple: false);

                    _logger.LogWarning(
                        ex,
                        "Error processing {EventType} from {QueueName} ({ConsumerQueueName}). Scheduled retry {RetryCount}/{MaxRetries} after {DelayMs}ms",
                        typeof(T).Name,
                        queueName,
                        consumerQueueName,
                        nextRetry,
                        maxRetries,
                        delayMs);
                    return;
                }

                _logger.LogError(ex, "Error processing {EventType} from {QueueName}. Sending to DLQ after max retries", typeof(T).Name, queueName);
                // Dead-letter to DLQ (configured on the consumer queue).
                channel.BasicReject(eventArgs.DeliveryTag, requeue: false);
            }
        };

        var consumerTag = channel.BasicConsume(queue: consumerQueueName, autoAck: false, consumer: consumer);
        _logger.LogInformation("Started consuming queue {QueueName} ({ConsumerQueueName}) for event type {EventType}", queueName, consumerQueueName, typeof(T).Name);

        try
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if (channel.IsOpen)
            {
                channel.BasicCancel(consumerTag);
            }
        }
    }
    #endregion

    #region Helpers
    private static string BuildConsumerQueueName(string queueName, string consumerId)
    {
        return $"{queueName}.{consumerId}";
    }

    private static int ReadRetryCount(IDictionary<string, object>? headers)
    {
        if (headers is null || !headers.TryGetValue(RetryHeader, out var value) || value is null)
        {
            return 0;
        }

        return value switch
        {
            byte[] byteArray when int.TryParse(Encoding.UTF8.GetString(byteArray), out var parsedValue) => parsedValue,
            sbyte v => v,
            byte v => v,
            short v => v,
            ushort v => v,
            int v => v,
            long v => (int)v,
            _ => 0
        };
    }
    #endregion
}


