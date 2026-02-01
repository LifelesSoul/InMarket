using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application.BackgroundServices;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Mappings;
using NotificationService.Infrastructure;

using AppNotificationService = NotificationService.Application.Services.NotificationService;

namespace NotificationService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<INotificationService, AppNotificationService>();

        services.AddRepositories(configuration);

        services.AddAutoMapper(typeof(MappingProfile));

        services.AddHostedService<RabbitMqListener>();

        return services;
    }
}
