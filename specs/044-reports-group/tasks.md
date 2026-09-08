# Tasks: Reports Group (RPT-01..06)

**Input**: Design documents from `/specs/044-reports-group/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md

**Tests**: No frontend tests — governance decision (AGENTS.md Frontend Architecture). Backend tests already exist.

**Organization**: Tasks grouped by report (user story) for independent implementation. Backend is 100% complete — all tasks are frontend-only.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which report this task belongs to (US1-US6 mapping to RPT-01..06)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Extend query keys, routes, and navigation to support all 6 reports

- [ ] T001 [P] Extend `src/Web/ClientApp/src/shared/api/query-keys.ts` — add `reportingKeys` entries for revenueCollections, disbursementRegister, availabilitySnapshot, trialBalance, and financialStatements (balanceSheet, incomeStatement, generalLedger, cashFlow, trialBalanceLegacy)
- [ ] T002 [P] Extend `src/Web/ClientApp/src/app/routes.tsx` — add routes for `/reporting/revenue-collections`, `/reporting/disbursement-register`, `/reporting/availability-snapshot`, `/reporting/trial-balance`, `/reporting/financial-statements/balance-sheet`, `/reporting/financial-statements/income-statement`, `/reporting/financial-statements/general-ledger`, `/reporting/financial-statements/cash-flow`, `/reporting/financial-statements/trial-balance` with appropriate permissions
- [ ] T003 [P] Extend `src/Web/ClientApp/src/layouts/navigation.ts` — add nav items under "التقارير" group for all new reports with Arabic labels

**Checkpoint**: Shared infrastructure ready — individual reports can now be built independently

---

## Phase 2: User Story 1 — Revenue Collections Report (RPT-02, Priority: P1)

**Goal**: Revenue officer monitors receipt vouchers and deposit status for a given period

**Independent Test**: Navigate to `/reporting/revenue-collections`, filter by period/party/method, verify lines load, click voucher for detail (lines + checks + deposit card), verify cancelled vouchers marked, export matches screen

### Implementation

- [ ] T004 [P] [US1] Create `src/Web/ClientApp/src/features/reporting/revenue-collections-report/shared/types.ts` — re-export DTOs from web-api-client.ts + define `RevenueCollectionsFilters` interface
- [ ] T005 [P] [US1] Create `src/Web/ClientApp/src/features/reporting/revenue-collections-report/shared/schemas.ts` — Zod schema for filter validation (fiscalYearId required, party/paymentMethod/status optional)
- [ ] T006 [P] [US1] Create `src/Web/ClientApp/src/features/reporting/revenue-collections-report/hooks/useRevenueCollectionsReport.ts` — TanStack Query hooks: `useRevenueCollectionsReport(filters)`, `useRevenueCollectionsDetail(receiptVoucherId)`, `useRevenueCollectionsFiscalYears()`, `useRevenueCollectionsFilterOptions()`
- [ ] T007 [P] [US1] Create `src/Web/ClientApp/src/components/ReportingRevenueCollectionsFilters.tsx` — filter bar with period, party, payment method, status dropdowns + clear button (follow ReportingBudgetExecutionFilters pattern)
- [ ] T008 [P] [US1] Create `src/Web/ClientApp/src/components/ReportingRevenueCollectionsDetail.tsx` — sheet with three sections: lines table + checks table + deposit slip info (follow ReportingBudgetExecutionDetail pattern)
- [ ] T009 [US1] Create `src/Web/ClientApp/src/features/reporting/revenue-collections-report/pages/RevenueCollectionsReportPage.tsx` — full page: filter bar + DataGrid (voucherNumber, voucherDate, partyName, totalAmount, paymentMethod, depositStatus) + cancelled voucher marking + export buttons (Excel/PDF) + empty/error/loading states + partial-data warning

**Checkpoint**: Revenue Collections report fully functional — filter, list, detail drill-down, export

---

## Phase 3: User Story 2 — Disbursement Register Report (RPT-03, Priority: P2)

**Goal**: Payment controller sees disbursement register with status counters at a glance

**Independent Test**: Navigate to `/reporting/disbursement-register`, verify status counter cards + lines, verify counters match line counts, click order for detail (payments, approver, paidAt), export matches screen

### Implementation

- [ ] T010 [P] [US2] Create `src/Web/ClientApp/src/features/reporting/disbursement-register-report/shared/types.ts` — re-export DTOs + `DisbursementRegisterFilters` interface
- [ ] T011 [P] [US2] Create `src/Web/ClientApp/src/features/reporting/disbursement-register-report/shared/schemas.ts` — Zod schema (fiscalYearId required, status/fund optional)
- [ ] T012 [P] [US2] Create `src/Web/ClientApp/src/features/reporting/disbursement-register-report/hooks/useDisbursementRegisterReport.ts` — TanStack Query hooks for list + detail
- [ ] T013 [P] [US2] Create `src/Web/ClientApp/src/components/ReportingDisbursementRegisterFilters.tsx` — filter bar
- [ ] T014 [P] [US2] Create `src/Web/ClientApp/src/components/ReportingDisbursementRegisterDetail.tsx` — sheet with payments table + approver + paidAt
- [ ] T015 [US2] Create `src/Web/ClientApp/src/features/reporting/disbursement-register-report/pages/DisbursementRegisterReportPage.tsx` — full page: status counter cards (draft/submitted/approved/paid/rejected + totalRequests) + DataGrid + detail drill-down + export + empty/error states

**Checkpoint**: Disbursement Register report fully functional — counters, list, detail, export

---

## Phase 4: User Story 3 — Availability Snapshot Report (RPT-04, Priority: P2)

**Goal**: Budget controller sees budget items with control state indicators

**Independent Test**: Navigate to `/reporting/availability-snapshot`, verify controlState colored indicators 100%, click item for triple detail (appropriations + encumbrances + payments), export matches screen

### Implementation

- [ ] T016 [P] [US3] Create `src/Web/ClientApp/src/features/reporting/availability-snapshot-report/shared/types.ts` — re-export DTOs + `AvailabilitySnapshotFilters` interface
- [ ] T017 [P] [US3] Create `src/Web/ClientApp/src/features/reporting/availability-snapshot-report/shared/schemas.ts` — Zod schema (fiscalYearId required, organizationalUnit optional)
- [ ] T018 [P] [US3] Create `src/Web/ClientApp/src/features/reporting/availability-snapshot-report/hooks/useAvailabilitySnapshotReport.ts` — TanStack Query hooks for list + detail
- [ ] T019 [P] [US3] Create `src/Web/ClientApp/src/components/ReportingAvailabilitySnapshotFilters.tsx` — filter bar
- [ ] T020 [P] [US3] Create `src/Web/ClientApp/src/components/ReportingAvailabilitySnapshotDetail.tsx` — sheet with three groups: appropriations + encumbrances + payments
- [ ] T021 [US3] Create `src/Web/ClientApp/src/features/reporting/availability-snapshot-report/pages/AvailabilitySnapshotReportPage.tsx` — full page: DataGrid with controlState colored indicator column + financial amounts + triple detail drill-down + export + empty/error states

**Checkpoint**: Availability Snapshot report fully functional — indicators, list, triple detail, export

---

## Phase 5: User Story 4 — Trial Balance Report (RPT-05, Priority: P1)

**Goal**: Accountant verifies books balance — debits equal credits

**Independent Test**: Navigate to `/reporting/trial-balance`, verify lines with 5 columns + 4 totals, verify balance indicator when balanced, verify red warning when imbalanced, click account for ledger movement, verify open period warning, export matches screen

### Implementation

- [ ] T022 [P] [US4] Create `src/Web/ClientApp/src/features/reporting/trial-balance-report/shared/types.ts` — re-export DTOs + `TrialBalanceFilters` interface
- [ ] T023 [P] [US4] Create `src/Web/ClientApp/src/features/reporting/trial-balance-report/shared/schemas.ts` — Zod schema (fiscalYearId required, fiscalPeriodId optional)
- [ ] T024 [P] [US4] Create `src/Web/ClientApp/src/features/reporting/trial-balance-report/hooks/useTrialBalanceReport.ts` — TanStack Query hooks: `useTrialBalanceReport(filters)`, `useLedgerMovement(accountId)`, `useTrialBalanceFiscalYears()`
- [ ] T025 [P] [US4] Create `src/Web/ClientApp/src/components/ReportingTrialBalanceFilters.tsx` — filter bar with fiscal year + period
- [ ] T026 [P] [US4] Create `src/Web/ClientApp/src/components/ReportingTrialBalanceDetail.tsx` — sheet with ledger movement entries table + totals
- [ ] T027 [US4] Create `src/Web/ClientApp/src/features/reporting/trial-balance-report/pages/TrialBalanceReportPage.tsx` — full page: DataGrid (accountCode, accountName, accountType, openingBalance, debitTotal, creditTotal, closingBalance) + totals row + balance indicator (computed: Σdebits == Σcredits, labeled "عرض") + red warning when imbalance + open period warning "أرقام قابلة للتغير" + ledger movement drill-down + export + empty/error states

**Checkpoint**: Trial Balance report fully functional — balance indicator, ledger movement, period warning, export

---

## Phase 6: User Story 5 — Financial Statements (RPT-06, Priority: P1/P2 mix)

**Goal**: Financial manager accesses official statements: balance sheet, income statement, general ledger, cash flow, legacy trial balance

**Independent Test**: Navigate to each statement page, verify correct data display, verify GL pagination with runningBalance, verify bilingual titleAr in RTL, verify export for each

### Implementation

#### Balance Sheet (P1)

- [ ] T028 [P] [US5] Create `src/Web/ClientApp/src/features/reporting/financial-statements/shared/types.ts` — re-export all financial statement DTOs + filter interfaces
- [ ] T029 [P] [US5] Create `src/Web/ClientApp/src/features/reporting/financial-statements/shared/schemas.ts` — Zod schemas for each statement (asOfDate for balance sheet, startDate/endDate for income statement, accountId + pagination for GL, etc.)
- [ ] T030 [P] [US5] Create `src/Web/ClientApp/src/features/reporting/financial-statements/hooks/useBalanceSheet.ts` — TanStack Query hook for balance sheet
- [ ] T031 [P] [US5] Create `src/Web/ClientApp/src/components/ReportingFinancialStatementsFilters.tsx` — filter bar with date pickers for each statement type
- [ ] T032 [US5] Create `src/Web/ClientApp/src/features/reporting/financial-statements/pages/BalanceSheetPage.tsx` — full page: assets/liabilities/equity groups with titleAr in RTL + liabilitiesAndEquity + balanced indicator + red warning when balanced=false + export

#### Income Statement (P1)

- [ ] T033 [P] [US5] Create `src/Web/ClientApp/src/features/reporting/financial-statements/hooks/useIncomeStatement.ts` — TanStack Query hook
- [ ] T034 [US5] Create `src/Web/ClientApp/src/features/reporting/financial-statements/pages/IncomeStatementPage.tsx` — full page: revenue/expenses groups + netIncome prominently displayed + export

#### General Ledger (P1)

- [ ] T035 [P] [US5] Create `src/Web/ClientApp/src/features/reporting/financial-statements/hooks/useGeneralLedger.ts` — TanStack Query hook with page/pageSize params
- [ ] T036 [US5] Create `src/Web/ClientApp/src/features/reporting/financial-statements/pages/GeneralLedgerPage.tsx` — full page: paginated DataGrid (documentDate, entryNumber, reference, narration, accountCode, accountName, debit, credit, runningBalance) + page controls + account/period filters + export

#### Cash Flow Statement (P2)

- [ ] T037 [P] [US5] Create `src/Web/ClientApp/src/features/reporting/financial-statements/hooks/useCashFlowStatement.ts` — TanStack Query hook
- [ ] T038 [US5] Create `src/Web/ClientApp/src/features/reporting/financial-statements/pages/CashFlowStatementPage.tsx` — full page: bilingual sections (title/titleAr in RTL) with items + totals + export

#### Legacy Trial Balance (P2)

- [ ] T039 [P] [US5] Create `src/Web/ClientApp/src/features/reporting/financial-statements/hooks/useTrialBalanceLegacy.ts` — TanStack Query hook
- [ ] T040 [US5] Create `src/Web/ClientApp/src/features/reporting/financial-statements/pages/TrialBalanceLegacyPage.tsx` — full page: sections + isBalanced + totalDebit/Credit + export

**Checkpoint**: All 5 financial statements functional — balance sheet, income statement, GL pagination, cash flow bilingual, legacy trial balance

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Final integration, verification, and cleanup

- [ ] T041 Verify all report routes load correctly and navigation items work
- [ ] T042 Verify all export buttons produce Excel + PDF matching screen for each report
- [ ] T043 Verify empty states render "لا توجد بيانات للفترة المحددة" with Arabic artwork for all reports
- [ ] T044 Verify RTL rendering across all report pages and exports
- [ ] T045 Verify unauthorized state renders when user lacks permission
- [ ] T046 Run `npm run lint` and fix any issues
- [ ] T047 Run `npm run build` and verify no build errors

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — can start immediately
- **Phase 2-6 (User Stories)**: All depend on Phase 1 completion — can proceed in parallel after setup
- **Phase 7 (Polish)**: Depends on all desired user stories being complete

### User Story Dependencies

- **RPT-02 Revenue Collections (P1)**: Independent after Phase 1
- **RPT-03 Disbursement Register (P2)**: Independent after Phase 1
- **RPT-04 Availability Snapshot (P2)**: Independent after Phase 1
- **RPT-05 Trial Balance (P1)**: Independent after Phase 1
- **RPT-06 Financial Statements (P1/P2)**: Independent after Phase 1; internal sub-reports are independent of each other

### Parallel Opportunities

- Phase 1 tasks T001-T003 can run in parallel (different files)
- All Phase 2-6 reports can be built in parallel by different developers
- Within each report: types, schemas, hooks, components can run in parallel (T004-T006, T010-T012, etc.)
- Phase 6 sub-reports (balance sheet, income statement, GL, cash flow, legacy TB) can run in parallel

---

## Parallel Example: Revenue Collections (RPT-02)

```bash
# Launch all shared files in parallel:
Task: "Create types.ts for revenue-collections-report"
Task: "Create schemas.ts for revenue-collections-report"
Task: "Create hooks for revenue-collections-report"
Task: "Create ReportingRevenueCollectionsFilters.tsx"
Task: "Create ReportingRevenueCollectionsDetail.tsx"

# Then implement page (depends on above):
Task: "Create RevenueCollectionsReportPage.tsx"
```

---

## Implementation Strategy

### MVP First (RPT-02 + RPT-05 — P1 reports)

1. Complete Phase 1: Setup (T001-T003)
2. Complete Phase 2: RPT-02 Revenue Collections (T004-T009)
3. Complete Phase 5: RPT-05 Trial Balance (T022-T027)
4. **STOP and VALIDATE**: Test both P1 reports independently
5. Deploy/demo if ready

### Incremental Delivery

1. Phase 1 → Foundation ready
2. Phase 2 (RPT-02) → Test independently → Deploy/Demo
3. Phase 5 (RPT-05) → Test independently → Deploy/Demo
4. Phase 3 (RPT-03) → Test independently → Deploy/Demo
5. Phase 4 (RPT-04) → Test independently → Deploy/Demo
6. Phase 6 (RPT-06) → Test independently → Deploy/Demo
7. Phase 7 → Polish all reports

### Parallel Team Strategy

With multiple developers:

1. Team completes Phase 1 together
2. Once Phase 1 is done:
   - Developer A: Phase 2 (RPT-02 Revenue Collections)
   - Developer B: Phase 5 (RPT-05 Trial Balance)
   - Developer C: Phase 6 (RPT-06 Financial Statements)
   - Developer D: Phase 3 (RPT-03 Disbursement Register)
   - Developer E: Phase 4 (RPT-04 Availability Snapshot)
3. All reports complete and integrate independently

---

## Notes

- Backend is 100% complete — no backend tasks needed
- All DTOs exist in `src/Application/Reporting/` and `src/Application/Accounting/Reports/`
- NSwag-generated clients exist in `web-api-client.ts` for all 6 reports
- Follow RPT-01 (`features/reporting/budget-execution-report/`) as the reference exemplar
- Feature-scoped components go in `src/components/` with domain prefix
- No frontend tests by governance decision
- Arabic-only hardcoded strings — no i18n
- RTL logical CSS properties only (ms-/me-, ps-/pe-, start/end)
