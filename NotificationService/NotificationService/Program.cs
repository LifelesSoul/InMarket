using Microsoft.AspNetCore.Authorization;
using NotificationService.API.Extensions;
using NotificationService.Applcaition.Hubs;
using NotificationService.Application;
using NotificationService.Application.Configurations;
using NotificationService.Authorization;
using NotificationService.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerInfrastructure();
builder.Services.AddSingleton<GlobalExceptionHandlingMiddleware>();
builder.Services.AddServices(builder.Configuration);

builder.Services.AddSignalR();

var allowedOrigins = builder.Configuration
    .GetRequiredSection("Cors:AllowedOrigins")
    .Get<string[]>();

if (allowedOrigins is null or { Length: 0 })
{
    throw new InvalidOperationException("CORS allowed origins are missing or empty in the configuration");
}

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddSingleton<IAuthorizationHandler, NotificationAuthorizationHandler>();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("MustBeOwner", policy =>
        policy.Requirements.Add(new SameAuthorRequirement()));

builder.Services.AddRabbitMqInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<NotificationHub>("/hubs/notifications");

await app.RunAsync();
