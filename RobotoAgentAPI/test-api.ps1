# Movie Chat API Test Script (PowerShell)
# Make sure the API is running on https://localhost:5001 before running this script

$API_URL = "http://localhost:5000/api/chat"
$API_URL_AGENTIC = "http://localhost:5000/api/chat/agentic"

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

# Test 1: Basic movie search (Manual Orchestration)
Write-Host "Test 1: Basic movie search for 'The Matrix' (Manual Orchestration)" -ForegroundColor Yellow
Write-Host "Request:"
Write-Host "POST $API_URL"
Write-Host '{"userId": "test-user-1", "query": "Find movies similar to The Matrix"}'
Write-Host ""
Write-Host "Response:" -ForegroundColor Cyan

try {
    if ($PSVersionTable.PSVersion.Major -ge 6) {
        $response1 = Invoke-RestMethod -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"userId": "test-user-1", "query": "Find movies similar to The Matrix"}' -SkipCertificateCheck
    } else {
        $response1 = Invoke-RestMethod -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"userId": "test-user-1", "query": "Find movies similar to The Matrix"}'
    }
    $response1 | ConvertTo-Json -Depth 10
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host ""

# Test 1b: Same query with Agentic approach
Write-Host "Test 1b: Basic movie search for 'The Matrix' (Agentic Approach)" -ForegroundColor Yellow
Write-Host "Request:"
Write-Host "POST $API_URL_AGENTIC"
Write-Host '{"userId": "test-user-1", "query": "Find movies similar to The Matrix"}'
Write-Host ""
Write-Host "Response:" -ForegroundColor Cyan

try {
    if ($PSVersionTable.PSVersion.Major -ge 6) {
        $response1b = Invoke-RestMethod -Uri $API_URL_AGENTIC -Method POST -ContentType "application/json" -Body '{"userId": "test-user-1", "query": "Find movies similar to The Matrix"}' -SkipCertificateCheck
    } else {
        $response1b = Invoke-RestMethod -Uri $API_URL_AGENTIC -Method POST -ContentType "application/json" -Body '{"userId": "test-user-1", "query": "Find movies similar to The Matrix"}'
    }
    $response1b | ConvertTo-Json -Depth 10
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host ""

# Test 2: Movie recommendation request
Write-Host "Test 2: Movie recommendation request (Manual)" -ForegroundColor Yellow
Write-Host "Request:"
Write-Host "POST $API_URL"
Write-Host '{"userId": "test-user-2", "query": "Recommend some action movies from the 90s"}'
Write-Host ""
Write-Host "Response:" -ForegroundColor Cyan

try {
    if ($PSVersionTable.PSVersion.Major -ge 6) {
        $response2 = Invoke-RestMethod -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"userId": "test-user-2", "query": "Recommend some action movies from the 90s"}' -SkipCertificateCheck
    } else {
        $response2 = Invoke-RestMethod -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"userId": "test-user-2", "query": "Recommend some action movies from the 90s"}'
    }
    $response2 | ConvertTo-Json -Depth 10
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host ""

# Test 3: Specific movie search
Write-Host "Test 3: Specific movie search (Manual)" -ForegroundColor Yellow
Write-Host "Request:"
Write-Host "POST $API_URL"
Write-Host '{"userId": "test-user-3", "query": "Tell me about Inception"}'
Write-Host ""
Write-Host "Response:" -ForegroundColor Cyan

try {
    if ($PSVersionTable.PSVersion.Major -ge 6) {
        $response3 = Invoke-RestMethod -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"userId": "test-user-3", "query": "Tell me about Inception"}' -SkipCertificateCheck
    } else {
        $response3 = Invoke-RestMethod -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"userId": "test-user-3", "query": "Tell me about Inception"}'
    }
    $response3 | ConvertTo-Json -Depth 10
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host ""

# Test 3b: Same query with Agentic approach
Write-Host "Test 3b: Specific movie search (Agentic)" -ForegroundColor Yellow
Write-Host "Request:"
Write-Host "POST $API_URL_AGENTIC"
Write-Host '{"userId": "test-user-3", "query": "Tell me about Inception"}'
Write-Host ""
Write-Host "Response:" -ForegroundColor Cyan

try {
    if ($PSVersionTable.PSVersion.Major -ge 6) {
        $response3b = Invoke-RestMethod -Uri $API_URL_AGENTIC -Method POST -ContentType "application/json" -Body '{"userId": "test-user-3", "query": "Tell me about Inception"}' -SkipCertificateCheck
    } else {
        $response3b = Invoke-RestMethod -Uri $API_URL_AGENTIC -Method POST -ContentType "application/json" -Body '{"userId": "test-user-3", "query": "Tell me about Inception"}'
    }
    $response3b | ConvertTo-Json -Depth 10
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host ""

# Test 4: Genre-based search
Write-Host "Test 4: Genre-based search (Manual)" -ForegroundColor Yellow
Write-Host "Request:"
Write-Host "POST $API_URL"
Write-Host '{"userId": "test-user-4", "query": "Show me some horror movies"}'
Write-Host ""
Write-Host "Response:" -ForegroundColor Cyan

try {
    if ($PSVersionTable.PSVersion.Major -ge 6) {
        $response4 = Invoke-RestMethod -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"userId": "test-user-4", "query": "Show me some horror movies"}' -SkipCertificateCheck
    } else {
        $response4 = Invoke-RestMethod -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"userId": "test-user-4", "query": "Show me some horror movies"}'
    }
    $response4 | ConvertTo-Json -Depth 10
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host ""

# Test 5: Non-movie query (should return polite rejection)
Write-Host "Test 5: Non-movie query (should return polite rejection)" -ForegroundColor Yellow
Write-Host "Request:"
Write-Host "POST $API_URL"
Write-Host '{"userId": "test-user-5", "query": "What is the weather today?"}'
Write-Host ""
Write-Host "Response:" -ForegroundColor Cyan

try {
    if ($PSVersionTable.PSVersion.Major -ge 6) {
        $response5 = Invoke-RestMethod -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"userId": "test-user-5", "query": "What is the weather today?"}' -SkipCertificateCheck
    } else {
        $response5 = Invoke-RestMethod -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"userId": "test-user-5", "query": "What is the weather today?"}'
    }
    $response5 | ConvertTo-Json -Depth 10
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host ""

# Test 6: Empty query (should return error)
Write-Host "Test 6: Empty query (should return error)" -ForegroundColor Yellow
Write-Host "Request:"
Write-Host "POST $API_URL"
Write-Host '{"userId": "test-user-6", "query": ""}'
Write-Host ""
Write-Host "Response:" -ForegroundColor Cyan

try {
    if ($PSVersionTable.PSVersion.Major -ge 6) {
        $response6 = Invoke-RestMethod -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"userId": "test-user-6", "query": ""}' -SkipCertificateCheck
    } else {
        $response6 = Invoke-RestMethod -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"userId": "test-user-6", "query": ""}'
    }
    $response6 | ConvertTo-Json -Depth 10
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host ""

# Test 7: Conversation continuity test
Write-Host "Test 7: Conversation continuity test" -ForegroundColor Yellow
Write-Host "Part 1 - Initial query:"
Write-Host "POST $API_URL"
Write-Host '{"userId": "conversation-test", "query": "Tell me about The Matrix"}'
Write-Host ""
Write-Host "Response:" -ForegroundColor Cyan

try {
    if ($PSVersionTable.PSVersion.Major -ge 6) {
        $response7a = Invoke-RestMethod -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"userId": "conversation-test", "query": "Tell me about The Matrix"}' -SkipCertificateCheck
    } else {
        $response7a = Invoke-RestMethod -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"userId": "conversation-test", "query": "Tell me about The Matrix"}'
    }
    $response7a | ConvertTo-Json -Depth 10
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "Part 2 - Follow-up query (should understand context):"
Write-Host "POST $API_URL"
Write-Host '{"userId": "conversation-test", "query": "What about the sequels?"}'
Write-Host ""
Write-Host "Response:" -ForegroundColor Cyan

try {
    Start-Sleep -Seconds 2  # Brief pause between related requests
    if ($PSVersionTable.PSVersion.Major -ge 6) {
        $response7b = Invoke-RestMethod -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"userId": "conversation-test", "query": "What about the sequels?"}' -SkipCertificateCheck
    } else {
        $response7b = Invoke-RestMethod -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"userId": "conversation-test", "query": "What about the sequels?"}'
    }
    $response7b | ConvertTo-Json -Depth 10
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host ""

# Test 8: Rate limit headers check
Write-Host "Test 8: Check rate limit headers" -ForegroundColor Yellow
Write-Host "Request:"
Write-Host "POST $API_URL"
Write-Host '{"userId": "rate-limit-test", "query": "Find movies like Star Wars"}'
Write-Host ""
Write-Host "Response with headers:" -ForegroundColor Cyan

try {
    if ($PSVersionTable.PSVersion.Major -ge 6) {
        $response8 = Invoke-WebRequest -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"userId": "rate-limit-test", "query": "Find movies like Star Wars"}' -SkipCertificateCheck
    } else {
        $response8 = Invoke-WebRequest -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"userId": "rate-limit-test", "query": "Find movies like Star Wars"}'
    }
    Write-Host "Status Code: $($response8.StatusCode)"
    Write-Host "Rate Limit Headers:"
    Write-Host "  X-RateLimit-Limit: $($response8.Headers['X-RateLimit-Limit'])"
    Write-Host "  X-RateLimit-Remaining: $($response8.Headers['X-RateLimit-Remaining'])"
    Write-Host "  X-RateLimit-Reset: $($response8.Headers['X-RateLimit-Reset'])"
    Write-Host ""
    Write-Host "Response Body:"
    $response8.Content | ConvertFrom-Json | ConvertTo-Json -Depth 10
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host ""

# Test 9: Agentic approach comparison test
Write-Host "Test 9: Agentic vs Manual approach comparison" -ForegroundColor Yellow
Write-Host "Testing complex query with both approaches:"
Write-Host '{"userId": "comparison-test", "query": "I like sci-fi movies with time travel themes. What would you recommend?"}'
Write-Host ""

Write-Host "Manual Orchestration Response:" -ForegroundColor Cyan
try {
    if ($PSVersionTable.PSVersion.Major -ge 6) {
        $responseManual = Invoke-RestMethod -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"userId": "comparison-test", "query": "I like sci-fi movies with time travel themes. What would you recommend?"}' -SkipCertificateCheck
    } else {
        $responseManual = Invoke-RestMethod -Uri $API_URL -Method POST -ContentType "application/json" -Body '{"userId": "comparison-test", "query": "I like sci-fi movies with time travel themes. What would you recommend?"}'
    }
    $responseManual | ConvertTo-Json -Depth 10
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "Agentic Approach Response:" -ForegroundColor Cyan
try {
    if ($PSVersionTable.PSVersion.Major -ge 6) {
        $responseAgentic = Invoke-RestMethod -Uri $API_URL_AGENTIC -Method POST -ContentType "application/json" -Body '{"userId": "comparison-test", "query": "I like sci-fi movies with time travel themes. What would you recommend?"}' -SkipCertificateCheck
    } else {
        $responseAgentic = Invoke-RestMethod -Uri $API_URL_AGENTIC -Method POST -ContentType "application/json" -Body '{"userId": "comparison-test", "query": "I like sci-fi movies with time travel themes. What would you recommend?"}'
    }
    $responseAgentic | ConvertTo-Json -Depth 10
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host ""

Write-Host "API Testing Complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Tests included:" -ForegroundColor Yellow
Write-Host "  ✓ Basic movie search (Manual and Agentic)" -ForegroundColor Green
Write-Host "  ✓ Movie recommendations" -ForegroundColor Green
Write-Host "  ✓ Specific movie queries" -ForegroundColor Green
Write-Host "  ✓ Genre-based searches" -ForegroundColor Green
Write-Host "  ✓ Non-movie query handling" -ForegroundColor Green
Write-Host "  ✓ Empty query validation" -ForegroundColor Green
Write-Host "  ✓ Conversation continuity" -ForegroundColor Green
Write-Host "  ✓ Rate limiting headers" -ForegroundColor Green
Write-Host "  ✓ Agentic vs Manual comparison" -ForegroundColor Green
Write-Host ""
Write-Host "PowerShell Version: $($PSVersionTable.PSVersion)" -ForegroundColor Yellow
if ($PSVersionTable.PSVersion.Major -lt 6) {
    Write-Host "Note: Using Windows PowerShell with custom SSL certificate handling." -ForegroundColor Yellow
} else {
    Write-Host "Note: Using PowerShell Core with -SkipCertificateCheck parameter." -ForegroundColor Yellow
}
Write-Host "In production, you should use proper SSL certificates." -ForegroundColor Yellow