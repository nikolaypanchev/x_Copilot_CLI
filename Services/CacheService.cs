using System.Text.Json;
using StackExchange.Redis;

namespace MinimalApiApp.Services;

public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redisConnection;
    private readonly IDatabase _database;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly string _instancePrefix;

    public RedisCacheService(
        IConnectionMultiplexer redisConnection,
        ILogger<RedisCacheService> logger,
        IConfiguration configuration)
    {
        _redisConnection = redisConnection;
        _database = redisConnection.GetDatabase();
        _logger = logger;
        _instancePrefix = configuration.GetValue<string>("Redis:InstanceName") ?? "MinimalApiApp:";
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var fullKey = $"{_instancePrefix}{key}";
            var cachedData = await _database.StringGetAsync(fullKey);
            
            if (!cachedData.HasValue || cachedData.IsNullOrEmpty)
                return default;

            return JsonSerializer.Deserialize<T>(cachedData.ToString());
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
            var fullKey = $"{_instancePrefix}{key}";
            var serializedData = JsonSerializer.Serialize(value);
            var expirationTime = expiration ?? TimeSpan.FromMinutes(5);

            await _database.StringSetAsync(fullKey, serializedData, expirationTime);
            
            _logger.LogDebug("Set cache key: {Key} with expiration: {Expiration}", key, expirationTime);
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
            var fullKey = $"{_instancePrefix}{key}";
            await _database.KeyDeleteAsync(fullKey);
            
            _logger.LogDebug("Removed cache key: {Key}", key);
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
            var endpoints = _redisConnection.GetEndPoints();
            
            if (endpoints.Length == 0)
            {
                _logger.LogWarning("No Redis endpoints found. Cannot remove keys by prefix: {Prefix}", prefix);
                return;
            }

            var server = _redisConnection.GetServer(endpoints[0]);
            var pattern = $"{_instancePrefix}{prefix}*";
            var keysToDelete = new List<RedisKey>();

            // Use SCAN to find all keys matching the pattern
            await foreach (var key in server.KeysAsync(pattern: pattern))
            {
                keysToDelete.Add(key);
            }

            if (keysToDelete.Count > 0)
            {
                // Delete keys in batches
                await _database.KeyDeleteAsync(keysToDelete.ToArray());
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
