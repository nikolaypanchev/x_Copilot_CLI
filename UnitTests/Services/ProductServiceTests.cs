using FluentAssertions;
using Microsoft.Extensions.Logging;
using MinimalApiApp.Data;
using MinimalApiApp.Models;
using MinimalApiApp.Services;
using Moq;
using Xunit;

namespace MinimalApiApp.UnitTests.Services;

public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IProductRepository> _mockProductRepository;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockProductRepository = new Mock<IProductRepository>();
        
        _mockUnitOfWork.Setup(u => u.Products).Returns(_mockProductRepository.Object);
        
        _productService = new ProductService(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task GetAllProductsAsync_ShouldReturnAllProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product1", Description = "Desc1", Price = 10.0m, Stock = 5 },
            new() { Id = 2, Name = "Product2", Description = "Desc2", Price = 20.0m, Stock = 10 }
        };

        _mockProductRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(products);

        // Act
        var result = await _productService.GetAllProductsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(products);
        _mockProductRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetProductByIdAsync_WithValidId_ShouldReturnProduct()
    {
        // Arrange
        var product = new Product 
        { 
            Id = 1, 
            Name = "Test", 
            Description = "TestDesc", 
            Price = 10.0m, 
            Stock = 5 
        };

        _mockProductRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);

        // Act
        var result = await _productService.GetProductByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(product);
        _mockProductRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task CreateProductAsync_ShouldCreateProductAndCommit()
    {
        // Arrange
        var product = new Product 
        { 
            Name = "Test", 
            Description = "TestDesc", 
            Price = 10.0m, 
            Stock = 5 
        };

        var createdProduct = new Product 
        { 
            Id = 1, 
            Name = "Test", 
            Description = "TestDesc", 
            Price = 10.0m, 
            Stock = 5 
        };

        _mockProductRepository.Setup(r => r.AddAsync(product))
            .ReturnsAsync(createdProduct);

        _mockUnitOfWork.Setup(u => u.CommitAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _productService.CreateProductAsync(product);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        _mockProductRepository.Verify(r => r.AddAsync(product), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldUpdateProductAndCommit()
    {
        // Arrange
        var product = new Product 
        { 
            Name = "Updated", 
            Description = "UpdatedDesc", 
            Price = 15.0m, 
            Stock = 8 
        };

        var updatedProduct = new Product 
        { 
            Id = 1, 
            Name = "Updated", 
            Description = "UpdatedDesc", 
            Price = 15.0m, 
            Stock = 8 
        };

        _mockProductRepository.Setup(r => r.UpdateAsync(1, product))
            .ReturnsAsync(updatedProduct);

        _mockUnitOfWork.Setup(u => u.CommitAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _productService.UpdateProductAsync(1, product);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(updatedProduct);
        _mockProductRepository.Verify(r => r.UpdateAsync(1, product), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteProductAsync_WithValidId_ShouldDeleteAndCommit()
    {
        // Arrange
        _mockProductRepository.Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        _mockUnitOfWork.Setup(u => u.CommitAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _productService.DeleteProductAsync(1);

        // Assert
        result.Should().BeTrue();
        _mockProductRepository.Verify(r => r.DeleteAsync(1), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteProductAsync_WhenDeletionFails_ShouldNotCommit()
    {
        // Arrange
        _mockProductRepository.Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(false);

        // Act
        var result = await _productService.DeleteProductAsync(1);

        // Assert
        result.Should().BeFalse();
        _mockProductRepository.Verify(r => r.DeleteAsync(1), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
    }
}
