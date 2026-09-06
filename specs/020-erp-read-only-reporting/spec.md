# Feature Specification: ERP Read-Only Reporting & Oversight

**Feature Branch**: `020-erp-read-only-reporting`

**Created**: 2026-09-06

**Status**: Draft

**Input**: User description: "Read-only reporting and oversight for the whole ERP: budget execution, revenue collections, disbursements, and availability snapshots. US1 (P1): Budget execution report — appropriations vs open encumbrances vs executed payments, filterable by fund, program, project, budget item, and period, exportable. US2 (P1): Revenue collections report — receipt vouchers by revenue account, party, method, and period, with deposit-slip status and check clearing state. US3 (P2): Disbursement register — requests and payments by status, fund, period, approver. US4 (P2): Availability snapshot for any budget line with full dimension breakdown and blocking state. US5 (P3): Trial balance and ledger movement reports scoped by fund/project dimensions. Requirements: strictly read-only (no mutations, no new transactional tables); every report filterable by period and fund; numbers must reconcile with the ledger. Entities: none new — queries over existing entities only. Success: every report returns under 2 seconds on a full fiscal year and reconciles to the ledger to the last riyal."

## Clarifications

### Session 2026-09-06

- Q: How should reports handle foreign-currency transactions when displaying amounts? → A: Base-currency totals only in all reports; original currency hidden at summary. Foreign-currency details available via drill-down to individual journal entries.
- Q: Should report rows be clickable to drill down into the underlying transactions? → A: Yes — click-to-drill from any summary row into its constituent records.

## User Scenarios & Testing

### User Story 1 - Budget Execution Report (Priority: P1)

As a budget officer, I need a report comparing appropriations against open encumbrances and executed payments so I can monitor budget consumption across the fiscal year. The report must be filterable by fund, program, project, budget item, and period, and exportable to Excel/PDF.

**Why this priority**: Budget execution is the primary oversight function. Without visibility into how appropriated funds are being consumed, managers cannot make informed spending decisions. This is foundational to all other reporting.

**Independent Test**: Can be fully tested by querying budget data for a given fiscal year and verifying that appropriations = encumbrances + payments + available balance. Delivers immediate value for budget monitoring.

**Acceptance Scenarios**:

1. **Given** a fiscal year with posted appropriations, encumbrances, and payments, **When** the user requests the budget execution report for that year, **Then** the report displays each budget line with columns for: appropriated amount, open encumbrances, executed payments, and available balance, where available = appropriated - encumbrances - payments.
2. **Given** budget data across multiple funds, **When** the user filters by a specific fund, **Then** only budget lines belonging to that fund are displayed.
3. **Given** budget data across multiple programs/projects, **When** the user filters by program or project, **Then** only matching budget lines are shown.
4. **Given** a budget execution report is displayed, **When** the user selects export, **Then** the report is downloadable in Excel or PDF format with all visible data.
5. **Given** a budget execution data for a full fiscal year, **When** the report is generated, **Then** it completes within 2 seconds.
6. **Given** a budget line in the execution report, **When** the user clicks the line, **Then** a detail view displays the individual encumbrances and payments that compose the summary totals.

---

### User Story 2 - Revenue Collections Report (Priority: P1)

As a revenue officer, I need a report of all receipt vouchers grouped by revenue account, party, payment method, and period, showing deposit-slip status and check clearing state, so I can reconcile collected revenue against bank deposits.

**Why this priority**: Revenue reconciliation is critical for financial integrity. Without visibility into collection status, the organization cannot confirm that collected funds have been deposited and cleared.

**Independent Test**: Can be fully tested by querying receipt vouchers for a period and verifying that total receipts match deposit slip totals. Delivers immediate value for revenue reconciliation.

**Acceptance Scenarios**:

1. **Given** receipt vouchers posted for a fiscal period, **When** the user requests the revenue collections report, **Then** the report lists each receipt voucher with revenue account, party, amount, payment method, deposit-slip reference, and clearing status.
2. **Given** receipt vouchers across multiple revenue accounts, **When** the user filters by revenue account, **Then** only receipts for that account are displayed.
3. **Given** receipt vouchers from multiple parties, **When** the user filters by party, **Then** only receipts from that party are shown.
4. **Given** receipt vouchers with various payment methods, **When** the user filters by method (cash, check, transfer), **Then** only matching receipts are displayed.
5. **Given** a revenue collections report for a full fiscal year, **When** the report is generated, **Then** it completes within 2 seconds.
6. **Given** a revenue summary row, **When** the user clicks it, **Then** a detail view displays the individual receipt vouchers composing that row.

---

### User Story 3 - Disbursement Register (Priority: P2)

As a financial controller, I need a register of all disbursement requests and payments showing their status, fund, period, and approver, so I can track payment workflow and identify bottlenecks.

**Why this priority**: Payment tracking supports operational efficiency and audit readiness. While important, it is secondary to budget execution and revenue reconciliation.

**Independent Test**: Can be fully tested by querying disbursement records for a period and verifying status counts. Delivers value for payment workflow management.

**Acceptance Scenarios**:

1. **Given** disbursement requests and payments in the system, **When** the user requests the disbursement register, **Then** the report lists each item with request date, payee, amount, status (draft/submitted/approved/paid/rejected), fund, and approver.
2. **Given** disbursements across multiple funds, **When** the user filters by fund, **Then** only disbursements for that fund are shown.
3. **Given** disbursements across multiple periods, **When** the user filters by period, **Then** only disbursements in that period are displayed.
4. **Given** disbursements with various statuses, **When** the user filters by status, **Then** only matching disbursements are shown.
5. **Given** a disbursement register for a full fiscal year, **When** the report is generated, **Then** it completes within 2 seconds.
6. **Given** a disbursement summary row, **When** the user clicks it, **Then** a detail view displays the individual requests and payments composing that row.

---

### User Story 4 - Budget Availability Snapshot (Priority: P2)

As a budget officer, I need to view the availability snapshot for any budget line showing the full dimension breakdown (fund, program, project, budget item) and blocking state, so I can understand what is available before approving new commitments.

**Why this priority**: Availability snapshots support real-time budget decisions. While important, they are used less frequently than the main execution report.

**Independent Test**: Can be fully tested by selecting a budget line and verifying the availability calculation matches the ledger. Delivers value for budget control decisions.

**Acceptance Scenarios**:

1. **Given** a budget line with appropriations, encumbrances, and payments, **When** the user requests the availability snapshot, **Then** the report shows: total appropriated, total encumbered, total paid, available balance, and the control state (none/warning/blocking).
2. **Given** a budget line with multi-dimensional allocation, **When** the user views the snapshot, **Then** the breakdown by fund, program, project, and budget item is displayed.
3. **Given** a budget line at a blocking threshold, **When** the availability is at or below zero, **Then** the blocking state is prominently displayed.
4. **Given** a budget availability snapshot, **When** the report is generated, **Then** it completes within 2 seconds.
5. **Given** a budget line in the availability snapshot, **When** the user clicks it, **Then** a detail view displays the individual appropriations, encumbrances, and payments that compose the availability totals.

---

### User Story 5 - Trial Balance & Ledger Movement Reports (Priority: P3)

As an auditor, I need trial balance and ledger movement reports scoped by fund and project dimensions, so I can verify ledger integrity and trace account activity.

**Why this priority**: Audit support is essential but used less frequently than operational reports. Trial balance verification can be performed periodically.

**Independent Test**: Can be fully tested by generating a trial balance for a period and verifying debits equal credits. Delivers value for audit and compliance.

**Acceptance Scenarios**:

1. **Given** posted journal entries for a fiscal period, **When** the user requests the trial balance, **Then** the report lists every account with opening balance, debits, credits, and closing balance, where total debits equal total credits.
2. **Given** trial balance data across multiple funds, **When** the user filters by fund, **Then** only accounts active in that fund are displayed.
3. **Given** posted journal entries for a specific account, **When** the user requests the ledger movement report, **Then** the report shows each posting with date, journal entry reference, debit amount, credit amount, and running balance.
4. **Given** a trial balance for a full fiscal year, **When** the report is generated, **Then** it completes within 2 seconds.
5. **Given** an account in the trial balance, **When** the user clicks it, **Then** a detail view displays the individual journal entries composing the account's debits and credits.

---

### Edge Cases

- What happens when no data exists for the selected filters? System displays an empty report with a message indicating no records match the criteria.
- What happens when a filter combination yields zero results? System shows zero totals with a clear indication that no data was found for the selected parameters.
- What happens when the ledger is mid-reconciliation? Reports read from the current ledger state; in-progress reconciliations are reflected as-is.
- What happens when a budget line has no appropriations? The availability snapshot shows zero across all dimensions with the control state based on configuration.
- What happens when export fails due to large data volume? System notifies the user and suggests narrowing filters.

## Requirements

### Functional Requirements

- **FR-001**: System MUST provide a budget execution report displaying appropriations, open encumbrances, executed payments, and available balance per budget line.
- **FR-002**: System MUST provide a revenue collections report listing receipt vouchers with revenue account, party, payment method, deposit-slip status, and check clearing state.
- **FR-003**: System MUST provide a disbursement register listing requests and payments with status, fund, period, and approver.
- **FR-004**: System MUST provide a budget availability snapshot showing full dimension breakdown and blocking state for any budget line.
- **FR-005**: System MUST provide trial balance and ledger movement reports scoped by fund and project dimensions.
- **FR-006**: All reports MUST be filterable by period and fund at minimum.
- **FR-007**: Budget execution report MUST be filterable by program, project, and budget item in addition to fund and period.
- **FR-008**: Revenue collections report MUST be filterable by revenue account, party, and payment method.
- **FR-009**: Disbursement register MUST be filterable by status and approver.
- **FR-010**: System MUST support export of reports to Excel and PDF formats.
- **FR-011**: All reports MUST complete within 2 seconds for a full fiscal year of data.
- **FR-012**: All report numbers MUST reconcile with the ledger to the last riyal (zero tolerance for rounding discrepancies).
- **FR-013**: Reports MUST be strictly read-only; no mutations or new transactional tables are permitted.
- **FR-014**: System MUST query only existing entities; no new entity creation is required for reporting.
- **FR-015**: Reports MUST respect the current user's authorization; users see only data they are permitted to access.
- **FR-016**: Every report MUST support click-to-drill from any summary row into the underlying detail records composing that row.

### Key Entities

No new entities are created. Reports query over existing entities including:
- **Appropriations**: Budget allocations by fund, program, project, budget item
- **Encumbrances**: Open commitments against appropriations
- **Payments**: Executed disbursements against encumbrances
- **Receipt Vouchers**: Revenue collection records with party, method, and deposit references
- **Journal Entries**: Ledger postings with lines, accounts, and dimensions
- **Budget Availability**: Calculated from appropriations minus encumbrances minus payments
- **Fiscal Periods**: Time boundaries for reporting
- **Users**: For approver attribution in disbursement register

## Success Criteria

### Measurable Outcomes

- **SC-001**: All five reports generate within 2 seconds for a full fiscal year of data.
- **SC-002**: Report totals reconcile with ledger balances to the last riyal (zero discrepancy).
- **SC-003**: Users can filter any report by period and fund, with additional dimension filters as specified per report.
- **SC-004**: Export to Excel and PDF completes within 5 seconds for a standard fiscal year dataset.
- **SC-005**: Reports display data that is consistent across all views; no conflicting totals between reports.
- **SC-006**: Authorization enforcement ensures users see only permitted data across all reports.

## Assumptions

- Existing database entities contain sufficient data for all required report dimensions (fund, program, project, budget item, period, party, method).
- Fiscal periods are properly configured and closed; reports respect period boundaries.
- Budget control levels (none/warning/blocking) are configured per budget line in the existing system.
- The current authentication and authorization framework can enforce report-level access control.
- Export functionality can leverage existing document generation capabilities.
- Report performance targets assume standard indexing on period, fund, and account columns.
- Arabic-first RTL rendering applies to all report layouts and exports.
- All report amounts display in base currency only; original foreign-currency amounts are not shown at summary level.
