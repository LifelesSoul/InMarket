using ProductService.DAL.Interfaces;
using RabbitMQ.Client;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace ProductService.DAL.Repositories;

[ExcludeFromCodeCoverage]
public class RabbitMqProducer : IMessageProducer, IDisposable
{
    private readonly IModel _channel;

    private const string ExchangeName = "notification-create";

    public RabbitMqProducer(IConnection connection)
    {
        _channel = connection.CreateModel();

        _channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Fanout, durable: true);
    }

    public void SendMessage<T>(T message)
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);
        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;

        lock (_channel)
        {
            _channel.BasicPublish(ExchangeName, string.Empty, properties, body);
        }
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
    }
}
