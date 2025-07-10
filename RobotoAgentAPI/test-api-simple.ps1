# Simple Movie Chat API Test Script
$API_URL = "http://localhost:5000/api/chat"
$API_URL_AGENTIC = "http://localhost:5000/api/chat/agentic"

Write-Host "=========================================" -ForegroundColor Green
Write-Host "Testing Movie Chat API" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green

# Test 1: Basic movie search (Manual)
Write-Host "Test 1: Basic movie search (Manual)" -ForegroundColor Yellow
try {
    $response1 = Invoke-RestMethod -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"userId": "test-user-1", "query": "Tell me about The Matrix"}'
    Write-Host "Response:" -ForegroundColor Cyan
    $response1 | ConvertTo-Json -Depth 3
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green

# Test 2: Basic movie search (Agentic)
Write-Host "Test 2: Basic movie search (Agentic)" -ForegroundColor Yellow
try {
    $response2 = Invoke-RestMethod -Uri $API_URL_AGENTIC -Method POST -ContentType "application/json" -Body '{"userId": "test-user-2", "query": "Tell me about The Matrix"}'
    Write-Host "Response:" -ForegroundColor Cyan
    $response2 | ConvertTo-Json -Depth 3
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green

# Test 3: Non-movie query
Write-Host "Test 3: Non-movie query (should reject)" -ForegroundColor Yellow
try {
    $response3 = Invoke-RestMethod -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"userId": "test-user-3", "query": "What is the weather today?"}'
    Write-Host "Response:" -ForegroundColor Cyan
    $response3 | ConvertTo-Json -Depth 3
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green

# Test 4: Empty query
Write-Host "Test 4: Empty query (should return error)" -ForegroundColor Yellow
try {
    $response4 = Invoke-RestMethod -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"userId": "test-user-4", "query": ""}'
    Write-Host "Response:" -ForegroundColor Cyan
    $response4 | ConvertTo-Json -Depth 3
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "API Testing Complete!" -ForegroundColor Green 