using NotificationService.API.Extensions;
using NotificationService.Application;
using NotificationService.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSwaggerInfrastructure();

builder.Services.AddSingleton<GlobalExceptionHandlingMiddleware>();

builder.Services.AddServices(builder.Configuration);

builder.Services.AddAuth0Authentication(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
