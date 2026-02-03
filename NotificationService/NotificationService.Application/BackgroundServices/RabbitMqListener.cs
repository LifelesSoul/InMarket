using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NotificationService.Application.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace NotificationService.Application.BackgroundServices;

public class GenericRabbitMqListener<TEvent> : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly string _queueName;
    private readonly string _exchangeName;
    private IModel? _channel;

    public GenericRabbitMqListener(
        IConnection connection,
        IServiceScopeFactory scopeFactory,
        string queueName,
        string exchangeName)
    {
        _connection = connection;
        _scopeFactory = scopeFactory;
        _queueName = queueName;
        _exchangeName = exchangeName;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(_exchangeName, ExchangeType.Fanout, durable: true);
        _channel.QueueDeclare(_queueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(_queueName, _exchangeName, string.Empty);

        _channel.BasicQos(0, 1, false);

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (sender, args) =>
        {
            var body = args.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var handler = scope.ServiceProvider.GetRequiredService<IIntegrationEventHandler<TEvent>>();

                    var eventData = JsonSerializer.Deserialize<TEvent>(message);

                    if (eventData is not null)
                    {
                        await handler.Handle(eventData);

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
                _channel?.BasicNack(args.DeliveryTag, false, requeue: false);
            }
        };

        _channel.BasicConsume(_queueName, false, consumer);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        base.Dispose();
    }
}
