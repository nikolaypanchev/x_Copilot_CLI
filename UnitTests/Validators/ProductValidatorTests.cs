using FluentAssertions;
using FluentValidation.TestHelper;
using MinimalApiApp.Models;
using MinimalApiApp.Services;
using MinimalApiApp.Validators;
using Moq;
using Xunit;

namespace MinimalApiApp.UnitTests.Validators;

public class ProductValidatorTests
{
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly ProductValidator _validator;

    public ProductValidatorTests()
    {
        _mockRepository = new Mock<IProductRepository>();
        _validator = new ProductValidator(_mockRepository.Object);
    }

    [Fact]
    public void Validate_ValidProduct_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test",
            Description = "TestDesc",
            Price = 10.0m,
            Stock = 5
        };

        // Act
        var result = _validator.TestValidate(product);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ProductWithNullName_ShouldHaveValidationError()
    {
        // Arrange
        var product = new Product
        {
            Name = null!,
            Description = "TestDesc",
            Price = 10.0m,
            Stock = 5
        };

        // Act
        var result = _validator.TestValidate(product);

        // Assert
        result.ShouldHaveValidationErrorFor(p => p.Name)
            .WithErrorMessage("Product name cannot be null");
    }

    [Fact]
    public void Validate_ProductWithEmptyName_ShouldHaveValidationError()
    {
        // Arrange
        var product = new Product
        {
            Name = "",
            Description = "TestDesc",
            Price = 10.0m,
            Stock = 5
        };

        // Act
        var result = _validator.TestValidate(product);

        // Assert
        result.ShouldHaveValidationErrorFor(p => p.Name)
            .WithErrorMessage("Product name is required");
    }

    [Fact]
    public void Validate_ProductWithNameTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var product = new Product
        {
            Name = "TooLong",
            Description = "TestDesc",
            Price = 10.0m,
            Stock = 5
        };

        // Act
        var result = _validator.TestValidate(product);

        // Assert
        result.ShouldHaveValidationErrorFor(p => p.Name)
            .WithErrorMessage("Product name must not exceed 5 characters");
    }

    [Fact]
    public void Validate_ProductWithNullDescription_ShouldHaveValidationError()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test",
            Description = null!,
            Price = 10.0m,
            Stock = 5
        };

        // Act
        var result = _validator.TestValidate(product);

        // Assert
        result.ShouldHaveValidationErrorFor(p => p.Description)
            .WithErrorMessage("Product description cannot be null");
    }

    [Fact]
    public void Validate_ProductWithEmptyDescription_ShouldHaveValidationError()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test",
            Description = "",
            Price = 10.0m,
            Stock = 5
        };

        // Act
        var result = _validator.TestValidate(product);

        // Assert
        result.ShouldHaveValidationErrorFor(p => p.Description)
            .WithErrorMessage("Product description is required");
    }

    [Fact]
    public void Validate_ProductWithDescriptionTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test",
            Description = "This is way too long",
            Price = 10.0m,
            Stock = 5
        };

        // Act
        var result = _validator.TestValidate(product);

        // Assert
        result.ShouldHaveValidationErrorFor(p => p.Description)
            .WithErrorMessage("Product description must not exceed 10 characters");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10.5)]
    public void Validate_ProductWithZeroOrNegativePrice_ShouldHaveValidationError(decimal price)
    {
        // Arrange
        var product = new Product
        {
            Name = "Test",
            Description = "TestDesc",
            Price = price,
            Stock = 5
        };

        // Act
        var result = _validator.TestValidate(product);

        // Assert
        result.ShouldHaveValidationErrorFor(p => p.Price)
            .WithErrorMessage("Product price must be greater than 0");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Validate_ProductWithNegativeStock_ShouldHaveValidationError(int stock)
    {
        // Arrange
        var product = new Product
        {
            Name = "Test",
            Description = "TestDesc",
            Price = 10.0m,
            Stock = stock
        };

        // Act
        var result = _validator.TestValidate(product);

        // Assert
        result.ShouldHaveValidationErrorFor(p => p.Stock)
            .WithErrorMessage("Product stock must be greater than or equal to 0");
    }

    [Fact]
    public void Validate_ProductWithZeroStock_ShouldNotHaveValidationError()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test",
            Description = "TestDesc",
            Price = 10.0m,
            Stock = 0
        };

        // Act
        var result = _validator.TestValidate(product);

        // Assert
        result.ShouldNotHaveValidationErrorFor(p => p.Stock);
    }
}
