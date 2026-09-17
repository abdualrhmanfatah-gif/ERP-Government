<# 
.SYNOPSIS
Drops the development database, removes existing EF Core migrations, recreates
an InitialCreate migration, and applies it.

.DESCRIPTION
This script is intended for local development only. It uses the Web project as
the working directory so DesignTimeDbContextFactory reads src/Web/appsettings.json.

.EXAMPLE
.\scripts\reset-ef-database.ps1

.EXAMPLE
.\scripts\reset-ef-database.ps1 -Force

.EXAMPLE
.\scripts\reset-ef-database.ps1 -MigrationName InitialCreate
#>

[CmdletBinding()]
param(
    [string]$MigrationName = "InitialCreate",
    [string]$ContextName = "ApplicationDbContext",
    [switch]$Force
)

$ErrorActionPreference = "Stop"

function Resolve-RepoPath {
    param([Parameter(Mandatory = $true)][string]$RelativePath)

    return [System.IO.Path]::GetFullPath((Join-Path $RepoRoot $RelativePath))
}

function Assert-PathInside {
    param(
        [Parameter(Mandatory = $true)][string]$CandidatePath,
        [Parameter(Mandatory = $true)][string]$ParentPath
    )

    $candidate = [System.IO.Path]::GetFullPath($CandidatePath).TrimEnd('\', '/')
    $parent = [System.IO.Path]::GetFullPath($ParentPath).TrimEnd('\', '/')

    if (!$candidate.StartsWith($parent, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to operate outside expected directory. Path: $candidate Parent: $parent"
    }
}

function Invoke-DotNetEf {
    param([Parameter(Mandatory = $true)][string[]]$EfArguments)

    Write-Host ""
    Write-Host "dotnet ef $($EfArguments -join ' ')" -ForegroundColor DarkCyan
    & dotnet ef @EfArguments

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet ef command failed with exit code $LASTEXITCODE."
    }
}

$ScriptDirectory = Split-Path -Parent $PSCommandPath
$RepoRoot = [System.IO.Path]::GetFullPath((Join-Path $ScriptDirectory ".."))
$InfrastructureRoot = Resolve-RepoPath "src/Infrastructure"
$InfrastructureProject = Resolve-RepoPath "src/Infrastructure/Infrastructure.csproj"
$WebRoot = Resolve-RepoPath "src/Web"
$WebProject = Resolve-RepoPath "src/Web/Web.csproj"

$MigrationDirectories = @(
    (Resolve-RepoPath "src/Infrastructure/Data/Migrations"),
    (Resolve-RepoPath "src/Infrastructure/Migrations")
)

Assert-PathInside -CandidatePath $InfrastructureProject -ParentPath $RepoRoot
Assert-PathInside -CandidatePath $WebProject -ParentPath $RepoRoot
Assert-PathInside -CandidatePath $WebRoot -ParentPath $RepoRoot

foreach ($directory in $MigrationDirectories) {
    Assert-PathInside -CandidatePath $directory -ParentPath $InfrastructureRoot
}

if (!(Test-Path -LiteralPath $InfrastructureProject)) {
    throw "Infrastructure project was not found: $InfrastructureProject"
}

if (!(Test-Path -LiteralPath $WebProject)) {
    throw "Web project was not found: $WebProject"
}

Write-Host "This will reset the local EF Core database and migrations." -ForegroundColor Yellow
Write-Host "Repository:             $RepoRoot"
Write-Host "Infrastructure project: $InfrastructureProject"
Write-Host "Web startup project:    $WebProject"
Write-Host "DbContext:              $ContextName"
Write-Host "New migration:          $MigrationName"
Write-Host "Migration directories:"
foreach ($directory in $MigrationDirectories) {
    Write-Host "  - $directory"
}

if (!$Force) {
    Write-Host ""
    Write-Host "WARNING: the configured development database will be dropped." -ForegroundColor Red
    $confirmation = Read-Host "Type DELETE to continue"

    if ($confirmation -ne "DELETE") {
        Write-Host "Cancelled."
        exit 1
    }
}

Push-Location $WebRoot
try {
    & dotnet ef --version | Out-Host
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet-ef is not available. Install it with: dotnet tool install --global dotnet-ef"
    }

    Invoke-DotNetEf @(
        "database", "drop",
        "--force",
        "--project", $InfrastructureProject,
        "--startup-project", $WebProject,
        "--context", $ContextName
    )

    foreach ($directory in $MigrationDirectories) {
        if (Test-Path -LiteralPath $directory) {
            Write-Host ""
            Write-Host "Removing migration directory: $directory" -ForegroundColor DarkCyan
            Remove-Item -LiteralPath $directory -Recurse -Force
        }
    }

    Invoke-DotNetEf @(
        "migrations", "add", $MigrationName,
        "--project", $InfrastructureProject,
        "--startup-project", $WebProject,
        "--context", $ContextName,
        "--output-dir", "Data/Migrations"
    )

    Invoke-DotNetEf @(
        "database", "update",
        "--project", $InfrastructureProject,
        "--startup-project", $WebProject,
        "--context", $ContextName
    )

    Write-Host ""
    Write-Host "Database and EF Core migrations were recreated successfully." -ForegroundColor Green
}
finally {
    Pop-Location
}
