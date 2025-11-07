# Folder Organization Guide

## Recommended Folder Structure

```
C:\Workspace\x_Copilot_CLI\
├── Models/
│   ├── User.cs
│   └── Product.cs
│
├── Services/
│   ├── IUserService.cs
│   ├── IProductService.cs
│   ├── UserService.cs
│   ├── ProductService.cs
│   ├── EfRepositories.cs
│   ├── IUnitOfWork.cs
│   ├── UnitOfWork.cs
│   └── CacheService.cs
│
├── Middleware/
│   ├── ErrorHandlingMiddleware.cs
│   ├── LoggingMiddleware.cs
│   ├── ResilienceMiddleware.cs
│   └── ValidationMiddleware.cs
│
├── Validators/
│   ├── UserValidator.cs
│   └── ProductValidator.cs
│
├── Configuration/
│   ├── SwaggerConfiguration.cs
│   └── ResiliencePolicies.cs
│
├── Data/
│   └── ApplicationDbContext.cs
│
└── Program.cs (stays in root)
```

## Manual Steps (Windows)

### 1. Create Folders
Open Command Prompt or PowerShell in `C:\Workspace\x_Copilot_CLI\`:

```cmd
mkdir Models
mkdir Services
mkdir Middleware
mkdir Validators
mkdir Configuration
mkdir Data
```

### 2. Move Files to Models/
```cmd
move User.cs Models\
move Product.cs Models\
```

### 3. Move Files to Services/
```cmd
move IUserService.cs Services\
move IProductService.cs Services\
move UserService.cs Services\
move ProductService.cs Services\
move EfRepositories.cs Services\
move IUnitOfWork.cs Services\
move UnitOfWork.cs Services\
move CacheService.cs Services\
```

### 4. Move Files to Middleware/
```cmd
move ErrorHandlingMiddleware.cs Middleware\
move LoggingMiddleware.cs Middleware\
move ResilienceMiddleware.cs Middleware\
move ValidationMiddleware.cs Middleware\
```

### 5. Move Files to Validators/
```cmd
move UserValidator.cs Validators\
move ProductValidator.cs Validators\
```

### 6. Move Files to Configuration/
```cmd
move SwaggerConfiguration.cs Configuration\
move ResiliencePolicies.cs Configuration\
```

### 7. Move Files to Data/
```cmd
move ApplicationDbContext.cs Data\
```

## Namespace Updates Required

After moving files, update the namespaces:

### Models/User.cs & Models/Product.cs
```csharp
namespace MinimalApiApp.Models;
// Already correct!
```

### Services/* files
```csharp
namespace MinimalApiApp.Services;
// Already correct!
```

### Middleware/* files
```csharp
namespace MinimalApiApp.Middleware;
// Already correct!
```

### Validators/* files
```csharp
namespace MinimalApiApp.Validators;
// Already correct!
```

### Configuration/* files
```csharp
namespace MinimalApiApp.Configuration;
// Already correct!
```

### Data/ApplicationDbContext.cs
```csharp
namespace MinimalApiApp.Data;
// Already correct!
```

## Good News!

✅ All namespaces are already correct!
✅ No code changes needed
✅ Just move the files to their folders

## Quick Script (PowerShell)

Copy and paste this entire script into PowerShell:

```powershell
cd C:\Workspace\x_Copilot_CLI

# Create folders
New-Item -ItemType Directory -Force -Path Models, Services, Middleware, Validators, Configuration, Data

# Move Models
Move-Item -Force User.cs Models\
Move-Item -Force Product.cs Models\

# Move Services
Move-Item -Force IUserService.cs Services\
Move-Item -Force IProductService.cs Services\
Move-Item -Force UserService.cs Services\
Move-Item -Force ProductService.cs Services\
Move-Item -Force EfRepositories.cs Services\
Move-Item -Force IUnitOfWork.cs Services\
Move-Item -Force UnitOfWork.cs Services\
Move-Item -Force CacheService.cs Services\

# Move Middleware
Move-Item -Force ErrorHandlingMiddleware.cs Middleware\
Move-Item -Force LoggingMiddleware.cs Middleware\
Move-Item -Force ResilienceMiddleware.cs Middleware\
Move-Item -Force ValidationMiddleware.cs Middleware\

# Move Validators
Move-Item -Force UserValidator.cs Validators\
Move-Item -Force ProductValidator.cs Validators\

# Move Configuration
Move-Item -Force SwaggerConfiguration.cs Configuration\
Move-Item -Force ResiliencePolicies.cs Configuration\

# Move Data
Move-Item -Force ApplicationDbContext.cs Data\

Write-Host "✅ Files organized successfully!" -ForegroundColor Green
```

## Quick Script (Command Prompt/Batch)

Save as `organize_files.bat` and run:

```batch
@echo off
cd C:\Workspace\x_Copilot_CLI

REM Create folders
mkdir Models 2>nul
mkdir Services 2>nul
mkdir Middleware 2>nul
mkdir Validators 2>nul
mkdir Configuration 2>nul
mkdir Data 2>nul

REM Move Models
move /Y User.cs Models\
move /Y Product.cs Models\

REM Move Services
move /Y IUserService.cs Services\
move /Y IProductService.cs Services\
move /Y UserService.cs Services\
move /Y ProductService.cs Services\
move /Y EfRepositories.cs Services\
move /Y IUnitOfWork.cs Services\
move /Y UnitOfWork.cs Services\
move /Y CacheService.cs Services\

REM Move Middleware
move /Y ErrorHandlingMiddleware.cs Middleware\
move /Y LoggingMiddleware.cs Middleware\
move /Y ResilienceMiddleware.cs Middleware\
move /Y ValidationMiddleware.cs Middleware\

REM Move Validators
move /Y UserValidator.cs Validators\
move /Y ProductValidator.cs Validators\

REM Move Configuration
move /Y SwaggerConfiguration.cs Configuration\
move /Y ResiliencePolicies.cs Configuration\

REM Move Data
move /Y ApplicationDbContext.cs Data\

echo.
echo ✅ Files organized successfully!
pause
```

## Verify After Moving

1. **Check folder structure:**
```cmd
dir Models
dir Services
dir Middleware
dir Validators
dir Configuration
dir Data
```

2. **Build the project:**
```cmd
dotnet build
```

3. **Run the project:**
```cmd
dotnet run
```

## Files That Should Stay in Root

- ✅ Program.cs
- ✅ MinimalApiApp.csproj
- ✅ appsettings.json
- ✅ appsettings.Development.json
- ✅ README.md
- ✅ *.md (all documentation files)
- ✅ .gitignore
- ✅ setup.bat

## Expected Final Structure

```
C:\Workspace\x_Copilot_CLI\
│
├── Models/
│   ├── User.cs
│   └── Product.cs
│
├── Services/
│   ├── IUserService.cs
│   ├── IProductService.cs
│   ├── UserService.cs
│   ├── ProductService.cs
│   ├── EfRepositories.cs
│   ├── IUnitOfWork.cs
│   ├── UnitOfWork.cs
│   └── CacheService.cs
│
├── Middleware/
│   ├── ErrorHandlingMiddleware.cs
│   ├── LoggingMiddleware.cs
│   ├── ResilienceMiddleware.cs
│   └── ValidationMiddleware.cs
│
├── Validators/
│   ├── UserValidator.cs
│   └── ProductValidator.cs
│
├── Configuration/
│   ├── SwaggerConfiguration.cs
│   └── ResiliencePolicies.cs
│
├── Data/
│   └── ApplicationDbContext.cs
│
├── Logs/
│   └── (generated log files)
│
├── bin/
├── obj/
├── Properties/
│
├── Program.cs
├── MinimalApiApp.csproj
├── appsettings.json
├── appsettings.Development.json
├── README.md
├── API_VERSIONING.md
├── API_VERSIONING_EXAMPLES.md
├── EF_CORE_GUIDE.md
├── EF_CORE_TESTING.md
├── LOGGING_GUIDE.md
├── REDIS_SETUP.md
└── .gitignore
```

## Benefits

✅ **Better Organization** - Related files grouped together
✅ **Easier Navigation** - Find files faster
✅ **Industry Standard** - Common .NET project structure
✅ **Scalability** - Easy to add more files to each category
✅ **Clean Root** - Only essential files in root directory

## Troubleshooting

### Issue: "File not found"
**Solution:** File might already be in a folder or have a different name

### Issue: Build errors after moving
**Solution:** 
1. Clean the solution: `dotnet clean`
2. Rebuild: `dotnet build`
3. Check all namespaces are correct

### Issue: Can't find files in IDE
**Solution:** Reload/refresh the project in your IDE (Visual Studio, VS Code, Rider)

## After Organization

Once files are moved, the project will:
- ✅ Compile successfully (namespaces already correct)
- ✅ Run without issues
- ✅ Have a cleaner structure
- ✅ Be easier to maintain
