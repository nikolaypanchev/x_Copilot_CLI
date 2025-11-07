# Minimal API Application

A production-ready C# .NET 8 Minimal API application with comprehensive features including API versioning, caching, logging, validation, and complete test coverage.

## 🚀 Features

### Core Features
- ✅ **RESTful API** - User and Product management endpoints
- ✅ **API Versioning** - Multiple API versions (v1, v2) using Asp.Versioning
- ✅ **Entity Framework Core** - In-Memory database for development
- ✅ **FluentValidation** - Request validation with custom middleware
- ✅ **Direct Redis Caching** - Pure Redis implementation using StackExchange.Redis (no IDistributedCache wrapper)
- ✅ **Cache Management API** - Full CRUD operations for cache entries with prefix-based removal
- ✅ **Serilog Logging** - Structured logging to file and console
- ✅ **Health Checks** - Database and Redis health endpoints
- ✅ **Swagger/OpenAPI** - Interactive API documentation with versioning support
- ✅ **Error Handling** - Global error handling middleware
- ✅ **Polly Resilience** - Retry policies for transient failures

### Architecture Patterns
- 🏗️ **Repository Pattern** - Data access abstraction
- 🏗️ **Unit of Work Pattern** - Transaction management
- 🏗️ **Dependency Injection** - Built-in .NET DI container
- 🏗️ **Middleware Pipeline** - Custom middleware for validation, logging, and resilience

## 📁 Project Structure

```
MinimalApiApp/
├── Models/                      # Domain entities
│   ├── CacheEntry.cs
│   ├── Product.cs
│   └── User.cs
├── Services/                    # Business logic and interfaces
│   ├── CacheService.cs
│   ├── IProductService.cs
│   ├── IUserService.cs
│   ├── ProductService.cs
│   └── UserService.cs
├── Data/                        # Database context and repositories
│   ├── ApplicationDbContext.cs
│   ├── EfRepositories.cs
│   ├── IUnitOfWork.cs
│   └── UnitOfWork.cs
├── Middleware/                  # Custom middleware components
│   ├── ErrorHandlingMiddleware.cs
│   ├── LoggingMiddleware.cs
│   ├── ResilienceMiddleware.cs
│   ├── ResiliencePolicies.cs
│   └── ValidationMiddleware.cs
├── Validators/                  # FluentValidation validators
│   ├── ProductValidator.cs
│   └── UserValidator.cs
├── Configuration/               # Configuration classes
│   └── SwaggerConfiguration.cs
├── IntegrationTests/           # Integration tests with WebApplicationFactory
│   ├── CustomWebApplicationFactory.cs
│   ├── ProductApiTests.cs
│   ├── UserApiTests.cs
│   └── HealthCheckTests.cs
├── UnitTests/                  # Unit tests with xUnit and Moq
│   ├── Models/
│   │   ├── ProductTests.cs
│   │   └── UserTests.cs
│   ├── Validators/
│   │   ├── ProductValidatorTests.cs
│   │   └── UserValidatorTests.cs
│   └── Services/
│       ├── ProductServiceTests.cs
│       └── CacheServiceTests.cs
└── Program.cs                  # Application entry point
```

## 🔧 Setup Instructions

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Redis](https://redis.io/download) - **Required** for caching (application will fail to start without Redis)

### 1. Start Redis

```bash
# Windows (if installed)
redis-server

# macOS (using Homebrew)
brew services start redis

# Linux
sudo systemctl start redis

# Docker
docker run -d -p 6379:6379 redis:latest
```

### 2. Clone and Build

```bash
cd MinimalApiApp
dotnet restore
dotnet build
```

### 3. Run the Application

```bash
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger`

### 4. Run Tests

**All Tests:**
```bash
dotnet test
```

**Unit Tests Only:**
```bash
dotnet test UnitTests/MinimalApiApp.UnitTests.csproj
```

**Integration Tests Only:**
```bash
dotnet test IntegrationTests/MinimalApiApp.IntegrationTests.csproj
```

## 📊 API Endpoints

### Users (Version 1)
- `GET /api/v1/users` - Get all users
- `GET /api/v1/users/{id}` - Get user by ID
- `POST /api/v1/users` - Create new user
- `PUT /api/v1/users/{id}` - Update user
- `DELETE /api/v1/users/{id}` - Delete user

### Users (Version 2)
- `GET /api/v2/users` - Get all users with enhanced response format

### Products (Version 1)
- `GET /api/v1/products` - Get all products
- `GET /api/v1/products/{id}` - Get product by ID
- `POST /api/v1/products` - Create new product
- `PUT /api/v1/products/{id}` - Update product
- `DELETE /api/v1/products/{id}` - Delete product

### Products (Version 2)
- `GET /api/v2/products` - Get all products with enhanced response format

### Cache Management ⭐ NEW!
- `POST /api/cache` - Set cache entry
- `GET /api/cache/{key}` - Get cache entry
- `DELETE /api/cache/{key}` - Remove single cache entry
- `DELETE /api/cache/prefix/{prefix}` - **Remove all cache entries by prefix** (uses Redis SCAN)

### Health Checks
- `GET /api/health` - Overall application health
- `GET /api/health/database` - Database connectivity check
- `GET /api/health/redis` - Redis connectivity check

## 🧪 Testing

### Test Coverage
- **69 Total Tests Passing** ✅
  - **47 Unit Tests** - Models, Validators, Services (with direct Redis mocking)
  - **22 Integration Tests** - Full API endpoint testing

### Testing Stack
- **xUnit** - Test framework
- **Moq** - Mocking framework (mocking IConnectionMultiplexer, IDatabase)
- **FluentAssertions** - Readable assertions
- **WebApplicationFactory** - Integration testing

### Test Categories

**Unit Tests:**
- Model property initialization and validation
- FluentValidation rule testing
- Service layer with mocked dependencies
- Direct Redis operations with mocked IConnectionMultiplexer
- Cache prefix removal functionality

**Integration Tests:**
- End-to-end API endpoint testing
- Request/response validation
- HTTP status code verification
- Database interaction testing
- Validation middleware testing

## 🔐 Validation Rules

### Product Validation
- **Name**: Required, max 5 characters
- **Description**: Required, max 10 characters
- **Price**: Must be greater than 0
- **Stock**: Must be greater than or equal to 0
- **ID**: Must be unique (for updates)

### User Validation
- **Name**: Required, not empty
- **Email**: Required, valid email format, must be unique
- **CreatedAt**: Auto-generated (UTC)

## 🎯 Middleware Pipeline

1. **Error Handling** - Global exception handler
2. **Resilience** - Retry logic for transient failures
3. **Validation** - Request validation before endpoint execution
4. **Logging** - Request/response logging with correlation IDs

## 📝 Logging

Logs are written to:
- **Console** - All log levels (Development)
- **File** - `Logs/log-YYYYMMDD.txt` (Rolling daily)

Log format includes:
- Timestamp
- Log level
- Request ID
- Message
- Exception details (if any)

## 🔄 Caching Strategy

### Direct Redis Implementation
- **Pure Redis** - Uses StackExchange.Redis directly (no Microsoft IDistributedCache wrapper)
- **Full Redis Access** - Direct access to all Redis commands and features
- **Cache Keys** - Prefixed by instance name (e.g., `MinimalApiApp:users:all`, `MinimalApiApp:product:1`)
- **Expiration** - 5 minutes default, configurable per entry
- **Cache Invalidation** - Automatic on create/update/delete operations
- **Prefix-Based Removal** - Uses Redis SCAN command for efficient bulk deletion

### Cache API Features
```bash
# Set cache entry
curl -X POST "http://localhost:5000/api/cache" \
  -H "Content-Type: application/json" \
  -d '{"key":"product:1","value":"Laptop","expirationMinutes":10}'

# Get cache entry
curl "http://localhost:5000/api/cache/product:1"

# Remove by prefix (removes all matching keys)
curl -X DELETE "http://localhost:5000/api/cache/prefix/product"
```

### Advanced Redis Features Available
With direct Redis access, you can now use:
- **Pub/Sub** - Real-time messaging
- **Transactions** - Atomic operations
- **Lua Scripts** - Server-side scripting
- **Lists, Sets, Hashes** - Advanced data structures
- **Key Expiration Events** - Notifications

See [REDIS_DIRECT_IMPLEMENTATION.md](REDIS_DIRECT_IMPLEMENTATION.md) for details.

## 📚 Additional Documentation

### Core Documentation
- [API Versioning Guide](API_VERSIONING.md)
- [API Versioning Examples](API_VERSIONING_EXAMPLES.md)
- [EF Core Setup Guide](EF_CORE_GUIDE.md)
- [EF Core Testing Guide](EF_CORE_TESTING.md)
- [Logging Configuration](LOGGING_GUIDE.md)
- [Folder Organization](FOLDER_ORGANIZATION.md)

### Redis & Caching Documentation ⭐
- [Redis Direct Implementation](REDIS_DIRECT_IMPLEMENTATION.md) - Pure Redis implementation guide
- [Redis Setup Guide](REDIS_SETUP.md) - Redis installation and configuration
- [Cache Service Guide](CACHE_SERVICE_GUIDE.md) - Complete cache service documentation
- [Cache API Endpoints](CACHE_API_ENDPOINTS.md) - Cache management API reference
- [Cache Demo Guide](CACHE_DEMO.md) - Testing guide with cURL examples
- [RemoveByPrefix Summary](REMOVEBYPREFIX_SUMMARY.md) - Prefix-based cache removal

## 🛠️ Technologies Used

- **.NET 8** - Framework
- **ASP.NET Core** - Web framework
- **Entity Framework Core** - ORM
- **FluentValidation** - Validation
- **Serilog** - Logging
- **StackExchange.Redis** - Direct Redis client (no IDistributedCache wrapper)
- **Polly** - Resilience
- **Swagger/Swashbuckle** - API documentation
- **Asp.Versioning** - API versioning
- **xUnit** - Testing framework
- **Moq** - Mocking framework
- **FluentAssertions** - Assertion library

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## 📄 License

This project is licensed under the MIT License.

## 🎓 Learning Resources

This project demonstrates:
- Clean Architecture principles
- SOLID principles
- Repository and Unit of Work patterns
- Dependency Injection
- Async/Await best practices
- Comprehensive testing strategies
- API versioning strategies
- Middleware development
- **Direct Redis integration** (no abstraction layers)
- **Advanced caching strategies** with prefix-based invalidation
- Structured logging

## 🎯 Key Features Highlights

### 1. Direct Redis Integration
No Microsoft IDistributedCache abstraction - pure StackExchange.Redis for:
- Better performance (fewer layers)
- Full access to Redis commands
- Advanced features (pub/sub, transactions, Lua scripts)
- Direct control over serialization

### 2. Cache Management API
Complete cache CRUD operations exposed via REST API:
- Set/Get cache entries
- Remove individual entries
- **Remove by prefix** (bulk deletion using Redis SCAN)
- Perfect for testing and debugging

### 3. Production-Ready Architecture
- Comprehensive error handling
- Request/response logging with correlation IDs
- Health checks for all dependencies
- Retry policies for transient failures
- Full test coverage

Perfect for learning modern .NET development practices! 🚀
