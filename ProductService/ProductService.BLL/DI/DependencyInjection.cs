using Microsoft.Extensions.DependencyInjection;
using ProductService.BLL.Mappings;
using ProductService.BLL.Services;
using ProductService.DAL.DI;
using System.Diagnostics.CodeAnalysis;

namespace ProductService.BLL.DI;

[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));

        services.AddScoped<IProductService, ProductsService>()
                .AddScoped<ICategoryService, CategoryService>()
                .AddScoped<IUserService, UsersService>();

        services.AddRepositories();

        services.AddScoped<IEventPublisher, EventPublisher>();

        return services;
    }
}
