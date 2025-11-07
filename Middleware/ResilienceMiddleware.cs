using Polly;
using Polly.Retry;

namespace MinimalApiApp.Middleware;

public class ResilienceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ResilienceMiddleware> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public ResilienceMiddleware(RequestDelegate next, ILogger<ResilienceMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _retryPolicy = Policy
            .Handle<Exception>(ex => ex is not NotFoundException && ex is not ArgumentException)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "Request failed. Retry attempt {RetryCount} after {DelayMs}ms. Error: {ErrorMessage}",
                        retryCount,
                        timeSpan.TotalMilliseconds,
                        exception.Message);
                });
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _retryPolicy.ExecuteAsync(async () => await _next(context));
    }
}
