using Serilog;

namespace MinimalApiApp.Middleware;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = Guid.NewGuid().ToString();
        context.Items["RequestId"] = requestId;

        _logger.LogInformation(
            "Request {RequestId}: {Method} {Path} started at {Time}",
            requestId,
            context.Request.Method,
            context.Request.Path,
            DateTime.UtcNow);

        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            await _next(context);
            sw.Stop();

            _logger.LogInformation(
                "Request {RequestId}: {Method} {Path} completed with {StatusCode} in {ElapsedMs}ms",
                requestId,
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();

            _logger.LogError(ex,
                "Request {RequestId}: {Method} {Path} failed after {ElapsedMs}ms",
                requestId,
                context.Request.Method,
                context.Request.Path,
                sw.ElapsedMilliseconds);

            throw;
        }
    }
}
