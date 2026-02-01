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

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = connection.CreateModel();
        _channel.ExchangeDeclare("notification-create", ExchangeType.Fanout, durable: true);

        var queueName = "notification.service.queue";
        _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(queueName, "notification-create", "");

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

                    if (notificationEvent != null)
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
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
