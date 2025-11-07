# Redis Setup Guide

## Prerequisites

You need to have Redis running locally or use a remote Redis instance.

### Option 1: Run Redis Locally with Docker

```bash
docker run -d --name redis -p 6379:6379 redis:latest
```

### Option 2: Install Redis Locally

**Windows:**
- Download Redis from: https://github.com/microsoftarchive/redis/releases
- Or use WSL with Ubuntu and run: `sudo apt-get install redis-server`

**Linux/Mac:**
```bash
# Ubuntu/Debian
sudo apt-get install redis-server

# macOS with Homebrew
brew install redis
brew services start redis
```

## Configuration

The Redis configuration is in `appsettings.json`:

```json
{
  "Redis": {
    "ConnectionString": "localhost:6379",
    "InstanceName": "MinimalApiApp:"
  }
}
```

For production, update `appsettings.Production.json` or use environment variables:

```bash
Redis__ConnectionString=your-redis-server:6379
Redis__InstanceName=MinimalApiApp:
```

## Testing Redis Connection

Once the application is running, test the Redis connection:

```bash
curl http://localhost:5000/api/health/redis
```

Expected response:
```json
{
  "status": "healthy",
  "message": "Redis is connected and working"
}
```

## How Caching Works

### Cache Keys:
- **Users:** `user:{id}` for individual users, `users:all` for all users
- **Products:** `product:{id}` for individual products, `products:all` for all products

### Cache Duration:
- Default expiration: 5 minutes
- Can be configured per operation

### Cache Invalidation:
- **Create:** Invalidates `all` cache, creates individual cache
- **Update:** Invalidates both individual and `all` cache
- **Delete:** Invalidates both individual and `all` cache

## Monitoring Redis

### Using Redis CLI:

```bash
# Connect to Redis
redis-cli

# View all keys
KEYS *

# View specific key value
GET MinimalApiApp:user:1

# Monitor real-time commands
MONITOR

# Check cache stats
INFO stats
```

## Benefits

1. **Performance:** Reduces database/memory queries by caching frequently accessed data
2. **Scalability:** Shared cache across multiple application instances
3. **Resilience:** Graceful degradation - if Redis fails, app still works (fetches from source)
4. **Flexibility:** Easy to invalidate and refresh cache

## Notes

- Redis is optional - if not available, the app will log errors but continue working
- Cache misses will fetch data from the in-memory store
- All cache operations are async and non-blocking
