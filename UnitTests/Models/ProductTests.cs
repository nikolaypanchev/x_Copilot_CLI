using FluentAssertions;
using MinimalApiApp.Models;
using Xunit;

namespace MinimalApiApp.UnitTests.Models;

public class ProductTests
{
    [Fact]
    public void Product_DefaultConstructor_ShouldInitializeProperties()
    {
        // Act
        var product = new Product();

        // Assert
        product.Id.Should().Be(0);
        product.Name.Should().Be(string.Empty);
        product.Description.Should().Be(string.Empty);
        product.Price.Should().Be(0);
        product.Stock.Should().Be(0);
    }

    [Fact]
    public void Product_SetProperties_ShouldRetainValues()
    {
        // Arrange
        var product = new Product();

        // Act
        product.Id = 1;
        product.Name = "Test Product";
        product.Description = "Test Description";
        product.Price = 99.99m;
        product.Stock = 10;

        // Assert
        product.Id.Should().Be(1);
        product.Name.Should().Be("Test Product");
        product.Description.Should().Be("Test Description");
        product.Price.Should().Be(99.99m);
        product.Stock.Should().Be(10);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Product_NegativeOrZeroPrice_ShouldBeAllowed(decimal price)
    {
        // Arrange
        var product = new Product();

        // Act
        product.Price = price;

        // Assert
        product.Price.Should().Be(price);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(1000)]
    public void Product_PositiveStock_ShouldBeAllowed(int stock)
    {
        // Arrange
        var product = new Product();

        // Act
        product.Stock = stock;

        // Assert
        product.Stock.Should().Be(stock);
    }
}
