# Feature Specification: Financial Reports Module

**Feature Branch**: `007-financial-reports`

**Created**: 2026-09-03

**Status**: Draft

**Input**: User description: "Financial Reports Module - Balance Sheet, Income Statement, General Ledger, Cash Flow Statement (4 reports, preserving existing Trial Balance)"

## Clarifications

### Session 2026-09-03

- Q: How should the General Ledger handle result sets that exceed practical display limits? → A: Server-side pagination with configurable page size (default 500 lines per page). ReportResult includes TotalLines count. Frontend uses paginated DataGrid.
- Q: How should the Cash Flow Statement compute Opening Cash? → A: From GL cash account balances as of startDate minus one day (live query, not materialized prior-period balance).

## User Scenarios & Testing

### User Story 1 - Balance Sheet Report (Priority: P1)

As an Accountant, I need to generate a Balance Sheet as of a specific date to verify that Assets equal Liabilities plus Equity and to review the financial position of the organization.

**Why this priority**: Balance Sheet is the foundational accounting report and the primary tool for verifying financial integrity (Principle IV).

**Independent Test**: Generate a Balance Sheet for a known date with posted entries and verify Assets = Liabilities + Equity holds true. Test with data spanning closed fiscal years.

**Acceptance Scenarios**:

1. **Given** posted journal entries exist with Assets=100,000, Liabilities=40,000, Equity=60,000, **When** Balance Sheet is requested for the current date, **Then** Assets total 100,000, Liabilities total 40,000, Equity total 60,000, Balanced=true.
2. **Given** asOfDate falls in a closed fiscal year, **When** Balance Sheet is generated, **Then** the system reads from materialized AccountBalances and returns correct totals.
3. **Given** asOfDate is in the future, **When** Balance Sheet is requested, **Then** the system returns a validation error (ProblemDetails, 400).
4. **Given** AccountGroups include Current Assets and Non-Current Assets, **When** Balance Sheet is generated, **Then** sections are grouped by AccountGroup within each type (Assets, Liabilities, Equity).
5. **Given** a reversing entry was posted, **When** Balance Sheet is generated, **Then** the reversal nets to zero in totals and does not distort the balance.

---

### User Story 2 - Income Statement Report (Priority: P1)

As an Accountant, I need to generate an Income Statement for a date range to determine Net Income (Revenue minus Expenses) and review profitability.

**Why this priority**: Income Statement provides the Net Income figure required by the Balance Sheet Equity section and is essential for financial period closing.

**Independent Test**: Generate an Income Statement for a known period, verify Revenue - Expenses = Net Income. Test with period crossing fiscal year boundaries.

**Acceptance Scenarios**:

1. **Given** Revenue=200,000 and Expenses=150,000 in posted entries for January, **When** Income Statement is requested for January 1-31, **Then** Revenue total is 200,000, Expenses total is 150,000, Net Income is 50,000.
2. **Given** startDate > endDate, **When** Income Statement is requested, **Then** the system returns a validation error (ProblemDetails, 400).
3. **Given** endDate is in the future, **When** Income Statement is requested, **Then** the system returns a validation error (ProblemDetails, 400).
4. **Given** a period crossing fiscal year boundaries (e.g., Dec 15 - Jan 15), **When** Income Statement is generated, **Then** the system splits the query internally and returns a single unified result.
5. **Given** reversing entries net to zero, **When** Income Statement is generated, **Then** the reversal does not inflate Revenue or Expenses.

---

### User Story 3 - General Ledger Report (Priority: P2)

As an Accountant, I need to view the General Ledger for specific accounts or all accounts over a period to trace individual transactions and their running balances.

**Why this priority**: General Ledger provides the transaction-level detail needed for auditing and reconciliation, supporting Principle VII (audit trails).

**Independent Test**: Generate a General Ledger for a specific account, verify ordered lines with running balance calculation. Test with account having no moves in period.

**Acceptance Scenarios**:

1. **Given** posted MoveLines for Account 1001 (Cash), **When** General Ledger is requested for Account 1001 in January, **Then** lines are ordered by DocumentDate ASC, EntryNumber, Sequence, with cumulative running balance per account.
2. **Given** an account with no posted moves in the requested period, **When** General Ledger is requested for that account, **Then** zero lines are returned with the AccountCode/AccountName header.
3. **Given** no filter parameters, **When** General Ledger is requested, **Then** all active accounts for the current fiscal year are shown.
4. **Given** a reversing entry exists, **When** General Ledger is generated, **Then** the reversal appears chronologically with correct running balance impact.
5. **Given** multiple accounts, **When** General Ledger is generated, **Then** running balance resets per account.

---

### User Story 4 - Cash Flow Statement Report (Priority: P2)

As an Accountant, I need to generate a Cash Flow Statement using the indirect method to reconcile opening cash to closing cash via Operating, Investing, and Financing activities.

**Why this priority**: Cash Flow Statement completes the set of primary financial statements and provides liquidity analysis.

**Independent Test**: Generate a Cash Flow Statement, verify Opening Cash + Net Change = Closing Cash, and Closing Cash matches GL cash account balances.

**Acceptance Scenarios**:

1. **Given** posted entries affecting cash accounts, **When** Cash Flow Statement is generated, **Then** Opening Cash is computed from GL cash account balances as of startDate minus one day, Operating/Investing/Financing sections are populated per classification rules, and Opening Cash + Net Change = Closing Cash.
2. **Given** Closing Cash matches GL cash account balances within 0.0001, **When** Cash Flow Statement is generated, **Then** Reconciled=true.
3. **Given** no cash accounts are configured, **When** Cash Flow Statement is generated, **Then** Reconciled=false is returned with a warning message.
4. **Given** startDate > endDate, **When** Cash Flow Statement is requested, **Then** the system returns a validation error (ProblemDetails, 400).

---

### User Story 5 - Multi-Format Export (Priority: P2)

As an Accountant, I need to export any report to Excel or PDF and print it, ensuring identical numbers across all formats.

**Why this priority**: Export and print capabilities are essential for distribution, archiving, and regulatory compliance.

**Independent Test**: Export the same report to Screen, Excel, and PDF; verify numeric values match to 4 decimal places across all three.

**Acceptance Scenarios**:

1. **Given** a Balance Sheet is generated, **When** exported to Excel, **Then** the file contains the same columns, totals, header metadata (report name, period, currency, generated timestamp, generator user), and numeric values match the screen output to 4 decimal places.
2. **Given** a Balance Sheet is generated, **When** exported to PDF, **Then** the document is landscape A4 with header (logo, org name, report name, period), footer (page N/M, generated by, timestamp), and numeric values match the screen output to 4 decimal places.
3. **Given** an Income Statement is generated, **When** exported to PDF, **Then** the document is portrait A4 (narrower report) with identical numeric values.
4. **Given** a report is exported, **When** the user does not have accounting.reports.export permission, **Then** the export request returns 403 Forbidden.
5. **Given** a report is printed via browser, **When** the print action is triggered, **Then** CSS print stylesheet applies and the user must have accounting.reports.print permission.

---

### User Story 6 - Permission-Based Access Control (Priority: P1)

As a system administrator, I need each report endpoint to enforce distinct permissions so that access is controlled per report type and export/print actions are separately gated.

**Why this priority**: Principle VII (NON-NEGOTIABLE) requires every endpoint to declare a required named permission. Anonymous access is prohibited.

**Independent Test**: Attempt to access each report endpoint without authentication (expect 401), with wrong permission (expect 403), and with correct permission (expect 200).

**Acceptance Scenarios**:

1. **Given** an unauthenticated request, **When** any report endpoint is accessed, **Then** 401 Unauthorized is returned.
2. **Given** a user without accounting.reports.balance-sheet permission, **When** the Balance Sheet endpoint is accessed, **Then** 403 Forbidden is returned.
3. **Given** a user without accounting.reports.export permission, **When** an Excel export is requested, **Then** 403 Forbidden is returned.
4. **Given** a user with accounting.reports.print permission, **When** the print action is triggered, **Then** the print CSS is applied successfully.

---

### User Story 7 - Audit Trail for Report Generation (Priority: P3)

As an Auditor, I need every report generation (view, export, print) to be recorded in the security audit log for compliance.

**Why this priority**: Principle VII requires every authorization decision to be recorded. Principle VIII requires audit trails to be INSERT-ONLY.

**Independent Test**: Generate a report (view + export + print), then verify SecurityAuditLog contains records for each action with UserId, ReportName, Params, Format, GeneratedAt, Success.

**Acceptance Scenarios**:

1. **Given** a user views a Balance Sheet, **When** the report is generated, **Then** SecurityAuditLog records UserId, ReportName="BalanceSheet", Params, Format="Screen", GeneratedAt, Success=true.
2. **Given** a user exports a report to Excel, **When** the export completes, **Then** SecurityAuditLog records Format="Excel" and the export permission check outcome.
3. **Given** a report generation fails, **When** the error is logged, **Then** SecurityAuditLog records Success=false and FailureReason.

---

### Edge Cases

- Balance Sheet asOfDate in a closed fiscal year: allow, read from materialized AccountBalances.
- Income Statement period crossing fiscal year: split internally, return single result.
- General Ledger account with no moves in period: return zero lines with AccountCode/AccountName header.
- Cash Flow with no cash account configured: return Reconciled=false with warning.
- Reversing entries: appear in General Ledger chronologically; net to zero in Balance Sheet/Income Statement totals.
- Currency not configured at system level: fail at startup, not at report run.
- Multiple simultaneous report requests for same parameters: each request computes independently (no shared cache without explicit materialization).
- Extremely large date range in General Ledger: server-side pagination with configurable page size (default 500 lines per page). ReportResult MUST include TotalLines count. Frontend uses paginated DataGrid.

## Requirements

### Functional Requirements

- **FR-001 (Common Engine)**: System MUST provide a ReportQuery/ReportResult abstraction where the same query produces identical numeric results across Screen, Excel, and PDF formats. Query parameters (asOfDate, startDate, endDate, accountId/accountCode, currency=Base) MUST be deterministically computed from posted Moves only (EntryStatus=Posted, PostedAt not null). No caching of calculated totals is permitted without explicit materialization.
- **FR-002 (Base Currency Only)**: All 4 reports MUST display base currency amounts only in v1. Every report DTO MUST include a Currency field set to the configured base currency code for future multicurrency extension. Foreign currency lines MUST be converted via MoveLine.ExchangeRate at posting time.
- **FR-003 (Posted Only)**: All reports MUST exclude Draft, Submitted, and Approved entries. All reports MUST include reversing entries per Principle IV. Reversals MUST net to zero in totals for Balance Sheet and Income Statement.
- **FR-004 (Balance Sheet)**: System MUST generate a Balance Sheet with asOfDate (required, date <= today) and optional fiscalPeriodId. Assets MUST be computed as sum of AccountBalances where Account.Type=Asset at asOfDate. Liabilities = sum (Type=Liability). Equity = sum (Type=Equity) + current period NetIncome from Income Statement. The accounting equation Assets = Liabilities + Equity MUST hold. Sections MUST be grouped by AccountGroup: Current Assets, Non-Current Assets, Current Liabilities, Non-Current Liabilities, Equity.
- **FR-005 (Income Statement)**: System MUST generate an Income Statement with startDate and endDate. Revenue = sum (Type=Revenue) over period. Expenses = sum (Type=Expense) over period. NetIncome = Revenue - Expenses. Sections MUST be grouped by AccountGroup within each type. System MUST reject requests where startDate > endDate or endDate > today.
- **FR-006 (General Ledger)**: System MUST generate a General Ledger with optional accountId/accountCode, optional fiscalPeriodId, optional startDate/endDate. Default = all active accounts, current fiscal year. Lines MUST be ordered by DocumentDate ASC, Move.EntryNumber, MoveLine.Sequence. Running balance MUST be cumulative per account and reset per account when multiple accounts are shown. Results MUST be server-side paginated with configurable page size (default 500 lines); ReportResult MUST include TotalLines count for the frontend to render pagination controls.
- **FR-007 (Cash Flow Statement)**: System MUST generate a Cash Flow Statement with startDate and endDate. Classification MUST use configurable mapping rules for Operating, Investing, and Financing sections. Indirect method is default. Opening Cash MUST be computed from GL cash account balances as of startDate minus one day (live query, not materialized). Opening Cash + Net Change = Closing Cash MUST reconcile to GL cash accounts. System MUST return Reconciled=false with warning when no cash accounts are configured.
- **FR-008 (Format Providers)**: Screen format MUST be JSON consumed by React TanStack Table. Excel format MUST use OpenXML with same columns, totals row, and header metadata. PDF format MUST use landscape A4 for Balance Sheet/Cash Flow and portrait A4 for Income Statement/General Ledger, with header (logo + org name + report name + period), footer (page N/M + generated by + timestamp), and RTL Arabic font fallback. Print format MUST use CSS print stylesheet. All formats MUST emit identical numeric values from a single ReportResult source.
- **FR-009 (Permissions)**: System MUST declare distinct permissions: accounting.reports.balance-sheet (ViewBalanceSheet), accounting.reports.income-statement (ViewIncomeStatement), accounting.reports.general-ledger (ViewGeneralLedger), accounting.reports.cash-flow (ViewCashFlow), accounting.reports.export (ExportReports), accounting.reports.print (PrintReports). Each endpoint MUST RequireAuthorization with corresponding code. Anonymous access MUST be denied. AuditTrail MUST record grant/deny on every export per Principle VII.
- **FR-010 (API Endpoints)**: System MUST expose GET /api/Reports/balance-sheet, GET /api/Reports/income-statement, GET /api/Reports/general-ledger, GET /api/Reports/cash-flow. A single export endpoint GET /api/Reports/{name}?format=excel|pdf MUST return file streams gated by export permission. All endpoints MUST validate dates server-side, return ProblemDetails on error, and support ETag for client cache. The General Ledger endpoint MUST accept optional page and pageSize query parameters for server-side pagination.
- **FR-011 (Frontend Pages)**: System MUST provide routes /reports/balance-sheet, /reports/income-statement, /reports/general-ledger, /reports/cash-flow. Each page MUST have: report title, period picker, Export dropdown (Excel/PDF/Print), Refresh button. Filters MUST be collapsible. DataGrid MUST have sticky headers, totals row bold, RTL logical properties, tabular numerals. Loading skeleton, empty state, and error boundary MUST be present. Shared ReportFilters component and useReport/useReportExport hooks MUST be used.
- **FR-012 (Determinism)**: Same input parameters + same posted moves MUST produce byte-identical numeric results across Screen, Excel, and PDF. No client-side recalculation is permitted. The DTO is the single source of truth.
- **FR-013 (Audit)**: Every report generation (view + export + print) MUST record SecurityAuditLog: UserId, ReportName, Params, Format, GeneratedAt, Success, FailureReason. Export MUST require Export permission separately from View permission.
- **FR-014 (Performance)**: Each report query MUST execute in under 2 seconds for a dataset of 100,000 posted MoveLines and 1,000 Accounts. Aggregation MUST happen server-side via database queries (no client-side LINQ-to-objects aggregation). Recommended index hints: IX_Moves_PostedStatus_DocumentDate, IX_MoveLines_AccountId_DocumentDate.

### Key Entities

- **ReportQuery**: Abstract representation of report input parameters (report type, date range, account filters, fiscal period).
- **ReportResult**: Abstract representation of computed report output (sections, lines, totals, currency, generated timestamp, TotalLines for paginated reports).
- **ReportSection**: Grouped data within a report (e.g., Current Assets, Revenue by AccountGroup).
- **ReportLine**: Individual line item within a section (account code, name, debit, credit, balance).
- **CashFlowMappingRule**: Configuration mapping AccountGroup to Cash Flow section (Operating, Investing, Financing).
- **SecurityAuditLog**: INSERT-ONLY log of report generation events (UserId, ReportName, Params, Format, GeneratedAt, Success, FailureReason).

## Success Criteria

### Measurable Outcomes

- **SC-001**: All 4 reports generate in under 2 seconds each for a dataset of 100,000 posted MoveLines and 1,000 Accounts.
- **SC-002**: Screen, Excel, and PDF numeric values match to 4 decimal places for the same parameters (verified by automated test).
- **SC-003**: Anonymous request to any report endpoint returns 401 Unauthorized.
- **SC-004**: User without the specific report permission (e.g., accounting.reports.balance-sheet) receives 403 Forbidden.
- **SC-005**: Cash Flow ClosingCash matches GL cash account balances within plus or minus 0.0001.
- **SC-006**: Balance Sheet Balanced=true when Assets = Liabilities + Equity (assertion test).
- **SC-007**: 95% of report generations complete without timeout or error.

## Assumptions

- Trial Balance (existing implementation) is untouched and lives in a separate feature branch.
- AccountBalances materialized table is the source for Balance Sheet computations; it is rebuildable from posted Moves per Principle IV.
- Cash Flow classification rules are seeded in a CashFlowMappingRule table mapping AccountGroup to section.
- A single base currency is configured in the Currencies entity (IsBase=true).
- Organization logo and header are available via a static asset or FinancialSettings entity (TODO if missing).
- The existing OpenAPI pipeline auto-generates client types; Excel and PDF libraries (ClosedXML and QuestPDF) will be added to Application and Infrastructure projects.
- Existing accounting module provides Account, AccountGroup, AccountType, MoveLine, Move entities with PostedStatus and ExchangeRate fields.
- The frontend uses React with TanStack Table, TanStack Query, and a shared design token system with RTL support.
