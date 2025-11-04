namespace CatalogService.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.TraceIdentifier;
        
        using (_logger.BeginScope(new Dictionary<string, object>
               {
                   ["CorrelationId"] = correlationId,
                   ["RequestPath"] = context.Request.Path,
                   ["RequestMethod"] = context.Request.Method
               }))
        {
            _logger.LogInformation(
                "HTTP {Method} {Path} started",
                context.Request.Method,
                context.Request.Path);

            var sw = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                await _next(context);
                sw.Stop();

                _logger.LogInformation(
                    "HTTP {Method} {Path} completed with status {StatusCode} in {ElapsedMs}ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                sw.Stop();
                
                _logger.LogError(ex,
                    "HTTP {Method} {Path} failed after {ElapsedMs}ms",
                    context.Request.Method,
                    context.Request.Path,
                    sw.ElapsedMilliseconds);

                throw;
            }
        }
    }
}