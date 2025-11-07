using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MinimalApiApp.Services;
using Moq;
using System.Text.Json;
using Xunit;
using StackExchange.Redis;

namespace MinimalApiApp.UnitTests.Services;

public class CacheServiceTests
{
    private readonly Mock<IConnectionMultiplexer> _mockConnectionMultiplexer;
    private readonly Mock<IDatabase> _mockDatabase;
    private readonly Mock<IServer> _mockServer;
    private readonly Mock<ILogger<RedisCacheService>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly RedisCacheService _cacheService;

    public CacheServiceTests()
    {
        _mockConnectionMultiplexer = new Mock<IConnectionMultiplexer>();
        _mockDatabase = new Mock<IDatabase>();
        _mockServer = new Mock<IServer>();
        _mockLogger = new Mock<ILogger<RedisCacheService>>();
        _mockConfiguration = new Mock<IConfiguration>();

        // Setup configuration
        _mockConfiguration.Setup(c => c["Redis:InstanceName"]).Returns("MinimalApiApp:");

        // Setup connection multiplexer
        _mockConnectionMultiplexer.Setup(c => c.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_mockDatabase.Object);

        _cacheService = new RedisCacheService(
            _mockConnectionMultiplexer.Object,
            _mockLogger.Object,
            _mockConfiguration.Object);
    }

    [Fact]
    public async Task GetAsync_WithExistingKey_ShouldReturnDeserializedValue()
    {
        // Arrange
        var key = "test-key";
        var value = "test-value";
        var serialized = JsonSerializer.Serialize(value);
        var redisValue = new RedisValue(serialized);

        _mockDatabase.Setup(d => d.StringGetAsync(
            "MinimalApiApp:test-key",
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(redisValue);

        // Act
        var result = await _cacheService.GetAsync<string>(key);

        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public async Task GetAsync_WithNonExistingKey_ShouldReturnDefault()
    {
        // Arrange
        var key = "non-existing-key";

        _mockDatabase.Setup(d => d.StringGetAsync(
            "MinimalApiApp:non-existing-key",
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        // Act
        var result = await _cacheService.GetAsync<string>(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_WhenExceptionOccurs_ShouldReturnDefaultAndLogError()
    {
        // Arrange
        var key = "error-key";

        _mockDatabase.Setup(d => d.StringGetAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<CommandFlags>()))
            .ThrowsAsync(new Exception("Redis error"));

        // Act
        var result = await _cacheService.GetAsync<string>(key);

        // Assert
        result.Should().BeNull();
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task SetAsync_ShouldSerializeAndCacheValue()
    {
        // Arrange
        var key = "test-key";
        var value = "test-value";
        var expiration = TimeSpan.FromMinutes(10);

        _mockDatabase.Setup(d => d.StringSetAsync(
            "MinimalApiApp:test-key",
            It.IsAny<RedisValue>(),
            expiration,
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        await _cacheService.SetAsync(key, value, expiration);

        // Assert
        _mockDatabase.Verify(d => d.StringSetAsync(
            "MinimalApiApp:test-key",
            It.IsAny<RedisValue>(),
            expiration,
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()), 
            Times.Once);
    }

    [Fact]
    public async Task SetAsync_WithoutExpiration_ShouldUseDefaultExpiration()
    {
        // Arrange
        var key = "test-key";
        var value = "test-value";

        _mockDatabase.Setup(d => d.StringSetAsync(
            "MinimalApiApp:test-key",
            It.IsAny<RedisValue>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        await _cacheService.SetAsync(key, value);

        // Assert
        _mockDatabase.Verify(d => d.StringSetAsync(
            "MinimalApiApp:test-key",
            It.IsAny<RedisValue>(),
            TimeSpan.FromMinutes(5),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()), 
            Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveKeyFromCache()
    {
        // Arrange
        var key = "test-key";

        _mockDatabase.Setup(d => d.KeyDeleteAsync(
            "MinimalApiApp:test-key",
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        await _cacheService.RemoveAsync(key);

        // Assert
        _mockDatabase.Verify(d => d.KeyDeleteAsync(
            "MinimalApiApp:test-key",
            It.IsAny<CommandFlags>()), 
            Times.Once);
    }

    [Fact]
    public async Task RemoveByPrefixAsync_WithValidConnection_ShouldRemoveKeys()
    {
        // Arrange
        var prefix = "test-prefix";
        var keys = new[] 
        { 
            new RedisKey("MinimalApiApp:test-prefix:key1"),
            new RedisKey("MinimalApiApp:test-prefix:key2")
        };

        _mockConnectionMultiplexer.Setup(c => c.GetEndPoints(It.IsAny<bool>()))
            .Returns(new System.Net.EndPoint[] { new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, 6379) });
        
        _mockConnectionMultiplexer.Setup(c => c.GetServer(It.IsAny<System.Net.EndPoint>(), It.IsAny<object>()))
            .Returns(_mockServer.Object);

        // Create async enumerable from array
        async IAsyncEnumerable<RedisKey> GetKeysAsync()
        {
            foreach (var key in keys)
            {
                await Task.Yield();
                yield return key;
            }
        }

        _mockServer.Setup(s => s.KeysAsync(It.IsAny<int>(), It.IsAny<RedisValue>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<CommandFlags>()))
            .Returns(GetKeysAsync());

        _mockDatabase.Setup(d => d.KeyDeleteAsync(It.IsAny<RedisKey[]>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(2);

        // Act
        await _cacheService.RemoveByPrefixAsync(prefix);

        // Assert
        _mockDatabase.Verify(d => d.KeyDeleteAsync(
            It.Is<RedisKey[]>(k => k.Length == 2),
            It.IsAny<CommandFlags>()), 
            Times.Once);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Removed")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }
}
