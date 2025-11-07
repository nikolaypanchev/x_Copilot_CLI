using FluentAssertions;
using MinimalApiApp.Models;
using Xunit;

namespace MinimalApiApp.UnitTests.Models;

public class UserTests
{
    [Fact]
    public void User_DefaultConstructor_ShouldInitializeProperties()
    {
        // Act
        var user = new User();

        // Assert
        user.Id.Should().Be(0);
        user.Name.Should().Be(string.Empty);
        user.Email.Should().Be(string.Empty);
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void User_SetProperties_ShouldRetainValues()
    {
        // Arrange
        var user = new User();
        var createdAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        user.Id = 1;
        user.Name = "John Doe";
        user.Email = "john@example.com";
        user.CreatedAt = createdAt;

        // Assert
        user.Id.Should().Be(1);
        user.Name.Should().Be("John Doe");
        user.Email.Should().Be("john@example.com");
        user.CreatedAt.Should().Be(createdAt);
    }

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.co.uk")]
    [InlineData("user+tag@example.com")]
    public void User_ValidEmailFormats_ShouldBeAllowed(string email)
    {
        // Arrange
        var user = new User();

        // Act
        user.Email = email;

        // Assert
        user.Email.Should().Be(email);
    }

    [Fact]
    public void User_CreatedAt_ShouldDefaultToUtcNow()
    {
        // Act
        var user = new User();

        // Assert
        user.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
