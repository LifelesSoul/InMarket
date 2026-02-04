using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application.BackgroundServices;
using NotificationService.Application.Interfaces;
using RabbitMQ.Client;

namespace NotificationService.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMqListener<TEvent, THandler>(
        this IServiceCollection services,
        string queueName,
        string exchangeName)
        where THandler : class, IIntegrationEventHandler<TEvent>
    {
        services.AddScoped<IIntegrationEventHandler<TEvent>, THandler>();

        services.AddHostedService<GenericRabbitMqListener<TEvent>>(sp =>
        {
            var connection = sp.GetRequiredService<IConnection>();
            var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
            return new GenericRabbitMqListener<TEvent>(connection, scopeFactory, queueName, exchangeName);
        });

        return services;
    }
}
