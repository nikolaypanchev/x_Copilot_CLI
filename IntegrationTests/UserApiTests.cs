using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MinimalApiApp.Models;
using Xunit;

namespace MinimalApiApp.IntegrationTests;

public class UserApiTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UserApiTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllUsers_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllUsers_ReturnsUserList()
    {
        // Act
        var users = await _client.GetFromJsonAsync<List<User>>("/api/v1/users");

        // Assert
        users.Should().NotBeNull();
        users.Should().HaveCountGreaterThanOrEqualTo(2); // Seed data
    }

    [Fact]
    public async Task GetUserById_WithValidId_ReturnsUser()
    {
        // Act
        var user = await _client.GetFromJsonAsync<User>("/api/v1/users/1");

        // Assert
        user.Should().NotBeNull();
        user!.Id.Should().Be(1);
        user.Name.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetUserById_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/users/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateUser_WithValidData_ReturnsCreated()
    {
        // Arrange
        var newUser = new User
        {
            Name = "Test User",
            Email = "test@example.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/users", newUser);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdUser = await response.Content.ReadFromJsonAsync<User>();
        createdUser.Should().NotBeNull();
        createdUser!.Name.Should().Be("Test User");
        createdUser.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task CreateUser_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var invalidUser = new User
        {
            Name = "Test User",
            Email = "invalid-email"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/users", invalidUser);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateUser_WithDuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var duplicateUser = new User
        {
            Name = "Duplicate User",
            Email = "john.doe@example.com" // This email exists in seed data
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/users", duplicateUser);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateUser_WithValidData_ReturnsOk()
    {
        // Arrange
        var updatedUser = new User
        {
            Id = 1,
            Name = "Updated Name",
            Email = "updated@example.com"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/v1/users/1", updatedUser);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteUser_WithValidId_ReturnsNoContent()
    {
        // Act
        var response = await _client.DeleteAsync("/api/v1/users/2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetAllUsersV2_ReturnsEnhancedResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/v2/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("version");
        content.Should().Contain("2.0");
    }
}
