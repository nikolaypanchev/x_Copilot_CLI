using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using MinimalApiApp.Services;
using Moq;
using System.Text;
using System.Text.Json;
using Xunit;

namespace MinimalApiApp.UnitTests.Services;

public class CacheServiceTests
{
    private readonly Mock<IDistributedCache> _mockCache;
    private readonly Mock<ILogger<RedisCacheService>> _mockLogger;
    private readonly RedisCacheService _cacheService;

    public CacheServiceTests()
    {
        _mockCache = new Mock<IDistributedCache>();
        _mockLogger = new Mock<ILogger<RedisCacheService>>();
        _cacheService = new RedisCacheService(_mockCache.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAsync_WithExistingKey_ShouldReturnDeserializedValue()
    {
        // Arrange
        var key = "test-key";
        var value = "test-value";
        var serialized = JsonSerializer.Serialize(value);
        var bytes = Encoding.UTF8.GetBytes(serialized);

        _mockCache.Setup(c => c.GetAsync(key, default))
            .ReturnsAsync(bytes);

        // Act
        var result = await _cacheService.GetAsync<string>(key);

        // Assert
        result.Should().Be(value);
        _mockCache.Verify(c => c.GetAsync(key, default), Times.Once);
    }

    [Fact]
    public async Task GetAsync_WithNonExistingKey_ShouldReturnDefault()
    {
        // Arrange
        var key = "non-existing-key";

        _mockCache.Setup(c => c.GetAsync(key, default))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _cacheService.GetAsync<string>(key);

        // Assert
        result.Should().BeNull();
        _mockCache.Verify(c => c.GetAsync(key, default), Times.Once);
    }

    [Fact]
    public async Task GetAsync_WhenExceptionOccurs_ShouldReturnDefaultAndLogError()
    {
        // Arrange
        var key = "error-key";

        _mockCache.Setup(c => c.GetAsync(key, default))
            .ThrowsAsync(new Exception("Cache error"));

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

        // Act
        await _cacheService.SetAsync(key, value, expiration);

        // Assert
        _mockCache.Verify(c => c.SetAsync(
            key,
            It.IsAny<byte[]>(),
            It.Is<DistributedCacheEntryOptions>(o => 
                o.AbsoluteExpirationRelativeToNow == expiration),
            default), Times.Once);
    }

    [Fact]
    public async Task SetAsync_WithoutExpiration_ShouldUseDefaultExpiration()
    {
        // Arrange
        var key = "test-key";
        var value = "test-value";

        // Act
        await _cacheService.SetAsync(key, value);

        // Assert
        _mockCache.Verify(c => c.SetAsync(
            key,
            It.IsAny<byte[]>(),
            It.Is<DistributedCacheEntryOptions>(o => 
                o.AbsoluteExpirationRelativeToNow == TimeSpan.FromMinutes(5)),
            default), Times.Once);
    }

    [Fact]
    public async Task SetAsync_WhenExceptionOccurs_ShouldLogError()
    {
        // Arrange
        var key = "error-key";
        var value = "test-value";

        _mockCache.Setup(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            default))
            .ThrowsAsync(new Exception("Cache error"));

        // Act
        await _cacheService.SetAsync(key, value);

        // Assert
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
    public async Task RemoveAsync_ShouldRemoveKeyFromCache()
    {
        // Arrange
        var key = "test-key";

        // Act
        await _cacheService.RemoveAsync(key);

        // Assert
        _mockCache.Verify(c => c.RemoveAsync(key, default), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_WhenExceptionOccurs_ShouldLogError()
    {
        // Arrange
        var key = "error-key";

        _mockCache.Setup(c => c.RemoveAsync(key, default))
            .ThrowsAsync(new Exception("Cache error"));

        // Act
        await _cacheService.RemoveAsync(key);

        // Assert
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
    public async Task RemoveByPrefixAsync_ShouldLogWarning()
    {
        // Arrange
        var prefix = "test-prefix";

        // Act
        await _cacheService.RemoveByPrefixAsync(prefix);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }
}
