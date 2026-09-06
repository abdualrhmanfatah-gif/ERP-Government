# Data Model: Financial Settings Administration UI

**Feature**: 024-financial-settings-admin
**Date**: 2026-09-06

> Frontend-only feature. Data model documents the DTO shapes consumed by the UI from the existing backend APIs. No new entities or migrations.

## DTOs (Frontend Types)

### FiscalYearDto

| Field | Type | Notes |
|-------|------|-------|
| id | number | Primary key |
| name | string | Display name |
| yearNumber | number | Numeric year (e.g. 2026) |
| startDate | string (date) | ISO date |
| endDate | string (date) | ISO date |
| status | string | Draft / Open / SoftClosed / HardClosed |
| isClosed | boolean | Whether year is closed |
| closingJournalEntryId | number? | Linked closing entry |
| isActive | boolean | Soft-delete flag |
| rowVersion | string | Optimistic concurrency token (base64) |

### FiscalPeriodDto

| Field | Type | Notes |
|-------|------|-------|
| id | number | Primary key |
| fiscalYearId | number | FK to FiscalYear |
| periodNumber | number | 1–12 for monthly |
| name | string | Arabic month name |
| startDate | string (date) | Period start |
| endDate | string (date) | Period end |
| isLockedForPosting | boolean | Lock flag |
| isActive | boolean | Soft-delete flag |
| rowVersion | string | Concurrency token |

### DocumentSequenceDto

| Field | Type | Notes |
|-------|------|-------|
| id | number | Primary key |
| name | string | Sequence name |
| documentType | string | Document type code |
| fiscalYearId | number? | null for year-independent sequences |
| currentNumber | number | Next number to allocate |
| resetPolicy | string | Yearly / Never |
| isActive | boolean | Active flag |
| rowVersion | string | Concurrency token |

### CurrencyDto

| Field | Type | Notes |
|-------|------|-------|
| id | number | Primary key |
| code | string | ISO 4217 code (e.g. "YER") |
| name | string | Currency name |
| symbol | string | Symbol (e.g. "﷼") |
| decimalPlaces | number | Decimal precision |
| roundingPrecision | number | Rounding step |
| isBase | boolean | Whether this is the base currency |
| isActive | boolean | Active flag |
| rowVersion | string | Concurrency token |

### Iso4217CodeDto

| Field | Type | Notes |
|-------|------|-------|
| code | string | ISO 4217 code |
| name | string | Currency name |
| decimalPlaces | number | Standard decimal places |

### ExchangeRateDto

| Field | Type | Notes |
|-------|------|-------|
| id | number | Primary key |
| baseCurrencyId | number | FK to base currency |
| baseCurrencyCode | string | Flattened from navigation |
| currencyId | number | FK to target currency |
| currencyCode | string | Flattened from navigation |
| rateDate | string (date) | Effective date |
| rateType | string | Official / Market |
| rate | number | Exchange rate value |
| isActive | boolean | Active flag |
| rowVersion | string | Concurrency token |

### ExchangeRateLookupDto

| Field | Type | Notes |
|-------|------|-------|
| rate | number | Applicable rate |
| rateDate | string (date) | Date of the applicable rate |
| rateType | string | Official / Market |

### ClosingEntryDto

| Field | Type | Notes |
|-------|------|-------|
| id | number | Primary key |
| closingEntryNumber | string | Generated number (YEC-{year}-{D4}) |
| fiscalYearId | number | FK to fiscal year |
| fiscalYearName | string | Flattened from navigation |
| closingDate | string (date) | Generation date |
| description | string? | Optional description |
| status | string | Draft / PendingApproval / Approved / Posted / Cancelled |
| isReversal | boolean | Whether this is a reversal entry |
| reversalOfId | number? | FK to original entry (if reversal) |
| reversalOfNumber | string? | Flattened from navigation |
| journalEntryId | number? | Linked journal entry |
| journalEntryEntryNumber | string? | Flattened from navigation |
| approvedById | string? | Approver user ID |
| isActive | boolean | Active flag |
| rowVersion | string | Concurrency token |

## Enums (Frontend)

### FiscalYearStatus

| Value | Arabic Label |
|-------|-------------|
| Draft | مسودة |
| Open | مفتوح |
| SoftClosed | مغلق ( مؤقت ) |
| HardClosed | مغلق |

### FiscalPeriodLockStatus

| Value | Arabic Label |
|-------|-------------|
| Unlocked | غير مقفل |
| Locked | مقفل للتقيد |

### ExchangeRateType

| Value | Arabic Label |
|-------|-------------|
| Official | رسمي |
| Market | سوق |

### ClosingEntryStatus

| Value | Arabic Label |
|-------|-------------|
| Draft | مسودة |
| PendingApproval | بانتظار الاعتماد |
| Approved | معتمد |
| Posted | مقيّد |
| Cancelled | ملغي |

### ResetPolicy

| Value | Arabic Label |
|-------|-------------|
| Yearly | سنوي |
| Never | أبداً |

## State Transitions

### Fiscal Year Lifecycle

```
Draft → Open (via POST /{id}/open)
Open → HardClosed (via POST /{id}/close)
```

### Closing Entry Lifecycle

```
Draft → Approved (via POST /{id}/approve)
Approved → Posted (auto-posted on approval)
Posted → Reversed (via POST /{id}/reverse)
```

### Exchange Rate Activation

```
Inactive → Active (via POST /{id}/activate)
Active → Inactive (via POST /{id}/deactivate)
```

### Currency Activation

```
Inactive → Active (via POST /{id}/activate)
Active → Inactive (via POST /{id}/deactivate)
```

## Relationships

```
FiscalYear 1──N FiscalPeriod
FiscalYear 1──N YearEndClosingEntry
FiscalYear 1──N DocumentSequence (nullable FK)
Currency 1──N ExchangeRate (as base or target)
YearEndClosingEntry N──1 YearEndClosingEntry (self-referencing: reversalOf)
```
