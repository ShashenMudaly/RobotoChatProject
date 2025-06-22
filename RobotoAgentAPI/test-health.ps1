# Simple health check and diagnostic script
$API_BASE = "http://localhost:5000"

Write-Host "=========================================" -ForegroundColor Green
Write-Host "API Health Check and Diagnostics" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green
Write-Host ""

# Test 1: Health check
Write-Host "Test 1: Health Check" -ForegroundColor Yellow
Write-Host "GET $API_BASE/api/chat/health"
Write-Host ""

try {
    $healthResponse = Invoke-RestMethod -Uri "$API_BASE/api/chat/health" -Method GET
    Write-Host "✅ Health Check Passed" -ForegroundColor Green
    $healthResponse | ConvertTo-Json -Depth 10
} catch {
    Write-Host "❌ Health Check Failed" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Make sure the API is running with 'dotnet run'" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host ""

# Test 2: Simple chat request
Write-Host "Test 2: Simple Chat Request" -ForegroundColor Yellow
Write-Host "POST $API_BASE/api/chat"
Write-Host '{"message": "Hello"}'
Write-Host ""

try {
    $chatResponse = Invoke-RestMethod -Uri "$API_BASE/api/chat" -Method POST -ContentType "application/json" -Body '{"message": "Hello"}'
    Write-Host "✅ Chat Request Successful" -ForegroundColor Green
    $chatResponse | ConvertTo-Json -Depth 10
} catch {
    Write-Host "❌ Chat Request Failed" -ForegroundColor Red
    Write-Host "Status Code: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    
    # Try to get more details from the response
    try {
        $errorStream = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($errorStream)
        $errorBody = $reader.ReadToEnd()
        Write-Host "Error Response Body:" -ForegroundColor Red
        Write-Host $errorBody -ForegroundColor Red
    } catch {
        Write-Host "Could not read error response body" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host ""

Write-Host "Diagnostic Complete!" -ForegroundColor Green
Write-Host ""
Write-Host "If the health check passes but chat fails, check:" -ForegroundColor Yellow
Write-Host "1. Azure OpenAI configuration in appsettings.Development.json" -ForegroundColor Yellow
Write-Host "2. Bono Search API configuration" -ForegroundColor Yellow
Write-Host "3. Server console logs for detailed error messages" -ForegroundColor Yellow 