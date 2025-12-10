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

    private static ExceptionResponse MapExceptionToResponse(Exception exception)
    {
        return exception switch
        {
            KeyNotFoundException ex => new ExceptionResponse(
                StatusCodes.Status404NotFound,
                ErrorMessages.Titles.NotFound,
                ex.Message),

            ArgumentException ex => new ExceptionResponse(
                StatusCodes.Status400BadRequest,
                ErrorMessages.Titles.InvalidArgument,
                ex.Message),

            DbUpdateException => new ExceptionResponse(
                StatusCodes.Status409Conflict,
                ErrorMessages.Titles.Conflict,
                ErrorMessages.Details.DbConflict),

            InvalidOperationException ex => new ExceptionResponse(
                StatusCodes.Status400BadRequest,
                ErrorMessages.Titles.OperationFailed,
                ex.Message),

            _ => new ExceptionResponse(
                StatusCodes.Status500InternalServerError,
                ErrorMessages.Titles.InternalServer,
                ErrorMessages.Details.InternalServer)
        };
    }

    private static class ErrorMessages
    {
        public static class Titles
        {
            public const string NotFound = "Resource Not Found";
            public const string InvalidArgument = "Invalid Argument";
            public const string Conflict = "Database Conflict";
            public const string OperationFailed = "Operation Failed";
            public const string InternalServer = "Internal Server Error";
        }

        public static class Details
        {
            public const string DbConflict = "Unique constraint violation or database error";
            public const string InternalServer = "An unexpected error occurred.";
        }
    }
}
