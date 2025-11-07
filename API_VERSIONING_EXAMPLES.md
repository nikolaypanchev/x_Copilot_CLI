# API Versioning - Quick Test Examples

## Prerequisites
Make sure the application is running: `dotnet run`

## Testing with cURL

### Version 1 Endpoints

#### Get All Users (V1)
```bash
curl -X GET http://localhost:5000/api/v1/users
```

#### Get User by ID (V1)
```bash
curl -X GET http://localhost:5000/api/v1/users/1
```

#### Create User (V1)
```bash
curl -X POST http://localhost:5000/api/v1/users \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Alice",
    "email": "alice@example.com"
  }'
```

#### Update User (V1)
```bash
curl -X PUT http://localhost:5000/api/v1/users/1 \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Alice Updated",
    "email": "alice.updated@example.com"
  }'
```

#### Delete User (V1)
```bash
curl -X DELETE http://localhost:5000/api/v1/users/1
```

---

### Version 2 Endpoints (Enhanced)

#### Get All Users (V2)
```bash
curl -X GET http://localhost:5000/api/v2/users
```
**Response:**
```json
{
  "version": "2.0",
  "count": 2,
  "data": [...],
  "timestamp": "2025-11-07T10:00:00Z"
}
```

#### Get User by ID (V2)
```bash
curl -X GET http://localhost:5000/api/v2/users/1
```

#### Create User (V2)
```bash
curl -X POST http://localhost:5000/api/v2/users \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Bob",
    "email": "bob@example.com"
  }'
```
**Response:**
```json
{
  "version": "2.0",
  "data": {
    "id": 1,
    "name": "Bob",
    "email": "bob@example.com",
    "createdAt": "2025-11-07T10:00:00Z"
  },
  "message": "User created successfully",
  "timestamp": "2025-11-07T10:00:00Z"
}
```

#### Update User (V2)
```bash
curl -X PUT http://localhost:5000/api/v2/users/1 \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Bob Updated",
    "email": "bob.updated@example.com"
  }'
```

#### Delete User (V2)
```bash
curl -X DELETE http://localhost:5000/api/v2/users/1
```
**Response:**
```json
{
  "version": "2.0",
  "message": "User deleted successfully",
  "timestamp": "2025-11-07T10:00:00Z"
}
```

---

## Testing Version Selection Methods

### 1. URL Path (Default - Recommended)
```bash
curl -X GET http://localhost:5000/api/v2/users
```

### 2. Header-based
```bash
curl -X GET http://localhost:5000/api/users \
  -H "X-Api-Version: 2.0"
```

### 3. Query String
```bash
curl -X GET "http://localhost:5000/api/users?api-version=2.0"
```

---

## Product Endpoints

### Get All Products (V1 vs V2)

**V1:**
```bash
curl -X GET http://localhost:5000/api/v1/products
```
Response: Direct array

**V2:**
```bash
curl -X GET http://localhost:5000/api/v2/products
```
Response: Wrapped with metadata

### Create Product (V2)
```bash
curl -X POST http://localhost:5000/api/v2/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Laptop",
    "description": "Gaming",
    "price": 1299.99,
    "stock": 10
  }'
```

---

## Testing with PowerShell

### Windows PowerShell Examples

```powershell
# Get All Users V1
Invoke-RestMethod -Uri "http://localhost:5000/api/v1/users" -Method Get

# Get All Users V2
Invoke-RestMethod -Uri "http://localhost:5000/api/v2/users" -Method Get

# Create User V2
$body = @{
    name = "Charlie"
    email = "charlie@example.com"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/v2/users" `
    -Method Post `
    -Body $body `
    -ContentType "application/json"

# Get with Header Versioning
$headers = @{
    "X-Api-Version" = "2.0"
}
Invoke-RestMethod -Uri "http://localhost:5000/api/users" `
    -Method Get `
    -Headers $headers
```

---

## Swagger UI Testing

1. Navigate to: `http://localhost:5000/swagger`
2. Select version from dropdown:
   - **API V1** - Basic endpoints
   - **API V2** - Enhanced endpoints
3. Click "Try it out" on any endpoint
4. Execute and see the response

---

## Comparing Responses

### Example: Get All Users

**Command:**
```bash
# V1
curl -X GET http://localhost:5000/api/v1/users | jq

# V2
curl -X GET http://localhost:5000/api/v2/users | jq
```

**V1 Response:**
```json
[
  {
    "id": 1,
    "name": "Alice",
    "email": "alice@example.com",
    "createdAt": "2025-11-07T10:00:00Z"
  }
]
```

**V2 Response:**
```json
{
  "version": "2.0",
  "count": 1,
  "data": [
    {
      "id": 1,
      "name": "Alice",
      "email": "alice@example.com",
      "createdAt": "2025-11-07T10:00:00Z"
    }
  ],
  "timestamp": "2025-11-07T10:05:30Z"
}
```

---

## Postman Collection

Import this JSON to test in Postman:

```json
{
  "info": {
    "name": "Minimal API - Versioned",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "V1 - Get Users",
      "request": {
        "method": "GET",
        "header": [],
        "url": {
          "raw": "http://localhost:5000/api/v1/users",
          "protocol": "http",
          "host": ["localhost"],
          "port": "5000",
          "path": ["api", "v1", "users"]
        }
      }
    },
    {
      "name": "V2 - Get Users",
      "request": {
        "method": "GET",
        "header": [],
        "url": {
          "raw": "http://localhost:5000/api/v2/users",
          "protocol": "http",
          "host": ["localhost"],
          "port": "5000",
          "path": ["api", "v2", "users"]
        }
      }
    }
  ]
}
```

---

## Expected Status Codes

| Operation | V1 Status | V2 Status |
|-----------|-----------|-----------|
| GET (Success) | 200 OK | 200 OK |
| POST (Success) | 201 Created | 201 Created |
| PUT (Success) | 200 OK | 200 OK |
| DELETE (Success) | 204 No Content | 200 OK with message |
| Not Found | 404 | 404 |
| Validation Error | 400 | 400 |
| Server Error | 500 | 500 |
