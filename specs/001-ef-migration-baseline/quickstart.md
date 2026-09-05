# Quickstart: EF Migration Baseline Validation

**Date**: 2026-09-02
**Feature**: EF Migration Baseline

## Prerequisites

- .NET SDK 10.0 installed
- SQL Server instance accessible (local or remote)
- Entity Framework Core tools installed (`dotnet tool install --global dotnet-ef`)
- Project configured with EF Core and SQL Server provider

## Validation Scenarios

### Scenario 1: Generate Baseline Migration

**Goal**: Verify that a baseline migration can be generated from the EF model.

**Steps**:

1. Navigate to the Infrastructure project directory.
2. Run the command to generate a baseline migration:
   ```bash
   dotnet ef migrations add InitialBaseline --context YourDbContext --output-dir Data/Migrations
   ```
3. Confirm that migration files are created in `Data/Migrations` with up and down operations.
4. Inspect the migration file to ensure it contains schema creation operations.

**Expected Outcome**: Migration files exist and represent the current model.

### Scenario 2: Validate Baseline Against Database

**Goal**: Ensure the baseline migration accurately reflects the existing database schema.

**Steps**:

1. Apply the baseline migration to a test database:
   ```bash
   dotnet ef database update --context YourDbContext
   ```
2. Run a validation script that compares the database schema with the EF model snapshot.
3. Check for any discrepancies.

**Expected Outcome**: No discrepancies found; database schema matches the model.

### Scenario 3: Generate Subsequent Migration

**Goal**: Verify that subsequent migrations are correctly generated relative to the baseline.

**Steps**:

1. Make a small change to the model (e.g., add a nullable column).
2. Generate a new migration:
   ```bash
   dotnet ef migrations add AddNullableColumn --context YourDbContext --output-dir Data/Migrations
   ```
3. Inspect the new migration to ensure it only contains the added column, not the entire schema.

**Expected Outcome**: New migration contains only the delta from the baseline.

### Scenario 4: Revert Migration

**Goal**: Confirm that the baseline migration can be reverted.

**Steps**:

1. Revert the most recent migration:
   ```bash
   dotnet ef database update PreviousMigration --context YourDbContext
   ```
2. Check that the schema changes are rolled back.

**Expected Outcome**: Database schema returns to the previous state.

### Scenario 5: Cross‑Version Compatibility

**Goal**: Test that the baseline migration works with different EF Core versions.

**Steps**:

1. Change the EF Core package version in the project file (6.x, 7.x, 8.x).
2. Restore packages and regenerate the migration.
3. Apply the migration and validate.

**Expected Outcome**: Migration generates and applies successfully across versions.

## Troubleshooting

- **Connection error**: Verify SQL Server is running and connection string is correct.
- **Migration not generated**: Ensure `dotnet-ef` tools are installed and the project builds.
- **Discrepancies found**: Review the model and database schema; update the model if needed.

## References

- [Entity Framework Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [Design-time DbContext Creation](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/design-time)