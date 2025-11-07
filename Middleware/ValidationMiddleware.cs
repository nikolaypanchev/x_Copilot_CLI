using System.Text.Json;
using FluentValidation;
using MinimalApiApp.Models;

namespace MinimalApiApp.Middleware;

public class ValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ValidationMiddleware> _logger;

    public ValidationMiddleware(RequestDelegate next, ILogger<ValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
    {
        if (context.Request.Method == HttpMethods.Post || context.Request.Method == HttpMethods.Put)
        {
            if (context.Request.Path.StartsWithSegments("/api/products"))
            {
                await ValidateRequest<Product>(context, serviceProvider);
                if (context.Response.HasStarted) return;
            }
            else if (context.Request.Path.StartsWithSegments("/api/users"))
            {
                await ValidateRequest<User>(context, serviceProvider);
                if (context.Response.HasStarted) return;
            }
        }

        await _next(context);
    }

    private async Task ValidateRequest<T>(HttpContext context, IServiceProvider serviceProvider) where T : class
    {
        context.Request.EnableBuffering();
        
        try
        {
            var entity = await JsonSerializer.DeserializeAsync<T>(
                context.Request.Body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (entity != null)
            {
                var validator = serviceProvider.GetRequiredService<IValidator<T>>();
                var validationResult = await validator.ValidateAsync(entity);

                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("{Type} validation failed: {Errors}", 
                        typeof(T).Name,
                        string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    context.Response.ContentType = "application/json";

                    var errors = validationResult.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()
                        );

                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Validation failed",
                        statusCode = 400,
                        errors = errors
                    });
                    return;
                }
            }
            
            context.Request.Body.Position = 0;
        }
        catch (JsonException)
        {
            // Let the endpoint handle deserialization errors
            context.Request.Body.Position = 0;
        }
    }
}
