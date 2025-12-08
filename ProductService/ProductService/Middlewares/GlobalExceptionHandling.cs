using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ProductService.Middlewares;

public class GlobalExceptionHandling(ILogger<GlobalExceptionHandling> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);

            if (context.Response.StatusCode == StatusCodes.Status404NotFound)
            {
                if (!context.Response.HasStarted)
                {
                    await HandleExceptionAsync(context, new KeyNotFoundException("The requested resource was not found."));
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled error occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, title, detail) = MapExceptionToResponse(exception);

        context.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        var json = JsonSerializer.Serialize(problemDetails);
        await context.Response.WriteAsync(json);
    }

    private static (int StatusCode, string Title, string Detail) MapExceptionToResponse(Exception exception)
    {
        return exception switch
        {
            KeyNotFoundException ex => (StatusCodes.Status404NotFound, "Resource Not Found", ex.Message),
            ArgumentException ex => (StatusCodes.Status400BadRequest, "Invalid Argument", ex.Message),
            DbUpdateException dbEx => (StatusCodes.Status409Conflict, "Database Conflict", "Unique constraint violation or database error"),
            InvalidOperationException ex => (StatusCodes.Status400BadRequest, "Operation Failed", ex.Message),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred.")
        };
    }
}
