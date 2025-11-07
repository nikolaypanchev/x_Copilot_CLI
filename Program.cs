using MinimalApiApp.Models;
using MinimalApiApp.Services;
using MinimalApiApp.Middleware;
using MinimalApiApp.Validators;
using FluentValidation;
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build())
    .CreateLogger();

try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog();

    // Configure Redis
    var redisConnectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString") ?? "localhost:6379";
    var redisInstanceName = builder.Configuration.GetValue<string>("Redis:InstanceName") ?? "MinimalApiApp:";

    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
        options.InstanceName = redisInstanceName;
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Register cache service
    builder.Services.AddSingleton<ICacheService, RedisCacheService>();

    // Register repository
    builder.Services.AddSingleton<IProductRepository, InMemoryProductRepository>();

    builder.Services.AddSingleton<IUserService, UserService>();
    builder.Services.AddSingleton<IProductService, ProductService>();

    // Register UnitOfWork which exposes the existing services
    builder.Services.AddSingleton<IUnitOfWork, UnitOfWork>();

    // Register FluentValidation
    builder.Services.AddScoped<IValidator<Product>, ProductValidator>();
    builder.Services.AddScoped<IValidator<User>, UserValidator>();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    // Logging middleware (should be first to log all requests)
    app.UseMiddleware<LoggingMiddleware>();

    // Resilience middleware (for retry policies)
    app.UseMiddleware<ResilienceMiddleware>();

    // Error handling middleware (must be first)
    app.UseMiddleware<ErrorHandlingMiddleware>();

    // Validation middleware for products and users
    app.UseMiddleware<ValidationMiddleware>();

// User endpoints
app.MapGet("/api/users", async (IUserService userService) =>
{
    var users = await userService.GetAllUsersAsync();
    return Results.Ok(users);
})
.WithName("GetAllUsers")
.WithOpenApi();

app.MapGet("/api/users/{id}", async (int id, IUserService userService) =>
{
    var user = await userService.GetUserByIdAsync(id);
    return Results.Ok(user);
})
.WithName("GetUserById")
.WithOpenApi();

app.MapPost("/api/users", async (User user, IUserService userService) =>
{
    var createdUser = await userService.CreateUserAsync(user);
    return Results.Created($"/api/users/{createdUser.Id}", createdUser);
})
.WithName("CreateUser")
.WithOpenApi();

app.MapPut("/api/users/{id}", async (int id, User user, IUserService userService) =>
{
    var updatedUser = await userService.UpdateUserAsync(id, user);
    return Results.Ok(updatedUser);
})
.WithName("UpdateUser")
.WithOpenApi();

app.MapDelete("/api/users/{id}", async (int id, IUserService userService) =>
{
    await userService.DeleteUserAsync(id);
    return Results.NoContent();
})
.WithName("DeleteUser")
.WithOpenApi();

// Product endpoints
app.MapGet("/api/products", async (IProductService productService) =>
{
    var products = await productService.GetAllProductsAsync();
    return Results.Ok(products);
})
.WithName("GetAllProducts")
.WithOpenApi();

app.MapGet("/api/products/{id}", async (int id, IProductService productService) =>
{
    var product = await productService.GetProductByIdAsync(id);
    return Results.Ok(product);
})
.WithName("GetProductById")
.WithOpenApi();

app.MapPost("/api/products", async (Product product, IProductService productService) =>
{
    var createdProduct = await productService.CreateProductAsync(product);
    return Results.Created($"/api/products/{createdProduct.Id}", createdProduct);
})
.WithName("CreateProduct")
.WithOpenApi();

app.MapPut("/api/products/{id}", async (int id, Product product, IProductService productService) =>
{
    var updatedProduct = await productService.UpdateProductAsync(id, product);
    return Results.Ok(updatedProduct);
})
.WithName("UpdateProduct")
.WithOpenApi();

app.MapDelete("/api/products/{id}", async (int id, IProductService productService) =>
{
    await productService.DeleteProductAsync(id);
    return Results.NoContent();
})
.WithName("DeleteProduct")
.WithOpenApi();

// Redis health check endpoint
app.MapGet("/api/health/redis", async (ICacheService cacheService) =>
{
    try
    {
        var testKey = "health_check";
        var testValue = DateTime.UtcNow.ToString();
        
        await cacheService.SetAsync(testKey, testValue, TimeSpan.FromSeconds(10));
        var retrieved = await cacheService.GetAsync<string>(testKey);
        await cacheService.RemoveAsync(testKey);
        
        if (retrieved == testValue)
        {
            return Results.Ok(new { status = "healthy", message = "Redis is connected and working" });
        }
        
        return Results.Problem("Redis health check failed - data mismatch");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Redis health check failed: {ex.Message}");
    }
})
.WithName("RedisHealthCheck")
.WithOpenApi();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }