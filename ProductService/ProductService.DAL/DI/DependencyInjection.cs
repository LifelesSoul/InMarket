using Microsoft.Extensions.DependencyInjection;
using ProductService.DAL.Repositories;
using System.Diagnostics.CodeAnalysis;

namespace ProductService.DAL.DI;

[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IProductRepository, ProductRepository>()
                .AddScoped<ICategoryRepository, CategoryRepository>()
                .AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}