# Logging Configuration

## Overview

This application uses **Serilog** for structured logging with file and console sinks.

## Configuration

Logging is configured in `appsettings.json`:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "Logs/app-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      }
    ]
  }
}
```

## Log Levels

- **Verbose** - Tracing information and debugging minutiae
- **Debug** - Internal control flow and diagnostic state dumps
- **Information** - Events of interest or that have relevance to outside observers
- **Warning** - Indicators of possible issues or service degradation
- **Error** - Indicating a failure within the application or connected system
- **Fatal** - Critical errors causing complete failure of the application

## File Logging

### Location
Logs are written to the `Logs/` folder in the application directory.

### File Naming
- Pattern: `app-YYYYMMDD.log`
- Example: `app-20251107.log`

### Rotation
- **Rolling Interval**: Daily (new file each day)
- **Retention**: 30 days (older files are automatically deleted)

### Log Format
```
2025-11-07 09:48:01.123 +00:00 [INF] Request 12345: GET /api/users started at 11/07/2025 09:48:01
2025-11-07 09:48:01.456 +00:00 [INF] Request 12345: GET /api/users completed with 200 in 333ms
```

## What Gets Logged

### LoggingMiddleware
Automatically logs all HTTP requests:
- Request ID (GUID for tracking)
- HTTP Method (GET, POST, PUT, DELETE)
- Request Path
- Start time
- Completion status code
- Elapsed time in milliseconds
- Exceptions (if any)

### Example Logs

**Successful Request:**
```
2025-11-07 09:48:01.123 +00:00 [INF] Request abc123: GET /api/users started at 11/07/2025 09:48:01
2025-11-07 09:48:01.456 +00:00 [INF] Request abc123: GET /api/users completed with 200 in 333ms
```

**Failed Request:**
```
2025-11-07 09:48:01.123 +00:00 [INF] Request def456: POST /api/users started at 11/07/2025 09:48:01
2025-11-07 09:48:01.789 +00:00 [ERR] Request def456: POST /api/users failed after 666ms
System.Exception: Validation failed
   at MinimalApiApp.Services.UserService.CreateUserAsync(User user)
```

### Other Components

All middleware and services log important events:
- **ErrorHandlingMiddleware**: Logs all unhandled exceptions
- **ResilienceMiddleware**: Logs retry attempts
- **ValidationMiddleware**: Logs validation failures
- **CacheService**: Logs cache errors
- **Services**: Log business logic errors

## Console Output

Logs are also written to the console for development and debugging.

## Structured Logging

Serilog uses structured logging, which means you can query logs efficiently:

```csharp
_logger.LogInformation(
    "User {UserId} created product {ProductId} with price {Price}",
    userId, productId, price);
```

This creates a structured log entry where `UserId`, `ProductId`, and `Price` are searchable fields.

## Viewing Logs

### During Development
- Watch console output in real-time
- Logs appear in the terminal where you run the app

### In Production
- Check the `Logs/` folder
- Use log aggregation tools (e.g., Seq, ELK Stack, Splunk)
- Tail logs: `tail -f Logs/app-20251107.log`

## Best Practices

1. **Don't log sensitive data**: Passwords, tokens, credit cards
2. **Use appropriate log levels**: 
   - Information for business events
   - Warning for degraded performance
   - Error for failures
3. **Include context**: Request IDs, User IDs, correlation data
4. **Log exceptions with full stack traces**
5. **Monitor log file size** in production

## Troubleshooting

### Logs Not Being Created
- Check write permissions on the application directory
- Ensure Serilog packages are installed
- Verify appsettings.json configuration

### Logs Growing Too Large
- Adjust `retainedFileCountLimit` to keep fewer days
- Implement log rotation policies
- Consider sending logs to external service

### Finding Specific Requests
- Search by Request ID (GUID)
- Filter by HTTP method or path
- Use structured logging queries with tools like Seq

## Integration with Monitoring

Serilog can be extended to write to:
- **Seq** - Structured log server
- **Elasticsearch** - Full-text search and analytics
- **Application Insights** - Azure monitoring
- **Datadog** - Cloud monitoring platform
- **Splunk** - Enterprise logging platform

Example for Seq:
```bash
dotnet add package Serilog.Sinks.Seq
```

Then update appsettings.json:
```json
{
  "WriteTo": [
    {
      "Name": "Seq",
      "Args": { "serverUrl": "http://localhost:5341" }
    }
  ]
}
```
