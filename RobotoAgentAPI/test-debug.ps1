# Debug test comparing Manual vs Agentic approaches
$API_URL_MANUAL = "http://localhost:5000/api/chat"
$API_URL_AGENTIC = "http://localhost:5000/api/chat/agentic"

Write-Host "=========================================" -ForegroundColor Green
Write-Host "Debug Test: Manual vs Agentic Comparison" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green

$testQuery = "Tell me about The Matrix"
$testUser = "debug-test"

Write-Host "Test Query: $testQuery" -ForegroundColor Cyan
Write-Host "Test User: $testUser" -ForegroundColor Cyan
Write-Host ""

# Test Manual Approach
Write-Host "1. MANUAL APPROACH" -ForegroundColor Yellow
Write-Host "POST $API_URL_MANUAL" -ForegroundColor Cyan
Write-Host ""

try {
    $responseManual = Invoke-RestMethod -Uri $API_URL_MANUAL -Method POST -ContentType "application/json" -Body "{`"userId`": `"$testUser`", `"query`": `"$testQuery`"}"
    Write-Host "MANUAL SUCCESS" -ForegroundColor Green
    Write-Host "Response length: $($responseManual.response.Length) characters" -ForegroundColor Green
    Write-Host "Response preview: $($responseManual.response.Substring(0, [Math]::Min(100, $responseManual.response.Length)))..." -ForegroundColor Green
} catch {
    Write-Host "MANUAL FAILED" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green

# Test Agentic Approach
Write-Host "2. AGENTIC APPROACH" -ForegroundColor Yellow
Write-Host "POST $API_URL_AGENTIC" -ForegroundColor Cyan
Write-Host ""

try {
    $responseAgentic = Invoke-RestMethod -Uri $API_URL_AGENTIC -Method POST -ContentType "application/json" -Body "{`"userId`": `"$testUser`", `"query`": `"$testQuery`"}"
    Write-Host "AGENTIC SUCCESS" -ForegroundColor Green
    Write-Host "Response length: $($responseAgentic.response.Length) characters" -ForegroundColor Green
    if ($responseAgentic.response.Length -gt 0) {
        Write-Host "Response preview: $($responseAgentic.response.Substring(0, [Math]::Min(100, $responseAgentic.response.Length)))..." -ForegroundColor Green
    } else {
        Write-Host "Response is EMPTY - this indicates the agentic method is not working properly" -ForegroundColor Red
    }
} catch {
    Write-Host "AGENTIC FAILED" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green

# Compare results
Write-Host "3. COMPARISON RESULTS" -ForegroundColor Yellow
Write-Host ""

if ($responseManual -and $responseAgentic) {
    Write-Host "Manual response length: $($responseManual.response.Length)" -ForegroundColor Cyan
    Write-Host "Agentic response length: $($responseAgentic.response.Length)" -ForegroundColor Cyan
    
    if ($responseManual.response.Length -gt 0 -and $responseAgentic.response.Length -eq 0) {
        Write-Host ""
        Write-Host "DIAGNOSIS: Manual works, Agentic returns empty response" -ForegroundColor Red
        Write-Host "This suggests the agentic method (ProcessMessageAgentic) is not functioning correctly." -ForegroundColor Red
        Write-Host "Possible issues:" -ForegroundColor Red
        Write-Host "  - MaxTokens parameter causing OpenAI API errors" -ForegroundColor Red
        Write-Host "  - Auto-function calling not working properly" -ForegroundColor Red
        Write-Host "  - Plugin invocation issues" -ForegroundColor Red
    } elseif ($responseManual.response.Length -eq 0 -and $responseAgentic.response.Length -eq 0) {
        Write-Host ""
        Write-Host "DIAGNOSIS: Both approaches return empty responses" -ForegroundColor Red
        Write-Host "This suggests a fundamental issue with the API configuration." -ForegroundColor Red
    } else {
        Write-Host ""
        Write-Host "DIAGNOSIS: Both approaches are working" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "Debug Test Complete!" -ForegroundColor Green 