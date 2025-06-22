# Run the API in Development mode
$env:ASPNETCORE_ENVIRONMENT = "Development"

Write-Host "Starting API in Development mode..." -ForegroundColor Green
Write-Host "Environment: $env:ASPNETCORE_ENVIRONMENT" -ForegroundColor Yellow
Write-Host "API will be available at:" -ForegroundColor Yellow
Write-Host "  HTTP:  http://localhost:5000" -ForegroundColor Cyan
Write-Host "  HTTPS: https://localhost:5001" -ForegroundColor Cyan
Write-Host ""
Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Yellow
Write-Host ""

dotnet run 