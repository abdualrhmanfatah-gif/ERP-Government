# API Contracts: Financial Settings Admin UI

**Feature**: 024-financial-settings-admin
**Date**: 2026-09-06

> Reference to existing backend endpoints. Frontend consumes these; no new endpoints created.

## Base URL

All endpoints are prefixed with `/api/` (route prefix from endpoint group mapping).

## Permission Codes

All actions require authorization. Permission codes from `PermissionCodes.cs`:

| Module | View | Create | Update | Activate | Deactivate | Open | Close | Lock | Generate | Approve | Reverse |
|--------|------|--------|--------|----------|------------|------|-------|------|----------|---------|---------|
| Currencies | CurrenciesView | CurrenciesCreate | CurrenciesUpdate | CurrenciesActivate | CurrenciesDeactivate | — | — | — | — | — | — |
| ExchangeRates | ExchangeRatesView | ExchangeRatesCreate | ExchangeRatesUpdate | ExchangeRatesActivate | ExchangeRatesDeactivate | — | — | — | — | — | — |
| FiscalYears | FiscalYearsView | FiscalYearsCreate | FiscalYearsUpdate | — | — | FiscalYearsOpen | FiscalYearsClose | — | — | — | — |
| FiscalPeriods | FiscalPeriodsView | FiscalPeriodsCreate | FiscalPeriodsCreate | — | — | — | — | FiscalPeriodsLock | — | — | — |
| DocumentSequences | DocumentSequencesView | DocumentSequencesCreate | DocumentSequencesCreate | — | — | — | — | — | — | — | — |
| ClosingEntries | ClosingEntriesView | — | — | — | — | — | — | — | ClosingEntriesGenerate | ClosingEntriesApprove | ClosingEntriesReverse |

## Currencies

| Method | Endpoint | Body/Query | Response | Permission |
|--------|----------|------------|----------|------------|
| GET | `/api/Currencies` | `?isActive=` (optional) | `CurrencyDto[]` | CurrenciesView |
| GET | `/api/Currencies/{id}` | — | `CurrencyDto?` | CurrenciesView |
| GET | `/api/Currencies/iso4217` | `?query=` (optional text filter) | `Iso4217CodeDto[]` | CurrenciesView |
| POST | `/api/Currencies` | `CreateCurrencyCommand` | 204 | CurrenciesCreate |
| PUT | `/api/Currencies/{id}` | `UpdateCurrencyCommand` | 204 | CurrenciesUpdate |
| POST | `/api/Currencies/{id}/activate` | `{ id, rowVersion }` | 204 | CurrenciesActivate |
| POST | `/api/Currencies/{id}/deactivate` | `{ id, rowVersion }` | 204 | CurrenciesDeactivate |

## Exchange Rates

| Method | Endpoint | Body/Query | Response | Permission |
|--------|----------|------------|----------|------------|
| GET | `/api/ExchangeRates` | `?currencyId=&rateType=&fromDate=&toDate=&isActive=` | `ExchangeRateDto[]` | ExchangeRatesView |
| GET | `/api/ExchangeRates/{id}` | — | `ExchangeRateDto?` | ExchangeRatesView |
| GET | `/api/ExchangeRates/lookup` | `?baseCurrencyId=&currencyId=&date=` | `ExchangeRateLookupDto?` (404 if none) | ExchangeRatesView |
| POST | `/api/ExchangeRates` | `CreateExchangeRateCommand` | 204 | ExchangeRatesCreate |
| PUT | `/api/ExchangeRates/{id}` | `UpdateExchangeRateCommand` | 204 | ExchangeRatesUpdate |
| POST | `/api/ExchangeRates/{id}/activate` | `{ id, rowVersion }` | 204 | ExchangeRatesActivate |
| POST | `/api/ExchangeRates/{id}/deactivate` | `{ id, rowVersion }` | 204 | ExchangeRatesDeactivate |

## Fiscal Years

| Method | Endpoint | Body/Query | Response | Permission |
|--------|----------|------------|----------|------------|
| GET | `/api/FiscalYears` | `?isActive=` (optional) | `FiscalYearDto[]` | FiscalYearsView |
| GET | `/api/FiscalYears/{id}` | — | `FiscalYearDto?` | FiscalYearsView |
| POST | `/api/FiscalYears` | `CreateFiscalYearCommand` | 204 | FiscalYearsCreate |
| PUT | `/api/FiscalYears/{id}` | `UpdateFiscalYearCommand` | 204 | FiscalYearsUpdate |
| POST | `/api/FiscalYears/{id}/open` | — (no body) | 204 | FiscalYearsOpen |
| POST | `/api/FiscalYears/{id}/close` | — (no body) | 204 | FiscalYearsClose |
| GET | `/api/FiscalYears/by-date` | `?date=` (DateTime) | `GetFiscalYearPeriodByDateResult?` | FiscalYearsView |

## Fiscal Periods

| Method | Endpoint | Body/Query | Response | Permission |
|--------|----------|------------|----------|------------|
| GET | `/api/FiscalPeriods` | `?fiscalYearId=` | `FiscalPeriodDto[]` | FiscalPeriodsView |
| GET | `/api/FiscalPeriods/{id}` | — | `FiscalPeriodDto?` | FiscalPeriodsView |
| POST | `/api/FiscalPeriods` | `CreateFiscalPeriodCommand` | 204 | FiscalPeriodsCreate |
| PUT | `/api/FiscalPeriods/{id}` | `UpdateFiscalPeriodCommand` | 204 | FiscalPeriodsCreate |
| POST | `/api/FiscalPeriods/{id}/lock` | — (no body) | 204 | FiscalPeriodsLock |
| POST | `/api/FiscalPeriods/{id}/unlock` | — (no body) | 204 | FiscalPeriodsLock |
| POST | `/api/FiscalPeriods/bulk-generate` | `BulkGeneratePeriodsCommand` | 204 | FiscalPeriodsCreate |

## Document Sequences

| Method | Endpoint | Body/Query | Response | Permission |
|--------|----------|------------|----------|------------|
| GET | `/api/DocumentSequences` | `?isActive=` (optional) | `DocumentSequenceDto[]` | DocumentSequencesView |
| GET | `/api/DocumentSequences/{id}` | — | `DocumentSequenceDto?` | DocumentSequencesView |
| POST | `/api/DocumentSequences` | `CreateDocumentSequenceCommand` | 204 | DocumentSequencesCreate |
| PUT | `/api/DocumentSequences/{id}` | `UpdateDocumentSequenceCommand` | 204 | DocumentSequencesCreate |
| POST | `/api/DocumentSequences/{id}/deactivate` | — (no body, sets IsActive=false) | 204 | DocumentSequencesCreate |

## Closing Entries

| Method | Endpoint | Body/Query | Response | Permission |
|--------|----------|------------|----------|------------|
| GET | `/api/ClosingEntries/by-fiscal-year/{fiscalYearId}` | — | `ClosingEntryDto[]` | ClosingEntriesView |
| GET | `/api/ClosingEntries/{id}` | — | `ClosingEntryDto?` | ClosingEntriesView |
| POST | `/api/ClosingEntries/generate` | `GenerateYearEndClosingCommand` | `int` (new entry ID) | ClosingEntriesGenerate |
| POST | `/api/ClosingEntries/{id}/approve` | — (no body) | 204 | ClosingEntriesApprove |
| POST | `/api/ClosingEntries/{id}/reverse` | `ReverseClosingEntryCommand` | `int` (reversal entry ID) | ClosingEntriesReverse |

## Error Contract

All errors return `ProblemDetails` (RFC 7807):

```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Bad Request",
  "status": 400,
  "detail": "Overlap detected with fiscal year 2025",
  "errors": { "StartDate": ["Overlaps with existing open year"] }
}
```
