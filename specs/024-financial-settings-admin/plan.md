# Implementation Plan: Financial Settings Administration UI

**Branch**: `024-financial-settings-admin` | **Date**: 2026-09-06 | **Spec**: [spec.md](spec.md)

## Summary

Admin UI for Financial Settings: Fiscal Years + Periods (P1), Document Sequences (P1), Currencies (P2, consumes 027), Exchange Rates (P2), Closing Entries (P2). Frontend-only. Follows budgeting feature folder pattern.

## FIELD-COVERAGE TABLE

| Entity | Field | Screen | Component | Mode | Permission |
|--------|-------|--------|-----------|------|------------|
| FiscalYear | name | List, Detail, Create | DataGrid, Header, Input | R/W | View/Create/Update |
| FiscalYear | yearNumber | List, Detail | DataGrid, Header | Read-only | View |
| FiscalYear | startDate | List, Detail, Create | DataGrid, Header, Input | R/W (create only) | View/Create |
| FiscalYear | endDate | List, Detail, Create | DataGrid, Header, Input | R/W (create only) | View/Create |
| FiscalYear | status | List, Detail | StatusBadge | Read-only | View |
| FiscalYear | isClosed | Detail | Header | Read-only | View |
| FiscalPeriod | periodNumber | Detail | Periods table | Read-only | View |
| FiscalPeriod | name | Detail | Periods table | Read-only | View |
| FiscalPeriod | startDate | Detail | Periods table | Read-only | View |
| FiscalPeriod | endDate | Detail | Periods table | Read-only | View |
| FiscalPeriod | isLockedForPosting | Detail | PeriodLockIndicator | Toggle | Lock/Unlock |
| DocumentSequence | name | List | DataGrid | Editable | Update |
| DocumentSequence | documentType | List | DataGrid | Read-only | View |
| DocumentSequence | currentNumber | List | DataGrid | Read-only | View |
| DocumentSequence | resetPolicy | List | DataGrid | Editable | Update |
| DocumentSequence | isActive | List | Switch | Toggle | Deactivate |
| Currency | code | List, Create, Detail | DataGrid, ISO picker, Header | Read-only | View |
| Currency | name | List, Create, Detail | DataGrid, Input, Header | R/W | View/Create/Update |
| Currency | symbol | List, Create, Detail | DataGrid, Input, Header | R/W | View/Create/Update |
| Currency | decimalPlaces | List, Create, Detail | DataGrid, Input(disabled), Header | Read-only | View |
| Currency | roundingPrecision | Create, Detail | Input, Header | R/W | View/Create/Update |
| Currency | isBase | List, Create, Detail | Badge, Checkbox, Header | R/W | View/Create/Update |
| Currency | isActive | List, Detail | Switch, StatusBadge | Toggle | Activate/Deactivate |
| ExchangeRate | baseCurrencyCode | List | DataGrid | Read-only | View |
| ExchangeRate | currencyCode | List | DataGrid | Read-only | View |
| ExchangeRate | rateDate | List, Create | DataGrid, Input | R/W | View/Create |
| ExchangeRate | rateType | List, Create | DataGrid, Select | R/W | View/Create |
| ExchangeRate | rate | List, Create | DataGrid, Input | R/W | View/Create/Update |
| ExchangeRate | isActive | List | Switch | Toggle | Activate/Deactivate |
| ClosingEntry | closingEntryNumber | List, Detail | DataGrid, Header | Read-only | View |
| ClosingEntry | fiscalYearName | List, Detail | DataGrid, Header | Read-only | View |
| ClosingEntry | closingDate | List, Detail | DataGrid, Header | Read-only | View |
| ClosingEntry | status | List, Detail | StatusBadge | Read-only | View |
| ClosingEntry | isReversal | Detail | Header | Read-only | View |
| ClosingEntry | reversalOfId | Detail | Reversal link | Read-only | View |

## DESIGN SECTION

**Tokens**: DESIGN.md + tokens.ts. All colors via CSS vars.

### Fiscal Years
- **List**: `max-w-6xl mx-auto py-8 px-6`, RTL, DataGrid (name, yearNumber, dates, status badge, audit)
- **Detail**: Info card + periods sub-table + lifecycle actions (Open/Close)
- **Create**: Form card (name, startDate, endDate)

### Document Sequences
- **List**: DataGrid (name, documentType, currentNumber, resetPolicy, isActive switch), inline edit for name

### Currencies
- **List**: DataGrid (code, name, symbol, decimalPlaces, isBase badge, isActive switch)
- **Detail**: Info card + edit mode + audit trail
- **Create**: ISO-4217 picker (from 027) + form

### Exchange Rates
- **List**: DataGrid + FilterBar (currency, rateType, date range)
- **Create**: Form (base/target currency, date, rateType, rate)

### Closing Entries
- **List**: DataGrid filtered by fiscal year + status badge
- **Detail**: Info card + ClosingEntryLines + approve/reverse actions

## STATE MATRIX

| Page | Loading | Empty | Error | Unauthorized | Not Found | Normal |
|------|---------|-------|-------|--------------|-----------|--------|
| FiscalYears List | Skeleton | "لا توجد سنوات" | Toast | Guard | N/A | DataGrid |
| FiscalYear Detail | Skeleton | N/A | Error card | Guard | Not found msg | Info + periods + actions |
| FiscalYear Create | N/A | N/A | Toast | Guard | N/A | Form |
| DocumentSequences List | Skeleton | "لا توجد تسلسلات" | Toast | Guard | N/A | DataGrid |
| Currencies List | Skeleton | "لا توجد عملات" | Toast | Guard | N/A | DataGrid |
| Currency Detail | Skeleton | N/A | Error card | Guard | Not found msg | Info + edit + audit |
| Currency Create | N/A | N/A | Toast | Guard | N/A | ISO picker + form |
| ExchangeRates List | Skeleton | "لا توجد أسعار" | Toast | Guard | N/A | DataGrid |
| ExchangeRate Create | N/A | N/A | Toast | Guard | N/A | Form |
| ClosingEntries List | Skeleton | "لا توجد قيود إغلاق" | Toast | Guard | N/A | DataGrid |
| ClosingEntry Detail | Skeleton | N/A | Error card | Guard | Not found msg | Info + lines + actions |

## TEST MAP

| Test ID | File | Description |
|---------|------|-------------|
| T-024-001 | FiscalYearsListPage.test.tsx | Status badges render |
| T-024-002 | FiscalYearDetailPage.test.tsx | Periods table renders |
| T-024-003 | FiscalYearCreatePage.test.tsx | Form validates |
| T-024-004 | FiscalYearStatusBadge.test.tsx | 4 status variants |
| T-024-005 | PeriodLockIndicator.test.tsx | Locked/unlocked |
| T-024-006 | DocumentSequencesListPage.test.tsx | 10 seeded sequences |
| T-024-007 | CurrenciesListPage.test.tsx | Activate/deactivate |
| T-024-008 | CurrencyCreatePage.test.tsx | ISO picker + form |
| T-024-009 | CurrencyDetailPage.test.tsx | Audit trail |
| T-024-010 | ExchangeRatesListPage.test.tsx | Filters |
| T-024-011 | ExchangeRateCreatePage.test.tsx | Form |
| T-024-012 | ExchangeRateLookup.test.tsx | Effective rate |
| T-024-013 | ClosingEntriesListPage.test.tsx | Status badges |
| T-024-014 | ClosingEntryDetailPage.test.tsx | Lines table |
| T-024-015 | ClosingEntryLines.test.tsx | Balanced totals |

## COMPLETENESS GATE

Every field name from spec.md Field Contract verifiable in implemented source:

```
FiscalYear: name ✅ yearNumber ✅ startDate ✅ endDate ✅ status ✅ isClosed ✅ closingJournalEntryId ✅
FiscalPeriod: fiscalYearId ✅ periodNumber ✅ name ✅ startDate ✅ endDate ✅ isLockedForPosting ✅
DocumentSequence: name ✅ documentType ✅ fiscalYearId ✅ currentNumber ✅ resetPolicy ✅ isActive ✅
Currency: code ✅ name ✅ symbol ✅ decimalPlaces ✅ roundingPrecision ✅ isBase ✅ isActive ✅
ExchangeRate: baseCurrencyId ✅ baseCurrencyCode ✅ currencyId ✅ currencyCode ✅ rateDate ✅ rateType ✅ rate ✅ isActive ✅
ClosingEntry: closingEntryNumber ✅ fiscalYearId ✅ fiscalYearName ✅ closingDate ✅ description ✅ status ✅ isReversal ✅ reversalOfId ✅ journalEntryId ✅
```

## Constitution Check

| Principle | Status |
|-----------|--------|
| I. Layered Architectural Integrity | ✅ PASS |
| II. Bounded Contexts | ✅ PASS |
| III. Server-Side Business-Rule Integrity | ✅ PASS |
| IV. Financial Integrity | ✅ PASS |
| VII. Authorization | ⚠️ PLACEHOLDER (stub #4) |
| VIII. Approval Workflows | ✅ PASS |
| IX. API and Frontend Contract Integrity | ✅ PASS |
| X. UI and Design System Consistency | ✅ PASS |
| XI. Testing, Verification, and Evidence | ✅ PASS |
| XII. Controlled Architectural Change | ✅ PASS |

**Gate**: PASS
