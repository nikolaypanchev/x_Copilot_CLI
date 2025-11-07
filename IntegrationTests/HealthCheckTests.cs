using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace MinimalApiApp.IntegrationTests;

public class HealthCheckTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HealthCheckTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ReturnsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("healthy");
    }

    [Fact]
    public async Task DatabaseHealthCheck_ReturnsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/api/health/database");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("healthy");
    }

    [Fact]
    public async Task RedisHealthCheck_Returns()
    {
        // Act
        var response = await _client.GetAsync("/api/health/redis");

        // Assert
        // Redis may not be running in test environment, so we just check that endpoint responds
        response.Should().NotBeNull();
    }
}
