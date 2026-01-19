using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application.Interfaces;
using NotificationService.Infrastructure;

using AppNotificationService = NotificationService.Application.Services.NotificationService;

namespace NotificationService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<INotificationService, AppNotificationService>();

        services.AddRepositories();

        return services;
    }
}
