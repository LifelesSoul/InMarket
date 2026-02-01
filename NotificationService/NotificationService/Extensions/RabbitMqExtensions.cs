using RabbitMQ.Client;

namespace NotificationService.API.Extensions;

public static class RabbitMqExtensions
{
    public static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IConnection>(sp =>
        {
            var connectionString = configuration.GetConnectionString("RabbitMq");

            var factory = new ConnectionFactory
            {
                Uri = new Uri(connectionString),
                ClientProvidedName = "NotificationService Listener"
            };

            return factory.CreateConnection();
        });

        return services;
    }
}
