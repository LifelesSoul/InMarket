using Hangfire;
using Microsoft.EntityFrameworkCore;
using ProductService.Infrastructure;
using Scalar.AspNetCore;
using System.Diagnostics.CodeAnalysis;

namespace ProductService.API.Extensions;

[ExcludeFromCodeCoverage]
public static class WebApplicationExtensions
{
    public static async Task ApplyDatabaseMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<ProductDbContext>();

            if (!await context.Database.CanConnectAsync())
            {
                await context.Database.MigrateAsync();
            }

            if (app.Environment.IsDevelopment())
            {
                context.SeedData();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database migration failed: {ex.Message}");
        }
    }

    public static void ConfigurePipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
                options
                    .WithTitle("Product Service API")
                    .WithTheme(ScalarTheme.Moon)
                    .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
            });
        }

        app.UseHttpsRedirection();
        app.UseCors();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.UseHangfireDashboard();
    }
}
