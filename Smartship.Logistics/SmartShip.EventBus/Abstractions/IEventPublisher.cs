/// <summary>
/// Provides backend implementation for IEventPublisher.
/// </summary>

namespace SmartShip.EventBus.Abstractions;

/// <summary>
/// Represents IEventPublisher.
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<T>(string queueName, T @event, CancellationToken cancellationToken = default)
        where T : class;
}


