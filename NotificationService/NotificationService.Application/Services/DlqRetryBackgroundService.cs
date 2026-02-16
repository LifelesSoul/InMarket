using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationService.Application.Configurations;
using RabbitMQ.Client;

namespace NotificationService.Application.Services;

public class DlqRetryBackgroundService : BackgroundService
{
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<DlqRetryBackgroundService> _logger;

    public DlqRetryBackgroundService(
        IOptions<RabbitMqSettings> settings,
        ILogger<DlqRetryBackgroundService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("The DLQ background check service has started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RetryFailedMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while trying to check DLQ queue.");
            }

            await Task.Delay(TimeSpan.FromSeconds(_settings.RetryIntervalSeconds), stoppingToken);
        }
    }

    private async Task RetryFailedMessagesAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.Host,
            VirtualHost = _settings.VirtualHost,
            UserName = _settings.Username,
            Password = _settings.Password,
            RequestedConnectionTimeout = TimeSpan.FromSeconds(5)
        };

        using var connection = await factory.CreateConnectionAsync(cancellationToken);
        using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        var errorQueue = $"{_settings.QueueName}_error";
        var mainQueue = _settings.QueueName;

        _logger.LogInformation("Checking DLQ. Error Queue: '{ErrorQueue}', Target Queue: '{MainQueue}'", errorQueue, mainQueue);

        try
        {
            await channel.QueueDeclarePassiveAsync(errorQueue, cancellationToken);
        }
        catch (RabbitMQ.Client.Exceptions.OperationInterruptedException ex)
        {
            _logger.LogWarning("Error queue '{ErrorQueue}' not found or not accessible. Reason: {Reason}", errorQueue, ex.Message);
            return;
        }

        var queueInfo = await channel.QueueDeclarePassiveAsync(errorQueue, cancellationToken);
        if (queueInfo.MessageCount == 0)
        {
            _logger.LogInformation("DLQ '{ErrorQueue}' is empty. Nothing to restore.", errorQueue);
            return;
        }

        _logger.LogInformation("Found {Count} messages in DLQ. Starting restoration...", queueInfo.MessageCount);

        int count = 0;

        while (true)
        {
            var result = await channel.BasicGetAsync(errorQueue, autoAck: false, cancellationToken);

            if (result == null) break;

            var properties = new BasicProperties(result.BasicProperties);

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: mainQueue,
                mandatory: true,
                basicProperties: properties,
                body: result.Body,
                cancellationToken: cancellationToken
            );

            await channel.BasicAckAsync(result.DeliveryTag, multiple: false, cancellationToken);
            count++;
        }

        if (count > 0)
        {
            _logger.LogInformation("Successfully restored {Count} messages from {ErrorQueue} to {MainQueue}", count, errorQueue, mainQueue);
        }
    }
}
