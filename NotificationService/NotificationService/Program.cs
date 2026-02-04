using Microsoft.AspNetCore.Authorization;
using NotificationService.API.Extensions;
using NotificationService.Application;
using NotificationService.Application.EventHandlers;
using NotificationService.Application.Models.Events;
using NotificationService.Authorization;
using NotificationService.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerInfrastructure();
builder.Services.AddSingleton<GlobalExceptionHandlingMiddleware>();

builder.Services.AddServices(builder.Configuration);

builder.Services.AddAuth0Authentication(builder.Configuration);

builder.Services.AddRabbitMq(builder.Configuration);

builder.Services.AddRabbitMqListener<CreateNotificationEvent, ProductCreatedEventHandler>(
    options => options.ProductCreated
);

builder.Services.AddSingleton<IAuthorizationHandler, NotificationAuthorizationHandler>();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("MustBeOwner", policy =>
        policy.Requirements.Add(new SameAuthorRequirement()));

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
