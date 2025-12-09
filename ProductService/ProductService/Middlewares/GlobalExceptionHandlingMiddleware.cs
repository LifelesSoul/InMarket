using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ProductService.Middlewares;

public class GlobalExceptionHandlingMiddleware(ILogger<GlobalExceptionHandlingMiddleware> logger) : IMiddleware
{
    private record ExceptionResponse(int StatusCode, string Title, string Detail);

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An unhandled error occurred: {Message}", exception.Message);

            await HandleExceptionAsync(context, exception);
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

    private static ExceptionResponse MapExceptionToResponse(Exception exceptionResponse)
    {
        return exceptionResponse switch
        {
            KeyNotFoundException exception => new ExceptionResponse(
                StatusCodes.Status404NotFound,
                "Resource Not Found",
                exception.Message),

            ArgumentException exception => new ExceptionResponse(
                StatusCodes.Status400BadRequest,
                "Invalid Argument",
                exception.Message),

            DbUpdateException => new ExceptionResponse(
                StatusCodes.Status409Conflict,
                "Database Conflict",
                "Unique constraint violation or database error"),

            InvalidOperationException exception => new ExceptionResponse(
                StatusCodes.Status400BadRequest,
                "Operation Failed",
                exception.Message),

            _ => new ExceptionResponse(
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred.")
        };
    }
}
