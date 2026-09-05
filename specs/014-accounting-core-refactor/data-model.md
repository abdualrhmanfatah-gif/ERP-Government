# Data Model: Accounting Core Refactor

**Feature**: 014-accounting-core-refactor
**Date**: 2026-09-05

## Entity: JournalEntry (renamed from Move)

| Field | Type | Nullable | Notes |
|-------|------|----------|-------|
| Id | int | No | PK (inherited from BaseAuditableEntity) |
| EntryNumber | string | No | Unique entry number |
| Ref | string | Yes | Reference |
| DocumentDate | DateOnly | No | Document date |
| PostingDate | DateOnly | Yes | Posting date |
| EntryType | MoveEntryType? | Yes | Entry type enum |
| EntryStatus | EntryStatus | No | **Changed from string to enum** (Draft/Posted/Reversed), default Draft |
| JournalId | int? | Yes | FK → Journal |
| PeriodId | int | No | FK → FiscalPeriod |
| FiscalYearId | int | No | FK → FiscalYear |
| Narration | string | Yes | Description |
| SourceEventId | int? | Yes | FK → AccountingEvent |
| ReversalOfId | int? | Yes | **Renamed from ReversalOfMoveId** — FK → JournalEntry (self) |
| ReversalReason | string | Yes | Reason for reversal |
| PostedById | int? | Yes | FK → User |
| PostedAt | DateTimeOffset? | Yes | Posting timestamp |
| CancelledById | int? | Yes | FK → User |
| CancelledAt | DateTimeOffset? | Yes | Cancellation timestamp |
| IsSystemGenerated | bool | No | System-generated flag |
| RowVersion | byte[] | No | Optimistic concurrency token |

**Relationships**:
- Self-referential: ReversalOfId → JournalEntry
- Journal: JournalId → Journal
- SourceEvent: SourceEventId → AccountingEvent
- Period: PeriodId → FiscalPeriod
- FiscalYear: FiscalYearId → FiscalYear
- PostedBy: PostedById → User
- CancelledBy: CancelledById → User
- Lines: one-to-many → JournalEntryLine

**Constraints**:
- EntryStatus enum: Draft(0), Posted(1), Reversed(2)
- Posted entries are immutable (corrections via reversal only)
- Optimistic concurrency on RowVersion

---

## Entity: JournalEntryLine (renamed from MoveLine)

| Field | Type | Nullable | Notes |
|-------|------|----------|-------|
| Id | long | No | PK (inherited from BaseLongAuditableEntity) |
| JournalEntryId | int | No | **Renamed from MoveId** — FK → JournalEntry |
| Sequence | int | No | Line sequence number |
| AccountId | int | No | FK → Account |
| Description | string | Yes | Line description |
| CurrencyId | int | No | FK → Currency |
| ExchangeRate | decimal | No | Exchange rate, default 1 |
| Debit | decimal | No | Debit amount |
| Credit | decimal | No | Credit amount |
| CostCenterId | int? | Yes | FK → CostCenter |
| FundId | int? | Yes | **NEW** — FK → Fund |
| ProjectId | int? | Yes | **NEW** — FK → Project |
| BudgetItemId | int? | Yes | **NEW** — FK → BudgetItem |
| EncumbranceId | int? | Yes | **NEW** — FK → Encumbrance |
| PaymentOrderId | int? | Yes | **NEW** — FK → PaymentOrder |
| RowVersion | byte[] | No | Optimistic concurrency token |

**Relationships**:
- JournalEntry: JournalEntryId → JournalEntry
- Account: AccountId → Account
- CostCenter: CostCenterId → CostCenter
- Fund: FundId → Fund
- Project: ProjectId → Project
- BudgetItem: BudgetItemId → BudgetItem
- Encumbrance: EncumbranceId → Encumbrance
- PaymentOrder: PaymentOrderId → PaymentOrder

**Constraints**:
- CHECK: NOT (Debit > 0 AND Credit > 0) — exactly one side per line
- FluentValidation: same XOR rule at application level

---

## Entity: AccountingEvent (extended)

| Field | Type | Nullable | Notes |
|-------|------|----------|-------|
| Id | int | No | PK (inherited from BaseAuditableEntity) |
| EventType | EventType | No | **Changed from string to enum** (ReceiptCollection, DepositClearing, PaymentExecution, Reversal, etc.) |
| SourceDocumentType | string | No | **Renamed from SourceTable** — entity type identifier |
| SourceDocumentId | int | No | **Renamed from SourceId** — source entity PK |
| Status | EventStatus | No | **Renamed from AccountingEventStatus** (Pending/Posted/Reversed) |
| JournalEntryId | int? | Yes | **NEW** — FK → JournalEntry (set when posted) |
| EventCategory | EventCategory | No | **NEW** — Revenue/Expenditure/Transfer/Adjustment/Other |
| ErrorMessage | string | Yes | Error message |
| ProcessedAt | DateTimeOffset? | Yes | Processing timestamp |
| RetryCount | int | No | Retry counter |
| RowVersion | byte[] | No | Optimistic concurrency token |

**Relationships**:
- JournalEntry: JournalEntryId → JournalEntry (set on post)

**Constraints**:
- UNIQUE: (EventType, SourceDocumentType, SourceDocumentId) WHERE Status = 'Posted' — prevents double-posting
- EventStatus enum: Pending(0), Posted(1), Reversed(2)
- EventCategory enum: Revenue(0), Expenditure(1), Transfer(2), Adjustment(3), Other(4)
- EventType enum: ReceiptCollection(0), DepositClearing(1), PaymentExecution(2), Reversal(3), etc.

---

## Entity: PaymentOrder (stripped)

| Field | Type | Nullable | Notes |
|-------|------|----------|-------|
| Id | int | No | PK |
| PaymentOrderNumber | string | No | Unique number |
| PaymentOrderDate | DateOnly | No | Order date |
| DueDate | DateOnly? | Yes | Due date |
| PaymentOrderType | string | No | Order type |
| VendorId | int | No | FK → Vendor |
| FundId | int | No | FK → Fund |
| FiscalYearId | int | No | FK → FiscalYear |
| AppropriationId | int | No | FK → Appropriation |
| BudgetClassificationId | int? | Yes | FK → BudgetClassification |
| CostCenterId | int? | Yes | FK → CostCenter |
| ProjectId | int? | Yes | FK → Project |
| PurchaseOrderId | int? | Yes | FK → PurchaseOrder |
| EncumbranceId | int? | Yes | FK → Encumbrance |
| CurrencyId | int | No | FK → Currency |
| ExchangeRate | decimal? | Yes | Exchange rate |
| AmountGross | decimal | No | **KEPT** — entered planning input |
| DeductionAmount | decimal | No | **KEPT** — entered planning input |
| PaymentMethod | PaymentMethod | No | **Extended** — added InKind(5), Other renumbered to 6 |
| BankAccountId | int? | Yes | FK → BankAccount |
| BeneficiaryName | string | No | Beneficiary name |
| BeneficiaryIban | string | Yes | IBAN |
| BeneficiaryAccountNumber | string | Yes | Account number |
| BeneficiaryBankName | string | Yes | Bank name |
| Status | PaymentOrderStatus | No | Order status |
| BudgetCheckStatus | BudgetCheckStatus | No | Budget check status |
| TreasuryStatus | string | Yes | Treasury status |
| TreasuryReference | string | Yes | Treasury reference |
| TreasurySentAt | DateTimeOffset? | Yes | Treasury sent timestamp |
| PaidAt | DateTimeOffset? | Yes | Payment timestamp |
| JournalEntryId | int? | Yes | **Renamed from MoveId** — FK → JournalEntry |
| AccountingEventId | int? | Yes | FK → AccountingEvent |
| Notes | string | Yes | Notes |
| RowVersion | byte[] | No | Optimistic concurrency token |

**Dropped fields**:
- ~~AmountNet~~ (computed: AmountGross − deductions)
- ~~BaseAmountNet~~ (computed)
- ~~TotalDeductionAmount~~ (computed)
- ~~TotalNetAmount~~ (computed)
- ~~TotalPaidAmount~~ (computed: sum of completed payments)
- ~~TotalRemainingAmount~~ (computed: net − paid)
- ~~IsFullyPaid~~ (computed: remaining = 0)
- ~~ApprovedById~~, ~~ApprovedAt~~ (ApprovalHistory only)
- ~~RejectedById~~, ~~RejectedAt~~ (ApprovalHistory only)
- ~~CancelledById~~, ~~CancelledAt~~ (ApprovalHistory only)
- ~~VoidedById~~, ~~VoidedAt~~ (ApprovalHistory only)

---

## Entity: PaymentOrderLine (stripped)

| Field | Type | Nullable | Notes |
|-------|------|----------|-------|
| Id | int | No | PK |
| PaymentOrderId | int | No | FK → PaymentOrder |
| Amount | decimal | No | **KEPT** — entered input |
| TaxAmount | decimal | No | **KEPT** — entered input |
| ... | ... | ... | Other non-computed fields retained |

**Dropped fields**: ~~BaseAmount~~, ~~AllocatedAmount~~, ~~RemainingAmount~~, ~~NetAmount~~ (all computed)

---

## Entity: PaymentOrderDeduction (stripped)

| Field | Type | Nullable | Notes |
|-------|------|----------|-------|
| Id | int | No | PK |
| PaymentOrderId | int | No | FK → PaymentOrder |
| Amount | decimal | No | **KEPT** — entered input |
| DeductionPercent | decimal | No | **KEPT** — entered input |
| ... | ... | ... | Other non-computed fields retained |

**Dropped fields**: ~~BaseAmount~~ (computed)

---

## New Enums

### EntryStatus
```csharp
public enum EntryStatus
{
    Draft = 0,
    Posted = 1,
    Reversed = 2
}
```

### EventCategory
```csharp
public enum EventCategory
{
    Revenue = 0,
    Expenditure = 1,
    Transfer = 2,
    Adjustment = 3,
    Other = 4
}
```

### EventType
```csharp
public enum EventType
{
    ReceiptCollection = 0,
    DepositClearing = 1,
    PaymentExecution = 2,
    Reversal = 3
    // Extensible — add values as new event types emerge
}
```

### EventStatus (renamed from AccountingEventStatus)
```csharp
public enum EventStatus
{
    Pending = 0,
    Posted = 1,
    Reversed = 2
}
```

### PaymentMethod (extended)
```csharp
public enum PaymentMethod
{
    Cash = 0,
    BankTransfer = 1,
    Check = 2,
    CreditCard = 3,
    WireTransfer = 4,
    InKind = 5,    // NEW
    Other = 6      // Renumbered from 5
}
```
