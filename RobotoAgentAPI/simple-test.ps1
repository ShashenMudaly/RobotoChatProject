#!/usr/bin/env pwsh

Write-Host "Testing Agentic Endpoint..." -ForegroundColor Green

$uri = "http://localhost:5000/api/chat/agentic"
$body = @{
    userId = "test-user"
    query = "Tell me about Star Wars"
} | ConvertTo-Json

Write-Host "Making request to: $uri"
Write-Host "Body: $body"

try {
    Write-Host "Sending request..." -ForegroundColor Yellow
    $response = Invoke-RestMethod -Uri $uri -Method POST -ContentType "application/json" -Body $body -Verbose
    Write-Host "SUCCESS! Response received:" -ForegroundColor Green
    Write-Host "Response type: $($response.GetType().Name)"
    if ($response -is [string]) {
        Write-Host "String response: $response"
    } else {
        $response | ConvertTo-Json -Depth 5
    }
} catch {
    Write-Host "ERROR!" -ForegroundColor Red
    Write-Host "Exception type: $($_.Exception.GetType().Name)"
    Write-Host "Message: $($_.Exception.Message)"
    if ($_.Exception.Response) {
        Write-Host "HTTP Status: $($_.Exception.Response.StatusCode)"
    }
}

Write-Host "Test completed." -ForegroundColor Blue 