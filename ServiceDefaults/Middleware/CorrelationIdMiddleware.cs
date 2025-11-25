using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace ServiceDefaults.Middleware;

/// <summary>
///middleware для генерації та передачі CorrelationId через всі сервіси
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        //чи є CorrelationId у вхідному запиті
        var correlationId = GetOrCreateCorrelationId(context);

        //до LogContext для Serilog автоматично в усі логи
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            //до response headers (для troubleshooting)
            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(CorrelationIdHeader))
                {
                    context.Response.Headers.Append(CorrelationIdHeader, correlationId);
                }
                return Task.CompletedTask;
            });

            _logger.LogInformation("Request started with CorrelationId: {CorrelationId}", correlationId);

            await _next(context);

            _logger.LogInformation("Request completed with CorrelationId: {CorrelationId}", correlationId);
        }
    }

    private string GetOrCreateCorrelationId(HttpContext context)
    {
        //надіслав CorrelationId - використовуємо його
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var existingId) 
            && !string.IsNullOrWhiteSpace(existingId))
        {
            return existingId.ToString();
        }

        //інакше генеруємо новий
        var newId = Guid.NewGuid().ToString();
        context.Request.Headers.Append(CorrelationIdHeader, newId);
        
        _logger.LogDebug("Generated new CorrelationId: {CorrelationId}", newId);
        
        return newId;
    }
}

/// <summary>
/// DelegatingHandler для автоматичної передачі CorrelationId у вихідний requests
/// </summary>
public class CorrelationIdDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public CorrelationIdDelegatingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        
        if (httpContext != null)
        {
            //CorrelationId з поточного request
            if (httpContext.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId))
            {
                //і до outgoing request
                request.Headers.TryAddWithoutValidation(CorrelationIdHeader, correlationId.ToString());
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}