# Tasks: Financial Reports Module

**Input**: Design documents from `/specs/007-financial-reports/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Not explicitly requested in spec. Test tasks omitted per template rules. Add during `/speckit.tasks` if TDD approach is desired.

**Organization**: Tasks grouped by user story for independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: NuGet packages, permissions, CashFlowMappingRule entity, migration, seed data

- [ ] T001 Add ClosedXML and QuestPDF NuGet packages to src/Infrastructure/Infrastructure.csproj (via Directory.Packages.props central management)
- [ ] T002 [P] Add 6 report permission constants to src/Application/Common/Security/PermissionCodes.cs: ViewBalanceSheet, ViewIncomeStatement, ViewGeneralLedger, ViewCashFlow, ExportReports, PrintReports
- [ ] T003 [P] Create CashFlowSectionType enum in src/Domain/Accounting/Enums/CashFlowSectionType.cs (Operating=0, Investing=1, Financing=2)
- [ ] T004 [P] Create CashFlowMappingRule entity in src/Domain/Accounting/Entities/CashFlowMappingRule.cs (AccountGroupId, Section, Description, IsActive, RowVersion)
- [ ] T005 Add CashFlowMappingRules DbSet to src/Application/Common/Interfaces/IApplicationDbContext.cs
- [ ] T006 Add CashFlowMappingRules DbSet to src/Infrastructure/Data/ApplicationDbContext.cs (=> Set<CashFlowMappingRule>())
- [ ] T007 [P] Create EF Core configuration in src/Infrastructure/Data/Configurations/CashFlowMappingRuleConfiguration.cs (unique filtered index on AccountGroupId where IsActive=true)
- [ ] T008 Create EF Core migration: dotnet ef migrations add AddFinancialReports --project src/Infrastructure
- [ ] T009 [P] Create seed data in src/Infrastructure/Data/Seeds/CashFlowMappingRuleSeedData.cs with default Operating/Investing/Financing classifications
- [ ] T010 Add CashFlowMappingRule seeding to src/Infrastructure/Data/ApplicationDbContextInitialiser.cs (idempotent: if !Any() then seed)
- [ ] T011 Register report services in src/Infrastructure/DependencyInjection.cs (IReportEngine, IReportExporter)

---

## Phase 2: Foundational (Report Engine Abstractions)

**Purpose**: Core report types and interfaces that ALL user stories depend on

**CRITICAL**: No user story work can begin until this phase is complete

- [ ] T012 [P] Create ReportSection class in src/Application/Accounting/Reports/Common/ReportSection.cs (Title, TitleEn, Lines, Total)
- [ ] T013 [P] Create ReportLine class in src/Application/Accounting/Reports/Common/ReportLine.cs (AccountCode, AccountName, Debit, Credit, Balance)
- [ ] T014 [P] Create ReportResult class in src/Application/Accounting/Reports/Common/ReportResult.cs (Currency, GeneratedAt, TotalLines, Sections)
- [ ] T015 [P] Create IReportExporter interface in src/Application/Accounting/Reports/Common/IReportExporter.cs (ExportExcelAsync, ExportPdfAsync, both taking ReportResult + Stream)
- [ ] T016 [P] Create ReportAuditService in src/Application/Accounting/Reports/Common/ReportAuditService.cs (wraps report execution, writes to SecurityAuditLog with Action/EntityType/NewValues)
- [ ] T017 Add AutoMapper profile for ReportSection/ReportLine in src/Application/Accounting/Reports/Common/ReportMappingProfile.cs

**Checkpoint**: Foundation ready - user story implementation can begin

---

## Phase 3: User Story 1 - Balance Sheet Report (Priority: P1) — MVP

**Goal**: Accountant generates Balance Sheet as of a date, verifies Assets = Liabilities + Equity

**Independent Test**: POST known posted entries, GET balance-sheet?asOfDate=..., assert Assets=Liabilities+Equity, Balanced=true

### Implementation for User Story 1

- [ ] T018 [US1] Create BalanceSheetDto class in src/Application/Accounting/Reports/BalanceSheet/BalanceSheetDto.cs (AsOfDate, Currency, Assets, Liabilities, Equity groups, LiabilitiesAndEquity, Balanced, GeneratedAt)
- [ ] T019 [US1] Create GetBalanceSheetQuery record in src/Application/Accounting/Reports/BalanceSheet/GetBalanceSheetQuery.cs (AsOfDate, FiscalPeriodId?)
- [ ] T020 [US1] Create GetBalanceSheetQueryValidator in src/Application/Accounting/Reports/BalanceSheet/GetBalanceSheetQueryValidator.cs (asOfDate <= today)
- [ ] T021 [US1] Implement GetBalanceSheetQueryHandler in src/Application/Accounting/Reports/BalanceSheet/GetBalanceSheetQueryHandler.cs (query AccountBalances by type, group by AccountGroup, compute NetIncome from Income Statement, assert balance)
- [ ] T022 [US1] Add balance-sheet endpoint to src/Web/Endpoints/Reports/Reports.cs (GET /api/Reports/balance-sheet, RequireAuthorization PermissionCodes.ViewBalanceSheet, ETag support)
- [ ] T023 [US1] Create frontend types in src/Web/ClientApp/src/features/reports/types.ts (BalanceSheetDto, ReportSection, ReportLine TypeScript interfaces)
- [ ] T024 [P] [US1] Create useReport hook in src/Web/ClientApp/src/features/reports/hooks/useReport.ts (TanStack Query with ETag If-None-Match)
- [ ] T025 [US1] Create ReportFilters component in src/Web/ClientApp/src/features/reports/components/ReportFilters.tsx (asOfDate picker, collapsible, RTL)
- [ ] T026 [US1] Create BalanceSheetGrid component in src/Web/ClientApp/src/features/reports/components/BalanceSheetGrid.tsx (DataGrid with Assets/Liabilities/Equity sections, totals bold, tabular numerals)
- [ ] T027 [US1] Create BalanceSheetPage in src/Web/ClientApp/src/features/reports/pages/BalanceSheetPage.tsx (PageHeader, ReportFilters, BalanceSheetGrid, loading skeleton, empty state, error boundary)
- [ ] T028 [US1] Register /reports/balance-sheet route in src/Web/ClientApp/src/app/routes.tsx
- [ ] T029 [US1] Add reports navigation item in src/Web/clientApp/src/layouts/navigation.ts (permission: "accounting.reports.balance-sheet")

**Checkpoint**: Balance Sheet fully functional - can generate, view, validate Assets=Liabilities+Equity

---

## Phase 4: User Story 2 - Income Statement Report (Priority: P1)

**Goal**: Accountant generates Income Statement for a date range, sees Revenue - Expenses = NetIncome

**Independent Test**: POST Revenue/Expense entries, GET income-statement?startDate=...&endDate=..., assert NetIncome = Revenue - Expenses

### Implementation for User Story 2

- [ ] T030 [US2] Create IncomeStatementDto class in src/Application/Accounting/Reports/IncomeStatement/IncomeStatementDto.cs (StartDate, EndDate, Currency, Revenue, Expenses groups, NetIncome, GeneratedAt)
- [ ] T031 [US2] Create GetIncomeStatementQuery record in src/Application/Accounting/Reports/IncomeStatement/GetIncomeStatementQuery.cs (StartDate, EndDate)
- [ ] T032 [US2] Create GetIncomeStatementQueryValidator in src/Application/Accounting/Reports/IncomeStatement/GetIncomeStatementQueryValidator.cs (startDate <= endDate, endDate <= today)
- [ ] T033 [US2] Implement GetIncomeStatementQueryHandler in src/Application/Accounting/Reports/IncomeStatement/GetIncomeStatementQueryHandler.cs (query MoveLines by type, group by AccountGroup, compute NetIncome)
- [ ] T034 [US2] Add income-statement endpoint to src/Web/Endpoints/Reports/Reports.cs (GET /api/Reports/income-statement, RequireAuthorization PermissionCodes.ViewIncomeStatement)
- [ ] T035 [P] [US2] Create IncomeStatementGrid component in src/Web/ClientApp/src/features/reports/components/IncomeStatementGrid.tsx (Revenue/Expenses sections, NetIncome total)
- [ ] T036 [US2] Create IncomeStatementPage in src/Web/ClientApp/src/features/reports/pages/IncomeStatementPage.tsx (startDate/endDate picker, period shortcuts, grid)
- [ ] T037 [US2] Register /reports/income-statement route in src/Web/ClientApp/src/app/routes.tsx

**Checkpoint**: Income Statement functional - Revenue/Expenses/NetIncome correct, period shortcuts work

---

## Phase 5: User Story 3 - General Ledger Report (Priority: P2)

**Goal**: Accountant views General Ledger for specific accounts with running balance and pagination

**Independent Test**: POST entries for Account 1001, GET general-ledger?accountId=1001&startDate=...&endDate=..., verify ordered lines with running balance, pagination works

### Implementation for User Story 3

- [ ] T038 [US3] Create GeneralLedgerDto class in src/Application/Accounting/Reports/GeneralLedger/GeneralLedgerDto.cs (Currency, TotalLines, Page, PageSize, Lines, Totals, GeneratedAt)
- [ ] T039 [US3] Create GeneralLedgerLine class in src/Application/Accounting/Reports/GeneralLedger/GeneralLedgerLine.cs (DocumentDate, EntryNumber, Reference, Narration, AccountCode, AccountName, Debit, Credit, RunningBalance)
- [ ] T040 [US3] Create GetGeneralLedgerQuery record in src/Application/Accounting/Reports/GeneralLedger/GetGeneralLedgerQuery.cs (AccountId?, AccountCode?, FiscalPeriodId?, StartDate?, EndDate?, Page, PageSize)
- [ ] T041 [US3] Create GetGeneralLedgerQueryValidator in src/Application/Accounting/Reports/GeneralLedger/GetGeneralLedgerQueryValidator.cs (accountId/accountCode mutually exclusive if both provided, pageSize <= 1000)
- [ ] T042 [US3] Implement GetGeneralLedgerQueryHandler in src/Application/Accounting/Reports/GeneralLedger/GetGeneralLedgerQueryHandler.cs (query MoveLines, order by date/entry/sequence, compute running balance per account, apply pagination)
- [ ] T043 [US3] Add general-ledger endpoint to src/Web/Endpoints/Reports/Reports.cs (GET /api/Reports/general-ledger, RequireAuthorization PermissionCodes.ViewGeneralLedger, page/pageSize params)
- [ ] T044 [P] [US3] Create GeneralLedgerGrid component in src/Web/ClientApp/src/features/reports/components/GeneralLedgerGrid.tsx (paginated DataGrid, running balance column, sticky headers)
- [ ] T045 [US3] Create GeneralLedgerPage in src/Web/ClientApp/src/features/reports/pages/GeneralLedgerPage.tsx (account filter, date range, pagination controls)
- [ ] T046 [US3] Register /reports/general-ledger route in src/Web/ClientApp/src/app/routes.tsx

**Checkpoint**: General Ledger functional with pagination, running balance, account filtering

---

## Phase 6: User Story 4 - Cash Flow Statement Report (Priority: P2)

**Goal**: Accountant generates Cash Flow Statement, Opening Cash + Net Change = Closing Cash reconciles

**Independent Test**: POST entries, GET cash-flow?startDate=...&endDate=..., assert OpeningCash + NetChange = ClosingCash, Reconciled=true

### Implementation for User Story 4

- [ ] T047 [US4] Create CashFlowStatementDto class in src/Application/Accounting/Reports/CashFlowStatement/CashFlowStatementDto.cs (StartDate, EndDate, Currency, Operating, Investing, Financing sections, NetChange, OpeningCash, ClosingCash, Reconciled, Warning, GeneratedAt)
- [ ] T048 [US4] Create CashFlowSection and CashFlowLineItem classes in src/Application/Accounting/Reports/CashFlowStatement/CashFlowSection.cs
- [ ] T049 [US4] Create GetCashFlowStatementQuery record in src/Application/Accounting/Reports/CashFlowStatement/GetCashFlowStatementQuery.cs (StartDate, EndDate)
- [ ] T050 [US4] Create GetCashFlowStatementQueryValidator in src/Application/Accounting/Reports/CashFlowStatement/GetCashFlowStatementQueryValidator.cs (startDate <= endDate, endDate <= today)
- [ ] T051 [US4] Implement GetCashFlowStatementQueryHandler in src/Application/Accounting/Reports/CashFlowStatement/GetCashFlowStatementQueryHandler.cs (compute OpeningCash from GL, classify by CashFlowMappingRule, NetChange, ClosingCash, reconcile)
- [ ] T052 [US4] Add cash-flow endpoint to src/Web/Endpoints/Reports/Reports.cs (GET /api/Reports/cash-flow, RequireAuthorization PermissionCodes.ViewCashFlow)
- [ ] T053 [P] [US4] Create CashFlowStatementGrid component in src/Web/ClientApp/src/features/reports/components/CashFlowStatementGrid.tsx (Operating/Investing/Financing sections, reconciliation indicator)
- [ ] T054 [US4] Create CashFlowStatementPage in src/Web/ClientApp/src/features/reports/pages/CashFlowStatementPage.tsx (date range picker, reconciliation status)
- [ ] T055 [US4] Register /reports/cash-flow route in src/Web/ClientApp/src/app/routes.tsx

**Checkpoint**: Cash Flow functional with reconciliation, all 4 reports viewable

---

## Phase 7: User Story 5 - Multi-Format Export (Priority: P2)

**Goal**: Export any report to Excel/PDF with identical numbers, print via CSS

**Independent Test**: Generate report, export to Excel and PDF, compare numeric values to 4 decimal places

### Implementation for User Story 5

- [ ] T056 [US5] Implement ExcelReportExporter in src/Infrastructure/Services/ExcelReportExporter.cs (ClosedXML: generate worksheet per report type, columns + totals row + header metadata, RTL)
- [ ] T057 [US5] Implement PdfReportExporter in src/Infrastructure/Services/PdfReportExporter.cs (QuestPDF: landscape A4 for BS/CF, portrait for IS/GL, header with logo/org/period, footer with page N/M, RTL Arabic font)
- [ ] T058 [US5] Add export endpoint to src/Web/Endpoints/Reports/Reports.cs (GET /api/Reports/{name}/export?format=excel|pdf, RequireAuthorization PermissionCodes.ExportReports, return file stream)
- [ ] T059 [P] [US5] Create useReportExport hook in src/Web/ClientApp/src/features/reports/hooks/useReportExport.ts (trigger download, handle blob response)
- [ ] T060 [US5] Create ReportExportDropdown component in src/Web/ClientApp/src/features/reports/components/ReportExportDropdown.tsx (Excel/PDF/Print buttons, permission-gated)
- [ ] T061 [US5] Integrate ReportExportDropdown into all 4 report pages (add to PageHeader actions)

**Checkpoint**: All reports exportable to Excel and PDF, numbers match screen output

---

## Phase 8: User Story 6 - Permission-Based Access Control (Priority: P1)

**Goal**: Each report endpoint enforces distinct permission, anonymous denied

**Independent Test**: Request endpoint without auth → 401; wrong permission → 403; correct → 200

### Implementation for User Story 6

> Note: Permissions are declared on endpoints via RequireAuthorization in Phases 3-7. This phase adds frontend permission gating and verifies all endpoints.

- [ ] T062 [US6] Add report permission constants to src/Web/ClientApp/src/shared/constants/permissions.ts (accounting.reports.balance-sheet, etc.)
- [ ] T063 [US6] Add permission checks to all 4 report pages using usePermission hook (show/hide export, disable actions)
- [ ] T064 [US6] Add permission-based navigation visibility in src/Web/ClientApp/src/layouts/navigation.ts (report items only visible if user has corresponding permission)

**Checkpoint**: Anonymous users see 401, unauthorized users see 403, authorized users see report pages

---

## Phase 9: User Story 7 - Audit Trail for Report Generation (Priority: P3)

**Goal**: Every report view/export/print logged to SecurityAuditLog

**Independent Test**: Generate report, query SecurityAuditLog, assert record exists with correct UserId, ReportName, Format, Success

### Implementation for User Story 7

- [ ] T065 [US7] Integrate ReportAuditService into all 4 report query handlers (wrap execution, log success/failure)
- [ ] T066 [US7] Integrate ReportAuditService into export endpoint (log ReportExport with format)
- [ ] T067 [US7] Add print audit logging in frontend useReportExport hook (log ReportPrint action on print trigger)

**Checkpoint**: SecurityAuditLog contains entries for every report generation event

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Performance, accessibility, RTL verification, cleanup

- [ ] T068 [P] Add CSS print stylesheet in src/Web/ClientApp/src/styles/print.css (hide nav/sidebar, show report only, page breaks)
- [ ] T069 [P] Add database indexes in migration: IX_Moves_PostedStatus_DocumentDate, IX_MoveLines_AccountId_DocumentDate (performance per FR-014)
- [ ] T070 Verify RTL rendering on all 4 report pages (dir="rtl", logical properties, tabular numerals)
- [ ] T071 Verify dark mode rendering on all report pages (design tokens only, no hardcoded colors)
- [ ] T072 Run quickstart.md validation scenarios end-to-end

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — can start immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 completion — BLOCKS all user stories
- **Phase 3 (US1 - Balance Sheet)**: Depends on Phase 2 — P1 MVP
- **Phase 4 (US2 - Income Statement)**: Depends on Phase 2 — can run parallel with US1
- **Phase 5 (US3 - General Ledger)**: Depends on Phase 2 — can run parallel with US1/US2
- **Phase 6 (US4 - Cash Flow)**: Depends on Phase 2 — can run parallel with US1/US2/US3
- **Phase 7 (US5 - Export)**: Depends on Phases 3-6 (needs all 4 reports to export)
- **Phase 8 (US6 - Permissions)**: Depends on Phases 3-7 (permissions on all endpoints)
- **Phase 9 (US7 - Audit)**: Depends on Phase 2 (ReportAuditService) + Phases 3-7 (report handlers)
- **Phase 10 (Polish)**: Depends on all user stories complete

### User Story Dependencies

- **US1 (Balance Sheet, P1)**: Can start after Phase 2. Independent. MVP.
- **US2 (Income Statement, P1)**: Can start after Phase 2. Independent. Depends on US1 for NetIncome in Balance Sheet Equity section.
- **US3 (General Ledger, P2)**: Can start after Phase 2. Fully independent.
- **US4 (Cash Flow, P2)**: Can start after Phase 2. Depends on US2 for NetIncome in Operating section.
- **US5 (Export, P2)**: Depends on all 4 reports existing.
- **US6 (Permissions, P1)**: Depends on all endpoints existing.
- **US7 (Audit, P3)**: Depends on all report handlers existing.

### Parallel Opportunities

```bash
# Phase 1: Setup tasks in parallel
Task: T002 (PermissionCodes)
Task: T003 (CashFlowSectionType enum)
Task: T004 (CashFlowMappingRule entity)

# Phase 2: Foundational tasks in parallel
Task: T012 (ReportSection)
Task: T013 (ReportLine)
Task: T014 (ReportResult)
Task: T015 (IReportExporter)
Task: T016 (ReportAuditService)

# After Phase 2: All 4 reports in parallel (if team capacity)
Task: Phase 3 (Balance Sheet)
Task: Phase 4 (Income Statement)
Task: Phase 5 (General Ledger)
Task: Phase 6 (Cash Flow)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (permissions, packages, entity)
2. Complete Phase 2: Foundational (report engine abstractions)
3. Complete Phase 3: Balance Sheet (US1)
4. **STOP and VALIDATE**: Test Balance Sheet independently — Assets=Liabilities+Equity
5. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Balance Sheet → Test independently → Deploy/Demo (MVP!)
3. Income Statement → Test independently → Deploy/Demo
4. General Ledger → Test independently → Deploy/Demo
5. Cash Flow → Test independently → Deploy/Demo
6. Export → Test Excel/PDF → Deploy/Demo
7. Permissions + Audit → Hardening → Deploy/Demo

### Parallel Team Strategy

With multiple developers:
1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: Balance Sheet (US1)
   - Developer B: Income Statement (US2)
   - Developer C: General Ledger (US3)
   - Developer D: Cash Flow (US4)
3. Merge, then Export + Permissions + Audit as cross-cutting

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Balance Sheet (US1) is the MVP — delivers core value with Assets=Liabilities+Equity assertion
- US2 (Income Statement) is needed by US1 for NetIncome in Equity section — implement early
- US4 (Cash Flow) depends on US2 for NetIncome in Operating section
