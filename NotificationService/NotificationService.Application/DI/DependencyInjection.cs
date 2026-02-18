using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Mappings;
using NotificationService.Application.Providers;
using NotificationService.Application.Services;
using NotificationService.Infrastructure;
using AppNotificationService = NotificationService.Application.Services.NotificationService;

namespace NotificationService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<INotificationService, AppNotificationService>();

        services.AddSingleton<IRabbitMqConnectionProvider, RabbitMqConnectionProvider>();

        services.AddHostedService<DeadLetterQueueRetryBackgroundService>();

        services.AddRepositories(configuration);
        services.AddAutoMapper(typeof(MappingProfile));

        return services;
    }
}
