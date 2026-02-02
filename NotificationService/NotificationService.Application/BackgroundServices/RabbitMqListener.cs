using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Models.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace NotificationService.Application.BackgroundServices;

public class RabbitMqListener(IConnection connection, IServiceScopeFactory scopeFactory) : BackgroundService
{
    private IModel? _channel;

    private const string ExchangeName = "notification-create";
    private const string QueueName = "notification.service.queue";

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = connection.CreateModel();
        _channel.ExchangeDeclare(ExchangeName, ExchangeType.Fanout, durable: true);

        var queueName = QueueName;
        _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(queueName, ExchangeName, string.Empty);

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (sender, args) =>
        {
            var body = args.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var service = scope.ServiceProvider.GetRequiredService<INotificationService>();
                    var notificationEvent = JsonSerializer.Deserialize<CreateNotificationEvent>(message);

                    if (notificationEvent is not null)
                    {
                        await service.HandleProductCreated(notificationEvent, stoppingToken);
                        _channel?.BasicAck(args.DeliveryTag, false);
                    }
                    else
                    {
                        _channel?.BasicAck(args.DeliveryTag, false);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        };

        _channel.BasicConsume(queueName, false, consumer);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        base.Dispose();
    }
}
