using Microsoft.Extensions.DependencyInjection;
using ProductService.BLL.Interfaces;
using ProductService.BLL.Services;
using ProductService.DAL.DI;

namespace ProductService.BLL.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddBllServices(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductsService>();

        services.AddDalServices();

        return services;
    }
}
