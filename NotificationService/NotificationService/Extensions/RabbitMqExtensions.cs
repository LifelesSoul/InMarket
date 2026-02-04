using Microsoft.Extensions.Options;
using NotificationService.API.Configurations;
using NotificationService.Application.BackgroundServices;
using NotificationService.Application.Interfaces;
using RabbitMQ.Client;

namespace NotificationService.API.Extensions;

public static class RabbitMqExtensions
{
    private const string ClientName = "NotificationService Listener";

    public static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));

        services.AddSingleton<IConnection>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value
                ?? throw new InvalidOperationException("RabbitMQ ConnectionString is missing");

            var factory = new ConnectionFactory
            {
                Uri = new Uri(options.ConnectionString),
                ClientProvidedName = ClientName
            };

            return factory.CreateConnection();
        });

        return services;
    }

    public static IServiceCollection AddRabbitMqListener<TEvent, THandler>(
        this IServiceCollection services,
        Func<RabbitMqOptions, QueueSettings> settingsSelector)
        where THandler : class, IIntegrationEventHandler<TEvent>
    {
        services.AddScoped<IIntegrationEventHandler<TEvent>, THandler>();

        services.AddHostedService<GenericRabbitMqListener<TEvent>>(sp =>
        {
            var connection = sp.GetRequiredService<IConnection>();
            var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();

            var options = sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value;

            var queueSettings = settingsSelector(options);

            return new GenericRabbitMqListener<TEvent>(
                connection,
                scopeFactory,
                queueSettings.QueueName,
                queueSettings.ExchangeName
            );
        });

        return services;
    }
}
