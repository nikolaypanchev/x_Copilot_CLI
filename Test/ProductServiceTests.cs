using MinimalApiApp.Models;
using MinimalApiApp.Services;

namespace MinimalApiApp.Tests;

public class ProductServiceTests
{
    private readonly IProductService _productService;

    public ProductServiceTests()
    {
        _productService = new ProductService();
    }

    [Fact]
    public async Task GetAllProductsAsync_ReturnsEmptyList_WhenNoProducts()
    {
        var products = await _productService.GetAllProductsAsync();
        
        Assert.NotNull(products);
        Assert.Empty(products);
    }

    [Fact]
    public async Task CreateProductAsync_CreatesProduct_WithGeneratedId()
    {
        var product = new Product 
        { 
            Name = "Laptop", 
            Description = "Gaming laptop",
            Price = 1299.99m,
            Stock = 10
        };
        
        var createdProduct = await _productService.CreateProductAsync(product);
        
        Assert.NotNull(createdProduct);
        Assert.Equal(1, createdProduct.Id);
        Assert.Equal("Laptop", createdProduct.Name);
        Assert.Equal(1299.99m, createdProduct.Price);
        Assert.Equal(10, createdProduct.Stock);
    }

    [Fact]
    public async Task GetProductByIdAsync_ReturnsProduct_WhenProductExists()
    {
        var product = new Product 
        { 
            Name = "Mouse", 
            Description = "Wireless mouse",
            Price = 29.99m,
            Stock = 50
        };
        await _productService.CreateProductAsync(product);
        
        var retrievedProduct = await _productService.GetProductByIdAsync(1);
        
        Assert.NotNull(retrievedProduct);
        Assert.Equal(1, retrievedProduct.Id);
        Assert.Equal("Mouse", retrievedProduct.Name);
    }

    [Fact]
    public async Task GetProductByIdAsync_ReturnsNull_WhenProductDoesNotExist()
    {
        var product = await _productService.GetProductByIdAsync(999);
        
        Assert.Null(product);
    }

    [Fact]
    public async Task UpdateProductAsync_UpdatesProduct_WhenProductExists()
    {
        var product = new Product 
        { 
            Name = "Keyboard", 
            Description = "Mechanical",
            Price = 99.99m,
            Stock = 20
        };
        await _productService.CreateProductAsync(product);
        
        var updatedProduct = new Product 
        { 
            Name = "Keyboard Pro", 
            Description = "RGB Mechanical",
            Price = 149.99m,
            Stock = 15
        };
        var result = await _productService.UpdateProductAsync(1, updatedProduct);
        
        Assert.NotNull(result);
        Assert.Equal("Keyboard Pro", result.Name);
        Assert.Equal("RGB Mechanical", result.Description);
        Assert.Equal(149.99m, result.Price);
        Assert.Equal(15, result.Stock);
    }

    [Fact]
    public async Task UpdateProductAsync_ReturnsNull_WhenProductDoesNotExist()
    {
        var product = new Product 
        { 
            Name = "Test", 
            Description = "Test product",
            Price = 10.00m,
            Stock = 5
        };
        
        var result = await _productService.UpdateProductAsync(999, product);
        
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteProductAsync_DeletesProduct_WhenProductExists()
    {
        var product = new Product 
        { 
            Name = "Headphones", 
            Description = "Noise cancelling",
            Price = 199.99m,
            Stock = 30
        };
        await _productService.CreateProductAsync(product);
        
        var deleted = await _productService.DeleteProductAsync(1);
        var retrievedProduct = await _productService.GetProductByIdAsync(1);
        
        Assert.True(deleted);
        Assert.Null(retrievedProduct);
    }

    [Fact]
    public async Task DeleteProductAsync_ReturnsFalse_WhenProductDoesNotExist()
    {
        var deleted = await _productService.DeleteProductAsync(999);
        
        Assert.False(deleted);
    }

    [Fact]
    public async Task GetAllProductsAsync_ReturnsAllProducts_WhenMultipleProductsExist()
    {
        await _productService.CreateProductAsync(new Product 
        { 
            Name = "Product1", 
            Description = "Desc1",
            Price = 10.00m,
            Stock = 100
        });
        await _productService.CreateProductAsync(new Product 
        { 
            Name = "Product2", 
            Description = "Desc2",
            Price = 20.00m,
            Stock = 200
        });
        await _productService.CreateProductAsync(new Product 
        { 
            Name = "Product3", 
            Description = "Desc3",
            Price = 30.00m,
            Stock = 300
        });
        
        var products = await _productService.GetAllProductsAsync();
        
        Assert.NotNull(products);
        Assert.Equal(3, products.Count());
    }

    [Fact]
    public async Task CreateProductAsync_AllowsZeroPrice()
    {
        var product = new Product 
        { 
            Name = "Free Item", 
            Description = "Free product",
            Price = 0m,
            Stock = 1000
        };
        
        var createdProduct = await _productService.CreateProductAsync(product);
        
        Assert.NotNull(createdProduct);
        Assert.Equal(0m, createdProduct.Price);
    }

    [Fact]
    public async Task CreateProductAsync_AllowsZeroStock()
    {
        var product = new Product 
        { 
            Name = "Out of Stock", 
            Description = "Currently unavailable",
            Price = 99.99m,
            Stock = 0
        };
        
        var createdProduct = await _productService.CreateProductAsync(product);
        
        Assert.NotNull(createdProduct);
        Assert.Equal(0, createdProduct.Stock);
    }
}
