# Quickstart Validation: Remove Liquidations and BudgetLedgerEntries

**Feature**: 009-remove-liquidations-budgetledger
**Date**: 2026-09-04

## Prerequisites

- .NET 10.0 SDK installed
- SQL Server instance available (via Aspire or local)
- Project builds cleanly before changes: `dotnet build`

## Validation Scenarios

### V1: Compilation passes with zero warnings

```bash
dotnet build ERP-Government.slnx -warnaserror
```

**Expected**: Build succeeds, zero warnings.

### V2: All remaining tests pass

```bash
dotnet test ERP-Government.slnx --no-build
```

**Expected**: 100% pass rate, zero failures.

### V3: Grep for Liquidation|BudgetLedgerEntry returns zero matches

```bash
rg "Liquidation|BudgetLedgerEntry" src/ tests/ --glob '!**/Migrations/**' --glob '!**/DeletionReport*'
```

**Expected**: Zero matches. Only historical migration files and deletion report artifacts may contain references.

### V4: Tables absent from model snapshot

```bash
rg "Liquidations|BudgetLedgerEntries" src/Infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs
```

**Expected**: Zero matches (tables removed from snapshot).

### V5: PaymentOrders has no LiquidationId column

```bash
rg "LiquidationId" src/Infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs
```

**Expected**: Zero matches.

### V6: OpenAPI document has no Liquidation/BudgetLedgerEntry schemas

```bash
rg "Liquidation|BudgetLedgerEntry" src/Web/wwwroot/openapi/v1.json
```

**Expected**: Zero matches. (Regenerate with `dotnet run --project src/Web` if needed.)

### V7: Constitution updated

```bash
rg "Liquidation" .specify/memory/constitution.md
```

**Expected**: Zero matches in Principle V chain. Decision record may reference "Liquidation" by name.

### V8: Migration applies cleanly on fresh DB

```bash
dotnet ef database update --project src/Infrastructure --startup-project src/Web
```

**Expected**: Migration applies without errors. Liquidations and BudgetLedgerEntries tables absent. Deletion report tables present.

### V9: Migration rolls back cleanly

```bash
dotnet ef database update LastGoodMigration --project src/Infrastructure --startup-project src/Web
```

**Expected**: Down migration recreates all dropped tables and columns.

### V10: Deletion report tables exist with correct schema

After migration up, query the database:
```sql
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LiquidationDeletionReport';
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'BudgetLedgerEntryDeletionReport';
```

**Expected**: LiquidationDeletionReport has 7 columns (Id, LiquidationNumber, EncumbranceId, Amount, Status, CreatedAt, DeletedAt). BudgetLedgerEntryDeletionReport has 8 columns (Id, BudgetId, EntryType, Amount, Direction, Status, OccurredAt, DeletedAt).
