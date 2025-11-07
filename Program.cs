using MinimalApiApp.Models;
using MinimalApiApp.Services;
using MinimalApiApp.Middleware;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<IProductService, ProductService>();

// Register UnitOfWork which exposes the existing services
builder.Services.AddSingleton<IUnitOfWork, UnitOfWork>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Error handling middleware (must be first)
app.UseMiddleware<ErrorHandlingMiddleware>();

// Email validation middleware for user endpoints
app.UseMiddleware<EmailValidationMiddleware>();

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

app.Run();

public partial class Program { }