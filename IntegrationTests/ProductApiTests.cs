using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MinimalApiApp.Models;
using Xunit;

namespace MinimalApiApp.IntegrationTests;

public class ProductApiTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProductApiTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllProducts_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllProducts_ReturnsProductList()
    {
        // Act
        var products = await _client.GetFromJsonAsync<List<Product>>("/api/v1/products");

        // Assert
        products.Should().NotBeNull();
        products.Should().HaveCountGreaterThanOrEqualTo(3); // Seed data
    }

    [Fact]
    public async Task GetProductById_WithValidId_ReturnsProduct()
    {
        // Act
        var product = await _client.GetFromJsonAsync<Product>("/api/v1/products/1");

        // Assert
        product.Should().NotBeNull();
        product!.Id.Should().Be(1);
        product.Name.Should().NotBeNullOrEmpty();
        product.Price.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetProductById_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/products/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ReturnsCreated()
    {
        // Arrange
        var newProduct = new Product
        {
            Name = "Test",
            Description = "TestDesc",
            Price = 99.99m,
            Stock = 10
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/products", newProduct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdProduct = await response.Content.ReadFromJsonAsync<Product>();
        createdProduct.Should().NotBeNull();
        createdProduct!.Name.Should().Be("Test");
    }

    [Fact]
    public async Task CreateProduct_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var invalidProduct = new Product
        {
            Name = "ThisNameIsWayTooLong",
            Description = "ThisDescriptionIsAlsoWayTooLong",
            Price = -10,
            Stock = -5
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/products", invalidProduct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateProduct_WithValidData_ReturnsOk()
    {
        // Arrange
        var updatedProduct = new Product
        {
            Name = "Phone",
            Description = "NewDevice",
            Price = 1499.99m,
            Stock = 5
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/v1/products/1", updatedProduct);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteProduct_WithValidId_ReturnsNoContent()
    {
        // Act
        var response = await _client.DeleteAsync("/api/v1/products/2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetAllProductsV2_ReturnsEnhancedResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/v2/products");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("version");
        content.Should().Contain("2.0");
        content.Should().Contain("count");
        content.Should().Contain("data");
        content.Should().Contain("timestamp");
    }
}
