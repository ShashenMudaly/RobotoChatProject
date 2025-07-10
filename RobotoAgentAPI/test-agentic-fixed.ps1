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
    Write-Host "SUCCESS" -ForegroundColor Green
    Write-Host "Response:" -ForegroundColor Green
    $response1 | ConvertTo-Json -Depth 3
} catch {
    Write-Host "FAILED" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green

# Test 2: Complex query (this should showcase agentic capabilities)
Write-Host "Test 2: Complex query (agentic decision making)" -ForegroundColor Yellow
Write-Host "POST $API_URL_AGENTIC" -ForegroundColor Cyan
Write-Host '{"userId": "agentic-test-2", "query": "I liked Blade Runner. What other movies explore similar themes?"}' -ForegroundColor Cyan
Write-Host ""

try {
    $response2 = Invoke-RestMethod -Uri $API_URL_AGENTIC -Method POST -ContentType "application/json" -Body '{"userId": "agentic-test-2", "query": "I liked Blade Runner. What other movies explore similar themes?"}'
    Write-Host "SUCCESS" -ForegroundColor Green
    Write-Host "Response:" -ForegroundColor Green
    $response2 | ConvertTo-Json -Depth 3
} catch {
    Write-Host "FAILED" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green

# Test 3: Non-movie query (should still reject politely)
Write-Host "Test 3: Non-movie query (should reject)" -ForegroundColor Yellow
Write-Host "POST $API_URL_AGENTIC" -ForegroundColor Cyan
Write-Host '{"userId": "agentic-test-3", "query": "What is the capital of France?"}' -ForegroundColor Cyan
Write-Host ""

try {
    $response3 = Invoke-RestMethod -Uri $API_URL_AGENTIC -Method POST -ContentType "application/json" -Body '{"userId": "agentic-test-3", "query": "What is the capital of France?"}'
    Write-Host "SUCCESS" -ForegroundColor Green
    Write-Host "Response:" -ForegroundColor Green
    $response3 | ConvertTo-Json -Depth 3
} catch {
    Write-Host "FAILED" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host "ChatAgentic Testing Complete!" -ForegroundColor Green 