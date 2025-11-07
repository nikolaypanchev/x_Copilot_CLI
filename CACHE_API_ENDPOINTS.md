# Cache Management API Endpoints

## Overview

The Minimal API now includes comprehensive cache management endpoints that allow you to interact with Redis cache directly, including the new **RemoveByPrefixAsync** functionality.

## Available Endpoints

### 1. Set Cache Entry
**POST** `/api/cache`

Add or update a cache entry.

**Request Body:**
```json
{
  "key": "product:1",
  "value": "Laptop",
  "expirationMinutes": 10
}
```

**Response:**
```json
{
  "message": "Cache entry 'product:1' set successfully",
  "key": "product:1",
  "expirationMinutes": 10,
  "timestamp": "2025-11-07T13:15:00.000Z"
}
```

### 2. Get Cache Entry
**GET** `/api/cache/{key}`

Retrieve a cache entry by key.

**Example:** `GET /api/cache/product:1`

**Response (Success):**
```json
{
  "key": "product:1",
  "value": "Laptop",
  "timestamp": "2025-11-07T13:15:00.000Z"
}
```

**Response (Not Found):**
```json
{
  "message": "Cache entry 'product:1' not found",
  "key": "product:1",
  "timestamp": "2025-11-07T13:15:00.000Z"
}
```

### 3. Remove Single Cache Entry
**DELETE** `/api/cache/{key}`

Remove a specific cache entry.

**Example:** `DELETE /api/cache/product:1`

**Response:**
```json
{
  "message": "Cache entry 'product:1' removed successfully",
  "timestamp": "2025-11-07T13:15:00.000Z"
}
```

### 4. Remove Cache Entries by Prefix ⭐ NEW!
**DELETE** `/api/cache/prefix/{prefix}`

Remove all cache entries that start with the specified prefix.

**Example:** `DELETE /api/cache/prefix/product`

This will remove:
- `product:1`
- `product:2`
- `product:3`
- `products:all`
- Any other key starting with `product`

**Response:**
```json
{
  "message": "All cache entries with prefix 'product' removed successfully",
  "prefix": "product",
  "timestamp": "2025-11-07T13:15:00.000Z"
}
```

## Quick Start Guide

### Using cURL

```bash
# 1. Set cache entries
curl -X POST "http://localhost:5000/api/cache" \
  -H "Content-Type: application/json" \
  -d '{"key":"product:1","value":"Laptop","expirationMinutes":10}'

curl -X POST "http://localhost:5000/api/cache" \
  -H "Content-Type: application/json" \
  -d '{"key":"product:2","value":"Mouse","expirationMinutes":10}'

curl -X POST "http://localhost:5000/api/cache" \
  -H "Content-Type: application/json" \
  -d '{"key":"user:1","value":"John Doe","expirationMinutes":10}'

# 2. Get cache entry
curl "http://localhost:5000/api/cache/product:1"

# 3. Remove all product caches by prefix
curl -X DELETE "http://localhost:5000/api/cache/prefix/product"

# 4. Verify products removed but users remain
curl "http://localhost:5000/api/cache/product:1"  # Returns 404
curl "http://localhost:5000/api/cache/user:1"     # Still exists
```

### Using PowerShell

```powershell
# Run the automated demo script
.\demo-cache-prefix.ps1
```

### Using Swagger UI

1. Navigate to: http://localhost:5000/swagger
2. Find the **Cache Management** section
3. Try out the endpoints interactively

## Use Cases

### 1. Invalidate Product Caches After Update

```bash
# User updates a product
PUT /api/v1/products/1

# Invalidate all product-related caches
DELETE /api/cache/prefix/product
```

### 2. Clear User Session Caches

```bash
# Clear all caches for a specific user
DELETE /api/cache/prefix/user:123:
```

### 3. Clear Category Caches

```bash
# Clear all electronics category caches
DELETE /api/cache/prefix/category:electronics
```

### 4. Multi-Tenancy Cache Isolation

```bash
# Clear all caches for tenant "ABC"
DELETE /api/cache/prefix/tenant:abc:
```

## Key Naming Conventions

Follow these patterns for consistent cache keys:

| Pattern | Example | Use Case |
|---------|---------|----------|
| `{entity}:{id}` | `product:123` | Single entity cache |
| `{entity}:all` | `products:all` | Collection cache |
| `{entity}:{id}:{sub}` | `user:123:orders` | Nested entity cache |
| `{scope}:{entity}:{id}` | `tenant:abc:user:123` | Multi-tenant cache |
| `{type}:{category}:{id}` | `cache:session:xyz` | Categorized cache |

## Implementation Details

### How It Works

The **RemoveByPrefixAsync** method:

1. Connects to Redis using `IConnectionMultiplexer`
2. Uses the `SCAN` command to find matching keys (pattern: `prefix*`)
3. Collects all matching keys
4. Deletes them in a single batch operation
5. Logs the number of keys removed

### Code Example

```csharp
// In your service
public async Task UpdateProductAsync(int id, Product product)
{
    // Update in database
    await _repository.UpdateAsync(id, product);
    
    // Invalidate all product caches
    await _cacheService.RemoveByPrefixAsync("product");
}
```

### Performance

- **SCAN** command is production-safe (non-blocking)
- Batch deletion for efficiency
- Fully asynchronous
- Handles errors gracefully

### Logging

Check application logs for cache operations:

```
[INF] Removed 4 cache keys with prefix: product
[WRN] Redis connection not available. Cannot remove keys by prefix: user
[ERR] Error removing cache keys by prefix: category
```

## Testing

### Automated Demo

Run the PowerShell demo script:

```powershell
# Start the application first
dotnet run

# In another terminal, run the demo
.\demo-cache-prefix.ps1
```

### Manual Testing

Follow the step-by-step guide in `CACHE_DEMO.md`

### Integration Tests

The cache service includes comprehensive tests:

```bash
# Run all tests
dotnet test

# Run cache service tests only
dotnet test --filter "FullyQualifiedName~CacheServiceTests"
```

## Best Practices

### ✅ DO

- Use meaningful, specific prefixes
- Include colons (`:`) as separators for clarity
- Test prefix patterns before using in production
- Monitor logs to verify expected behavior
- Use appropriate expiration times

### ❌ DON'T

- Use overly broad prefixes (e.g., `p` instead of `product`)
- Remove with empty prefix (removes everything!)
- Forget to handle Redis connection failures
- Cache sensitive data without encryption
- Use prefixes without a naming convention

## Troubleshooting

### Redis Not Available

The API gracefully handles Redis unavailability:

```json
{
  "message": "All cache entries with prefix 'product' removed successfully",
  "prefix": "product",
  "timestamp": "2025-11-07T13:15:00.000Z"
}
```

Check logs:
```
[WRN] Redis connection not available. Cannot remove keys by prefix: product
```

### Keys Not Being Removed

**Possible causes:**
1. Instance name prefix mismatch
2. Pattern doesn't match actual keys
3. Redis authentication issues

**Solution:** Include instance name in prefix if configured:

```bash
# If instance name is "MinimalApiApp:"
DELETE /api/cache/prefix/MinimalApiApp:product
```

## API Documentation

Full Swagger documentation available at:
- Development: http://localhost:5000/swagger
- Swagger JSON: http://localhost:5000/swagger/v1/swagger.json

## Additional Resources

- **Cache Service Guide**: `CACHE_SERVICE_GUIDE.md` - Comprehensive implementation guide
- **Demo Script**: `demo-cache-prefix.ps1` - Automated demonstration
- **cURL Examples**: `CACHE_DEMO.md` - Manual testing guide
- **Redis Setup**: `REDIS_SETUP.md` - Redis configuration guide

## Summary

The new cache management endpoints provide a powerful way to manage Redis cache directly from the API, with the **RemoveByPrefixAsync** functionality enabling efficient invalidation of related cache entries. This is especially useful for:

- Cache invalidation after updates
- Session management
- Multi-tenant applications
- Category-based cache clearing
- Testing and debugging

Try it now:
1. `dotnet run`
2. Open http://localhost:5000/swagger
3. Navigate to **Cache Management** section
4. Start testing! 🚀
