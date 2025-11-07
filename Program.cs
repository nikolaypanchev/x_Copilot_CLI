using MinimalApiApp.Models;
using MinimalApiApp.Services;
using MinimalApiApp.Middleware;
using MinimalApiApp.Validators;
using MinimalApiApp.Configuration;
using MinimalApiApp.Data;
using FluentValidation;
using Serilog;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

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

    // Configure Entity Framework Core with InMemory Database
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("MinimalApiDb"));

    // Configure API Versioning
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = ApiVersionReader.Combine(
            new UrlSegmentApiVersionReader(),
            new HeaderApiVersionReader("X-Api-Version"),
            new QueryStringApiVersionReader("api-version")
        );
    }).AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    // Configure Redis - Direct Connection (No Microsoft IDistributedCache)
    var redisConnectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString") ?? "localhost:6379";

    // Register IConnectionMultiplexer for direct Redis operations
    try
    {
        var redisConnection = ConnectionMultiplexer.Connect(redisConnectionString);
        builder.Services.AddSingleton<IConnectionMultiplexer>(redisConnection);
        Log.Information("Redis connection established successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to connect to Redis. Application requires Redis to function.");
        throw new InvalidOperationException("Redis connection is required. Please ensure Redis is running.", ex);
    }

    builder.Services.AddEndpointsApiExplorer();
    
    // Add versioned Swagger
    builder.Services.AddVersionedSwagger();

    // Register cache service (Redis-only, no Microsoft IDistributedCache)
    builder.Services.AddSingleton<ICacheService, RedisCacheService>();

    // Register EF Core repositories (use scoped for DbContext)
    builder.Services.AddScoped<IProductRepository, EfProductRepository>();
    builder.Services.AddScoped<IUserService, EfUserRepository>();
    builder.Services.AddScoped<IProductService, ProductService>();

    // Register UnitOfWork which exposes the existing services
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    // Register FluentValidation
    builder.Services.AddScoped<IValidator<Product>, ProductValidator>();
    builder.Services.AddScoped<IValidator<User>, UserValidator>();

    var app = builder.Build();

    // Ensure database is created and seeded
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.EnsureCreated();
        Log.Information("Database initialized with seed data");
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseVersionedSwagger();
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

    // Create API version sets
    var apiVersionSet = app.NewApiVersionSet()
        .HasApiVersion(new ApiVersion(1, 0))
        .HasApiVersion(new ApiVersion(2, 0))
        .ReportApiVersions()
        .Build();

    // ===== V1 ENDPOINTS =====
    var v1 = app.MapGroup("/api/v{version:apiVersion}")
        .WithApiVersionSet(apiVersionSet);

    // V1 User endpoints
    v1.MapGet("/users", async (IUserService userService) =>
    {
        var users = await userService.GetAllUsersAsync();
        return Results.Ok(users);
    })
    .WithName("GetAllUsersV1")
    .WithOpenApi()
    .MapToApiVersion(1, 0);

    v1.MapGet("/users/{id}", async (int id, IUserService userService) =>
    {
        var user = await userService.GetUserByIdAsync(id);
        return Results.Ok(user);
    })
    .WithName("GetUserByIdV1")
    .WithOpenApi()
    .MapToApiVersion(1, 0);

    v1.MapPost("/users", async (User user, IUserService userService) =>
    {
        var createdUser = await userService.CreateUserAsync(user);
        return Results.Created($"/api/v1/users/{createdUser.Id}", createdUser);
    })
    .WithName("CreateUserV1")
    .WithOpenApi()
    .MapToApiVersion(1, 0);

    v1.MapPut("/users/{id}", async (int id, User user, IUserService userService) =>
    {
        var updatedUser = await userService.UpdateUserAsync(id, user);
        return Results.Ok(updatedUser);
    })
    .WithName("UpdateUserV1")
    .WithOpenApi()
    .MapToApiVersion(1, 0);

    v1.MapDelete("/users/{id}", async (int id, IUserService userService) =>
    {
        await userService.DeleteUserAsync(id);
        return Results.NoContent();
    })
    .WithName("DeleteUserV1")
    .WithOpenApi()
    .MapToApiVersion(1, 0);

    // V1 Product endpoints
    v1.MapGet("/products", async (IProductService productService) =>
    {
        var products = await productService.GetAllProductsAsync();
        return Results.Ok(products);
    })
    .WithName("GetAllProductsV1")
    .WithOpenApi()
    .MapToApiVersion(1, 0);

    v1.MapGet("/products/{id}", async (int id, IProductService productService) =>
    {
        var product = await productService.GetProductByIdAsync(id);
        return Results.Ok(product);
    })
    .WithName("GetProductByIdV1")
    .WithOpenApi()
    .MapToApiVersion(1, 0);

    v1.MapPost("/products", async (Product product, IProductService productService) =>
    {
        var createdProduct = await productService.CreateProductAsync(product);
        return Results.Created($"/api/v1/products/{createdProduct.Id}", createdProduct);
    })
    .WithName("CreateProductV1")
    .WithOpenApi()
    .MapToApiVersion(1, 0);

    v1.MapPut("/products/{id}", async (int id, Product product, IProductService productService) =>
    {
        var updatedProduct = await productService.UpdateProductAsync(id, product);
        return Results.Ok(updatedProduct);
    })
    .WithName("UpdateProductV1")
    .WithOpenApi()
    .MapToApiVersion(1, 0);

    v1.MapDelete("/products/{id}", async (int id, IProductService productService) =>
    {
        await productService.DeleteProductAsync(id);
        return Results.NoContent();
    })
    .WithName("DeleteProductV1")
    .WithOpenApi()
    .MapToApiVersion(1, 0);

    // ===== V2 ENDPOINTS (Enhanced with additional features) =====
    var v2 = app.MapGroup("/api/v{version:apiVersion}")
        .WithApiVersionSet(apiVersionSet);

    // V2 User endpoints with enhanced response
    v2.MapGet("/users", async (IUserService userService) =>
    {
        var users = await userService.GetAllUsersAsync();
        return Results.Ok(new
        {
            version = "2.0",
            count = users.Count(),
            data = users,
            timestamp = DateTime.UtcNow
        });
    })
    .WithName("GetAllUsersV2")
    .WithOpenApi()
    .MapToApiVersion(2, 0);

    v2.MapGet("/users/{id}", async (int id, IUserService userService) =>
    {
        var user = await userService.GetUserByIdAsync(id);
        return Results.Ok(new
        {
            version = "2.0",
            data = user,
            timestamp = DateTime.UtcNow
        });
    })
    .WithName("GetUserByIdV2")
    .WithOpenApi()
    .MapToApiVersion(2, 0);

    v2.MapPost("/users", async (User user, IUserService userService) =>
    {
        var createdUser = await userService.CreateUserAsync(user);
        return Results.Created($"/api/v2/users/{createdUser.Id}", new
        {
            version = "2.0",
            data = createdUser,
            message = "User created successfully",
            timestamp = DateTime.UtcNow
        });
    })
    .WithName("CreateUserV2")
    .WithOpenApi()
    .MapToApiVersion(2, 0);

    v2.MapPut("/users/{id}", async (int id, User user, IUserService userService) =>
    {
        var updatedUser = await userService.UpdateUserAsync(id, user);
        return Results.Ok(new
        {
            version = "2.0",
            data = updatedUser,
            message = "User updated successfully",
            timestamp = DateTime.UtcNow
        });
    })
    .WithName("UpdateUserV2")
    .WithOpenApi()
    .MapToApiVersion(2, 0);

    v2.MapDelete("/users/{id}", async (int id, IUserService userService) =>
    {
        await userService.DeleteUserAsync(id);
        return Results.Ok(new
        {
            version = "2.0",
            message = "User deleted successfully",
            timestamp = DateTime.UtcNow
        });
    })
    .WithName("DeleteUserV2")
    .WithOpenApi()
    .MapToApiVersion(2, 0);

    // V2 Product endpoints with enhanced response
    v2.MapGet("/products", async (IProductService productService) =>
    {
        var products = await productService.GetAllProductsAsync();
        return Results.Ok(new
        {
            version = "2.0",
            count = products.Count(),
            data = products,
            timestamp = DateTime.UtcNow
        });
    })
    .WithName("GetAllProductsV2")
    .WithOpenApi()
    .MapToApiVersion(2, 0);

    v2.MapGet("/products/{id}", async (int id, IProductService productService) =>
    {
        var product = await productService.GetProductByIdAsync(id);
        return Results.Ok(new
        {
            version = "2.0",
            data = product,
            timestamp = DateTime.UtcNow
        });
    })
    .WithName("GetProductByIdV2")
    .WithOpenApi()
    .MapToApiVersion(2, 0);

    v2.MapPost("/products", async (Product product, IProductService productService) =>
    {
        var createdProduct = await productService.CreateProductAsync(product);
        return Results.Created($"/api/v2/products/{createdProduct.Id}", new
        {
            version = "2.0",
            data = createdProduct,
            message = "Product created successfully",
            timestamp = DateTime.UtcNow
        });
    })
    .WithName("CreateProductV2")
    .WithOpenApi()
    .MapToApiVersion(2, 0);

    v2.MapPut("/products/{id}", async (int id, Product product, IProductService productService) =>
    {
        var updatedProduct = await productService.UpdateProductAsync(id, product);
        return Results.Ok(new
        {
            version = "2.0",
            data = updatedProduct,
            message = "Product updated successfully",
            timestamp = DateTime.UtcNow
        });
    })
    .WithName("UpdateProductV2")
    .WithOpenApi()
    .MapToApiVersion(2, 0);

    v2.MapDelete("/products/{id}", async (int id, IProductService productService) =>
    {
        await productService.DeleteProductAsync(id);
        return Results.Ok(new
        {
            version = "2.0",
            message = "Product deleted successfully",
            timestamp = DateTime.UtcNow
        });
    })
    .WithName("DeleteProductV2")
    .WithOpenApi()
    .MapToApiVersion(2, 0);

    // ===== CACHE MANAGEMENT ENDPOINTS (Non-versioned) =====
    
    // Remove cache entry by key
    app.MapDelete("/api/cache/{key}", async (string key, ICacheService cacheService) =>
    {
        await cacheService.RemoveAsync(key);
        return Results.Ok(new
        {
            message = $"Cache entry '{key}' removed successfully",
            timestamp = DateTime.UtcNow
        });
    })
    .WithName("RemoveCacheByKey")
    .WithOpenApi()
    .WithTags("Cache Management");

    // Remove cache entries by prefix (NEW!)
    app.MapDelete("/api/cache/prefix/{prefix}", async (string prefix, ICacheService cacheService) =>
    {
        await cacheService.RemoveByPrefixAsync(prefix);
        return Results.Ok(new
        {
            message = $"All cache entries with prefix '{prefix}' removed successfully",
            prefix = prefix,
            timestamp = DateTime.UtcNow
        });
    })
    .WithName("RemoveCacheByPrefix")
    .WithOpenApi()
    .WithSummary("Remove all cache entries that start with the specified prefix")
    .WithDescription("This endpoint uses Redis SCAN to find and remove all keys matching the prefix pattern. Example: prefix='product' will remove 'product:1', 'product:2', 'products:all', etc.")
    .WithTags("Cache Management");

    // Set cache entry (for testing)
    app.MapPost("/api/cache", async (CacheEntry entry, ICacheService cacheService) =>
    {
        var expiration = entry.ExpirationMinutes.HasValue 
            ? TimeSpan.FromMinutes(entry.ExpirationMinutes.Value) 
            : TimeSpan.FromMinutes(5);
            
        await cacheService.SetAsync(entry.Key, entry.Value, expiration);
        return Results.Ok(new
        {
            message = $"Cache entry '{entry.Key}' set successfully",
            key = entry.Key,
            expirationMinutes = entry.ExpirationMinutes ?? 5,
            timestamp = DateTime.UtcNow
        });
    })
    .WithName("SetCacheEntry")
    .WithOpenApi()
    .WithTags("Cache Management");

    // Get cache entry (for testing)
    app.MapGet("/api/cache/{key}", async (string key, ICacheService cacheService) =>
    {
        var value = await cacheService.GetAsync<object>(key);
        if (value == null)
        {
            return Results.NotFound(new
            {
                message = $"Cache entry '{key}' not found",
                key = key,
                timestamp = DateTime.UtcNow
            });
        }
        
        return Results.Ok(new
        {
            key = key,
            value = value,
            timestamp = DateTime.UtcNow
        });
    })
    .WithName("GetCacheEntry")
    .WithOpenApi()
    .WithTags("Cache Management");

    // ===== HEALTH CHECK ENDPOINTS (Non-versioned) =====
    
    // Database health check endpoint
    app.MapGet("/api/health/database", async (ApplicationDbContext dbContext) =>
    {
        try
        {
            // Try to query the database
            var userCount = await dbContext.Users.CountAsync();
            var productCount = await dbContext.Products.CountAsync();
            
            return Results.Ok(new 
            { 
                status = "healthy", 
                message = "Database is connected and accessible",
                statistics = new 
                {
                    users = userCount,
                    products = productCount
                }
            });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Database health check failed: {ex.Message}");
        }
    })
    .WithName("DatabaseHealthCheck")
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

    // General API health check
    app.MapGet("/api/health", () => 
    {
        return Results.Ok(new 
        { 
            status = "healthy", 
            timestamp = DateTime.UtcNow,
            version = "1.0"
        });
    })
    .WithName("ApiHealthCheck")
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