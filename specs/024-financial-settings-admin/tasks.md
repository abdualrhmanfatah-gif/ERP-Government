# Tasks: Financial Settings Administration UI

**Input**: Design documents from `/specs/024-financial-settings-admin/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Test tasks included per Constitution Principle XI (TDD mandatory for new features).

**Organization**: Tasks grouped by user story for independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3, US4, US5)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Feature folder structure, shared types, API client, and cache key factory

- [x] T001 [P] Create feature directory structure under `src/Web/ClientApp/src/features/financial-settings/` with subdirectories: shared/, hooks/, components/, fiscal-years/pages/, document-sequences/pages/, currencies/pages/, exchange-rates/pages/, closing-entries/pages/, __tests__/
- [x] T002 [P] Create `src/Web/ClientApp/src/features/financial-settings/shared/types.ts` with all DTOs (FiscalYearDto, FiscalPeriodDto, DocumentSequenceDto, CurrencyDto, Iso4217CodeDto, ExchangeRateDto, ExchangeRateLookupDto, ClosingEntryDto), enums (FiscalYearStatus, ExchangeRateType, ClosingEntryStatus, ResetPolicy), Arabic label maps, filter types, command types, and ProblemDetails interface per data-model.md
- [x] T003 [P] Create `src/Web/ClientApp/src/features/financial-settings/shared/client.ts` with hand-written fetch wrapper (ApiError, handleResponse, buildQuery), cache key factory (`financialSettingsKeys`), and API client objects (fiscalYearsClient, fiscalPeriodsClient, documentSequencesClient, currenciesClient, exchangeRatesClient, closingEntriesClient) per contracts/financial-settings-api.md
- [x] T004 [P] Create `src/Web/ClientApp/src/features/financial-settings/shared/index.ts` barrel re-export

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Hooks and reusable components that ALL user stories depend on

**CRITICAL**: No user story work can begin until this phase is complete

- [x] T005 [P] Create `src/Web/ClientApp/src/features/financial-settings/hooks/useFiscalYears.ts` with TanStack Query hooks: useFiscalYearsList, useFiscalYearDetail, useCreateFiscalYear, useUpdateFiscalYear, useOpenFiscalYear, useCloseFiscalYear
- [x] T006 [P] Create `src/Web/ClientApp/src/features/financial-settings/hooks/useFiscalPeriods.ts` with TanStack Query hooks: useFiscalPeriodsList, useFiscalPeriodDetail, useCreateFiscalPeriod, useUpdateFiscalPeriod, useLockFiscalPeriod, useUnlockFiscalPeriod, useBulkGeneratePeriods
- [x] T007 [P] Create `src/Web/ClientApp/src/features/financial-settings/hooks/useDocumentSequences.ts` with TanStack Query hooks: useDocumentSequencesList, useDocumentSequenceDetail, useCreateDocumentSequence, useUpdateDocumentSequence, useDeactivateDocumentSequence
- [x] T008 [P] Create `src/Web/ClientApp/src/features/financial-settings/hooks/useCurrencies.ts` with TanStack Query hooks: useCurrenciesList, useCurrencyDetail, useIso4217Codes, useCreateCurrency, useUpdateCurrency, useActivateCurrency, useDeactivateCurrency
- [x] T009 [P] Create `src/Web/ClientApp/src/features/financial-settings/hooks/useExchangeRates.ts` with TanStack Query hooks: useExchangeRatesList, useExchangeRateDetail, useLookupExchangeRate, useCreateExchangeRate, useUpdateExchangeRate, useActivateExchangeRate, useDeactivateExchangeRate
- [x] T010 [P] Create `src/Web/ClientApp/src/features/financial-settings/hooks/useClosingEntries.ts` with TanStack Query hooks: useClosingEntriesByFiscalYear, useClosingEntryDetail, useGenerateClosingEntry, useApproveClosingEntry, useReverseClosingEntry
- [x] T011 [P] Create `src/Web/ClientApp/src/features/financial-settings/components/FiscalYearStatusBadge.tsx` — status badge component for Draft/Open/SoftClosed/HardClosed using shared StatusBadge with Arabic labels
- [x] T012 [P] Create `src/Web/ClientApp/src/features/financial-settings/components/PeriodLockIndicator.tsx` — lock status indicator for fiscal periods (locked/unlocked icon + Arabic label)
- [x] T013 [P] Create `src/Web/ClientApp/src/features/financial-settings/components/ClosingEntryLines.tsx` — reviewable closing entry line table showing account, debit, credit, description with balanced totals
- [x] T014 [P] Create `src/Web/ClientApp/src/features/financial-settings/components/ExchangeRateLookup.tsx` — effective rate resolution display showing rate value, source date, and rate type

**Checkpoint**: Foundation ready — user story implementation can now begin in parallel

---

## Phase 3: User Story 1 — Fiscal Year Lifecycle (Priority: P1) — MVP

**Goal**: Admin can create fiscal years, generate periods, open/close years, lock/unlock periods

**Independent Test**: Create a fiscal year, generate 12 periods, open it, lock a period, close the year. Verify all status transitions and period management.

### Tests for User Story 1

- [x] T015 [P] [US1] Create `src/Web/ClientApp/src/features/financial-settings/__tests__/FiscalYearsListPage.test.tsx` — test list rendering, empty state, status badges, create button visibility
- [x] T016 [P] [US1] Create `src/Web/ClientApp/src/features/financial-settings/__tests__/FiscalYearDetailPage.test.tsx` — test detail view, periods table, lifecycle actions (open/close), period lock/unlock
- [x] T017 [P] [US1] Create `src/Web/ClientApp/src/features/financial-settings/__tests__/FiscalYearCreatePage.test.tsx` — test form rendering, validation, submission, error handling
- [x] T018 [P] [US1] Create `src/Web/ClientApp/src/features/financial-settings/__tests__/FiscalYearStatusBadge.test.tsx` — test all 4 status variants render correct Arabic labels
- [x] T019 [P] [US1] Create `src/Web/ClientApp/src/features/financial-settings/__tests__/PeriodLockIndicator.test.tsx` — test locked/unlocked states

### Implementation for User Story 1

- [x] T020 [US1] Create `src/Web/ClientApp/src/features/financial-settings/fiscal-years/pages/FiscalYearsListPage.tsx` — list page with table (name, yearNumber, startDate, endDate, status badge, audit info), FilterBar, create button, loading/empty states per budgeting pattern
- [x] T021 [US1] Create `src/Web/ClientApp/src/features/financial-settings/fiscal-years/pages/FiscalYearDetailPage.tsx` — detail page with info card (all fields + audit), status badge, lifecycle actions (Open/Close buttons gated by status), periods sub-table with lock/unlock actions, bulk generate button
- [x] T022 [US1] Create `src/Web/ClientApp/src/features/financial-settings/fiscal-years/pages/FiscalYearCreatePage.tsx` — create form with name, startDate, endDate fields, validation, back button, submit with error handling

**Checkpoint**: Fiscal year lifecycle fully functional — can create, generate periods, open, lock periods, close

---

## Phase 4: User Story 2 — Document Sequences Administration (Priority: P1)

**Goal**: Admin can view all seeded sequences, edit settings, deactivate sequences

**Independent Test**: View sequences list showing prefix/next number/format/active state. Edit a sequence name. Deactivate a sequence. Verify already-issued numbers are untouched.

### Tests for User Story 2

- [x] T023 [P] [US2] Create `src/Web/ClientApp/src/features/financial-settings/__tests__/DocumentSequencesListPage.test.tsx` — test list rendering with 10 seeded sequences, columns (prefix, next number, format, active), edit/deactivate actions

### Implementation for User Story 2

- [x] T024 [US2] Create `src/Web/ClientApp/src/features/financial-settings/document-sequences/pages/DocumentSequencesListPage.tsx` — list page with table showing all 10 seeded sequences (name, documentType, currentNumber, resetPolicy, isActive), inline edit for name/resetPolicy, deactivate action with ConfirmDialog, audit info display

**Checkpoint**: Document sequences visible and editable — state always visible in list view

---

## Phase 5: User Story 3 — Currency Management (Priority: P2)

**Goal**: Admin can create currencies from ISO-4217 list, activate/deactivate them

**Independent Test**: Create a currency from ISO list, verify it's inactive, activate it, deactivate it. Verify existing documents retain the currency.

### Tests for User Story 3

- [x] T025 [P] [US3] Create `src/Web/ClientApp/src/features/financial-settings/__tests__/CurrenciesListPage.test.tsx` — test list rendering, ISO search, activate/deactivate buttons, base currency indicator
- [x] T026 [P] [US3] Create `src/Web/ClientApp/src/features/financial-settings/__tests__/CurrencyCreatePage.test.tsx` — test ISO combobox search, form fields, validation, submission

### Implementation for User Story 3

- [x] T027 [US3] Create `src/Web/ClientApp/src/features/financial-settings/currencies/pages/CurrenciesListPage.tsx` — list page with table (code, name, symbol, decimalPlaces, isBase, isActive), activate/deactivate actions with ConfirmDialog, audit info
- [x] T028 [US3] Create `src/Web/ClientApp/src/features/financial-settings/currencies/pages/CurrencyCreatePage.tsx` — create form with ISO-4217 Combobox search (queries /api/Currencies/iso4217), auto-fill name/symbol/decimals from selection, submit

**Checkpoint**: Currency management functional — create from ISO list, activate, deactivate

---

## Phase 6: User Story 4 — Exchange Rate Management (Priority: P2)

**Goal**: Admin can record exchange rates, activate/deactivate, lookup by date

**Independent Test**: Record a rate for a currency pair, activate it, verify lookup returns the applicable rate. Activate a conflicting rate and verify overlap deactivation.

### Tests for User Story 4

- [x] T029 [P] [US4] Create `src/Web/ClientApp/src/features/financial-settings/__tests__/ExchangeRatesListPage.test.tsx` — test list rendering with filters (currency, rateType, date range), activate/deactivate actions
- [x] T030 [P] [US4] Create `src/Web/ClientApp/src/features/financial-settings/__tests__/ExchangeRateCreatePage.test.tsx` — test form with currency pair selects, date picker, rate type, rate input, validation
- [x] T031 [P] [US4] Create `src/Web/ClientApp/src/features/financial-settings/__tests__/ExchangeRateLookup.test.tsx` — test lookup display showing rate, source date, rate type

### Implementation for User Story 4

- [x] T032 [US4] Create `src/Web/ClientApp/src/features/financial-settings/exchange-rates/pages/ExchangeRatesListPage.tsx` — list page with table (baseCurrencyCode, currencyCode, rateDate, rateType, rate, isActive), FilterBar with currency/rateType/date filters, activate/deactivate with ConfirmDialog, audit info
- [x] T033 [US4] Create `src/Web/ClientApp/src/features/financial-settings/exchange-rates/pages/ExchangeRateCreatePage.tsx` — create form with base currency select, target currency select, rateDate picker, rateType select, rate input, submit

**Checkpoint**: Exchange rate management functional — create, lookup by date, activate with conflict deactivation

---

## Phase 7: User Story 5 — Closing Entries (Priority: P2)

**Goal**: Admin can generate closing entry proposals, approve, and reverse

**Independent Test**: Generate a closing entry for a fiscal year with posted activity, review balanced lines, approve it, reverse it. Verify status transitions and reversal linking.

### Tests for User Story 5

- [x] T034 [P] [US5] Create `src/Web/ClientApp/src/features/financial-settings/__tests__/ClosingEntriesListPage.test.tsx` — test list rendering by fiscal year, status badges, generate/approve/reverse actions gated by status
- [x] T035 [P] [US5] Create `src/Web/ClientApp/src/features/financial-settings/__tests__/ClosingEntryDetailPage.test.tsx` — test detail view with lines table, approve/reverse buttons, reversal link display
- [x] T036 [P] [US5] Create `src/Web/ClientApp/src/features/financial-settings/__tests__/ClosingEntryLines.test.tsx` — test lines table rendering with account, debit, credit, description columns and balanced totals

### Implementation for User Story 5

- [x] T037 [US5] Create `src/Web/ClientApp/src/features/financial-settings/closing-entries/pages/ClosingEntriesListPage.tsx` — list page filtered by fiscal year, table (closingEntryNumber, fiscalYearName, closingDate, status, isReversal), generate button, status-gated actions
- [x] T038 [US5] Create `src/Web/ClientApp/src/features/financial-settings/closing-entries/pages/ClosingEntryDetailPage.tsx` — detail page with info card, ClosingEntryLines table, approve/reverse actions with ConfirmDialog, reversal entry link display

**Checkpoint**: Closing entries fully functional — generate proposal, review lines, approve, reverse

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Final integration, routing, and quality checks

- [x] T039 Register financial-settings routes in `src/Web/ClientApp/src/App.tsx` (or route config) with nested routes for all 5 sub-sections: fiscal-years, document-sequences, currencies, exchange-rates, closing-entries
- [x] T040 Add navigation entry for Financial Settings in the sidebar/main nav with required permission identifier matching backend policy naming
- [x] T041 [P] Run `npm run lint` in `src/Web/ClientApp/` and fix any lint errors
- [x] T042 [P] Run `npm run test -- --run src/features/financial-settings/__tests__/` and verify all tests pass
- [x] T043 [P] Run `npm run build` in `src/Web/ClientApp/` and verify no build errors
- [x] T044 Run quickstart.md validation scenarios V1–V6 manually against running dev server

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **US1 (Phase 3)**: Depends on Foundational — MVP
- **US2 (Phase 4)**: Depends on Foundational — can run parallel with US1
- **US3 (Phase 5)**: Depends on Foundational — can run parallel with US1/US2
- **US4 (Phase 6)**: Depends on Foundational + US3 (currencies must exist for exchange rates) — can run parallel with US1/US2 after US3
- **US5 (Phase 7)**: Depends on Foundational + US1 (fiscal year must exist for closing entries) — can run parallel with US2/US3/US4 after US1
- **Polish (Phase 8)**: Depends on all desired user stories being complete

### User Story Dependencies

- **US1 (Fiscal Year Lifecycle, P1)**: Can start after Foundational — No dependencies on other stories
- **US2 (Document Sequences, P1)**: Can start after Foundational — Independent of other stories
- **US3 (Currencies, P2)**: Can start after Foundational — Independent of other stories
- **US4 (Exchange Rates, P2)**: Depends on US3 (needs currencies to exist) — Independent of US1/US2
- **US5 (Closing Entries, P2)**: Depends on US1 (needs fiscal year to exist) — Independent of US2/US3

### Within Each User Story

- Tests written FIRST (TDD per Constitution Principle XI)
- Shared types/client already exist from Setup phase
- Hooks already exist from Foundational phase
- Pages implement against existing hooks
- Components integrate into pages

### Parallel Opportunities

- All Phase 1 tasks (T001–T004) can run in parallel
- All Phase 2 tasks (T005–T014) can run in parallel
- US1 and US2 can run in parallel (P1 stories, independent)
- US3 can run in parallel with US1/US2
- All test tasks within a story marked [P] can run in parallel
- Phase 8 polish tasks marked [P] can run in parallel

---

## Parallel Example: User Story 1

```bash
# Launch all tests for US1 together (TDD — tests first):
Task: "T015 FiscalYearsListPage.test.tsx"
Task: "T016 FiscalYearDetailPage.test.tsx"
Task: "T017 FiscalYearCreatePage.test.tsx"
Task: "T018 FiscalYearStatusBadge.test.tsx"
Task: "T019 PeriodLockIndicator.test.tsx"

# Then implement (tests should fail before implementation):
Task: "T020 FiscalYearsListPage.tsx"
Task: "T021 FiscalYearDetailPage.tsx"
Task: "T022 FiscalYearCreatePage.tsx"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (shared types + client)
2. Complete Phase 2: Foundational (all hooks + components)
3. Complete Phase 3: User Story 1 (fiscal year lifecycle)
4. **STOP and VALIDATE**: Test fiscal year create → periods → open → lock → close flow
5. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add US1 (Fiscal Years) → Test independently → Deploy/Demo (MVP!)
3. Add US2 (Document Sequences) → Test independently → Deploy/Demo
4. Add US3 (Currencies) → Test independently → Deploy/Demo
5. Add US4 (Exchange Rates) → Test independently → Deploy/Demo (needs US3)
6. Add US5 (Closing Entries) → Test independently → Deploy/Demo (needs US1)
7. Polish → Final integration → Full admin UI complete

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: US1 (Fiscal Years) → then US5 (Closing Entries, depends on US1)
   - Developer B: US2 (Document Sequences) — fully independent
   - Developer C: US3 (Currencies) → then US4 (Exchange Rates, depends on US3)
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- TDD mandatory: tests first, observe red, implement, green
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- All monetary values use shared MoneyDisplay component (Principle X)
- All RTL layout using logical properties (start/end, not left/right)
- Arabic-first labels from shared/types.ts label maps
- Permission codes from usePermission hook (registered exception #4 — stubs)
