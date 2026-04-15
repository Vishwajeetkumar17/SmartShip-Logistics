using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SmartShip.EventBus.Configuration;

namespace SmartShip.EventBus.Infrastructure;

/// <summary>
/// Domain model for rabbit mqconnection manager.
/// </summary>
public sealed class RabbitMQConnectionManager : IDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMQConnectionManager> _logger;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private IConnection? _connection;

    /// <summary>
    /// Processes rabbit mqconnection manager.
    /// </summary>
    public RabbitMQConnectionManager(IOptions<RabbitMqOptions> options, ILogger<RabbitMQConnectionManager> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Returns connection async.
    /// </summary>
    public async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (_connection is { IsOpen: true })
        {
            return _connection;
        }

        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_connection is { IsOpen: true })
            {
                return _connection;
            }

            _connection?.Dispose();

            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnection();
            _logger.LogInformation("RabbitMQ connection established to {Host}:{Port}", _options.HostName, _options.Port);

            return _connection;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    /// <summary>
    /// Releases managed resources.
    /// </summary>
    public void Dispose()
    {
        _connection?.Dispose();
        _connectionLock.Dispose();
    }
}


