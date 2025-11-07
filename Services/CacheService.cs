using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace MinimalApiApp.Services;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly IConnectionMultiplexer? _redisConnection;

    public RedisCacheService(
        IDistributedCache cache, 
        ILogger<RedisCacheService> logger,
        IConnectionMultiplexer? redisConnection = null)
    {
        _cache = cache;
        _logger = logger;
        _redisConnection = redisConnection;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var cachedData = await _cache.GetStringAsync(key);
            if (cachedData == null)
                return default;

            return JsonSerializer.Deserialize<T>(cachedData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving data from cache for key: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var serializedData = JsonSerializer.Serialize(value);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5)
            };

            await _cache.SetStringAsync(key, serializedData, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting data in cache for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _cache.RemoveAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing data from cache for key: {Key}", key);
        }
    }

    public async Task RemoveByPrefixAsync(string prefix)
    {
        try
        {
            if (_redisConnection == null)
            {
                _logger.LogWarning("Redis connection not available. Cannot remove keys by prefix: {Prefix}", prefix);
                return;
            }

            var database = _redisConnection.GetDatabase();
            var endpoints = _redisConnection.GetEndPoints();
            
            if (endpoints.Length == 0)
            {
                _logger.LogWarning("No Redis endpoints found. Cannot remove keys by prefix: {Prefix}", prefix);
                return;
            }

            var server = _redisConnection.GetServer(endpoints[0]);
            var pattern = $"MinimalApiApp:{prefix}";
            var keysToDelete = new List<RedisKey>();

            // Use SCAN to find all keys matching the pattern
            await foreach (var key in server.KeysAsync(pattern: pattern))
            {
                keysToDelete.Add(key);
            }

            if (keysToDelete.Count > 0)
            {
                // Delete keys in batches
                await database.KeyDeleteAsync(keysToDelete.ToArray());
                _logger.LogInformation("Removed {Count} cache keys with prefix: {Prefix}", keysToDelete.Count, prefix);
            }
            else
            {
                _logger.LogInformation("No cache keys found with prefix: {Prefix}", prefix);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache keys by prefix: {Prefix}", prefix);
        }
    }
}
