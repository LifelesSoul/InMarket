using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Infrastructure.Interfaces;
using NotificationService.Infrastructure.Repositories;
using NotificationService.Infrastructure.Settings;

namespace NotificationService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoDbSettings>(configuration.GetSection("MongoSettings"));

        services.AddScoped<INotificationRepository, NotificationRepository>();

        return services;
    }
}
