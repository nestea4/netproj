using System.Net;
using System.Text.Json;
using CatalogService.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        var (statusCode, problemDetails) = exception switch
        {
            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                CreateProblemDetails(
                    context,
                    (int)HttpStatusCode.NotFound,
                    "Not Found",
                    notFoundEx.Message
                )
            ),
            BusinessException businessEx => (
                HttpStatusCode.BadRequest,
                CreateProblemDetails(
                    context,
                    (int)HttpStatusCode.BadRequest,
                    "Business Rule Violation",
                    businessEx.Message
                )
            ),
            ConflictException conflictEx => (
                HttpStatusCode.Conflict,
                CreateProblemDetails(
                    context,
                    (int)HttpStatusCode.Conflict,
                    "Conflict",
                    conflictEx.Message
                )
            ),
            FluentValidation.ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                CreateValidationProblemDetails(
                    context,
                    validationEx
                )
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                CreateProblemDetails(
                    context,
                    (int)HttpStatusCode.InternalServerError,
                    "Internal Server Error",
                    _environment.IsDevelopment() 
                        ? exception.Message 
                        : "An error occurred while processing your request"
                )
            )
        };

        context.Response.StatusCode = (int)statusCode;

        // Додаємо stack trace тільки в Development
        if (_environment.IsDevelopment() && exception is not NotFoundException)
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            problemDetails.Extensions["innerException"] = exception.InnerException?.Message;
        }

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private ProblemDetails CreateProblemDetails(
        HttpContext context,
        int status,
        string title,
        string detail)
    {
        return new ProblemDetails
        {
            Type = $"https://httpstatuses.io/{status}",
            Title = title,
            Status = status,
            Detail = detail,
            Instance = context.Request.Path,
            Extensions =
            {
                ["traceId"] = context.TraceIdentifier,
                ["timestamp"] = DateTime.UtcNow
            }
        };
    }

    private ValidationProblemDetails CreateValidationProblemDetails(
        HttpContext context,
        FluentValidation.ValidationException validationException)
    {
        var errors = validationException.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        return new ValidationProblemDetails(errors)
        {
            Type = "https://httpstatuses.io/400",
            Title = "Validation Error",
            Status = (int)HttpStatusCode.BadRequest,
            Detail = "One or more validation errors occurred",
            Instance = context.Request.Path,
            Extensions =
            {
                ["traceId"] = context.TraceIdentifier,
                ["timestamp"] = DateTime.UtcNow
            }
        };
    }
}