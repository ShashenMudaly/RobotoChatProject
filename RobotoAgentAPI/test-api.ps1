# Movie Chat API Test Script (PowerShell)
# Make sure the API is running on https://localhost:5001 before running this script

$API_URL = "http://localhost:5000/api/chat"

# Handle SSL certificate validation for older PowerShell versions
if ($PSVersionTable.PSVersion.Major -lt 6) {
    # For Windows PowerShell 5.1 and earlier
    add-type @"
        using System.Net;
        using System.Security.Cryptography.X509Certificates;
        public class TrustAllCertsPolicy : ICertificatePolicy {
            public bool CheckValidationResult(
                ServicePoint srvPoint, X509Certificate certificate,
                WebRequest request, int certificateProblem) {
                return true;
            }
        }
"@
    [System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy
    [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12
}

Write-Host "=========================================" -ForegroundColor Green
Write-Host "Testing Movie Chat API" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green
Write-Host ""

# Test 1: Basic movie search
Write-Host "Test 1: Basic movie search for 'The Matrix'" -ForegroundColor Yellow
Write-Host "Request:"
Write-Host "POST $API_URL"
Write-Host '{"message": "Find movies similar to The Matrix"}'
Write-Host ""
Write-Host "Response:" -ForegroundColor Cyan

try {
    if ($PSVersionTable.PSVersion.Major -ge 6) {
        $response1 = Invoke-RestMethod -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"message": "Find movies similar to The Matrix"}' -SkipCertificateCheck
    } else {
        $response1 = Invoke-RestMethod -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"message": "Find movies similar to The Matrix"}'
    }
    $response1 | ConvertTo-Json -Depth 10
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host ""

# Test 2: Movie recommendation request
Write-Host "Test 2: Movie recommendation request" -ForegroundColor Yellow
Write-Host "Request:"
Write-Host "POST $API_URL"
Write-Host '{"message": "Recommend some action movies from the 90s"}'
Write-Host ""
Write-Host "Response:" -ForegroundColor Cyan

try {
    if ($PSVersionTable.PSVersion.Major -ge 6) {
        $response2 = Invoke-RestMethod -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"message": "Recommend some action movies from the 90s"}' -SkipCertificateCheck
    } else {
        $response2 = Invoke-RestMethod -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"message": "Recommend some action movies from the 90s"}'
    }
    $response2 | ConvertTo-Json -Depth 10
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host ""

# Test 3: Specific movie search
Write-Host "Test 3: Specific movie search" -ForegroundColor Yellow
Write-Host "Request:"
Write-Host "POST $API_URL"
Write-Host '{"message": "Tell me about Inception"}'
Write-Host ""
Write-Host "Response:" -ForegroundColor Cyan

try {
    if ($PSVersionTable.PSVersion.Major -ge 6) {
        $response3 = Invoke-RestMethod -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"message": "Tell me about Inception"}' -SkipCertificateCheck
    } else {
        $response3 = Invoke-RestMethod -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"message": "Tell me about Inception"}'
    }
    $response3 | ConvertTo-Json -Depth 10
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host ""

# Test 4: Genre-based search
Write-Host "Test 4: Genre-based search" -ForegroundColor Yellow
Write-Host "Request:"
Write-Host "POST $API_URL"
Write-Host '{"message": "Show me some horror movies"}'
Write-Host ""
Write-Host "Response:" -ForegroundColor Cyan

try {
    if ($PSVersionTable.PSVersion.Major -ge 6) {
        $response4 = Invoke-RestMethod -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"message": "Show me some horror movies"}' -SkipCertificateCheck
    } else {
        $response4 = Invoke-RestMethod -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"message": "Show me some horror movies"}'
    }
    $response4 | ConvertTo-Json -Depth 10
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host ""

# Test 5: Non-movie query (should return empty array)
Write-Host "Test 5: Non-movie query (should return empty array)" -ForegroundColor Yellow
Write-Host "Request:"
Write-Host "POST $API_URL"
Write-Host '{"message": "What is the weather today?"}'
Write-Host ""
Write-Host "Response:" -ForegroundColor Cyan

try {
    if ($PSVersionTable.PSVersion.Major -ge 6) {
        $response5 = Invoke-RestMethod -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"message": "What is the weather today?"}' -SkipCertificateCheck
    } else {
        $response5 = Invoke-RestMethod -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"message": "What is the weather today?"}'
    }
    $response5 | ConvertTo-Json -Depth 10
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host ""

# Test 6: Empty message (should return error)
Write-Host "Test 6: Empty message (should return error)" -ForegroundColor Yellow
Write-Host "Request:"
Write-Host "POST $API_URL"
Write-Host '{"message": ""}'
Write-Host ""
Write-Host "Response:" -ForegroundColor Cyan

try {
    if ($PSVersionTable.PSVersion.Major -ge 6) {
        $response6 = Invoke-RestMethod -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"message": ""}' -SkipCertificateCheck
    } else {
        $response6 = Invoke-RestMethod -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"message": ""}'
    }
    $response6 | ConvertTo-Json -Depth 10
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host ""

# Test 7: Rate limit headers check
Write-Host "Test 7: Check rate limit headers" -ForegroundColor Yellow
Write-Host "Request:"
Write-Host "POST $API_URL"
Write-Host '{"message": "Find movies like Star Wars"}'
Write-Host ""
Write-Host "Response with headers:" -ForegroundColor Cyan

try {
    if ($PSVersionTable.PSVersion.Major -ge 6) {
        $response7 = Invoke-WebRequest -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"message": "Find movies like Star Wars"}' -SkipCertificateCheck
    } else {
        $response7 = Invoke-WebRequest -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"message": "Find movies like Star Wars"}'
    }
    Write-Host "Status Code: $($response7.StatusCode)"
    Write-Host "Rate Limit Headers:"
    Write-Host "  X-RateLimit-Limit: $($response7.Headers['X-RateLimit-Limit'])"
    Write-Host "  X-RateLimit-Remaining: $($response7.Headers['X-RateLimit-Remaining'])"
    Write-Host "  X-RateLimit-Reset: $($response7.Headers['X-RateLimit-Reset'])"
    Write-Host ""
    Write-Host "Response Body:"
    $response7.Content | ConvertFrom-Json | ConvertTo-Json -Depth 10
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host ""

Write-Host "API Testing Complete!" -ForegroundColor Green
Write-Host ""
Write-Host "PowerShell Version: $($PSVersionTable.PSVersion)" -ForegroundColor Yellow
if ($PSVersionTable.PSVersion.Major -lt 6) {
    Write-Host "Note: Using Windows PowerShell with custom SSL certificate handling." -ForegroundColor Yellow
} else {
    Write-Host "Note: Using PowerShell Core with -SkipCertificateCheck parameter." -ForegroundColor Yellow
}
Write-Host "In production, you should use proper SSL certificates." -ForegroundColor Yellow 