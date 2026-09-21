using ProductService.API.Extensions;
using ProductService.BLL.DI;
using ProductService.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiLayer(builder.Configuration);
builder.Services.AddInfrastructureLayer(builder.Configuration);
builder.Services.AddSecurityLayer(builder.Configuration);

builder.Services.AddServices();
builder.Services.AddRabbitMqInfrastructure(builder.Configuration);

builder.Services.AddSingleton<GlobalExceptionHandlingMiddleware>();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.ConfigurePipeline();

await app.ApplyDatabaseMigrationsAsync();

await app.RunAsync();
