# API Versioning Guide

## Overview

This API supports versioning to maintain backward compatibility while introducing new features and improvements. Currently, we support **Version 1.0 (v1)** and **Version 2.0 (v2)**.

## Versioning Strategy

We use **URL Path Versioning** as the primary method, with additional support for header and query string versioning.

### Supported Methods:

1. **URL Path** (Recommended): `/api/v1/users` or `/api/v2/users`
2. **Header**: `X-Api-Version: 1.0` or `X-Api-Version: 2.0`
3. **Query String**: `/api/users?api-version=1.0`

## API Versions

### Version 1.0 (v1) - Current Stable

**Base URL:** `/api/v1`

**Features:**
- Basic CRUD operations for Users and Products
- Standard REST responses
- Returns direct data objects

**Example Endpoints:**
```
GET    /api/v1/users
GET    /api/v1/users/{id}
POST   /api/v1/users
PUT    /api/v1/users/{id}
DELETE /api/v1/users/{id}

GET    /api/v1/products
GET    /api/v1/products/{id}
POST   /api/v1/products
PUT    /api/v1/products/{id}
DELETE /api/v1/products/{id}
```

**Response Format (v1):**
```json
// GET /api/v1/users
[
  {
    "id": 1,
    "name": "John Doe",
    "email": "john@example.com",
    "createdAt": "2025-11-07T10:00:00Z"
  }
]
```

### Version 2.0 (v2) - Enhanced

**Base URL:** `/api/v2`

**Features:**
- All v1 features
- Enhanced response format with metadata
- Additional response messages
- Timestamp tracking
- Count metadata for collections
- Standardized response structure

**Example Endpoints:**
```
GET    /api/v2/users
GET    /api/v2/users/{id}
POST   /api/v2/users
PUT    /api/v2/users/{id}
DELETE /api/v2/users/{id}

GET    /api/v2/products
GET    /api/v2/products/{id}
POST   /api/v2/products
PUT    /api/v2/products/{id}
DELETE /api/v2/products/{id}
```

**Response Format (v2):**
```json
// GET /api/v2/users
{
  "version": "2.0",
  "count": 2,
  "data": [
    {
      "id": 1,
      "name": "John Doe",
      "email": "john@example.com",
      "createdAt": "2025-11-07T10:00:00Z"
    }
  ],
  "timestamp": "2025-11-07T10:05:00Z"
}

// POST /api/v2/users (Success)
{
  "version": "2.0",
  "data": {
    "id": 3,
    "name": "Jane Smith",
    "email": "jane@example.com",
    "createdAt": "2025-11-07T10:05:00Z"
  },
  "message": "User created successfully",
  "timestamp": "2025-11-07T10:05:00Z"
}
```

## Making API Calls

### Using URL Path Versioning (Recommended)

```bash
# Version 1
curl -X GET "https://api.example.com/api/v1/users"

# Version 2
curl -X GET "https://api.example.com/api/v2/users"
```

### Using Header Versioning

```bash
curl -X GET "https://api.example.com/api/users" \
  -H "X-Api-Version: 2.0"
```

### Using Query String Versioning

```bash
curl -X GET "https://api.example.com/api/users?api-version=2.0"
```

## Default Version

If no version is specified, the API defaults to **Version 1.0**.

```bash
# These are equivalent:
curl -X GET "https://api.example.com/api/users"
curl -X GET "https://api.example.com/api/v1/users"
```

## Version Headers

All responses include version information in the response headers:

```
api-supported-versions: 1.0, 2.0
api-deprecated-versions: (none)
```

## Swagger Documentation

Interactive API documentation is available at:

- **Swagger UI**: `https://api.example.com/swagger`
  - Version 1: Select "API V1" from dropdown
  - Version 2: Select "API V2" from dropdown

## Migration Guide

### Migrating from V1 to V2

**Key Differences:**

1. **Response Structure**
   ```json
   // V1 Response
   {
     "id": 1,
     "name": "John"
   }
   
   // V2 Response
   {
     "version": "2.0",
     "data": {
       "id": 1,
       "name": "John"
     },
     "timestamp": "2025-11-07T10:00:00Z"
   }
   ```

2. **Collection Responses**
   ```json
   // V1 Response
   [
     { "id": 1, "name": "John" },
     { "id": 2, "name": "Jane" }
   ]
   
   // V2 Response
   {
     "version": "2.0",
     "count": 2,
     "data": [
       { "id": 1, "name": "John" },
       { "id": 2, "name": "Jane" }
     ],
     "timestamp": "2025-11-07T10:00:00Z"
   }
   ```

3. **Success Messages**
   - V1: Returns 204 No Content for DELETE
   - V2: Returns 200 OK with success message for DELETE

**Code Changes:**

```javascript
// JavaScript/TypeScript client example

// V1
const users = await fetch('/api/v1/users').then(r => r.json());
console.log(users); // Array directly

// V2
const response = await fetch('/api/v2/users').then(r => r.json());
console.log(response.data); // Access data property
console.log(response.count); // Access count
```

```csharp
// C# client example

// V1
var users = await client.GetFromJsonAsync<List<User>>("/api/v1/users");

// V2
var response = await client.GetFromJsonAsync<ApiResponse<List<User>>>("/api/v2/users");
var users = response.Data;
var count = response.Count;
```

## Deprecation Policy

### Timeline

1. **Announcement**: Deprecation announced 6 months in advance
2. **Warning Period**: Version marked as deprecated (3 months)
3. **Sunset**: Version removed from production

### Current Status

- **V1**: Stable - No deprecation planned
- **V2**: Current - Actively developed

## Best Practices

### For API Consumers

1. ✅ **Always specify version explicitly** in production code
2. ✅ **Use URL path versioning** for clarity
3. ✅ **Handle both response formats** if migrating
4. ✅ **Monitor deprecation announcements**
5. ✅ **Test against new versions** before migrating
6. ❌ **Don't rely on default version** in production

### For API Development

1. ✅ **Never break existing versions**
2. ✅ **Add new features to new versions**
3. ✅ **Maintain backward compatibility**
4. ✅ **Document all changes**
5. ✅ **Test all versions thoroughly**

## Error Handling

Error responses are consistent across all versions:

```json
{
  "error": "User with ID 999 not found",
  "statusCode": 404,
  "type": "NotFoundException"
}
```

## Rate Limiting

Rate limits apply per version:
- V1: 100 requests/minute
- V2: 100 requests/minute

Headers included in all responses:
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1699350000
```

## Examples

### Complete CRUD Examples

#### Create User (V1)
```bash
curl -X POST "https://api.example.com/api/v1/users" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "John Doe",
    "email": "john@example.com"
  }'

# Response (201 Created)
{
  "id": 1,
  "name": "John Doe",
  "email": "john@example.com",
  "createdAt": "2025-11-07T10:00:00Z"
}
```

#### Create User (V2)
```bash
curl -X POST "https://api.example.com/api/v2/users" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "John Doe",
    "email": "john@example.com"
  }'

# Response (201 Created)
{
  "version": "2.0",
  "data": {
    "id": 1,
    "name": "John Doe",
    "email": "john@example.com",
    "createdAt": "2025-11-07T10:00:00Z"
  },
  "message": "User created successfully",
  "timestamp": "2025-11-07T10:00:00Z"
}
```

## Support

For questions or issues:
- Email: support@minimalapi.com
- Documentation: https://api.example.com/docs
- Changelog: https://api.example.com/changelog
