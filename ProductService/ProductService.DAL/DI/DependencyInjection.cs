using Microsoft.Extensions.DependencyInjection;
using ProductService.DAL.Repositories;

namespace ProductService.DAL.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
