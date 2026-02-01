using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProductService.API.Configurations;
using ProductService.BLL.DI;
using ProductService.BLL.Validators;
using ProductService.Infrastructure;
using ProductService.Mappings;
using ProductService.Middlewares;
using RabbitMQ.Client;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddSingleton<GlobalExceptionHandlingMiddleware>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateCategoryModelValidator>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ProductDbContext>(options =>
{
    options.UseLazyLoadingProxies();

    options.UseSqlServer(connectionString);
});

builder.Services.AddSingleton<IConnection>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("RabbitMq");

    var factory = new ConnectionFactory
    {
        Uri = new Uri(connectionString),
        ClientProvidedName = "ProductService API"
    };

    return factory.CreateConnection();
});

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddServices();

builder.Services.Configure<WebhookSettings>(builder.Configuration.GetSection("Webhooks"));

builder.Services.Configure<Auth0Settings>(builder.Configuration.GetSection("Auth0"));
var auth0Settings = builder.Configuration.GetSection("Auth0").Get<Auth0Settings>()
    ?? throw new InvalidOperationException("Auth0 settings are missing in Configuration!");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://{auth0Settings.Domain}/";
        options.Audience = auth0Settings.Audience;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            RoleClaimType = "https://inmarket-api/roles"
        };
    });

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

using (var scope = app.Services.CreateScope())
{
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
        Console.WriteLine($"{ex.Message}");
    }
}

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
