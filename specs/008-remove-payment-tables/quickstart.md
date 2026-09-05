# Quickstart Validation Guide: Remove Payment Sub-Entity Tables

**Date**: 2026-09-03
**Feature**: 008-remove-payment-tables

## Prerequisites

- .NET 10 SDK (10.0.201)
- SQL Server instance with the ERP-Government database
- The database must have the 4 tables present (AdvancePayments, PaymentExecutions, PaymentAllocations, PaymentMethods)

## Validation Scenarios

### V1: Build Compiles with Zero Warnings

```bash
dotnet build ERP-Government.slnx --warnaserrors
```

**Expected**: Build succeeds with 0 warnings, 0 errors.

### V2: All Remaining Tests Pass

```bash
dotnet test ERP-Government.slnx
```

**Expected**: All tests pass. No test failures. No skipped tests that were previously passing.

### V3: Deleted Tables Absent from Schema

After migration runs, verify tables are gone:

```sql
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME IN ('AdvancePayments', 'PaymentExecutions', 'PaymentAllocations', 'PaymentMethods');
```

**Expected**: 0 rows returned.

### V4: No Code References to Deleted Entities

```bash
# In src/ directory
grep -r "AdvancePayment" src/ --include="*.cs" | grep -v "AdvancePaymentDeletionReport"
grep -r "PaymentExecution[^s]" src/ --include="*.cs" | grep -v "PaymentExecutionDeletionReport"
grep -r "PaymentAllocation" src/ --include="*.cs"
grep -r "class PaymentMethod" src/ --include="*.cs"  # entity class, not enum
```

**Expected**: Zero matches (excluding migration artifact references and the new enum).

### V5: PaymentMethod Enum Used in DTOs

```bash
grep -r "PaymentMethod" src/Application/Payments/Common/DTOs/PaymentOrderDto.cs
grep -r "PaymentMethod" src/Application/Revenue/Common/DTOs/RevenueReceiptDtos.cs
```

**Expected**: Both files reference `PaymentMethod` as an enum type (not `int PaymentMethodId`).

### V6: Deletion Reports Exist

```sql
SELECT COUNT(*) FROM AdvancePaymentDeletionReport;
SELECT COUNT(*) FROM PaymentExecutionDeletionReport;
SELECT COUNT(*) FROM PaymentMethodMigrationReport;
```

**Expected**: Counts match pre-deletion record counts. PaymentMethodMigrationReport may be 0 if all PaymentMethodIds mapped cleanly.

### V7: SecurityAuditLog Entries for Deletions

```sql
SELECT * FROM SecurityAuditLogs
WHERE EventCategory = 'System' AND Action = 'SchemaDeletion'
ORDER BY Timestamp DESC;
```

**Expected**: 4 entries (one per table dropped) with correct TableName targets.

### V8: Migration Reversibility

```bash
dotnet ef database update <PreviousMigration> --project src/Infrastructure --startup-project src/AppHost
```

**Expected**: All 4 tables recreated with original schema. PaymentMethods seed data restored. SecurityPermissions restored. DocumentSequence rows restored.

Then re-run forward migration:
```bash
dotnet ef database update --project src/Infrastructure --startup-project src/AppHost
```

**Expected**: Back to post-migration state. No data loss on round-trip.

### V9: PaymentOrder Permissions Unchanged

```sql
SELECT Code FROM SecurityPermissions WHERE Code LIKE 'Payments.PaymentOrders%';
```

**Expected**: All original PaymentOrder permissions present (View, Create, Submit, Approve, Reject, Cancel, SendToTreasury, Void).

### V10: Deleted Permissions Gone

```sql
SELECT Code FROM SecurityPermissions
WHERE Code LIKE 'Payments.PaymentMethods%'
   OR Code LIKE 'Payments.PaymentExecutions%'
   OR Code LIKE 'Payments.PaymentAllocations%'
   OR Code LIKE 'Payments.AdvancePayments%';
```

**Expected**: 0 rows returned.
