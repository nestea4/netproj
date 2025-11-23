using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ReviewService.Domain.Exceptions;

namespace ReviewService.WebAPI.Middleware;

/// <summary>
///глобальна обробка винятків
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred");
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, problemDetails) = MapExceptionToProblemDetails(exception, context);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private (HttpStatusCode statusCode, ProblemDetails problemDetails) MapExceptionToProblemDetails(
        Exception exception,
        HttpContext context)
    {
        return exception switch
        {
            //Domain Exceptions
            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                new ProblemDetails
                {
                    Title = "Resource Not Found",
                    Status = (int)HttpStatusCode.NotFound,
                    Detail = notFoundEx.Message,
                    Instance = context.Request.Path
                }),

            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                new ValidationProblemDetails(validationEx.Errors)
                {
                    Title = "Validation Error",
                    Status = (int)HttpStatusCode.BadRequest,
                    Detail = "One or more validation errors occurred",
                    Instance = context.Request.Path
                }),

            ConflictException conflictEx => (
                HttpStatusCode.Conflict,
                new ProblemDetails
                {
                    Title = "Conflict",
                    Status = (int)HttpStatusCode.Conflict,
                    Detail = conflictEx.Message,
                    Instance = context.Request.Path
                }),

            BusinessRuleException businessEx => (
                HttpStatusCode.UnprocessableEntity,
                new ProblemDetails
                {
                    Title = "Business Rule Violation",
                    Status = (int)HttpStatusCode.UnprocessableEntity,
                    Detail = businessEx.Message,
                    Instance = context.Request.Path
                }),

            DomainException domainEx => (
                HttpStatusCode.BadRequest,
                new ProblemDetails
                {
                    Title = "Domain Error",
                    Status = (int)HttpStatusCode.BadRequest,
                    Detail = domainEx.Message,
                    Instance = context.Request.Path
                }),

            //MongoDB уxceptions
            MongoConnectionException mongoConnEx => (
                HttpStatusCode.ServiceUnavailable,
                new ProblemDetails
                {
                    Title = "Database Connection Error",
                    Status = (int)HttpStatusCode.ServiceUnavailable,
                    Detail = "Unable to connect to the database. Please try again later.",
                    Instance = context.Request.Path
                }),

            MongoWriteException mongoWriteEx when IsDuplicateKeyError(mongoWriteEx) => (
                HttpStatusCode.Conflict,
                new ProblemDetails
                {
                    Title = "Duplicate Entry",
                    Status = (int)HttpStatusCode.Conflict,
                    Detail = "A record with this data already exists",
                    Instance = context.Request.Path
                }),

            MongoWriteException mongoWriteEx => (
                HttpStatusCode.BadRequest,
                new ProblemDetails
                {
                    Title = "Database Write Error",
                    Status = (int)HttpStatusCode.BadRequest,
                    Detail = "An error occurred while writing to the database",
                    Instance = context.Request.Path
                }),

            MongoException mongoEx => (
                HttpStatusCode.InternalServerError,
                new ProblemDetails
                {
                    Title = "Database Error",
                    Status = (int)HttpStatusCode.InternalServerError,
                    Detail = "An unexpected database error occurred",
                    Instance = context.Request.Path
                }),

            //Generic Exception
            _ => (
                HttpStatusCode.InternalServerError,
                new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Status = (int)HttpStatusCode.InternalServerError,
                    Detail = "An unexpected error occurred. Please contact support if the problem persists.",
                    Instance = context.Request.Path
                })
        };
    }

    private static bool IsDuplicateKeyError(MongoWriteException exception)
    {
        return exception.WriteError?.Code == 11000; // Duplicate key error code
    }
}

/// <summary>
///еxtension для додавання middleware в pipeline
/// </summary>
public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionMiddleware>();
    }
}