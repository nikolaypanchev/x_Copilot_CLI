# Entity Framework Core Integration

## Overview

The application now uses **Entity Framework Core** with an **InMemory database** for data persistence. This provides a real database experience without requiring external database setup.

## What Changed

### Before (In-Memory Lists)
```csharp
private readonly List<User> _users = new();
private readonly List<Product> _products = new();
```

### After (EF Core + InMemory Database)
```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
}
```

## Benefits

✅ **Real Database Features**
- Primary keys with auto-increment
- Unique constraints on email
- Database seeding
- Change tracking
- Async queries
- Query optimization

✅ **Easy Migration Path**
- Can switch to SQL Server, PostgreSQL, MySQL with minimal code changes
- Just change the connection string and provider

✅ **Production Ready**
- Proper repository pattern
- Transaction support via SaveChanges
- Concurrency handling

## Database Schema

### Users Table
| Column | Type | Constraints |
|--------|------|-------------|
| Id | int | Primary Key, Auto-increment |
| Name | string(100) | Required |
| Email | string(255) | Required, Unique |
| CreatedAt | DateTime | Required, Default UTC |

### Products Table
| Column | Type | Constraints |
|--------|------|-------------|
| Id | int | Primary Key, Auto-increment |
| Name | string(100) | Required |
| Description | string(500) | Optional |
| Price | decimal(18,2) | Required |
| Stock | int | Required |

## Seed Data

### Initial Users
1. **John Doe** - john.doe@example.com
2. **Jane Smith** - jane.smith@example.com

### Initial Products
1. **Laptop** - $1,299.99 (10 in stock)
2. **Mouse** - $29.99 (50 in stock)
3. **Keyboard** - $89.99 (25 in stock)

## Architecture

```
┌─────────────────────────────────────────┐
│          API Endpoints (V1/V2)          │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│      Service Layer (Business Logic)     │
│   - UserService / ProductService        │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│      Repository Pattern (EF Core)       │
│   - EfUserRepository                    │
│   - EfProductRepository                 │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│      ApplicationDbContext               │
│   (EF Core DbContext)                   │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│      InMemory Database Provider         │
│   (In-Memory storage)                   │
└─────────────────────────────────────────┘
```

## Key Components

### 1. ApplicationDbContext
**Location:** `ApplicationDbContext.cs`

Main database context with:
- DbSet for Users and Products
- Model configuration (constraints, indexes)
- Database seeding

```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure entities
        // Seed initial data
    }
}
```

### 2. EF Core Repositories
**Location:** `EfRepositories.cs`

- **EfUserRepository**: Implements IUserService using EF Core
- **EfProductRepository**: Implements IProductRepository using EF Core

Features:
- ✅ Async database operations
- ✅ Redis caching integration
- ✅ Polly retry policies
- ✅ Logging
- ✅ Exception handling

### 3. Service Registration
**Location:** `Program.cs`

```csharp
// Register DbContext with InMemory provider
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("MinimalApiDb"));

// Register repositories as scoped (per request)
builder.Services.AddScoped<IUserService, EfUserRepository>();
builder.Services.AddScoped<IProductRepository, EfProductRepository>();
```

## Scoped vs Singleton

### Important Change
EF Core DbContext **must be scoped** (per-request lifetime):
- ❌ Before: `AddSingleton` - One instance for entire app
- ✅ Now: `AddScoped` - New instance per HTTP request

This prevents:
- Thread safety issues
- Stale data
- Memory leaks

## Features

### 1. Auto-Increment IDs
IDs are now automatically generated:
```csharp
var user = new User { Name = "Alice", Email = "alice@example.com" };
await context.Users.Add(user);
await context.SaveChanges();
// user.Id is now automatically set (e.g., 3)
```

### 2. Unique Email Constraint
Email addresses must be unique:
```csharp
// This will throw an exception if email already exists
modelBuilder.Entity<User>()
    .HasIndex(e => e.Email)
    .IsUnique();
```

### 3. Default Values
CreatedAt is automatically set:
```csharp
entity.Property(e => e.CreatedAt)
    .HasDefaultValueSql("GETUTCDATE()");
```

### 4. Query Optimization
```csharp
// EF Core generates optimized SQL
var users = await context.Users
    .Where(u => u.Email.Contains("@example.com"))
    .OrderBy(u => u.Name)
    .ToListAsync();
```

### 5. Change Tracking
```csharp
var user = await context.Users.FindAsync(1);
user.Name = "Updated Name";
// EF Core tracks the change
await context.SaveChanges(); // Only updates modified fields
```

## Testing

### Health Check
```bash
curl http://localhost:5000/api/health/database
```

**Response:**
```json
{
  "status": "healthy",
  "message": "Database is connected and accessible",
  "statistics": {
    "users": 2,
    "products": 3
  }
}
```

### Create User
```bash
curl -X POST http://localhost:5000/api/v1/users \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Alice",
    "email": "alice@example.com"
  }'
```

### Get All Users (Should include seed data)
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

## Switching to a Real Database

### SQL Server
```csharp
// Install: Microsoft.EntityFrameworkCore.SqlServer
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
```

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MinimalApiDb;Trusted_Connection=True;"
  }
}
```

### PostgreSQL
```csharp
// Install: Npgsql.EntityFrameworkCore.PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
```

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=MinimalApiDb;Username=postgres;Password=password"
  }
}
```

### MySQL
```csharp
// Install: Pomelo.EntityFrameworkCore.MySql
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(configuration.GetConnectionString("DefaultConnection"))
    ));
```

## Migrations (For Real Databases)

### Create Migration
```bash
dotnet ef migrations add InitialCreate
```

### Apply Migration
```bash
dotnet ef database update
```

### Remove Migration
```bash
dotnet ef migrations remove
```

## Advantages Over In-Memory Lists

| Feature | In-Memory Lists | EF Core InMemory |
|---------|----------------|------------------|
| Persistence | ❌ Lost on restart | ❌ Lost on restart |
| Relationships | ❌ Manual | ✅ Navigation properties |
| Queries | ❌ LINQ to Objects | ✅ LINQ to Entities |
| Constraints | ❌ Manual validation | ✅ Database constraints |
| Transactions | ❌ No | ✅ Yes (SaveChanges) |
| Async | ❌ Fake async | ✅ True async |
| Change Tracking | ❌ Manual | ✅ Automatic |
| Migration Path | ❌ Hard | ✅ Easy (change provider) |

## Performance

### Caching Layer
All database queries are cached with Redis:
1. Check Redis cache first
2. If not in cache, query database
3. Store result in Redis (5 min TTL)
4. Return data

### Retry Policy
All database operations have automatic retry with exponential backoff:
- Retry 1: 100ms delay
- Retry 2: 200ms delay
- Retry 3: 400ms delay

## Limitations of InMemory Database

⚠️ **Data is lost when application restarts**
⚠️ **No SQL profiling or query logs**
⚠️ **Some SQL features not supported** (triggers, stored procedures)
⚠️ **Not suitable for production** (use SQL Server, PostgreSQL, etc.)

## Best Practices

### 1. Always Use Async
```csharp
// ✅ Good
var users = await context.Users.ToListAsync();

// ❌ Bad
var users = context.Users.ToList();
```

### 2. Use AsNoTracking for Read-Only Queries
```csharp
// Better performance for read-only data
var users = await context.Users
    .AsNoTracking()
    .ToListAsync();
```

### 3. Dispose DbContext Properly
```csharp
// Scoped lifetime handles this automatically
// Don't manually dispose in middleware/services
```

### 4. Use Transactions When Needed
```csharp
using var transaction = await context.Database.BeginTransactionAsync();
try
{
    // Multiple operations
    await context.SaveChangesAsync();
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

## Troubleshooting

### Issue: DbContext disposed error
**Solution:** Ensure services are registered as `Scoped`, not `Singleton`

### Issue: Duplicate key error
**Solution:** Check unique constraints (email must be unique)

### Issue: No data in database
**Solution:** Ensure `EnsureCreated()` is called on startup

### Issue: Changes not persisted
**Solution:** Call `SaveChangesAsync()` after modifications

## Summary

✅ **Entity Framework Core** integrated with InMemory database
✅ **Seed data** automatically loaded on startup
✅ **Database health check** endpoint added
✅ **Production-ready architecture** - easy to switch to real database
✅ **Caching + Retry policies** for reliability and performance
✅ **Proper scoping** for DbContext lifecycle
