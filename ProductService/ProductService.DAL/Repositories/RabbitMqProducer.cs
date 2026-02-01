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

    private bool _disposed;

    public RabbitMqProducer(IConnection connection)
    {
        _channel = connection.CreateModel();
        _channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Fanout, durable: true);
    }

    public void SendMessage<T>(T message)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(RabbitMqProducer));
        }

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
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _channel?.Close();
            _channel?.Dispose();
        }

        _disposed = true;
    }
}
