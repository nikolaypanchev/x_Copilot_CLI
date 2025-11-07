using Polly;
using Polly.Retry;

namespace MinimalApiApp.Middleware;

public static class ResiliencePolicies
{
    public static AsyncRetryPolicy CreateRetryPolicy()
    {
        return Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    Console.WriteLine($"Retry {retryCount} after {timeSpan.TotalSeconds}s due to: {exception.Message}");
                });
    }

    public static AsyncRetryPolicy<T> CreateRetryPolicy<T>()
    {
        return Policy<T>
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timeSpan, retryCount, context) =>
                {
                    Console.WriteLine($"Retry {retryCount} after {timeSpan.TotalSeconds}s due to: {outcome.Exception?.Message}");
                });
    }
}
