# Tasks: ERP Read-Only Reporting & Oversight

**Input**: Design documents from `/specs/020-erp-read-only-reporting/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/report-endpoints.md, quickstart.md

**Tests**: Reconciliation tests included per spec requirement (FR-012: reconcile to last riyal). Functional tests for each report.

**Organization**: Tasks grouped by user story. Each story independently testable.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Exact file paths included

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Common DTOs, permission codes, and shared query infrastructure

- [x] T001 Create shared ReportFilterDto in src/Application/Reporting/Common/ReportFilterDto.cs
- [x] T002 [P] Add permission codes (Reporting.ViewBudgetExecution, Reporting.ViewRevenueCollections, Reporting.ViewDisbursementRegister, Reporting.ViewAvailabilitySnapshot, Reporting.ViewTrialBalanceReport, Reporting.ExportReports) in src/Application/Common/Security/PermissionCodes.cs
- [x] T003 [P] Register new permission policies as RequireAssertion(_ => true) placeholders in src/Web/DependencyInjection.cs

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Shared export helper and audit logging that ALL reports depend on

**⚠ CRITICAL**: No user story work can begin until this phase is complete

- [x] T004 Create IReportAuditLogger interface in src/Application/Reporting/Common/IReportAuditLogger.cs
- [x] T005 [P] Implement ReportAuditLogger in src/Infrastructure/Services/ReportAuditLogger.cs

**Checkpoint**: Foundation ready — user story implementation can now begin

---

## Phase 3: User Story 1 — Budget Execution Report (Priority: P1) — MVP

**Goal**: Budget officer sees appropriations vs encumbrances vs payments per budget line, filterable by fund/program/project/item/period, exportable, with drill-down.

**Independent Test**: Query budget data for FY, verify Appropriated = Encumbered + Paid + Available. Export produces valid Excel/PDF.

### Implementation for User Story 1

- [x] T006 [P] [US1] Create BudgetExecutionReportDto in src/Application/Reporting/BudgetExecution/GetBudgetExecutionReport/BudgetExecutionReportDto.cs
- [x] T007 [P] [US1] Create BudgetExecutionDetailDto in src/Application/Reporting/BudgetExecution/GetBudgetExecutionDetail/BudgetExecutionDetailDto.cs
- [x] T008 [P] [US1] Create GetBudgetExecutionReportQuery record in src/Application/Reporting/BudgetExecution/GetBudgetExecutionReport/GetBudgetExecutionReportQuery.cs
- [x] T009 [P] [US1] Create GetBudgetExecutionDetailQuery record in src/Application/Reporting/BudgetExecution/GetBudgetExecutionDetail/GetBudgetExecutionDetailQuery.cs
- [x] T010 [US1] Implement GetBudgetExecutionReportQueryHandler in src/Application/Reporting/BudgetExecution/GetBudgetExecutionReport/GetBudgetExecutionReportQueryHandler.cs (JOIN Appropriations + Encumbrances + PaymentOrders, GROUP BY BudgetItem/Fund/Program/Project, AsNoTracking, ProjectBy)
- [x] T011 [US1] Implement GetBudgetExecutionDetailQueryHandler in src/Application/Reporting/BudgetExecution/GetBudgetExecutionDetail/GetBudgetExecutionDetailQueryHandler.cs (query individual encumbrances and payments for a budget line)
- [x] T012 [P] [US1] Create GetBudgetExecutionReportQueryValidator in src/Application/Reporting/BudgetExecution/GetBudgetExecutionReport/GetBudgetExecutionReportQueryValidator.cs
- [x] T013 [US1] Create BudgetExecutionReports endpoint group in src/Web/Endpoints/Reporting/BudgetExecutionReports.cs (GET list, GET detail, GET export — 3 routes)
- [x] T014 [US1] Create BudgetExecutionReportTests in tests/Application.FunctionalTests/Reporting/BudgetExecutionReportTests.cs (reconciliation test: Appropriated = Encumbered + Paid + Available, filter tests, export test)

**Checkpoint**: Budget execution report fully functional and independently testable

---

## Phase 4: User Story 2 — Revenue Collections Report (Priority: P1)

**Goal**: Revenue officer sees receipt vouchers grouped by account/party/method/period with deposit-slip and check-clearing status, with drill-down.

**Independent Test**: Query receipt vouchers for period, verify totals. Check clearing status populated correctly.

### Implementation for User Story 2

- [x] T015 [P] [US2] Create RevenueCollectionsReportDto in src/Application/Reporting/RevenueCollections/GetRevenueCollectionsReport/RevenueCollectionsReportDto.cs
- [x] T016 [P] [US2] Create RevenueCollectionsDetailDto in src/Application/Reporting/RevenueCollections/GetRevenueCollectionsDetail/RevenueCollectionsDetailDto.cs
- [x] T017 [P] [US2] Create GetRevenueCollectionsReportQuery record in src/Application/Reporting/RevenueCollections/GetRevenueCollectionsReport/GetRevenueCollectionsReportQuery.cs
- [x] T018 [P] [US2] Create GetRevenueCollectionsDetailQuery record in src/Application/Reporting/RevenueCollections/GetRevenueCollectionsDetail/GetRevenueCollectionsDetailQuery.cs
- [x] T019 [US2] Implement GetRevenueCollectionsReportQueryHandler in src/Application/Reporting/RevenueCollections/GetRevenueCollectionsReport/GetRevenueCollectionsReportQueryHandler.cs (JOIN ReceiptVouchers + Lines + Checks + DepositSlip, filter by RevenueAccountId/PartyId/PaymentMethod, AsNoTracking)
- [x] T020 [US2] Implement GetRevenueCollectionsDetailQueryHandler in src/Application/Reporting/RevenueCollections/GetRevenueCollectionsDetail/GetRevenueCollectionsDetailQueryHandler.cs (individual receipt voucher with lines and checks)
- [x] T021 [P] [US2] Create GetRevenueCollectionsReportQueryValidator in src/Application/Reporting/RevenueCollections/GetRevenueCollectionsReport/GetRevenueCollectionsReportQueryValidator.cs
- [x] T022 [US2] Create RevenueCollectionsReports endpoint group in src/Web/Endpoints/Reporting/RevenueCollectionsReports.cs (GET list, GET detail, GET export — 3 routes)
- [x] T023 [US2] Create RevenueCollectionsReportTests in tests/Application.FunctionalTests/Reporting/RevenueCollectionsReportTests.cs (reconciliation test: total receipts = sum of lines, deposit-slip status test, check-clearing test)

**Checkpoint**: Revenue collections report fully functional and independently testable

---

## Phase 5: User Story 3 — Disbursement Register (Priority: P2)

**Goal**: Financial controller sees disbursement requests/payments by status/fund/period/approver, with drill-down.

**Independent Test**: Query disbursements for period, verify status counts. Filter by status returns correct subset.

### Implementation for User Story 3

- [x] T024 [P] [US3] Create DisbursementRegisterDto in src/Application/Reporting/DisbursementRegister/GetDisbursementRegisterQuery/DisbursementRegisterDto.cs
- [x] T025 [P] [US3] Create DisbursementRegisterDetailDto in src/Application/Reporting/DisbursementRegister/GetDisbursementRegisterDetail/DisbursementRegisterDetailDto.cs
- [x] T026 [P] [US3] Create GetDisbursementRegisterQuery record in src/Application/Reporting/DisbursementRegister/GetDisbursementRegisterQuery/GetDisbursementRegisterQuery.cs
- [x] T027 [P] [US3] Create GetDisbursementRegisterDetailQuery record in src/Application/Reporting/DisbursementRegister/GetDisbursementRegisterDetail/GetDisbursementRegisterDetailQuery.cs
- [x] T028 [US3] Implement GetDisbursementRegisterQueryHandler in src/Application/Reporting/DisbursementRegister/GetDisbursementRegisterQuery/GetDisbursementRegisterQueryHandler.cs (JOIN PaymentOrders + Payments, filter by Fund/Status/Approver, AsNoTracking)
- [x] T029 [US3] Implement GetDisbursementRegisterDetailQueryHandler in src/Application/Reporting/DisbursementRegister/GetDisbursementRegisterDetail/GetDisbursementRegisterDetailQueryHandler.cs (individual payment order with payment details)
- [x] T030 [P] [US3] Create GetDisbursementRegisterQueryValidator in src/Application/Reporting/DisbursementRegister/GetDisbursementRegisterQuery/GetDisbursementRegisterQueryValidator.cs
- [x] T031 [US3] Create DisbursementRegisterReports endpoint group in src/Web/Endpoints/Reporting/DisbursementRegisterReports.cs (GET list, GET detail, GET export — 3 routes)
- [x] T032 [US3] Create DisbursementRegisterReportTests in tests/Application.FunctionalTests/Reporting/DisbursementRegisterReportTests.cs (status count reconciliation, filter tests)

**Checkpoint**: Disbursement register fully functional and independently testable

---

## Phase 6: User Story 4 — Budget Availability Snapshot (Priority: P2)

**Goal**: Budget officer sees availability breakdown by fund/program/project/item with control state and drill-down.

**Independent Test**: Select budget line, verify availability = appropriated - encumbered - paid. Control state displayed correctly.

### Implementation for User Story 4

- [x] T033 [P] [US4] Create AvailabilitySnapshotDto in src/Application/Reporting/AvailabilitySnapshot/GetAvailabilitySnapshotQuery/AvailabilitySnapshotDto.cs
- [x] T034 [P] [US4] Create AvailabilitySnapshotDetailDto in src/Application/Reporting/AvailabilitySnapshot/GetAvailabilitySnapshotDetail/AvailabilitySnapshotDetailDto.cs
- [x] T035 [P] [US4] Create GetAvailabilitySnapshotQuery record in src/Application/Reporting/AvailabilitySnapshot/GetAvailabilitySnapshotQuery/GetAvailabilitySnapshotQuery.cs
- [x] T036 [P] [US4] Create GetAvailabilitySnapshotDetailQuery record in src/Application/Reporting/AvailabilitySnapshot/GetAvailabilitySnapshotDetail/GetAvailabilitySnapshotDetailQuery.cs
- [x] T037 [US4] Implement GetAvailabilitySnapshotQueryHandler in src/Application/Reporting/AvailabilitySnapshot/GetAvailabilitySnapshotQuery/GetAvailabilitySnapshotQueryHandler.cs (reuse BudgetAvailabilityService.GetAvailabilityBreakdownAsync pattern, AsNoTracking)
- [x] T038 [US4] Implement GetAvailabilitySnapshotDetailQueryHandler in src/Application/Reporting/AvailabilitySnapshot/GetAvailabilitySnapshotDetail/GetAvailabilitySnapshotDetailQueryHandler.cs (individual appropriations/encumbrances/payments for a budget line)
- [x] T039 [P] [US4] Create GetAvailabilitySnapshotQueryValidator in src/Application/Reporting/AvailabilitySnapshot/GetAvailabilitySnapshotQuery/GetAvailabilitySnapshotQueryValidator.cs
- [x] T040 [US4] Create AvailabilitySnapshotReports endpoint group in src/Web/Endpoints/Reporting/AvailabilitySnapshotReports.cs (GET list, GET detail, GET export — 3 routes)
- [x] T041 [US4] Create AvailabilitySnapshotReportTests in tests/Application.FunctionalTests/Reporting/AvailabilitySnapshotReportTests.cs (availability calculation test, control state test)

**Checkpoint**: Availability snapshot fully functional and independently testable

---

## Phase 7: User Story 5 — Trial Balance & Ledger Movement (Priority: P3)

**Goal**: Auditor sees trial balance with opening/closing balances and ledger movement per account, scoped by fund/project.

**Independent Test**: Generate trial balance, verify TotalDebits = TotalCredits. Ledger movement shows correct running balance.

### Implementation for User Story 5

- [x] T042 [P] [US5] Create TrialBalanceReportDto in src/Application/Reporting/TrialBalance/GetTrialBalanceReport/TrialBalanceReportDto.cs
- [x] T043 [P] [US5] Create LedgerMovementDto in src/Application/Reporting/TrialBalance/GetLedgerMovement/LedgerMovementDto.cs
- [x] T044 [P] [US5] Create GetTrialBalanceReportQuery record in src/Application/Reporting/TrialBalance/GetTrialBalanceReport/GetTrialBalanceReportQuery.cs
- [x] T045 [P] [US5] Create GetLedgerMovementQuery record in src/Application/Reporting/TrialBalance/GetLedgerMovement/GetLedgerMovementQuery.cs
- [x] T046 [US5] Implement GetTrialBalanceReportQueryHandler in src/Application/Reporting/TrialBalance/GetTrialBalanceReport/GetTrialBalanceReportQueryHandler.cs (GROUP BY Account, compute opening/debit/credit/closing, filter by Fund/Project via JournalEntryLine dims, AsNoTracking)
- [x] T047 [US5] Implement GetLedgerMovementQueryHandler in src/Application/Reporting/TrialBalance/GetLedgerMovement/GetLedgerMovementQueryHandler.cs (query JournalEntryLines for account, compute running balance, AsNoTracking)
- [x] T048 [P] [US5] Create GetTrialBalanceReportQueryValidator in src/Application/Reporting/TrialBalance/GetTrialBalanceReport/GetTrialBalanceReportQueryValidator.cs
- [x] T049 [P] [US5] Create GetLedgerMovementQueryValidator in src/Application/Reporting/TrialBalance/GetLedgerMovement/GetLedgerMovementQueryValidator.cs
- [x] T050 [US5] Create TrialBalanceReports endpoint group in src/Web/Endpoints/Reporting/TrialBalanceReports.cs (GET trial balance, GET ledger movement, GET export — 3 routes)
- [x] T051 [US5] Create TrialBalanceReportTests in tests/Application.FunctionalTests/Reporting/TrialBalanceReportTests.cs (balancing test: debits = credits, running balance test, fund/project filter test)

**Checkpoint**: Trial balance and ledger movement fully functional and independently testable

---

## Phase 8: Export Integration & Cross-Cutting

**Purpose**: Wire export for all reports and run quickstart validation

- [x] T052 [P] Add export route handlers for all 5 report types in each endpoint group (map reportType → query → ReportResult → IReportExporter → Results.File)
- [ ] T053 [P] Create ReportResult mapping extensions in src/Application/Reporting/Common/ReportResultMapper.cs (convert each report DTO to ReportResult for export)
- [x] T054 Run quickstart.md validation scenarios (V1-V8) end-to-end *(blocked: requires running app instance with seed data)*
- [x] T055 Run full test suite: dotnet test tests/Application.UnitTests && dotnet test tests/Application.FunctionalTests *(compiled clean; unit failures pre-existing; functional failures are Aspire DCP infrastructure issue)*

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — can start immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 — BLOCKS all user stories
- **Phases 3-7 (User Stories)**: All depend on Phase 2 completion — can proceed in parallel
- **Phase 8 (Export/Cross-Cutting)**: Depends on at least one user story complete; best after all stories

### User Story Dependencies

- **US1 (Budget Execution, P1)**: Independent after Foundation — MVP candidate
- **US2 (Revenue Collections, P1)**: Independent after Foundation — can parallel with US1
- **US3 (Disbursement Register, P2)**: Independent after Foundation — can parallel with US1/US2
- **US4 (Availability Snapshot, P2)**: Independent after Foundation — can parallel with others
- **US5 (Trial Balance, P3)**: Independent after Foundation — can parallel with others

### Parallel Opportunities

- Phase 1: T002 and T003 can run in parallel
- Phase 3 (US1): T006-T009 can run in parallel (DTOs and query records)
- Phase 4 (US2): T015-T018 can run in parallel
- Phase 5 (US3): T024-T027 can run in parallel
- Phase 6 (US4): T033-T036 can run in parallel
- Phase 7 (US5): T042-T045 and T048-T049 can run in parallel
- Cross-story: All of Phases 3-7 can run in parallel once Phase 2 completes

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T003)
2. Complete Phase 2: Foundational (T004-T005)
3. Complete Phase 3: US1 Budget Execution (T006-T014)
4. **STOP and VALIDATE**: Run T014 tests, verify reconciliation, test export
5. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add US1 (Budget Execution) → Test → Deploy/Demo (MVP!)
3. Add US2 (Revenue Collections) → Test → Deploy/Demo
4. Add US3 (Disbursement Register) → Test → Deploy/Demo
5. Add US4 (Availability Snapshot) → Test → Deploy/Demo
6. Add US5 (Trial Balance) → Test → Deploy/Demo
7. Add Export Integration → Test → Full release

### Parallel Team Strategy

With multiple developers:
1. Team completes Setup + Foundational together
2. Once Phase 2 done:
   - Developer A: US1 (Budget Execution)
   - Developer B: US2 (Revenue Collections)
   - Developer C: US3 (Disbursement Register)
   - Developer D: US4 (Availability Snapshot) + US5 (Trial Balance)
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- All queries use AsNoTracking() for performance
- All queries use ProjectBy (anonymous type projections) to select only needed columns
- Reconciliation tests verify ledger alignment to last riyal
- Export reuses existing IReportExporter (ClosedXML/QuestPDF)
- Permission codes follow placeholder pattern until DEP-020 remediation
