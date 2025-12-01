using Microsoft.Extensions.DependencyInjection;
using ProductService.DAL.Interfaces;
using ProductService.DAL.Repositories;

namespace ProductService.DAL.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IProductRepository, ProductRepository>();

        return services;
    }
}
