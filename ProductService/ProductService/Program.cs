using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProductService.BLL.DI;
using ProductService.BLL.Validators;
using ProductService.Infrastructure;
using ProductService.Mappings;
using ProductService.Middlewares;
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

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddServices();

var domain = $"https://{builder.Configuration["Auth0:Domain"]}/";
var audience = builder.Configuration["Auth0:Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = domain;
    options.Audience = audience;

    options.TokenValidationParameters = new TokenValidationParameters
    {
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
