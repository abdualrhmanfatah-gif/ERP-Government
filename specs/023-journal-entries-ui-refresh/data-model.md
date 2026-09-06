# Data Model: Journal Entries UI Refresh

**Feature**: 023-journal-entries-ui-refresh
**Date**: 2026-09-06

## Frontend Data Shapes

These are the TypeScript interfaces consumed by the UI. They mirror the backend DTOs from the NSwag-generated client.

### JournalEntryDto

```typescript
interface JournalEntryDto {
  id: number;
  entryNumber: string;           // e.g., "JE-000123"
  documentDate: DateOnly;
  postingDate: DateOnly | null;
  entryType: MoveEntryType | null;
  entryStatus: EntryStatus;
  journalId: number | null;
  journalName: string | null;
  periodId: number;
  periodName: string;
  fiscalYearId: number;
  fiscalYearName: string;
  narration: string | null;
  ref: string | null;
  isSystemGenerated: boolean;
  reversalOfId: number | null;
  reversalReason: string | null;
  totalDebit: number;            // computed from lines
  totalCredit: number;           // computed from lines
  baseCurrencyId: number;
  totalBaseDebit: number;
  totalBaseCredit: number;
  lines: JournalEntryLineDto[];
  rowVersion: string;            // base64 concurrency token
}
```

### JournalEntryLineDto

```typescript
interface JournalEntryLineDto {
  id: number;                    // long
  journalEntryId: number;
  sequence: number;
  accountId: number;
  accountCode: string;
  accountName: string;
  description: string | null;
  currencyId: number;
  currencyCode: string;
  exchangeRate: number;
  debit: number;
  credit: number;
  costCenterId: number | null;
  costCenterName: string | null;
  fundId: number | null;
  fundName: string | null;
  projectId: number | null;
  projectName: string | null;
  budgetItemId: number | null;
  budgetItemCode: string | null;
  encumbranceId: number | null;
  encumbranceNumber: string | null;
  paymentOrderId: number | null;
  paymentOrderNumber: string | null;
  baseDebit: number;
  baseCredit: number;
  resolvedRate: number;
  resolvedRateDate: DateOnly;
  rowVersion: string;
}
```

### EntryStatus Enum

```typescript
enum EntryStatus {
  Draft = "Draft",
  Submitted = "Submitted",
  Approved = "Approved",
  Posted = "Posted",
  Reversed = "Reversed",
  Cancelled = "Cancelled",
}
```

### MoveEntryType Enum

```typescript
enum MoveEntryType {
  Standard = "Standard",
  Reversing = "Reversing",
  Adjusting = "Adjusting",
  Closing = "Closing",
  Opening = "Opening",
  SystemGenerated = "SystemGenerated",
}
```

### CreateJournalEntryCommand

```typescript
interface CreateJournalEntryCommand {
  documentDate: DateOnly;
  journalId: number | null;
  periodId: number;
  fiscalYearId: number;
  baseCurrencyId: number;
  narration: string | null;
  ref: string | null;
  entryType: MoveEntryType | null;
}
```

### CreateJournalEntryLineCommand

```typescript
interface CreateJournalEntryLineCommand {
  accountId: number;
  description: string | null;
  currencyId: number;
  exchangeRate: number;
  debit: number;
  credit: number;
  costCenterId: number | null;
  fundId: number | null;
  projectId: number | null;
  budgetItemId: number | null;
  encumbranceId: number | null;
  paymentOrderId: number | null;
}
```

### Lifecycle Commands

```typescript
interface SubmitJournalEntryCommand {
  reason: string | null;
}

interface ApproveJournalEntryCommand {
  reason: string | null;
}

interface PostJournalEntryCommand {
  reason: string | null;
}

interface ReverseJournalEntryCommand {
  reason: string;                // required
}

interface CancelJournalEntryCommand {
  reason: string | null;
}
```

## State Machine

```
Draft ──→ Submitted ──→ Approved ──→ Posted ──→ Reversed
  │           │
  └──→ Cancelled └──→ Cancelled
```

**Valid transitions** (FR-017):
| From | Allowed Actions |
|---|---|
| Draft | Submit, Cancel, Edit |
| Submitted | Approve, Cancel |
| Approved | Post |
| Posted | Reverse |
| Reversed | (none — terminal) |
| Cancelled | (none — terminal) |

## Relationships

```
JournalEntry 1──→ N JournalEntryLine
JournalEntry 1──→ 0..1 JournalEntry (ReversalOf — self-ref)
JournalEntry N──→ 1 FiscalPeriod
JournalEntry N──→ 1 FiscalYear
JournalEntry N──→ 0..1 Journal
JournalEntry N──→ 0..1 AccountingEvent (SourceEvent)
JournalEntryLine N──→ 1 Account
JournalEntryLine N──→ 1 Currency
JournalEntryLine N──→ 0..1 CostCenter
JournalEntryLine N──→ 0..1 Fund
JournalEntryLine N──→ 0..1 Project
JournalEntryLine N──→ 0..1 BudgetItem
JournalEntryLine N──→ 0..1 Encumbrance
JournalEntryLine N──→ 0..1 PaymentOrder
```
