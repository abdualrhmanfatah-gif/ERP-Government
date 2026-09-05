# Database Schema — Current State (Post-Refactor 015)

**Date**: 2026-09-05  
**Source**: `specs/015-budgeting-backend-completion/data-model.md`

## Renamed Tables

| Old Table | New Table | Migration |
|-----------|-----------|-----------|
| `Moves` | `JournalEntries` | T014 |
| `MoveLines` | `JournalEntryLines` | T014 |

## Renamed Columns

| Table | Old Column | New Column | Migration |
|-------|-----------|------------|-----------|
| `JournalEntries` | `ReversalOfMoveId` | `ReversalOfId` | T014 |
| `JournalEntryLines` | `MoveId` | `JournalEntryId` | T014 |
| `AccountingEvents` | `SourceTable` | `SourceDocumentType` | T043 |
| `AccountingEvents` | `SourceId` | `SourceDocumentId` | T043 |
| `PaymentOrders` | `MoveId` | `JournalEntryId` | T035 |
| `RecurringEntries` | `GeneratedMoveId` | `GeneratedJournalEntryId` | T014 |
| `Appropriations` | `MoveId` | `JournalEntryId` | T014 |
| `Assets` (5 tables) | `MoveId` | `JournalEntryId` | T014 |

## New Enums

| Enum | Values | File |
|------|--------|------|
| `EntryStatus` | Draft=0, Submitted=1, Approved=2, Posted=3, Reversed=4, Cancelled=5 | `src/Domain/Accounting/Enums/EntryStatus.cs` |
| `EventStatus` | Pending=0, Posted=1, Reversed=2 | `src/Domain/Accounting/Enums/EventStatus.cs` |
| `EventType` | ReceiptCollection=0, DepositClearing=1, PaymentExecution=2, Reversal=3, PurchaseOrderApproved=4, PaymentOrderApproved=5, PaymentOrderExecuted=6, RevenueReceiptApproved=7, RevenueReceiptPosted=8, JournalEntryPosted=9, YearEndClosingEntryPosted=10 | `src/Domain/Accounting/Enums/EventType.cs` |
| `EventCategory` | Revenue=0, Expenditure=1, Transfer=2, Adjustment=3, Other=4 | `src/Domain/Accounting/Enums/EventCategory.cs` |
| `PaymentMethod` | Cash=0, Check=1, BankTransfer=2, CreditCard=3, DebitCard=4, InKind=5, Other=6 | `src/Domain/Payments/Enums/PaymentMethod.cs` |

## Dropped Columns (PaymentOrder Aggregate Strip)

### PaymentOrders (T035)
- `AmountNet`, `BaseAmountNet`, `TotalDeductionAmount`, `TotalNetAmount`, `TotalPaidAmount`, `TotalRemainingAmount`, `IsFullyPaid`
- `ApprovedById`, `ApprovedAt`, `RejectedById`, `RejectedAt`, `CancelledById`, `CancelledAt`, `VoidedById`, `VoidedAt`

### PaymentOrderLines (T035)
- `BaseAmount`, `AllocatedAmount`, `RemainingAmount`, `NetAmount`

### PaymentOrderDeductions (T035)
- `BaseAmount`

## New Columns

### AccountingEvents (T043)
- `JournalEntryId` (nullable FK → JournalEntries)

### JournalEntryLines (T024)
- `FundId` (nullable FK → Funds)
- `ProjectId` (nullable FK → Projects)
- `BudgetItemId` (nullable FK → BudgetItems)
- `EncumbranceId` (nullable FK → Encumbrances)
- `PaymentOrderId` (nullable FK → PaymentOrders)

## New Constraints

- `AccountingEvents`: Unique index on `(EventType, SourceDocumentType, SourceDocumentId)` where `Status = Posted` (T043)
- `JournalEntryLines`: CHECK constraint `Debit > 0 XOR Credit > 0` (T014)
- `JournalEntries`: CHECK constraint `EntryStatus IN (0,1,2,3,4,5)` (T014)

## Budgeting Tables (015)

| Table | Key Columns | Constraints |
|-------|-------------|-------------|
| `Budgets` | Id, FundId, BudgetTypeId, FiscalYearId, Code, Description, TotalAmount, AllowOverrun (nullable bool), EffectiveFrom, EffectiveTo (nullable), Status (enum), RowVersion | UNIQUE (Code, FiscalYearId) |
| `BudgetItems` | Id, BudgetId, ClassificationId, BudgetNumber, Description, Remarks, RowVersion | FK → Budgets |
| `Appropriations` | Id, BudgetItemId, AppropriationNumber, Effect (0=Normal,1=Transfer), Amount, Status (enum), TargetBudgetItemId (nullable FK → BudgetItems), RowVersion | FK → BudgetItems; Index on TargetBudgetItemId |
| `Encumbrances` | Id, AppropriationId, EncumbranceNumber, Type, Description, VendorId, Amount, Status (enum), PostedById, PostedAt, ReversalOfId (nullable FK → self), RowVersion | FK → Appropriations; Index on VendorId |
| `BudgetItemMonthlyPlans` | Id, BudgetItemId, Month (1–12), PlannedAmount (decimal 23,2 ≥ 0), RowVersion | FK → BudgetItems; UNIQUE (BudgetItemId, Month) |

### Budgeting Enums

| Enum | Values |
|------|--------|
| `AppropriationStatus` | Draft=0, Submitted=1, Approved=2, Activated=3, Suspended=4, Closed=5, Cancelled=6, Reversed=7 |
| `EncumbranceStatus` | Draft=0, Submitted=1, Approved=2, Activated=3, Suspended=4, Closed=5, Cancelled=6, Reversed=7 |
| `AppropriationEffect` | Normal=0, Transfer=1 |

## Pending Migrations (require Docker/DB)

| Migration | Task | Description |
|-----------|------|-------------|
| `AccountingCoreRefactor` | T014 | Rename tables, rename columns, EntryStatus string→enum, CHECK constraints |
| `AddAnalyticDimensions` | T024 | Add 5 nullable FK columns to JournalEntryLines |
| `StripPaymentOrderAggregates` | T035 | Drop stored aggregate/approval columns from PaymentOrder* |
| `ExtendAccountingEvent` | T043 | Add JournalEntryId FK, rename columns, unique constraint, EventType string→enum |
| `ExtendPaymentMethod` | T049 | Renumber Other 5→6 for InKind insertion (if data migration needed) |
