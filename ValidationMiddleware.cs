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
                context.Request.EnableBuffering();
                
                try
                {
                    var product = await JsonSerializer.DeserializeAsync<Product>(
                        context.Request.Body,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    if (product != null)
                    {
                        var validator = serviceProvider.GetRequiredService<IValidator<Product>>();
                        var validationResult = await validator.ValidateAsync(product);

                        if (!validationResult.IsValid)
                        {
                            _logger.LogWarning("Product validation failed: {Errors}", 
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

        await _next(context);
    }
}
