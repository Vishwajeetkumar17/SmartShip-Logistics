/// <summary>
/// Provides backend implementation for RabbitMQPublisher.
/// </summary>

using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SmartShip.EventBus.Abstractions;
using SmartShip.EventBus.Configuration;

namespace SmartShip.EventBus.Infrastructure;

/// <summary>
/// Represents RabbitMQPublisher.
/// </summary>
public sealed class RabbitMQPublisher : IEventPublisher
{
    private readonly RabbitMQConnectionManager _connectionManager;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMQPublisher> _logger;
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    public RabbitMQPublisher(
        RabbitMQConnectionManager connectionManager,
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMQPublisher> logger)
    {
        _connectionManager = connectionManager;
        _options = options.Value;
        _logger = logger;
    }

    public async Task PublishAsync<T>(string queueName, T @event, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName);
        ArgumentNullException.ThrowIfNull(@event);

        var payload = JsonSerializer.SerializeToUtf8Bytes(@event, _serializerOptions);
        var maxAttempts = Math.Max(1, _options.PublishMaxRetryAttempts);

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var connection = await _connectionManager.GetConnectionAsync(cancellationToken);
                using var channel = connection.CreateModel();

                channel.ExchangeDeclare(exchange: queueName, type: ExchangeType.Fanout, durable: true, autoDelete: false, arguments: null);

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.ContentType = "application/json";
                properties.Type = typeof(T).Name;
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.Now.ToUnixTimeSeconds());

                channel.BasicPublish(exchange: queueName, routingKey: string.Empty, basicProperties: properties, body: payload);

                _logger.LogInformation("Published {EventType} event to topic {QueueName}", typeof(T).Name, queueName);
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                var delay = TimeSpan.FromSeconds(_options.BaseRetryDelaySeconds * Math.Pow(2, attempt - 1));
                _logger.LogWarning(ex, "Failed to publish {EventType} to {QueueName}. Retrying attempt {Attempt}/{MaxAttempts}", typeof(T).Name, queueName, attempt, maxAttempts);
                await Task.Delay(delay, cancellationToken);
            }
        }

        throw new InvalidOperationException($"Failed to publish event {typeof(T).Name} to queue '{queueName}' after {maxAttempts} attempts.");
    }
}


