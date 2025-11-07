# Minimal API Application

A production-ready C# .NET 8 Minimal API application with comprehensive features including API versioning, caching, logging, validation, and complete test coverage.

## 🚀 Features

### Core Features
- ✅ **RESTful API** - User and Product management endpoints
- ✅ **API Versioning** - Multiple API versions (v1, v2) using Asp.Versioning
- ✅ **Entity Framework Core** - In-Memory database for development
- ✅ **FluentValidation** - Request validation with custom middleware
- ✅ **Redis Caching** - Distributed caching with StackExchange.Redis
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
- [Redis](https://redis.io/download) (optional, for caching features)

### 1. Clone and Build

```bash
cd MinimalApiApp
dotnet restore
dotnet build
```

### 2. Run the Application

```bash
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger`

### 3. Run Tests

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

### Health Checks
- `GET /api/health` - Overall application health
- `GET /api/health/database` - Database connectivity check
- `GET /api/health/redis` - Redis connectivity check

## 🧪 Testing

### Test Coverage
- **78 Total Tests** - 100% Passing ✅
  - **56 Unit Tests** - Models, Validators, Services
  - **22 Integration Tests** - Full API endpoint testing

### Testing Stack
- **xUnit** - Test framework
- **Moq** - Mocking framework
- **FluentAssertions** - Readable assertions
- **WebApplicationFactory** - Integration testing

### Test Categories

**Unit Tests:**
- Model property initialization and validation
- FluentValidation rule testing
- Service layer with mocked dependencies
- Cache service operations

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

- **Distributed Cache** - Redis for production
- **Cache Keys** - Prefixed by entity type (e.g., `users:all`, `product:1`)
- **Expiration** - 5 minutes default
- **Cache Invalidation** - Automatic on create/update/delete operations

## 📚 Additional Documentation

- [API Versioning Guide](API_VERSIONING.md)
- [API Versioning Examples](API_VERSIONING_EXAMPLES.md)
- [EF Core Setup Guide](EF_CORE_GUIDE.md)
- [EF Core Testing Guide](EF_CORE_TESTING.md)
- [Logging Configuration](LOGGING_GUIDE.md)
- [Redis Setup Guide](REDIS_SETUP.md)
- [Folder Organization](FOLDER_ORGANIZATION.md)

## 🛠️ Technologies Used

- **.NET 8** - Framework
- **ASP.NET Core** - Web framework
- **Entity Framework Core** - ORM
- **FluentValidation** - Validation
- **Serilog** - Logging
- **Redis** - Caching
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
- Caching strategies
- Structured logging

Perfect for learning modern .NET development practices! 🚀
