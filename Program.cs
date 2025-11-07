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
    return user is not null ? Results.Ok(user) : Results.NotFound();
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
    return updatedUser is not null ? Results.Ok(updatedUser) : Results.NotFound();
})
.WithName("UpdateUser")
.WithOpenApi();

app.MapDelete("/api/users/{id}", async (int id, IUserService userService) =>
{
    var deleted = await userService.DeleteUserAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound();
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
    return product is not null ? Results.Ok(product) : Results.NotFound();
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
    return updatedProduct is not null ? Results.Ok(updatedProduct) : Results.NotFound();
})
.WithName("UpdateProduct")
.WithOpenApi();

app.MapDelete("/api/products/{id}", async (int id, IProductService productService) =>
{
    var deleted = await productService.DeleteProductAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteProduct")
.WithOpenApi();

app.Run();

public partial class Program { }