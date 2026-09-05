# Data Model: BF-001 Journal Entries Frontend

**Date**: 2026-09-02
**Feature**: BF-001 Journal Entries Frontend
**Note**: Frontend-only. These are the TypeScript types consumed from the API. No database schema changes.

## Entity: MoveDto (Journal Entry)

Returned by `GET /api/Moves` and `GET /api/Moves/{id}`.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| id | number | YES | Unique identifier |
| entryNumber | string | YES | Auto-generated entry number (e.g., "JRN-2026-001") |
| ref | string \| null | NO | User reference |
| documentDate | string | YES | Document date (ISO date string) |
| postingDate | string \| null | YES | Posting date (set when posted) |
| entryType | string \| null | NO | Entry type (Normal/Reversal/Adjustment/Closing/Opening) |
| entryStatus | string | YES | Status: Draft/Submitted/Approved/Posted/Reversed/Cancelled |
| journalId | number \| null | NO | Journal foreign key |
| journalName | string \| null | NO | Journal display name |
| periodId | number | YES | Fiscal period foreign key |
| fiscalYearId | number | YES | Fiscal year foreign key |
| narration | string \| null | NO | Entry narration/description |
| reversalOfMoveId | number \| null | NO | Original entry ID (if this is a reversal) |
| reversalReason | string \| null | NO | Reversal reason (max 500 chars) |
| postedById | number \| null | NO | User ID who posted |
| postedByName | string \| null | NO | User name who posted |
| postedAt | string \| null | NO | Posting timestamp |
| cancelledById | number \| null | NO | User ID who cancelled |
| cancelledByName | string \| null | NO | User name who cancelled |
| cancelledAt | string \| null | NO | Cancellation timestamp |
| isSystemGenerated | boolean | YES | System-generated flag |
| rowVersion | string | YES | Optimistic concurrency token |
| baseCurrencyId | number \| null | NO | Base currency ID |
| totalBaseDebit | number \| null | NO | Total base currency debit |
| totalBaseCredit | number \| null | NO | Total base currency credit |
| lines | MoveLineDto[] | YES | Move lines collection |

## Entity: MoveLineDto (Move Line)

Nested within MoveDto.lines.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| id | number | YES | Unique identifier (long) |
| moveId | number | YES | Parent move foreign key |
| sequence | number | YES | Line sequence number |
| accountId | number | YES | Account foreign key |
| accountCode | string | YES | Account code (e.g., "1101") |
| accountName | string | YES | Account display name |
| description | string \| null | NO | Line description |
| currencyId | number | YES | Currency foreign key |
| exchangeRate | number | YES | Exchange rate (default 1 for base currency) |
| debit | number | YES | Debit amount |
| credit | number | YES | Credit amount |
| costCenterId | number \| null | NO | Cost center foreign key |
| costCenterName | string \| null | NO | Cost center display name |
| rowVersion | string | YES | Optimistic concurrency token |
| baseDebit | number \| null | NO | Base currency debit (computed) |
| baseCredit | number \| null | NO | Base currency credit (computed) |
| resolvedRate | number \| null | NO | Resolved exchange rate |
| resolvedRateDate | string \| null | NO | Rate lookup date |

## Entity: JournalDto (Journal Configuration)

Returned by `GET /api/Journals`.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| id | number | YES | Unique identifier |
| code | string | YES | Journal code (e.g., "GEN") |
| name | string | YES | Journal display name |
| type | string | YES | Journal type enum |
| accountId | number \| null | NO | Default account |
| suspenseAccountId | number \| null | NO | Suspense account |
| allowForeignCurrency | boolean | YES | Allow foreign currency lines |
| sequenceId | number \| null | NO | Document sequence for auto-numbering |
| requireApprovalBeforePosting | boolean | YES | Require approval before posting |
| isActive | boolean | YES | Active flag |

## Entity: FiscalYearPeriodResult

Returned by `GET /api/FiscalYears/by-date`.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| fiscalYearId | number | YES | Fiscal year ID |
| fiscalYearCode | string \| null | NO | Fiscal year code |
| fiscalYearName | string \| null | NO | Fiscal year display name |
| fiscalPeriodId | number | YES | Fiscal period ID |
| fiscalPeriodName | string \| null | NO | Fiscal period display name |

## Entity: ExchangeRateLookupDto

Returned by `GET /api/ExchangeRates/lookup`.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| rate | number | YES | Exchange rate |
| rateDate | string | YES | Rate date |
| rateType | string | YES | Rate type (Official/Market) |

## State Transitions

```
Draft ──────→ Submitted ──────→ Approved ──────→ Posted
  │               │                │                │
  │               │                │                ↓
  │               │                │            Reversed
  │               │                │
  ↓               ↓                │
Cancelled     Cancelled            │
                                    │
                                    ↓
                               (end — read-only)
```

- **Draft → Submitted**: `POST /api/Moves/{id}/submit`
- **Submitted → Approved**: `POST /api/Moves/{id}/approve`
- **Approved → Posted**: `POST /api/Moves/{id}/post`
- **Draft → Cancelled**: `POST /api/Moves/{id}/cancel`
- **Submitted → Cancelled**: `POST /api/Moves/{id}/cancel`
- **Posted → Reversed**: `POST /api/Moves/{id}/reverse` (creates new reversal entry)

## Validation Rules (Frontend)

| Rule | Field | Error Message |
|------|-------|--------------|
| Document date required | documentDate | تاريخ المستند مطلوب |
| Account required | accountId | الحساب مطلوب |
| Debit XOR Credit | debit/credit | يجب إدخال مدين أو دائن فقط |
| Currency required | currencyId | العملة مطلوبة |
| Exchange rate > 0 | exchangeRate | سعر الصرف يجب أن يكون أكبر من صفر |
| Narration max 1000 chars | narration | البيان لا يتجاوز 1000 حرف |
| Description max 500 chars | description | البيان لا يتجاوز 500 حرف |
| Reversal reason required | reversalReason | سب العكس مطلوب |
| Reversal reason max 500 chars | reversalReason | سب العكس لا يتجاوز 500 حرف |
