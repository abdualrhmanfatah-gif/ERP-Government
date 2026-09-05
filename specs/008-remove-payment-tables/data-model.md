# Data Model: Remove Payment Sub-Entity Tables and Convert PaymentMethod to Enum

**Date**: 2026-09-03
**Feature**: 008-remove-payment-tables

## Changes Summary

| Action | Entity/Table | Scope |
|--------|-------------|-------|
| DELETE | PaymentMethod (entity) | Domain/Payments/Entities/PaymentMethod.cs |
| DELETE | AdvancePayment (entity) | Domain/Payments/Entities/AdvancePayment.cs |
| DELETE | PaymentExecution (entity) | Domain/Payments/Entities/PaymentExecution.cs |
| DELETE | PaymentAllocation (entity) | Domain/Payments/Entities/PaymentAllocation.cs |
| ADD | PaymentMethod (enum) | Domain/Payments/Enums/PaymentMethod.cs |
| MODIFY | PaymentOrder | Replace `int PaymentMethodId` with `PaymentMethod PaymentMethod` (enum) |
| MODIFY | RevenueReceipt | Replace `int PaymentMethodId` with `PaymentMethod PaymentMethod` (enum) |
| DELETE | PaymentMethods table | SQL Server table + seed data |
| DELETE | AdvancePayments table | SQL Server table |
| DELETE | PaymentExecutions table | SQL Server table |
| DELETE | PaymentAllocations table | SQL Server table |
| ADD | AdvancePaymentDeletionReport | Temporary migration artifact table |
| ADD | PaymentExecutionDeletionReport | Temporary migration artifact table |
| ADD | PaymentMethodMigrationReport | Temporary migration artifact table |

## New Enum: PaymentMethod

```
Domain/Payments/Enums/PaymentMethod.cs

public enum PaymentMethod
{
    Cash = 0,
    BankTransfer = 1,
    Check = 2,
    CreditCard = 3,
    WireTransfer = 4,
    Other = 5
}
```

## Modified Entities

### PaymentOrder (after migration)

```
Domain/Payments/Entities/PaymentOrder.cs

- Remove: int PaymentMethodId
+ Add:    PaymentMethod PaymentMethod  (enum, stored as int column)

All other fields unchanged.
State machine unchanged: Draft(0) → Submitted(1) → Approved(2) → SentToTreasury(3) → Paid(4)/PartiallyPaid(5)
```

### RevenueReceipt (after migration)

```
Domain/Revenue/Entities/RevenueReceipt.cs

- Remove: int PaymentMethodId
+ Add:    PaymentMethod PaymentMethod  (enum, stored as int column)

All other fields unchanged.
State machine unchanged: Draft(0) → Approved(1) → Posted(2)
```

## Deleted Entities (for reference)

### PaymentMethod (entity — being deleted)

```
Fields: Id, Code, Name, PaymentType, IsCash, IsBank, IsElectronic,
        RequiresBankAccount, RequiresBeneficiaryIban, RequiresTreasuryApproval,
        IsImmediate, DefaultCurrencyId, MaxAmount, MinAmount, IsActive,
        CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, RowVersion
```

### AdvancePayment (being deleted)

```
Fields: Id, AdvanceNumber, PaymentOrderId, PaymentExecutionId, VendorId,
        FundId, CurrencyId, Amount, Description, Status, MoveId,
        ConsumedAmount, ConsumedDate, CancelledAt, CancelledById,
        CancelledReason, CreatedAt, CreatedBy, RowVersion
```

### PaymentExecution (being deleted)

```
Fields: Id, ExecutionNumber, PaymentOrderId, PaymentMethodId, BankAccountId,
        Amount, Status, ScheduledDate, ExecutedDate, ReferenceNumber,
        Notes, MoveId, ApprovedById, ApprovedAt, SentAt, CompletedAt,
        CreatedAt, CreatedBy, RowVersion
```

### PaymentAllocation (being deleted)

```
Fields: Id, PaymentExecutionId, PaymentOrderId, InvoiceId, Amount,
        Status, Notes, ConfirmedAt, ConfirmedById, ReversedAt,
        ReversedById, ReverseReason, CreatedAt, CreatedBy, RowVersion
```

## Entity Relationships (Before → After)

### Before

```
PaymentExecutions ──FK(Restrict)──> PaymentOrders
PaymentAllocations ──FK(Restrict)──> PaymentExecutions
PaymentOrders ──Index(No FK)──> PaymentMethods
PaymentExecutions ──Index(No FK)──> PaymentMethods
RevenueReceipts ──Index(No FK)──> PaymentMethods
AdvancePayments ──Index(No FK)──> PaymentOrders
AdvancePayments ──Index(No FK)──> PaymentExecutions
```

### After

```
PaymentOrders ──(enum int column)──> PaymentMethod enum
RevenueReceipts ──(enum int column)──> PaymentMethod enum
```

## Migration Schema Changes

### Step 1: Add nullable enum columns

```sql
ALTER TABLE PaymentOrders ADD PaymentMethod int NULL;
ALTER TABLE RevenueReceipts ADD PaymentMethod int NULL;
```

### Step 2: Backfill from PaymentMethods lookup

```sql
-- PaymentOrders
UPDATE po SET po.PaymentMethod = CASE
    WHEN pm.Code = 'Cash' THEN 0
    WHEN pm.Code = 'BankTransfer' THEN 1
    WHEN pm.Code = 'Check' THEN 2
    WHEN pm.Code = 'CreditCard' THEN 3
    WHEN pm.Code = 'WireTransfer' THEN 4
    WHEN pm.Code = 'Other' THEN 5
    ELSE 5  -- Default to Other for unmapped
END
FROM PaymentOrders po
LEFT JOIN PaymentMethods pm ON po.PaymentMethodId = pm.Id;

-- RevenueReceipts (same mapping)
UPDATE rr SET rr.PaymentMethod = CASE ...
FROM RevenueReceipts rr
LEFT JOIN PaymentMethods pm ON rr.PaymentMethodId = pm.Id;
```

### Step 3: Fail if unmapped values remain

```sql
-- Check for NULL PaymentMethod after backfill
IF EXISTS (SELECT 1 FROM PaymentOrders WHERE PaymentMethod IS NULL)
    THROW 50001, 'Unmapped PaymentMethodId in PaymentOrders', 1;
IF EXISTS (SELECT 1 FROM RevenueReceipts WHERE PaymentMethod IS NULL)
    THROW 50002, 'Unmapped PaymentMethodId in RevenueReceipts', 1;
```

### Step 4: Drop old FK columns and indexes

```sql
-- Drop indexes first
DROP INDEX IX_PaymentOrders_PaymentMethodId ON PaymentOrders;
DROP INDEX IX_RevenueReceipts_PaymentMethodId ON RevenueReceipts;
DROP INDEX IX_PaymentExecutions_PaymentMethodId ON PaymentExecutions;

-- Drop columns
ALTER TABLE PaymentOrders DROP COLUMN PaymentMethodId;
ALTER TABLE RevenueReceipts DROP COLUMN PaymentMethodId;
```

### Step 5: Alter new columns to NOT NULL

```sql
ALTER TABLE PaymentOrders ALTER COLUMN PaymentMethod int NOT NULL DEFAULT 5;
ALTER TABLE RevenueReceipts ALTER COLUMN PaymentMethod int NOT NULL DEFAULT 5;
```

### Step 6: Drop tables (in FK dependency order)

```sql
-- No inbound FKs
DROP TABLE PaymentAllocations;
DROP TABLE PaymentExecutions;
DROP TABLE PaymentMethods;
DROP TABLE AdvancePayments;
```

### Step 7: Delete DocumentSequence rows

```sql
DELETE FROM DocumentSequences WHERE DocumentType IN ('PaymentExecution', 'AdvancePayment');
```

### Step 8: Delete SecurityPermission rows

```sql
DELETE FROM SecurityPermissions WHERE Code LIKE 'Payments.PaymentMethods%'
    OR Code LIKE 'Payments.PaymentExecutions%'
    OR Code LIKE 'Payments.PaymentAllocations%'
    OR Code LIKE 'Payments.AdvancePayments%';
```

## Migration Artifact Tables

### AdvancePaymentDeletionReport

```sql
CREATE TABLE AdvancePaymentDeletionReport (
    Id int NOT NULL,
    Number nvarchar(max) NULL,
    Amount decimal(18,2) NOT NULL,
    Status int NOT NULL,
    CreatedAt datetimeoffset NOT NULL
);
-- Populated: INSERT INTO ... SELECT Id, AdvanceNumber, Amount, Status, CreatedAt FROM AdvancePayments
```

### PaymentExecutionDeletionReport

```sql
CREATE TABLE PaymentExecutionDeletionReport (
    Id int NOT NULL,
    Number nvarchar(max) NULL,
    Status int NOT NULL,
    Amount decimal(18,2) NOT NULL,
    CreatedAt datetimeoffset NOT NULL
);
-- Populated: INSERT INTO ... SELECT Id, ExecutionNumber, Status, Amount, CreatedAt FROM PaymentExecutions
```

### PaymentMethodMigrationReport

```sql
CREATE TABLE PaymentMethodMigrationReport (
    Id int IDENTITY(1,1) NOT NULL,
    TableName nvarchar(max) NOT NULL,
    RecordId int NOT NULL,
    OldPaymentMethodId int NULL,
    NewPaymentMethod int NOT NULL,
    WarningNote nvarchar(max) NULL,
    CreatedAt datetimeoffset NOT NULL DEFAULT SYSUTCDATETIME()
);
-- Populated during backfill for deactivated/unmapped PaymentMethodId records
```
