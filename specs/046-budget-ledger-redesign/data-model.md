# Data Model: Budget Preparation — BudgetItemAllocations Model

**Date**: 2026-09-10 (Amended)

## Entity Relationship Overview

```
Budget ──1:N── BudgetItem ──1:N── BudgetItemMonthlyPlan
   │                │
   │                ├──1:N── EncumbranceLine
   │                │
   │                └──1:N── BudgetItemAllocations ──1:N── BudgetTransaction
   │                                  │
   │                                  └──(UNIQUE BudgetId + BudgetItemId)
   │
   ├──1:N── BudgetTransaction (per allocation, no lines)
   │
   └──1:N── YearClosingRun ──1:1── FinalAccounts ──1:N── FinalAccountLine

Encumbrance ──1:N── EncumbranceLine

PaymentOrder ──→ BudgetItemAllocations (BudgetItemAllocationId FK)
             ──→ BudgetItem (BudgetItemId FK — backward compat)

Fund (FK on Budget)
BudgetClassification (FK on BudgetItem)
BudgetType (FK on Budget)
```

## New Entity: BudgetItemAllocations

| Field | Type | Notes |
|-------|------|-------|
| Id | int PK | |
| BudgetId | int FK → Budgets | required |
| BudgetItemId | int FK → BudgetItems | required |
| ProposedAmount | decimal(23,2) | editable in Draft |
| ApprovedAmount | decimal(23,2)? | set = ProposedAmount on approval; frozen after |
| Remarks | string(1000)? | |
| CreatedAt | DateTimeOffset | audit |
| CreatedBy | string? | audit |
| LastModifiedAt | DateTimeOffset? | audit |
| LastModifiedBy | string? | audit |
| RowVersion | byte[] | concurrency |

**UNIQUE**: (BudgetId, BudgetItemId) — prevents duplicate items per budget.

**GL Account Uniqueness**: Each BudgetItem.AccountId is linked to at most one BudgetItem with an allocation within the same budget and fiscal year. Prevented at allocation creation/update time.

**Lifecycle**: Managed through Budget status.
- Budget Draft → allocations editable (ProposedAmount, Remarks)
- Budget Submitted → allocations read-only
- Budget Approved → ApprovedAmount = ProposedAmount, frozen
- Budget Active → allocations active, BudgetTransactions can reference them

## Modified Entity: BudgetTransaction

| Field | Type | Notes |
|-------|------|-------|
| Id | int PK | |
| TransactionNumber | string(50) | unique, BTR-NNNNNN |
| BudgetItemAllocationId | int FK → BudgetItemAllocations | **NEW** (replaces BudgetTransactionLines) |
| TransactionType | enum | see below |
| TransactionDate | DateOnly | |
| Amount | decimal(23,2) | |
| Direction | enum Increase/Decrease | |
| DocumentType | string(100)? | |
| DocumentId | int? | |
| Description | string(1000)? | |
| Status | enum Draft/Submitted/Approved/Posted/Reversed/Rejected | |
| ApprovedAt | DateTimeOffset? | |
| ApprovedBy | string? | |
| PostedAt | DateTimeOffset? | |
| PostedBy | string? | |
| ReversalOfId | int? FK self | |
| ReversalReason | string(1000)? | |
| RowVersion | byte[] | |
| audit fields | | |

**Lifecycle**: Draft → Submitted → Approved → Posted | Submitted → Rejected | Posted → Reversed

**On Post**: ApprovedAmount of linked BudgetItemAllocation is updated atomically:
- Direction Increase → ApprovedAmount += Amount
- Direction Decrease → ApprovedAmount -= Amount (MUST NOT go below zero; posting rejected if it would)

## Removed Entity: BudgetTransactionLines

Entirely removed. Each BudgetTransaction now targets exactly one BudgetItemAllocation.
Existing BudgetTransactionLines data migrated to BudgetTransaction records during schema migration.

## Modified Entity: PaymentOrder (new field)

| Field | Type | Notes |
|-------|------|-------|
| ... | ... | existing fields |
| BudgetItemAllocationId | int? FK → BudgetItemAllocations | **NEW** — primary budget control link |
| BudgetItemId | int? FK → BudgetItem | **RETAINED** — backward compatibility |

## Modified Entity: BudgetItem (no changes to existing fields)

| Field | Type | Notes |
|-------|------|-------|
| AccountId | int? FK → Account | GL account link — used for expenditure attribution |

## Enums

### BudgetTransactionType (Transfer=3 preserved but not createable)
```
InitialAppropriation = 0
Supplement = 1
Reduction = 2
Transfer = 3  (preserved in DB, not available for new transactions)
CarryForward = 4
Adjustment = 5
Lapse = 6
Reversal = 7
```

### BudgetTransactionStatus
```
Draft = 0
Submitted = 1
Approved = 2
Posted = 3
Reversed = 4
Rejected = 5
```

### TransactionDirection
```
Increase = 0
Decrease = 1
```

## Availability Formulas

```
ActualExpenditure(BudgetItemAllocation) =
    SUM(JournalEntryLine.Debit)
    - SUM(JournalEntryLine.Credit)
    WHERE JournalEntryLine.AccountId = BudgetItem.AccountId
    AND JournalEntry.FiscalYearId = Budget.FiscalYearId
    AND JournalEntry.EntryStatus = Posted
    AND JournalEntry.ReversalOfId IS NULL
    AND JournalEntry.EntryStatus != Cancelled

RemainingAmount(BudgetItemAllocation) =
    ApprovedAmount - ActualExpenditure

OutstandingEncumbrance(BudgetItemId) =
    SUM(EncumbranceLine.Amount - LiquidatedAmount - CancelledAmount)
    WHERE EncumbranceLine.BudgetItemId = @budgetItemId
    AND Encumbrance.Status IN (Active, PartiallyReleased, PartiallyLiquidated)
    AND Encumbrance.ReversalOfId IS NULL

AvailableAmount(BudgetItemAllocation) =
    RemainingAmount - OutstandingEncumbrance(BudgetItem.BudgetItemId)
```

**Budget Control**: Evaluated against AvailableAmount.
- None → always allowed
- Warning → allowed with warning log if exceeded
- Blocking → rejected if exceeded

**AllowOverrun chain**: BudgetItem.AllowOverrun → Budget.AllowOverrun → BudgetType.AllowOverrun

## Document Sequences

| Type | Prefix | Format |
|------|--------|--------|
| BudgetTransaction | BTR | BTR-NNNNNN |
| Encumbrance | ENC | ENC-NNNNNN |
| PaymentOrder | PO | PO-NNNNNN (existing) |

## Migration Notes

- BudgetItemAllocations: NEW table created.
- BudgetTransactionLines: DROPPED. Data migrated to BudgetTransaction records with BudgetItemAllocationId resolved from BudgetItemId.
- BudgetTransaction: BudgetItemAllocationId column ADDED. Existing transactions with BudgetTransactionLines get migrated to per-allocation records.
- PaymentOrder: BudgetItemAllocationId column ADDED (nullable). Existing BudgetItemId retained.
- Transfer enum value (3) preserved in database; not available for new transactions.
