# Data Model: Journal Entry Lifecycle Screens

**Date**: 2026-09-06
**Feature**: 025-journal-entry-lifecycle

## Entities

### Journal Entry Header (JournalEntryDto)

The top-level record for a journal entry. Fields rendered in the UI:

| Field | Type | Notes |
|-------|------|-------|
| id | number | Primary key |
| entryNumber | string | Displayed at creation (server-generated) |
| ref | string? | Optional reference |
| documentDate | Date | Must be within fiscal period range |
| postingDate | Date? | Set when Posted |
| entryType | EntryType2? | Standard, Reversing, Adjusting, Closing, Opening, SystemGenerated |
| entryStatus | string | Draft, Submitted, Approved, Posted, Reversed, Cancelled |
| journalId | number? | Journal reference |
| journalName | string? | Display name |
| periodId | number | Fiscal period reference |
| fiscalYearId | number | Fiscal year reference |
| narration | string? | Entry narration |
| reversalOfId | number? | Link to original entry (when this is a reversal) |
| reversalReason | string? | Reason for reversal |
| postedById | number? | Poster identity |
| postedByName | string? | Poster display name |
| postedAt | Date? | Posting timestamp |
| cancelledById | number? | Cancelled-by identity |
| cancelledByName | string? | Cancelled-by display name |
| cancelledAt | Date? | Cancellation timestamp |
| isSystemGenerated | boolean | System badge + read-only lock |
| rowVersion | string | Optimistic concurrency token |
| baseCurrencyId | number? | Base currency |
| totalBaseDebit | number? | Computed total debit in base currency |
| totalBaseCredit | number? | Computed total credit in base currency |
| lines | JournalEntryLineDto[] | Entry lines |

### Journal Entry Line (JournalEntryLineDto)

Individual debit or credit record. Fields rendered in the UI:

| Field | Type | Notes |
|-------|------|-------|
| id | number | Primary key |
| journalEntryId | number | Parent entry |
| sequence | number | Line order |
| accountId | number | Account reference |
| accountCode | string? | Display code |
| accountName | string? | Display name |
| description | string? | Line description |
| currencyId | number | Currency |
| exchangeRate | number | Exchange rate |
| debit | number | Debit amount (XOR with credit) |
| credit | number | Credit amount (XOR with debit) |
| costCenterId | number? | Analytic dimension |
| costCenterName | string? | Display name |
| rowVersion | string | Optimistic concurrency token |
| baseDebit | number? | Converted debit in base currency |
| baseCredit | number? | Converted credit in base currency |
| resolvedRate | number? | Effective exchange rate |
| resolvedRateDate | Date? | Rate resolution date |

### Enums

#### EntryStatus (Frontend)
```
Draft | Submitted | Approved | Posted | Reversed | Cancelled
```

#### MoveEntryType (Entry Type)
```
Standard | Reversing | Adjusting | Closing | Opening | SystemGenerated
```

## State Machine

```
Draft ──(submit)──> Submitted ──(approve)──> Approved ──(post)──> Posted ──(reverse)──> Reversed
  │                    │
  └──(cancel)──────────┴──(cancel)──> Cancelled
```

### Guard Rules

| Transition | Guard |
|------------|-------|
| Draft → Submitted | ≥1 line + balanced (totalDebit = totalCredit) |
| Submitted → Approved | Status = Submitted |
| Approved → Posted | Status = Approved + period unlocked + documentDate within period |
| Posted → Reversed | Status = Posted + period unlocked + year Open + period not finalized |
| Draft → Cancelled | Status = Draft |
| Submitted → Cancelled | Status = Submitted |

### Valid Actions Per State

| State | Valid Actions |
|-------|---------------|
| Draft | Edit lines, Submit, Cancel |
| Submitted | Approve, Cancel |
| Approved | Post |
| Posted | Reverse |
| Reversed | None |
| Cancelled | None |

## Validation Rules

1. **Debit XOR Credit**: Exactly one of debit or credit must be non-zero per line.
2. **Balance**: Total debits must equal total credits (in base currency) for submission.
3. **Minimum Lines**: At least one line required for submission.
4. **Period Lock**: Posting blocked when period is locked for posting.
5. **Period Range**: Document date must fall within period start/end dates.
6. **Year Open**: Reversal requires fiscal year to be Open.
7. **Period Finalized**: Reversal blocked when period is finalized (balances closed).
8. **No Double Reversal**: Reversed entries cannot be reversed again.
9. **RowVersion**: Every mutation sends rowVersion; stale → 409 Conflict.
