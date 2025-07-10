# Test script specifically for ChatAgentic endpoint
$API_URL_AGENTIC = "http://localhost:5000/api/chat/agentic"

Write-Host "=========================================" -ForegroundColor Green
Write-Host "Testing ChatAgentic Endpoint" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green

# Test 1: Basic movie query
Write-Host "Test 1: Basic movie query" -ForegroundColor Yellow
Write-Host "POST $API_URL_AGENTIC" -ForegroundColor Cyan
Write-Host '{"userId": "agentic-test-1", "query": "Tell me about The Matrix"}' -ForegroundColor Cyan
Write-Host ""

try {
    $response1 = Invoke-RestMethod -Uri $API_URL_AGENTIC -Method POST -ContentType "application/json" -Body '{"userId": "agentic-test-1", "query": "Tell me about The Matrix"}'
    Write-Host "✓ SUCCESS" -ForegroundColor Green
    Write-Host "Response:" -ForegroundColor Green
    $response1 | ConvertTo-Json -Depth 3
} catch {
    Write-Host "✗ FAILED" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green

# Test 2: Movie recommendation
Write-Host "Test 2: Movie recommendation" -ForegroundColor Yellow
Write-Host "POST $API_URL_AGENTIC" -ForegroundColor Cyan
Write-Host '{"userId": "agentic-test-2", "query": "Recommend some sci-fi movies"}' -ForegroundColor Cyan
Write-Host ""

try {
    $response2 = Invoke-RestMethod -Uri $API_URL_AGENTIC -Method POST -ContentType "application/json" -Body '{"userId": "agentic-test-2", "query": "Recommend some sci-fi movies"}'
    Write-Host "✓ SUCCESS" -ForegroundColor Green
    Write-Host "Response:" -ForegroundColor Green
    $response2 | ConvertTo-Json -Depth 3
} catch {
    Write-Host "✗ FAILED" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green

# Test 3: Complex query (this should showcase agentic capabilities)
Write-Host "Test 3: Complex query (agentic decision making)" -ForegroundColor Yellow
Write-Host "POST $API_URL_AGENTIC" -ForegroundColor Cyan
Write-Host '{"userId": "agentic-test-3", "query": "I liked Blade Runner and Minority Report. What other movies explore similar themes about AI and future society?"}' -ForegroundColor Cyan
Write-Host ""

try {
    $response3 = Invoke-RestMethod -Uri $API_URL_AGENTIC -Method POST -ContentType "application/json" -Body '{"userId": "agentic-test-3", "query": "I liked Blade Runner and Minority Report. What other movies explore similar themes about AI and future society?"}'
    Write-Host "✓ SUCCESS" -ForegroundColor Green
    Write-Host "Response:" -ForegroundColor Green
    $response3 | ConvertTo-Json -Depth 3
} catch {
    Write-Host "✗ FAILED" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green

# Test 4: Non-movie query (should still reject politely)
Write-Host "Test 4: Non-movie query (should reject)" -ForegroundColor Yellow
Write-Host "POST $API_URL_AGENTIC" -ForegroundColor Cyan
Write-Host '{"userId": "agentic-test-4", "query": "What is the capital of France?"}' -ForegroundColor Cyan
Write-Host ""

try {
    $response4 = Invoke-RestMethod -Uri $API_URL_AGENTIC -Method POST -ContentType "application/json" -Body '{"userId": "agentic-test-4", "query": "What is the capital of France?"}'
    Write-Host "✓ SUCCESS" -ForegroundColor Green
    Write-Host "Response:" -ForegroundColor Green
    $response4 | ConvertTo-Json -Depth 3
} catch {
    Write-Host "✗ FAILED" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green

# Test 5: Empty query
Write-Host "Test 5: Empty query validation" -ForegroundColor Yellow
Write-Host "POST $API_URL_AGENTIC" -ForegroundColor Cyan
Write-Host '{"userId": "agentic-test-5", "query": ""}' -ForegroundColor Cyan
Write-Host ""

try {
    $response5 = Invoke-RestMethod -Uri $API_URL_AGENTIC -Method POST -ContentType "application/json" -Body '{"userId": "agentic-test-5", "query": ""}'
    Write-Host "✓ SUCCESS" -ForegroundColor Green
    Write-Host "Response:" -ForegroundColor Green
    $response5 | ConvertTo-Json -Depth 3
} catch {
    Write-Host "✗ FAILED" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host "ChatAgentic Testing Complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Expected behavior:" -ForegroundColor Yellow
Write-Host "- Tests 1-3: Should return movie information/recommendations" -ForegroundColor Yellow
Write-Host "- Test 4: Should politely reject non-movie queries" -ForegroundColor Yellow
Write-Host "- Test 5: Should return validation error for empty query" -ForegroundColor Yellow
Write-Host ""
Write-Host "Note: If all tests fail with 500 errors, check that:" -ForegroundColor Yellow
Write-Host "1. The API server is running (dotnet run)" -ForegroundColor Yellow
Write-Host "2. Azure OpenAI API keys are configured in appsettings.json" -ForegroundColor Yellow
Write-Host "3. The endpoint http://localhost:5000 is accessible" -ForegroundColor Yellow 