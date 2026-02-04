using MassTransit;
using Microsoft.Extensions.Options;
using NotificationService.Application.Services;
using NotificationService.Configurations;

namespace NotificationService.API.Extensions;

public static class MassTransitExtensions
{
    public static IServiceCollection AddRabbitMqInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqSettings>(configuration.GetSection(RabbitMqSettings.SectionName));

        services.AddMassTransit(x =>
        {
            x.AddConsumer<NotificationCreateConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var settings = context.GetRequiredService<IOptions<RabbitMqSettings>>().Value;

                cfg.Host(settings.Host, settings.VirtualHost, h =>
                {
                    h.Username(settings.Username);
                    h.Password(settings.Password);
                });

                cfg.ReceiveEndpoint(settings.QueueName, e =>
                {
                    e.ConfigureConsumer<NotificationCreateConsumer>(context);

                    e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                });
            });
        });

        return services;
    }
}
