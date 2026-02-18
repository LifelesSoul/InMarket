using Microsoft.Extensions.Options;
using NotificationService.Application.Configurations;
using RabbitMQ.Client;

namespace NotificationService.Application.Providers;

public interface IRabbitMqConnectionProvider
{
    Task<IConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);

    string MainQueueName { get; }

    string ErrorQueueName { get; }

    int RetryIntervalSeconds { get; }
}

public class RabbitMqConnectionProvider : IRabbitMqConnectionProvider
{
    private readonly RabbitMqSettings _settings;
    private readonly ConnectionFactory _connectionFactory;

    public RabbitMqConnectionProvider(IOptions<RabbitMqSettings> settings)
    {
        _settings = settings.Value;

        _connectionFactory = new ConnectionFactory
        {
            HostName = _settings.Host,
            VirtualHost = _settings.VirtualHost,
            UserName = _settings.Username,
            Password = _settings.Password,
            RequestedConnectionTimeout = TimeSpan.FromSeconds(5)
        };
    }

    public async Task<IConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        return await _connectionFactory.CreateConnectionAsync(cancellationToken);
    }

    public string MainQueueName => _settings.QueueName;
    public string ErrorQueueName => $"{_settings.QueueName}_error";
    public int RetryIntervalSeconds => _settings.RetryIntervalSeconds;
}
