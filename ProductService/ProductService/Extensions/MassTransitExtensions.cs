using MassTransit;
using Microsoft.Extensions.Options;
using ProductService.Configurations;

namespace ProductService.API.Extensions;

public static class MassTransitExtensions
{
    public static IServiceCollection AddRabbitMqInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqSettings>(configuration.GetSection(RabbitMqSettings.SectionName));

        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                var settings = context.GetRequiredService<IOptions<RabbitMqSettings>>().Value;

                cfg.Host(settings.Host, settings.VirtualHost, h =>
                {
                    h.Username(settings.Username);
                    h.Password(settings.Password);
                });
            });
        });

        return services;
    }
}
