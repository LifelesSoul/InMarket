using RabbitMQ.Client;

namespace NotificationService.API.Extensions;

public static class RabbitMqExtensions
{
    private const string ConnectionStringName = "RabbitMq";

    private const string ClientName = "NotificationService Listener";

    public static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IConnection>(sp =>
        {
            var connectionString = configuration.GetConnectionString(ConnectionStringName)
                ?? throw new InvalidOperationException($"Connection string '{ConnectionStringName}' not found.");

            var factory = new ConnectionFactory
            {
                Uri = new Uri(connectionString),
                ClientProvidedName = ClientName
            };

            return factory.CreateConnection();
        });

        return services;
    }
}
