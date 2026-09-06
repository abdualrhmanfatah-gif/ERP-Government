# Tasks: Financial Settings Administration UI

**Input**: Design documents from `/specs/024-financial-settings-admin/`
**Prerequisites**: plan.md, spec.md, data-model.md, contracts/

## Phase 1: Setup (Shared Infrastructure)

- [x] T001 Create feature directory structure
- [x] T002 Create shared/types.ts — DTOs, enums, label maps, command types
- [x] T003 Create shared/client.ts — API client + cache key factory
- [x] T004 Create shared/index.ts — barrel export

## Phase 2: Foundational

- [x] T005 Create useFiscalYears.ts — hooks
- [x] T006 Create useFiscalPeriods.ts — hooks
- [x] T007 Create useDocumentSequences.ts — hooks
- [x] T008 Create useCurrencies.ts — hooks
- [x] T009 Create useExchangeRates.ts — hooks
- [x] T010 Create useClosingEntries.ts — hooks
- [x] T011 Create FiscalYearStatusBadge.tsx
- [x] T012 Create PeriodLockIndicator.tsx
- [x] T013 Create ClosingEntryLines.tsx
- [x] T014 Create ExchangeRateLookup.tsx

## Phase 3: US1 — Fiscal Year Lifecycle

- [x] T015 FiscalYearsListPage.test.tsx
- [x] T016 FiscalYearDetailPage.test.tsx
- [x] T017 FiscalYearCreatePage.test.tsx
- [x] T018 FiscalYearStatusBadge.test.tsx
- [x] T019 PeriodLockIndicator.test.tsx
- [x] T020 FiscalYearsListPage.tsx
- [x] T021 FiscalYearDetailPage.tsx
- [x] T022 FiscalYearCreatePage.tsx

## Phase 4: US2 — Document Sequences

- [x] T023 DocumentSequencesListPage.test.tsx
- [x] T024 DocumentSequencesListPage.tsx

## Phase 5: US3 — Currencies (consumes 027)

- [x] T025 CurrenciesListPage.test.tsx
- [x] T026 CurrencyCreatePage.test.tsx
- [x] T027 CurrenciesListPage.tsx
- [x] T028 CurrencyCreatePage.tsx

## Phase 6: US4 — Exchange Rates

- [x] T029 ExchangeRatesListPage.test.tsx
- [x] T030 ExchangeRateCreatePage.test.tsx
- [x] T031 ExchangeRateLookup.test.tsx
- [x] T032 ExchangeRatesListPage.tsx
- [x] T033 ExchangeRateCreatePage.tsx

## Phase 7: US5 — Closing Entries

- [x] T034 ClosingEntriesListPage.test.tsx
- [x] T035 ClosingEntryDetailPage.test.tsx
- [x] T036 ClosingEntryLines.test.tsx
- [x] T037 ClosingEntriesListPage.tsx
- [x] T038 ClosingEntryDetailPage.tsx

## Phase 8: Polish

- [x] T039 Register routes
- [x] T040 Add navigation entry
- [x] T041 Run lint
- [x] T042 Run tests
- [x] T043 Run build

## Permission Repair (024-specific)

- [x] T044 Add DocumentSequencesUpdate + DocumentSequencesDeactivate to PermissionCodes.cs
- [x] T045 Register policies in DependencyInjection.cs
- [x] T046 Fix DocumentSequences.cs endpoint permissions (Update→DocumentSequencesUpdate, Deactivate→DocumentSequencesDeactivate)
- [x] T047 Add Update/Deactivate to frontend permissions.ts
- [x] T048 Update DocumentSequencesListPage.tsx to use granular permissions

## Field Coverage Verification

- [ ] T049 Verify FiscalYear fields: name, yearNumber, startDate, endDate, status, isClosed
- [ ] T050 Verify FiscalPeriod fields: periodNumber, name, startDate, endDate, isLockedForPosting
- [ ] T051 Verify DocumentSequence fields: name, documentType, currentNumber, resetPolicy, isActive
- [ ] T052 Verify Currency fields: code, name, symbol, decimalPlaces, roundingPrecision, isBase, isActive
- [ ] T053 Verify ExchangeRate fields: baseCurrencyCode, currencyCode, rateDate, rateType, rate, isActive
- [ ] T054 Verify ClosingEntry fields: closingEntryNumber, fiscalYearName, closingDate, status, isReversal
