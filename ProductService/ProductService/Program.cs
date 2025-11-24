using Microsoft.EntityFrameworkCore;
using ProductService.DAL.Shared;
using ProductService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ProductDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

var app = builder.Build();

var currentTime = SystemDateTimeProvider.UtcNow;

Console.WriteLine($"Current UTC Time: {currentTime}");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
