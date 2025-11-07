# Testing Entity Framework Core Integration

## Quick Start

1. **Run the application:**
```bash
dotnet run
```

2. **Check database health:**
```bash
curl http://localhost:5000/api/health/database
```

## Expected Initial State

### Seed Data Loaded
- **2 Users** (John Doe, Jane Smith)
- **3 Products** (Laptop, Mouse, Keyboard)

## Test Scenarios

### 1. Verify Seed Data

#### Get All Users
```bash
curl http://localhost:5000/api/v1/users
```

**Expected Response:**
```json
[
  {
    "id": 1,
    "name": "John Doe",
    "email": "john.doe@example.com",
    "createdAt": "2025-01-01T00:00:00Z"
  },
  {
    "id": 2,
    "name": "Jane Smith",
    "email": "jane.smith@example.com",
    "createdAt": "2025-01-02T00:00:00Z"
  }
]
```

#### Get All Products
```bash
curl http://localhost:5000/api/v1/products
```

**Expected Response:**
```json
[
  {
    "id": 1,
    "name": "Laptop",
    "description": "High-performance laptop",
    "price": 1299.99,
    "stock": 10
  },
  {
    "id": 2,
    "name": "Mouse",
    "description": "Wireless mouse",
    "price": 29.99,
    "stock": 50
  },
  {
    "id": 3,
    "name": "Keyboard",
    "description": "Mechanical keyboard",
    "price": 89.99,
    "stock": 25
  }
]
```

### 2. Test Auto-Increment IDs

#### Create User (ID should be 3)
```bash
curl -X POST http://localhost:5000/api/v2/users \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Alice Johnson",
    "email": "alice@example.com"
  }'
```

**Expected Response:**
```json
{
  "version": "2.0",
  "data": {
    "id": 3,  // Auto-generated
    "name": "Alice Johnson",
    "email": "alice@example.com",
    "createdAt": "2025-11-07T10:14:35Z"  // Auto-generated
  },
  "message": "User created successfully",
  "timestamp": "2025-11-07T10:14:35Z"
}
```

### 3. Test Unique Email Constraint

#### Try to create user with existing email
```bash
curl -X POST http://localhost:5000/api/v1/users \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Another John",
    "email": "john.doe@example.com"
  }'
```

**Expected Response:**
```json
{
  "error": "Email is already in use",
  "statusCode": 400,
  "errors": {
    "Email": ["Email is already in use"]
  }
}
```

### 4. Test Database Queries

#### Get User by ID
```bash
curl http://localhost:5000/api/v1/users/1
```

**Expected Response:**
```json
{
  "id": 1,
  "name": "John Doe",
  "email": "john.doe@example.com",
  "createdAt": "2025-01-01T00:00:00Z"
}
```

#### Update User
```bash
curl -X PUT http://localhost:5000/api/v1/users/1 \
  -H "Content-Type: application/json" \
  -d '{
    "name": "John Updated",
    "email": "john.updated@example.com"
  }'
```

#### Verify Update
```bash
curl http://localhost:5000/api/v1/users/1
```

### 5. Test Caching

#### First Request (From Database)
```bash
curl -v http://localhost:5000/api/v1/products/1
```

Check logs - should see: "Retrieved product 1 from database"

#### Second Request (From Cache)
```bash
curl -v http://localhost:5000/api/v1/products/1
```

Check logs - should see: "Retrieved product 1 from cache"

### 6. Test Product Operations

#### Create Product (ID should be 4)
```bash
curl -X POST http://localhost:5000/api/v2/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Monitor",
    "description": "4K Display",
    "price": 399.99,
    "stock": 15
  }'
```

**Expected Response:**
```json
{
  "version": "2.0",
  "data": {
    "id": 4,  // Auto-generated
    "name": "Monitor",
    "description": "4K Display",
    "price": 399.99,
    "stock": 15
  },
  "message": "Product created successfully",
  "timestamp": "2025-11-07T10:14:35Z"
}
```

#### Update Product Stock
```bash
curl -X PUT http://localhost:5000/api/v1/products/1 \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Laptop",
    "description": "High-performance laptop",
    "price": 1299.99,
    "stock": 5
  }'
```

#### Delete Product
```bash
curl -X DELETE http://localhost:5000/api/v1/products/3
```

#### Verify Deletion
```bash
curl http://localhost:5000/api/v1/products/3
```

**Expected Response:**
```json
{
  "error": "Product with ID 3 not found",
  "statusCode": 404,
  "type": "NotFoundException"
}
```

### 7. Test V2 Enhanced Responses

#### Get All Users with Metadata
```bash
curl http://localhost:5000/api/v2/users
```

**Expected Response:**
```json
{
  "version": "2.0",
  "count": 3,  // Should show total count
  "data": [
    // All users
  ],
  "timestamp": "2025-11-07T10:14:35Z"
}
```

### 8. Health Checks

#### Database Health
```bash
curl http://localhost:5000/api/health/database
```

**Expected Response:**
```json
{
  "status": "healthy",
  "message": "Database is connected and accessible",
  "statistics": {
    "users": 3,
    "products": 4
  }
}
```

#### Redis Health
```bash
curl http://localhost:5000/api/health/redis
```

#### General API Health
```bash
curl http://localhost:5000/api/health
```

## PowerShell Tests

```powershell
# Get all users
$users = Invoke-RestMethod -Uri "http://localhost:5000/api/v1/users"
Write-Host "Total Users: $($users.Count)"

# Get database health
$health = Invoke-RestMethod -Uri "http://localhost:5000/api/health/database"
Write-Host "Database Status: $($health.status)"
Write-Host "User Count: $($health.statistics.users)"
Write-Host "Product Count: $($health.statistics.products)"

# Create new user
$newUser = @{
    name = "Bob Smith"
    email = "bob@example.com"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:5000/api/v2/users" `
    -Method Post `
    -Body $newUser `
    -ContentType "application/json"

Write-Host "Created User ID: $($response.data.id)"

# Verify creation
$users = Invoke-RestMethod -Uri "http://localhost:5000/api/v1/users"
Write-Host "Total Users Now: $($users.Count)"
```

## Comparison: Before vs After

### Before (In-Memory Lists)
```bash
# Create user - ID manually assigned
# No persistence between restarts
# No database constraints
```

### After (EF Core InMemory)
```bash
# Create user - ID auto-generated by database
# Seed data loaded on startup
# Unique constraints enforced
# Change tracking automatic
# Transaction support
```

## Validation Tests

### Invalid Product (Name too long)
```bash
curl -X POST http://localhost:5000/api/v1/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "This name is way too long and exceeds the maximum allowed length",
    "description": "Test",
    "price": 99.99,
    "stock": 10
  }'
```

**Expected:** Validation error (max 5 characters for name)

### Invalid Price
```bash
curl -X POST http://localhost:5000/api/v1/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test",
    "description": "Test",
    "price": 0,
    "stock": 10
  }'
```

**Expected:** Validation error (price must be > 0)

## Performance Test

### Test Caching Impact

```bash
# First request (database + cache write)
time curl http://localhost:5000/api/v1/products

# Second request (cache hit - should be faster)
time curl http://localhost:5000/api/v1/products

# Clear cache by updating a product
curl -X PUT http://localhost:5000/api/v1/products/1 \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Laptop",
    "description": "Updated",
    "price": 1299.99,
    "stock": 10
  }'

# Next request will hit database again
time curl http://localhost:5000/api/v1/products
```

## Swagger Testing

1. Navigate to: `http://localhost:5000/swagger`
2. Select **API V1** or **API V2**
3. Expand GET /api/v1/users
4. Click "Try it out" → "Execute"
5. Verify seed data is returned

## Expected Behavior

✅ **Seed data loaded** on application start
✅ **Auto-increment IDs** work correctly
✅ **Unique email constraint** enforced
✅ **Caching** reduces database queries
✅ **Logging** shows database vs cache hits
✅ **Validation** works with EF Core
✅ **All CRUD operations** functional
✅ **Health checks** return accurate stats

## Troubleshooting

### No seed data?
- Check logs for "Database initialized with seed data"
- Restart application

### Duplicate email error?
- Expected behavior - email must be unique
- Use different email

### Cache not working?
- Check Redis is running
- Check logs for cache errors

### IDs not auto-incrementing?
- Verify using EF Core repositories (not old in-memory lists)
- Check Program.cs registration

## Summary

✅ **Entity Framework Core** successfully integrated
✅ **InMemory database** with seed data
✅ **Auto-increment IDs** working
✅ **Unique constraints** enforced
✅ **Caching layer** operational
✅ **All endpoints** updated to use EF Core
✅ **Health checks** show database statistics
