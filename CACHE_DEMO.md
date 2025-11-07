# Cache Management Demo - cURL Commands

This file contains cURL commands to demonstrate the `RemoveByPrefixAsync` functionality.

## Prerequisites

1. Start the application: `dotnet run`
2. Ensure Redis is running (or the app will gracefully degrade)

## Demo Steps

### Step 1: Create Cache Entries with Different Prefixes

```bash
# Create product cache entries
curl -X POST "http://localhost:5000/api/cache" \
  -H "Content-Type: application/json" \
  -d '{"key":"product:1","value":"Laptop","expirationMinutes":10}'

curl -X POST "http://localhost:5000/api/cache" \
  -H "Content-Type: application/json" \
  -d '{"key":"product:2","value":"Mouse","expirationMinutes":10}'

curl -X POST "http://localhost:5000/api/cache" \
  -H "Content-Type: application/json" \
  -d '{"key":"product:3","value":"Keyboard","expirationMinutes":10}'

curl -X POST "http://localhost:5000/api/cache" \
  -H "Content-Type: application/json" \
  -d '{"key":"products:all","value":"All Products List","expirationMinutes":10}'

# Create user cache entries
curl -X POST "http://localhost:5000/api/cache" \
  -H "Content-Type: application/json" \
  -d '{"key":"user:1","value":"John Doe","expirationMinutes":10}'

curl -X POST "http://localhost:5000/api/cache" \
  -H "Content-Type: application/json" \
  -d '{"key":"user:2","value":"Jane Smith","expirationMinutes":10}'

curl -X POST "http://localhost:5000/api/cache" \
  -H "Content-Type: application/json" \
  -d '{"key":"users:all","value":"All Users List","expirationMinutes":10}'
```

### Step 2: Verify Cache Entries Exist

```bash
# Check product entries
curl "http://localhost:5000/api/cache/product:1"
curl "http://localhost:5000/api/cache/product:2"
curl "http://localhost:5000/api/cache/product:3"
curl "http://localhost:5000/api/cache/products:all"

# Check user entries
curl "http://localhost:5000/api/cache/user:1"
curl "http://localhost:5000/api/cache/user:2"
curl "http://localhost:5000/api/cache/users:all"
```

### Step 3: Remove All Product Cache Entries by Prefix

**This is the RemoveByPrefixAsync in action!**

```bash
# Remove all entries starting with "product"
curl -X DELETE "http://localhost:5000/api/cache/prefix/product"
```

**Expected Response:**
```json
{
  "message": "All cache entries with prefix 'product' removed successfully",
  "prefix": "product",
  "timestamp": "2025-11-07T13:15:00.000Z"
}
```

### Step 4: Verify Product Entries Removed, User Entries Remain

```bash
# These should return 404 (removed)
curl "http://localhost:5000/api/cache/product:1"
curl "http://localhost:5000/api/cache/product:2"
curl "http://localhost:5000/api/cache/product:3"
curl "http://localhost:5000/api/cache/products:all"

# These should still exist
curl "http://localhost:5000/api/cache/user:1"
curl "http://localhost:5000/api/cache/user:2"
curl "http://localhost:5000/api/cache/users:all"
```

### Step 5: Remove All User Cache Entries by Prefix

```bash
# Remove all entries starting with "user"
curl -X DELETE "http://localhost:5000/api/cache/prefix/user"
```

### Step 6: Verify All Entries Removed

```bash
# All should return 404
curl "http://localhost:5000/api/cache/user:1"
curl "http://localhost:5000/api/cache/user:2"
curl "http://localhost:5000/api/cache/users:all"
```

## Additional Examples

### Remove Specific Category Caches

```bash
# Create category caches
curl -X POST "http://localhost:5000/api/cache" \
  -H "Content-Type: application/json" \
  -d '{"key":"category:electronics:products","value":"Electronics Products","expirationMinutes":10}'

curl -X POST "http://localhost:5000/api/cache" \
  -H "Content-Type: application/json" \
  -d '{"key":"category:books:products","value":"Books Products","expirationMinutes":10}'

# Remove only electronics category
curl -X DELETE "http://localhost:5000/api/cache/prefix/category:electronics"

# Verify
curl "http://localhost:5000/api/cache/category:electronics:products"  # 404
curl "http://localhost:5000/api/cache/category:books:products"        # Still exists
```

### Remove User-Specific Caches

```bash
# Create user-specific caches
curl -X POST "http://localhost:5000/api/cache" \
  -H "Content-Type: application/json" \
  -d '{"key":"user:123:profile","value":"User Profile","expirationMinutes":10}'

curl -X POST "http://localhost:5000/api/cache" \
  -H "Content-Type: application/json" \
  -d '{"key":"user:123:orders","value":"User Orders","expirationMinutes":10}'

curl -X POST "http://localhost:5000/api/cache" \
  -H "Content-Type: application/json" \
  -d '{"key":"user:456:profile","value":"Another User","expirationMinutes":10}'

# Remove all caches for user 123
curl -X DELETE "http://localhost:5000/api/cache/prefix/user:123"

# Verify
curl "http://localhost:5000/api/cache/user:123:profile"  # 404
curl "http://localhost:5000/api/cache/user:123:orders"   # 404
curl "http://localhost:5000/api/cache/user:456:profile"  # Still exists
```

## Use Cases

### 1. Invalidate Related Caches After Update

When a product is updated, invalidate all product-related caches:

```bash
# Update product (your existing endpoint)
curl -X PUT "http://localhost:5000/api/v1/products/1" \
  -H "Content-Type: application/json" \
  -d '{"name":"Updated Product","description":"New Desc","price":99.99,"stock":10}'

# Invalidate all product caches
curl -X DELETE "http://localhost:5000/api/cache/prefix/product"
```

### 2. Clear Session Caches

```bash
# Clear all session caches
curl -X DELETE "http://localhost:5000/api/cache/prefix/session"
```

### 3. Clear Tenant-Specific Caches (Multi-tenancy)

```bash
# Clear all caches for tenant ABC
curl -X DELETE "http://localhost:5000/api/cache/prefix/tenant:abc"
```

## Testing in Swagger UI

1. Open Swagger UI: http://localhost:5000/swagger
2. Look for **Cache Management** section
3. Try the following endpoints:
   - **POST /api/cache** - Set cache entry
   - **GET /api/cache/{key}** - Get cache entry
   - **DELETE /api/cache/{key}** - Remove single entry
   - **DELETE /api/cache/prefix/{prefix}** - Remove by prefix (NEW!)

## Monitoring Cache Operations

Check the application logs to see cache operations:

```bash
# Watch logs
tail -f Logs/log-*.txt

# You'll see entries like:
[INF] Removed 4 cache keys with prefix: product
[INF] Removed 3 cache keys with prefix: user
```

## Tips

1. **Use meaningful prefixes**: `product:`, `user:`, `category:`, etc.
2. **Be specific**: Use `product:123:` instead of just `product` if you want to target specific items
3. **Test first**: Always test with a specific prefix before running in production
4. **Monitor**: Check logs to see how many keys were removed

## Common Pitfalls to Avoid

❌ **Don't use too broad prefixes**:
```bash
curl -X DELETE "http://localhost:5000/api/cache/prefix/p"  # Too broad! Will remove product, person, payment, etc.
```

✅ **Use specific prefixes**:
```bash
curl -X DELETE "http://localhost:5000/api/cache/prefix/product"  # Better
curl -X DELETE "http://localhost:5000/api/cache/prefix/product:category:"  # Even more specific
```

❌ **Don't remove everything**:
```bash
curl -X DELETE "http://localhost:5000/api/cache/prefix/"  # Dangerous!
```

## Performance Notes

- Uses Redis SCAN command (production-safe, non-blocking)
- Batch deletion for performance
- Logs the number of keys removed
- Gracefully handles Redis unavailability

## Next Steps

Explore the comprehensive Cache Service Guide: `CACHE_SERVICE_GUIDE.md`
