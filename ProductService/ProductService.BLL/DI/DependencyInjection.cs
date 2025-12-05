using Microsoft.Extensions.DependencyInjection;
using ProductService.BLL.Mappings;
using ProductService.BLL.Services;
using ProductService.DAL.DI;

namespace ProductService.BLL.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(BllMappingProfile));

        services.AddScoped<IProductService, ProductsService>();

        services.AddRepositories();

        return services;
    }
}
