param(
    [string]$Context = "ApplicationDbContext",
    [string]$Project = "src/Infrastructure/Infrastructure.csproj",
    [string]$StartupProject = "src/Web/Web.csproj"
)

$ErrorActionPreference = "Stop"

Write-Host "=== Comparing EF Model vs Database Schema ===" -ForegroundColor Cyan

# This script uses EF Core metadata to compare model and database
# For baseline, we ensure migration covers 100% of model

Write-Host "Listing migrations..." -ForegroundColor Yellow
dotnet ef migrations list --context $Context --project $Project --startup-project $StartupProject

Write-Host ""
Write-Host "Checking for pending model changes..." -ForegroundColor Yellow
$hasChanges = dotnet ef migrations has-pending-model-changes --context $Context --project $Project --startup-project $StartupProject 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "No pending model changes - baseline is reconciled" -ForegroundColor Green
} else {
    Write-Host "Pending model changes detected:" -ForegroundColor Yellow
    Write-Host $hasChanges
    Write-Host "Baseline needs reconciliation - run migrations add" -ForegroundColor Yellow
}

Write-Host "=== Compare Complete ===" -ForegroundColor Green
