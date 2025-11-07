using FluentAssertions;
using FluentValidation.TestHelper;
using MinimalApiApp.Models;
using MinimalApiApp.Services;
using MinimalApiApp.Validators;
using Moq;
using Xunit;

namespace MinimalApiApp.UnitTests.Validators;

public class UserValidatorTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly UserValidator _validator;

    public UserValidatorTests()
    {
        _mockUserService = new Mock<IUserService>();
        _validator = new UserValidator(_mockUserService.Object);
    }

    [Fact]
    public async Task Validate_ValidUser_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var user = new User
        {
            Name = "John Doe",
            Email = "john@example.com"
        };

        _mockUserService.Setup(s => s.GetAllUsersAsync())
            .ReturnsAsync(new List<User>());

        // Act
        var result = await _validator.TestValidateAsync(user);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_UserWithNullName_ShouldHaveValidationError()
    {
        // Arrange
        var user = new User
        {
            Name = null!,
            Email = "john@example.com"
        };

        // Act
        var result = await _validator.TestValidateAsync(user);

        // Assert
        result.ShouldHaveValidationErrorFor(u => u.Name)
            .WithErrorMessage("User name cannot be null");
    }

    [Fact]
    public async Task Validate_UserWithEmptyName_ShouldHaveValidationError()
    {
        // Arrange
        var user = new User
        {
            Name = "",
            Email = "john@example.com"
        };

        // Act
        var result = await _validator.TestValidateAsync(user);

        // Assert
        result.ShouldHaveValidationErrorFor(u => u.Name)
            .WithErrorMessage("User name is required");
    }

    [Fact]
    public async Task Validate_UserWithNullEmail_ShouldHaveValidationError()
    {
        // Arrange
        var user = new User
        {
            Name = "John Doe",
            Email = null!
        };

        // Act
        var result = await _validator.TestValidateAsync(user);

        // Assert
        result.ShouldHaveValidationErrorFor(u => u.Email)
            .WithErrorMessage("Email cannot be null");
    }

    [Fact]
    public async Task Validate_UserWithEmptyEmail_ShouldHaveValidationError()
    {
        // Arrange
        var user = new User
        {
            Name = "John Doe",
            Email = ""
        };

        // Act
        var result = await _validator.TestValidateAsync(user);

        // Assert
        result.ShouldHaveValidationErrorFor(u => u.Email)
            .WithErrorMessage("Email is required");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("invalid@")]
    [InlineData("@example.com")]
    [InlineData("invalid.com")]
    public async Task Validate_UserWithInvalidEmail_ShouldHaveValidationError(string email)
    {
        // Arrange
        var user = new User
        {
            Name = "John Doe",
            Email = email
        };

        // Act
        var result = await _validator.TestValidateAsync(user);

        // Assert
        result.ShouldHaveValidationErrorFor(u => u.Email)
            .WithErrorMessage("A valid email is required");
    }

    [Fact]
    public async Task Validate_UserWithDuplicateEmail_ShouldHaveValidationError()
    {
        // Arrange
        var existingUser = new User
        {
            Id = 1,
            Name = "Existing User",
            Email = "existing@example.com"
        };

        var newUser = new User
        {
            Id = 0,
            Name = "New User",
            Email = "existing@example.com"
        };

        _mockUserService.Setup(s => s.GetAllUsersAsync())
            .ReturnsAsync(new List<User> { existingUser });

        // Act
        var result = await _validator.TestValidateAsync(newUser);

        // Assert
        result.ShouldHaveValidationErrorFor(u => u.Email)
            .WithErrorMessage("Email is already in use");
    }

    [Fact]
    public async Task Validate_UserUpdatingOwnEmail_ShouldNotHaveValidationError()
    {
        // Arrange
        var existingUser = new User
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@example.com"
        };

        _mockUserService.Setup(s => s.GetAllUsersAsync())
            .ReturnsAsync(new List<User> { existingUser });

        // Act - same user updating their own record
        var result = await _validator.TestValidateAsync(existingUser);

        // Assert
        result.ShouldNotHaveValidationErrorFor(u => u.Email);
    }

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.co.uk")]
    [InlineData("user+tag@example.com")]
    public async Task Validate_UserWithValidEmailFormats_ShouldNotHaveValidationError(string email)
    {
        // Arrange
        var user = new User
        {
            Name = "John Doe",
            Email = email
        };

        _mockUserService.Setup(s => s.GetAllUsersAsync())
            .ReturnsAsync(new List<User>());

        // Act
        var result = await _validator.TestValidateAsync(user);

        // Assert
        result.ShouldNotHaveValidationErrorFor(u => u.Email);
    }
}
