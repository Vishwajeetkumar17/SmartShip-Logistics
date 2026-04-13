/// <summary>
/// Provides backend implementation for IEventConsumer.
/// </summary>

namespace SmartShip.EventBus.Abstractions;

/// <summary>
/// Represents IEventConsumer.
/// </summary>
public interface IEventConsumer
{
    Task ConsumeAsync<T>(string queueName, Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken = default)
        where T : class;
}


