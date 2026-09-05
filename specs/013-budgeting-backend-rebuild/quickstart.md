# Quickstart: Rebuild Budgeting Module Backend

**Date**: 2026-09-05
**Feature**: 013-budgeting-backend-rebuild

## Prerequisites

- .NET 10.0 SDK
- SQL Server instance (local or containerized via Aspire)
- Existing database with FiscalYears, Accounts, CostCenters, Suppliers, ApprovalHistory tables

## Validation Scenarios

### 1. Migration Wipes and Recreates

```bash
dotnet ef migrations add RebuildBudgeting --project src/Infrastructure --startup-project src/Web
dotnet ef database update --project src/Infrastructure --startup-project src/Web
```

**Verify**:
- All 7 old Budgeting tables dropped
- 7 new tables created with correct schema
- DocumentSequence rows for BGT/APR/ENC preserved
- BudgetType and BudgetClassification seed data present
- No duplicate seed data on second run

### 2. Build Passes with Zero Warnings

```bash
dotnet build --no-incremental
```

**Verify**: Zero warnings in output

### 3. Unit Tests Pass

```bash
dotnet test tests/Application.UnitTests --filter Category=Budgeting
```

**Verify**: All tests pass (success + failure paths for each command)

### 4. Functional Tests Pass

```bash
dotnet test tests/Application.FunctionalTests --filter Category=Budgeting
```

**Verify**:
- Budget lifecycle (Draft -> Submitted -> Approved -> Active -> Suspended -> Closed)
- Availability computation (Original + Supplement - Reduction - Adjustment)
- Blocking control rejects over-availability
- Warning control allows with audit log entry
- AllowOverrun 3-level null-inherit chain
- Level computed correctly in deep hierarchy (5+ levels)
- IsReversed computed via existence check
- Concurrency conflict on RowVersion

### 5. API Endpoints Respond

```bash
# List budget types (requires auth + BudgetTypes.Read permission)
curl -H "Authorization: Bearer $TOKEN" http://localhost:5000/api/BudgetTypes

# Get availability (requires auth + Encumbrances.Read permission)
curl -H "Authorization: Bearer $TOKEN" "http://localhost:5000/api/Encumbrances/availability?appropriationId=1"
```

**Verify**:
- `200 OK` with correct data
- `401 Unauthorized` without token
- `403 Forbidden` with wrong permission

### 6. Dropped Columns Grep

```bash
# Search Domain entities for dropped columns
grep -r "Level\b" src/Domain/Budgeting/  # Should return zero matches
grep -r "IsReversed" src/Domain/Budgeting/  # Should return zero matches (computed only)
grep -r "FundId\|FiscalYearId" src/Domain/Budgeting/Entities/Appropriation.cs  # Zero matches
grep -r "FundId\|FiscalYearId\|BudgetId\|BudgetItemId" src/Domain/Budgeting/Entities/Encumbrance.cs  # Zero matches (except AppropriationId)
grep -r "ApprovedById\|ApprovedAt\|IsActive" src/Domain/Budgeting/Entities/Budget.cs  # Zero matches
```

### 7. Permission Enforcement

```bash
# Anonymous → 401
curl http://localhost:5000/api/BudgetTypes

# Wrong permission → 403
curl -H "Authorization: Bearer $ANALYST_TOKEN" -X POST http://localhost:5000/api/Budgets -d '{"budgetName":"test"}'
```

## Key Verification Points

| Check | Command | Expected |
|-------|---------|----------|
| Build warnings | `dotnet build` | 0 warnings |
| Unit tests | `dotnet test tests/Application.UnitTests` | 100% pass |
| Functional tests | `dotnet test tests/Application.FunctionalTests` | 100% pass |
| Dropped columns | `grep -r` on Domain entities | 0 matches |
| Availability accuracy | Functional test: Original+Supplement-Reduction | 2 decimal precision |
| Blocking rejection | Functional test: over-availability with Blocking | Explicit failure |
| AllowOverrun chain | Functional test: null->null->false | Resolves to false |
| Level computation | Functional test: 5-level hierarchy | Correct levels |
| IsReversed computation | Functional test: reversal row exists | true via projection |
| Anonymous 401 | curl without token | 401 |
| Wrong permission 403 | curl with wrong role | 403 |
