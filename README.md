# Minimal API Application

A C# Minimal API application with User and Product services.

## Setup Instructions

Since PowerShell 6+ is not available, please follow these steps:

### 1. Create Directory Structure

Run this in Command Prompt:
```cmd
mkdir MinimalApiApp
mkdir MinimalApiApp\Models
mkdir MinimalApiApp\Services
mkdir MinimalApiApp.Tests
```

### 2. Copy Files

Copy all the files from the project_files directory to their respective locations.

### 3. Build and Run

```cmd
cd MinimalApiApp
dotnet restore
dotnet build
dotnet run
```

### 4. Run Tests

```cmd
cd MinimalApiApp.Tests
dotnet test
```

## API Endpoints

### Users
- GET /api/users - Get all users
- GET /api/users/{id} - Get user by ID
- POST /api/users - Create new user
- PUT /api/users/{id} - Update user
- DELETE /api/users/{id} - Delete user

### Products
- GET /api/products - Get all products
- GET /api/products/{id} - Get product by ID
- POST /api/products - Create new product
- PUT /api/products/{id} - Update product
- DELETE /api/products/{id} - Delete product

## Swagger UI

When running in development mode, access Swagger UI at: https://localhost:5001/swagger
