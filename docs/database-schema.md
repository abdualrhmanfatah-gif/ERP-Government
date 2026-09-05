# Database Schema — Current State (Post-Refactor 015 + 016 + 019)

**Date**: 2026-09-06  
**Sources**: `specs/015-budgeting-backend-completion/data-model.md`, `specs/016-unified-party-document/data-model.md`, `specs/019-budget-availability-closing/data-model.md`

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

---

## 016 — Unified Party + Document Infrastructure

### New Tables

| Table | Key Columns | Constraints |
|-------|-------------|-------------|
| `Parties` | Id, PartyCode, PartyType (enum), NameAr, NameEn, TaxNumber, NationalId, Phone, Email, Address, Notes, IsActive, RowVersion | UNIQUE (PartyCode); INDEX (PartyType, IsActive); INDEX (TaxNumber); INDEX (NameAr) |
| `DocumentStatusLogs` | Id, EntityName, DocumentId, FromStatus, ToStatus, ChangedById (FK→Users), ChangedAt, Reason | INDEX (EntityName, DocumentId); INDEX (ChangedAt) |
| `DocumentAttachmentRequirements` | Id, DocumentType, AttachmentTypeCode, TitleAr, IsMandatory, IsActive, RowVersion | UNIQUE (DocumentType, AttachmentTypeCode); INDEX (DocumentType) |

### New Enums

| Enum | Values | File |
|------|--------|------|
| `PartyType` | Supplier=0, Customer=1, GovEntity=2, TaxAuthority=3, Other=4 | `src/Domain/Parties/Enums/PartyType.cs` |
| `ApprovalAction` | Submit=0, Approve=1, Reject=2, Return=3, Cancel=4 | `src/Domain/Security/Enums/ApprovalAction.cs` |

### Modified Tables

#### ApprovalHistory
- **Added**: `ApprovalStep` (int, default 1), `Action` (ApprovalAction enum)
- **Retained**: `Decision` (legacy note; Action is authoritative)

#### Attachments
- **Added**: `DocumentType` (string, required), `IsRequired` (bool, default false), `AttachmentTypeCode` (string, required)

#### PaymentOrders
- **Added**: `VendorPartyId` (int, FK→Parties)

#### Encumbrances
- **Added**: `VendorPartyId` (nullable int, FK→Parties)

#### PurchaseOrders
- **Added**: `SupplierPartyId` (int, FK→Parties)
- **Removed**: `Supplier` navigation property

#### Quotations
- **Added**: `PartyId` (int, FK→Parties)

#### RFQSuppliers
- **Added**: `PartyId` (int, FK→Parties)

### Deleted Tables

| Table | Migration |
|-------|-----------|
| `Suppliers` | 016 migration (data migrated to Parties with first-wins dedup) |

### New Pending Migration

| Migration | Task | Description |
|-----------|------|-------------|
| `AddPartyAndDocumentInfrastructure` | 016 | Create Parties, DocumentStatusLogs, DocumentAttachmentRequirements; alter ApprovalHistory, Attachments; FK migration; data backfill; drop Suppliers; seed prefixes (PTY, RCV, DSL, DSB, PAY) |

## Feature 019: Financial Control Layer

### New Enums

| Enum | Values | File |
|------|--------|------|
| `YearClosingRunStatus` | Completed=0, Reversed=1 | `src/Domain/Budgeting/Enums/YearClosingRunStatus.cs` |
| `FinalAccountStatus` | Draft=0, Issued=1 | `src/Domain/Budgeting/Enums/FinalAccountStatus.cs` |
| `YearClosingRunType` | Lapse=0, Reopen=1 | `src/Domain/Budgeting/Enums/YearClosingRunType.cs` |
| `FinalAccountLineDimension` | Fund=0, Program=1, Project=2, Item=3 | `src/Domain/Budgeting/Enums/FinalAccountLineDimension.cs` |

### New Tables

#### YearClosingRun
Append-only record of fiscal year closing or reopening operations.

| Column | Type | Constraints | Notes |
|--------|------|-------------|-------|
| `Id` | int | PK, Identity | |
| `FiscalYearId` | int | FK→FiscalYears, NOT NULL | |
| `RunAt` | DateTimeOffset | NOT NULL | Timestamp of the run |
| `RunById` | int | FK→Users, NOT NULL | Actor who initiated |
| `RunType` | int | NOT NULL | Lapse=0, Reopen=1 |
| `LapsedAppropriationTotal` | decimal(23,2) | NOT NULL, default 0 | Total appropriation amount lapsed |
| `LapsedEncumbranceTotal` | decimal(23,2) | NOT NULL, default 0 | Total encumbrance amount lapsed |
| `Status` | int | NOT NULL | Completed=0, Reversed=1 |
| `ReversedById` | int? | FK→Users, nullable | Set when Status=Reversed |
| `ReversedAt` | DateTimeOffset? | nullable | Set when Status=Reversed |
| `RowVersion` | byte[] | concurrency token | |
| `Created` | DateTimeOffset | NOT NULL | Audit |
| `CreatedBy` | string? | nullable | Audit |
| `LastModified` | DateTimeOffset | NOT NULL | Audit |
| `LastModifiedBy` | string? | nullable | Audit |

**Indexes**:
- Unique filtered: `(FiscalYearId) WHERE Status = 0` — prevents duplicate active lapses

#### FinalAccount
Authoritative financial statement for a closed fiscal year.

| Column | Type | Constraints | Notes |
|--------|------|-------------|-------|
| `Id` | int | PK, Identity | |
| `FiscalYearId` | int | FK→FiscalYears, UNIQUE, NOT NULL | One final account per year |
| `GeneratedAt` | DateTimeOffset | NOT NULL | |
| `GeneratedById` | int | FK→Users, NOT NULL | |
| `Status` | int | NOT NULL | Draft=0, Issued=1 |
| `IssuedAt` | DateTimeOffset? | nullable | Set on approval |
| `IssuedById` | int? | FK→Users, nullable | Set on approval |
| `RowVersion` | byte[] | concurrency token | |
| `Created` | DateTimeOffset | NOT NULL | Audit |
| `CreatedBy` | string? | nullable | Audit |
| `LastModified` | DateTimeOffset | NOT NULL | Audit |
| `LastModifiedBy` | string? | nullable | Audit |

**State transitions**: Draft → Issued (immutable after Issued)

#### FinalAccountLine
Line item in the final account — materialized at generation time.

| Column | Type | Constraints | Notes |
|--------|------|-------------|-------|
| `Id` | int | PK, Identity | |
| `FinalAccountId` | int | FK→FinalAccounts, NOT NULL | |
| `Dimension` | int | NOT NULL | Fund=0, Program=1, Project=2, Item=3 |
| `DimensionId` | int | NOT NULL | FK value for the dimension entity |
| `DimensionCode` | string | NOT NULL | Denormalized code for display |
| `DimensionName` | string | NOT NULL | Denormalized name for display |
| `BudgetedAmount` | decimal(23,2) | NOT NULL | Appropriation amount |
| `ActualAmount` | decimal(23,2) | NOT NULL | Executed payment amount |
| `Variance` | decimal(23,2) | NOT NULL | BudgetedAmount - ActualAmount |
| `RowVersion` | byte[] | concurrency token | |

**Indexes**:
- Unique: `(FinalAccountId, Dimension, DimensionId)` — one line per dimension value per final account

### New Permission Codes

| Code | Module | Description |
|------|--------|-------------|
| `FinancialControl.LapseYear` | Budgeting | Lapse and reopen fiscal year operations |
| `FinancialControl.ApproveFinalAccount` | Budgeting | Generate and issue final account |

### New Domain Event

| Event | SourceEntityType | File |
|-------|-----------------|------|
| `ClosingEntryGenerated` | FinalAccount | `src/Domain/Events/Budgeting/ClosingEntryGenerated.cs` |

### New EventType

| Value | Name | Description |
|-------|------|-------------|
| 14 | `ClosingEntry` | Closing entry for revenue/expense accounts at year-end |
