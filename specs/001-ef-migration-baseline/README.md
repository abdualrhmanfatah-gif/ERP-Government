# EF Migration Baseline

## Overview

This feature provides a baseline Entity Framework Core migration that captures the current database schema using a code‑first approach with mandatory reconciliation. It is a one‑time operation for existing databases that need to start using EF Core migrations.

## Key Decisions

- **Database Provider**: SQL Server only (Aspire integration)
- **EF Core Versions**: Supports all currently supported versions (6.x, 7.x, 8.x)
- **Baseline Source**: Generated from the EF model definition, reconciled against the existing database
- **Reconciliation**: When model and database differ, the migration includes up/down operations to apply model changes
- **Credential Management**: Uses existing project configuration (appsettings.json)

## Components

- **Design‑time factory**: `src/Infrastructure/Data/DesignTimeDbContextFactory.cs`
- **Baseline migration**: `src/Infrastructure/Migrations/20260902025015_InitialBaseline.cs`
- **Validation scripts**: `scripts/validate-baseline.ps1`, `scripts/compare-schema.ps1`
- **Documentation**: `quickstart.md`, `research.md`, `data-model.md`

## Usage

```bash
# Generate baseline migration
dotnet ef migrations add InitialBaseline --context ApplicationDbContext --output-dir Migrations

# Validate baseline
./scripts/validate-baseline.ps1
```

## Validation

1. Run `dotnet ef migrations list` – should show `InitialBaseline (Pending)`
2. Run `dotnet ef migrations has-pending-model-changes` – should report no changes
3. Run `./scripts/validate-baseline.ps1` – should pass all checks

## Links

- [Specification](spec.md)
- [Implementation Plan](plan.md)
- [Tasks](tasks.md)
- [Quickstart Guide](quickstart.md)
- [Research](research.md)
- [Data Model](data-model.md)