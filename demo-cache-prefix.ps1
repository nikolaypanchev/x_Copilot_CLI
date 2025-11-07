# Cache Management Demo Script
# Demonstrates the RemoveByPrefixAsync functionality

Write-Host "=== Cache Management Demo ===" -ForegroundColor Cyan
Write-Host ""

$baseUrl = "http://localhost:5000"

# Function to make API calls
function Invoke-ApiCall {
    param(
        [string]$Method,
        [string]$Endpoint,
        [object]$Body = $null
    )
    
    $url = "$baseUrl$Endpoint"
    
    try {
        if ($Body) {
            $jsonBody = $Body | ConvertTo-Json
            $response = Invoke-RestMethod -Uri $url -Method $Method -Body $jsonBody -ContentType "application/json"
        } else {
            $response = Invoke-RestMethod -Uri $url -Method $Method
        }
        return $response
    } catch {
        Write-Host "Error: $_" -ForegroundColor Red
        return $null
    }
}

Write-Host "Step 1: Creating cache entries with different prefixes..." -ForegroundColor Yellow
Write-Host ""

# Create product cache entries
Write-Host "Creating product cache entries:" -ForegroundColor Green
$productEntries = @(
    @{ Key = "product:1"; Value = "Laptop"; ExpirationMinutes = 10 }
    @{ Key = "product:2"; Value = "Mouse"; ExpirationMinutes = 10 }
    @{ Key = "product:3"; Value = "Keyboard"; ExpirationMinutes = 10 }
    @{ Key = "products:all"; Value = "All Products List"; ExpirationMinutes = 10 }
)

foreach ($entry in $productEntries) {
    $result = Invoke-ApiCall -Method POST -Endpoint "/api/cache" -Body $entry
    if ($result) {
        Write-Host "  ✓ Created: $($entry.Key)" -ForegroundColor Green
    }
}

Write-Host ""

# Create user cache entries
Write-Host "Creating user cache entries:" -ForegroundColor Green
$userEntries = @(
    @{ Key = "user:1"; Value = "John Doe"; ExpirationMinutes = 10 }
    @{ Key = "user:2"; Value = "Jane Smith"; ExpirationMinutes = 10 }
    @{ Key = "users:all"; Value = "All Users List"; ExpirationMinutes = 10 }
)

foreach ($entry in $userEntries) {
    $result = Invoke-ApiCall -Method POST -Endpoint "/api/cache" -Body $entry
    if ($result) {
        Write-Host "  ✓ Created: $($entry.Key)" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "Step 2: Verifying cache entries exist..." -ForegroundColor Yellow
Write-Host ""

# Verify entries exist
$allKeys = @("product:1", "product:2", "product:3", "products:all", "user:1", "user:2", "users:all")
Write-Host "Checking cache entries:" -ForegroundColor Green
foreach ($key in $allKeys) {
    $result = Invoke-ApiCall -Method GET -Endpoint "/api/cache/$key"
    if ($result -and $result.value) {
        Write-Host "  ✓ Found: $key = $($result.value)" -ForegroundColor Green
    } else {
        Write-Host "  ✗ Not found: $key" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Step 3: Removing cache entries by prefix 'product'..." -ForegroundColor Yellow
Write-Host ""

# Remove by prefix
$result = Invoke-ApiCall -Method DELETE -Endpoint "/api/cache/prefix/product"
if ($result) {
    Write-Host "  ✓ $($result.message)" -ForegroundColor Green
}

Write-Host ""
Write-Host "Step 4: Verifying product entries removed but user entries remain..." -ForegroundColor Yellow
Write-Host ""

# Verify product entries removed
Write-Host "Checking product entries (should be removed):" -ForegroundColor Cyan
foreach ($key in @("product:1", "product:2", "product:3", "products:all")) {
    $result = Invoke-ApiCall -Method GET -Endpoint "/api/cache/$key"
    if ($result -and $result.value) {
        Write-Host "  ✗ Still exists: $key" -ForegroundColor Red
    } else {
        Write-Host "  ✓ Removed: $key" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "Checking user entries (should still exist):" -ForegroundColor Cyan
foreach ($key in @("user:1", "user:2", "users:all")) {
    $result = Invoke-ApiCall -Method GET -Endpoint "/api/cache/$key"
    if ($result -and $result.value) {
        Write-Host "  ✓ Still exists: $key = $($result.value)" -ForegroundColor Green
    } else {
        Write-Host "  ✗ Missing: $key" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Step 5: Removing remaining user entries by prefix 'user'..." -ForegroundColor Yellow
Write-Host ""

# Remove user entries
$result = Invoke-ApiCall -Method DELETE -Endpoint "/api/cache/prefix/user"
if ($result) {
    Write-Host "  ✓ $($result.message)" -ForegroundColor Green
}

Write-Host ""
Write-Host "Step 6: Verifying all entries removed..." -ForegroundColor Yellow
Write-Host ""

# Verify all removed
Write-Host "Final verification:" -ForegroundColor Cyan
foreach ($key in $allKeys) {
    $result = Invoke-ApiCall -Method GET -Endpoint "/api/cache/$key"
    if ($result -and $result.value) {
        Write-Host "  ✗ Still exists: $key" -ForegroundColor Red
    } else {
        Write-Host "  ✓ Removed: $key" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "=== Demo Complete! ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Summary:" -ForegroundColor Yellow
Write-Host "  - Created 7 cache entries (4 with 'product' prefix, 3 with 'user' prefix)"
Write-Host "  - Used RemoveByPrefixAsync to remove all 'product' entries"
Write-Host "  - Verified 'user' entries remained intact"
Write-Host "  - Used RemoveByPrefixAsync to remove all 'user' entries"
Write-Host "  - Verified all entries removed"
Write-Host ""
Write-Host "Try it yourself in Swagger UI at: http://localhost:5000/swagger" -ForegroundColor Green
