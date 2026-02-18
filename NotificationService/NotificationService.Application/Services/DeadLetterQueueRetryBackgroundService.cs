using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Providers;
using RabbitMQ.Client;

namespace NotificationService.Application.Services;

public class DeadLetterQueueRetryBackgroundService : BackgroundService
{
    private readonly IRabbitMqConnectionProvider _connectionProvider;
    private readonly ILogger<DeadLetterQueueRetryBackgroundService> _logger;

    public DeadLetterQueueRetryBackgroundService(
        IRabbitMqConnectionProvider connectionProvider,
        ILogger<DeadLetterQueueRetryBackgroundService> logger)
    {
        _connectionProvider = connectionProvider;
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

            await Task.Delay(TimeSpan.FromSeconds(_connectionProvider.RetryIntervalSeconds), stoppingToken);
        }
    }

    private async Task RetryFailedMessagesAsync(CancellationToken cancellationToken)
    {
        using var connection = await _connectionProvider.CreateConnectionAsync(cancellationToken);
        using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        var errorQueue = _connectionProvider.ErrorQueueName;
        var mainQueue = _connectionProvider.MainQueueName;

        _logger.LogInformation("Checking DLQ. Error Queue: '{ErrorQueue}', Target Queue: '{MainQueue}'", errorQueue, mainQueue);

        try
        {
            await channel.QueueDeclarePassiveAsync(errorQueue, cancellationToken);
        }
        catch (RabbitMQ.Client.Exceptions.OperationInterruptedException ex)
        {
            _logger.LogWarning("Error queue '{ErrorQueue}' not found. Reason: {Reason}", errorQueue, ex.Message);
            return;
        }

        var queueInfo = await channel.QueueDeclarePassiveAsync(errorQueue, cancellationToken);
        if (queueInfo.MessageCount == 0)
        {
            _logger.LogInformation("DLQ '{ErrorQueue}' is empty.", errorQueue);
            return;
        }

        _logger.LogInformation("Found {Count} messages inside DLQ.", queueInfo.MessageCount);

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
            _logger.LogInformation("Restored {Count} messages from {ErrorQueue} to {MainQueue}", count, errorQueue, mainQueue);
        }
    }
}
