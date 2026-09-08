# Feature Specification: Reports Group (RPT-01..06)

**Feature Branch**: `044-reports-group`

**Created**: 2026-09-08

**Status**: Draft

**Input**: User description: "المجموعة: التقارير (RPT) — 6 تقارير مشتركة: RPT-01 تنفيذ الموازنة، RPT-02 تحصيل الإيرادات، RPT-03 سجل الصرف، RPT-04 لقطة التوفر، RPT-05 ميزان المراجعة، RPT-06 القوائم المالية. متطلبات مشتركة RC-1..RC-6 تُرث في كل تقرير."

## Clarifications

### Session 2026-09-08

- Q: What export format(s) must reports support? → A: Excel + PDF both, matching screen exactly.
- Q: When pagination activates (beyond the assumed 500-row threshold), should totals be computed over all filtered rows or only the displayed page? → A: Totals over all filtered rows.
- Q: Should the fiscal year filter be mandatory before showing any data, or should reports auto-select the currently open fiscal year? → A: Auto-select the currently open fiscal year; changeable via filter.
- Q: What is the exact formula for the "available" amount in RPT-01? → A: `appropriated - encumbered - paid` — server-computed and returned in the DTO; frontend displays without recomputation (RC-1, Constitution III).
- Q: Should reports compute fresh or use cached data? → A: Fresh computation per request — no caching or materialized views. Staleness is unacceptable for budget control data.
- Q: Is the 500-row pagination threshold confirmed? → A: No pagination threshold. Reports load the complete result set. Server returns all rows; frontend must not impose a row limit.

> **Scope note**: The user input describes a reports group (RPT) with shared requirements (RC-1..RC-6) inherited by RPT-01..06, plus individual spec details for each report. Per Spec Kit rules, ONE feature per invocation — this spec covers the **entire RPT group** as a single feature with sub-modules. Each sub-module (RPT-01..06) is documented as a distinct section within this spec.
>
> **CONTRACT NOTE**: The data contracts in this spec are binding verbatim (web-api-client.ts). They document the published HTTP payload shape — per Constitution IX the backend-generated OpenAPI document remains the single source of truth for the HTTP contract.

## Shared Report Requirements (RC-1..RC-6)

These requirements are inherited by ALL report sub-modules (RPT-01..06) and MUST NOT be repeated in each sub-module's section:

- **RC-1**: All numbers are server-sourced — the report displays what the server provides only; no client-side financial recomputation.
- **RC-2**: Filters apply to the same query used for export — export MUST match the screen (same filters, same ordering).
- **RC-3**: Loading states: skeleton · error+retry · explicit empty state (Arabic artwork + "لا توجد بيانات للفترة المحددة") · unauthorized.
- **RC-4**: Reports load the complete result set — no client-side pagination threshold. Server returns all rows subject to query/performance constraints. Export of current period includes a "بيانات جزئية" (partial data) warning.
- **RC-5**: Full RTL in view and export; numeric columns use logical start/end alignment.
- **RC-6**: Every report is associated with a separate view permission and export permission (table per sub-module below).
- **RC-7**: All reports MUST compute fresh from the database on every request. No caching, materialized views, or pre-computed snapshots. Staleness is unacceptable for budget control data.

## Sub-Module: RPT-01 — Budget Execution Report

### User Scenarios

#### US1 (P1) — Budget Execution Overview

As a decision maker, I need to see the execution status of every budget item — appropriated, encumbered, paid, and available — so I can monitor budget consumption across the fiscal year.

**Acceptance Scenarios**:

1. **Given** a fiscal year with appropriations, encumbrances, and payments, **When** the user opens the report with filters (fiscal year / fund / program / project), **Then** the report displays one line per budget item with columns: item code, item name, fund, program, project, appropriated, encumbered, paid, available — plus a totals row with the four amounts fixed at the bottom.
2. **Given** budget items across multiple funds, **When** the user filters by a specific fund, **Then** only lines belonging to that fund are displayed and the totals row recomputes over the filtered set.
3. **Given** the report is displayed, **When** the user reads the usage-ratio column, **Then** it shows paid/appropriated computed by the display layer, clearly marked as a display-only column (not a server-issued field).
4. **Given** a fiscal year with no data for the selected filters, **When** the report loads, **Then** an explicit empty state renders with Arabic artwork and the message "لا توجد بيانات للفترة المحددة" (RC-3).

#### US2 (P1) — Budget Item Detail

As a budget officer, I need to drill from any summary line into its constituent movements, so I can trace what encumbrances and payments compose the summary numbers.

**Acceptance Scenarios**:

1. **Given** a budget item line in the report, **When** the user clicks the line, **Then** a detail view displays the item's encumbrances and payments in two tabs (التزامات / مدفوعات).
2. **Given** a budget item with no encumbrances but with payments, **When** the detail opens, **Then** the encumbrances tab shows an explicit empty state and the payments tab lists the payments.

#### US3 (P2) — Export

As a decision maker, I need to export the report to a paper/electronic output that matches the screen, so I can share and archive the execution snapshot.

**Acceptance Scenarios**:

1. **Given** a filtered report on screen, **When** the user exports, **Then** the exported file contains exactly the same rows, columns, and ordering as the screen (RC-2).
2. **Given** the current (open) fiscal period, **When** the user exports data that includes it, **Then** the export is delivered with a "بيانات جزئية" warning (RC-4).

### Functional Requirements

- **FR-001**: The report MUST display a table with columns: itemCode, itemName, fund (number + name), program (code), project (code), appropriated, encumbered, paid, available. The `available` amount is computed server-side as `appropriated - encumbered - paid` and returned in the DTO; the frontend MUST NOT recompute it.
- **FR-002**: The report MUST display a totals row with the four amounts fixed at the bottom. Totals MUST be computed over ALL rows in the result set.
- **FR-003**: The usage ratio (paid/appropriated) MUST be computed by the display layer as an additional column labeled "عرض" — it MUST NOT be issued as a server field.
- **FR-004**: The budget item detail MUST present encumbrances and payments in two tabs.
- **FR-005**: Inherit RC-1..RC-6.
- **FR-006**: Every displayed and exported amount MUST come from the server; the frontend MUST NOT recompute financial amounts (RC-1, Constitution III).
- **FR-007**: The report MUST be strictly read-only — no mutations, no new transactional tables.

### Data Contract

- **BudgetExecutionReportDto**: `{fiscalYearId, fiscalYearName, lines[], totals}`
- **BudgetExecutionLineDto**: budgetItemId, itemCode, itemName, fundId, fundNumber, fundName, programId?, programCode?, projectId?, projectCode?, appropriatedAmount, encumberedAmount, paidAmount, availableAmount
- **BudgetExecutionTotalDto**: appropriatedAmount, encumberedAmount, paidAmount, availableAmount
- **BudgetExecutionDetailDto**: budgetItemId, itemCode, itemName, fundId, fundNumber, encumbrances: EncumbranceDetailDto[], payments: PaymentDetailDto[]

### API

GET `/api/Reporting/BudgetExecutionReports` · GET `/{budgetItemId}/detail` · GET `/export`

### Permissions

| Code | Status |
|---|---|
| Reporting.ViewBudgetExecution | ✓ |
| Reporting.ExportReports | ✓ |

### Success Criteria

- **SC-001**: A full fiscal year's report loads in under 3 seconds.
- **SC-002**: Totals row equals the sum of displayed lines (verified by a comparison test).
- **SC-003**: The exported output matches the on-screen report exactly for the same filters.
- **SC-004**: For any fiscal year, appropriated = encumbrances + payments + available per line, where available = appropriated - encumbered - paid (financial invariant, Constitution V).

### Tests

T1 Filters + columns · T2 Totals · T3 Detail with tabs · T4 Export · T5 Empty state (RC-3)

### Open Questions

- **OQ-N1** (Engineering): Exact query filters (fiscal year mandatory? fund/program/project optional?) — from handler signature.

---

## Sub-Module: RPT-02 — Revenue Collections Report

### User Scenarios

#### US1 (P1) — Revenue Collections

As a revenue officer, I need to monitor receipt vouchers and their deposit status for a given period.

**Acceptance Scenarios**:

1. **Given** a period, **When** the user filters by party / payment method / status, **Then** lines display with voucher number, date, party name, total amount, payment method, and deposit card status.
2. **Given** a period with no data, **When** the report loads, **Then** an explicit empty state renders (RC-3).

#### US2 (P1) — Voucher Detail

As a revenue officer, I need to drill into any receipt voucher to see its complete structure.

**Acceptance Scenarios**:

1. **Given** a receipt voucher, **When** the user clicks it, **Then** a detail view shows lines (with calculated amounts), checks, and deposit slip number/status.
2. **Given** a cancelled voucher, **When** it appears in the list, **Then** it is clearly marked with its cancelled status.

#### US3 (P2) — Export

**Acceptance Scenarios**:

1. **Given** a filtered report on screen, **When** the user exports, **Then** the exported file matches the screen exactly (RC-2).

### Functional Requirements

- **FR-001**: Lines display: voucherNumber, voucherDate, partyName, totalAmount, paymentMethod, deposit card + status.
- **FR-002**: Detail view is three-part: lines + checks + deposit card.
- **FR-003**: Cancelled vouchers are clearly marked with their status.
- **FR-004**: Inherit RC-1..RC-6.

### Data Contract

- **RevenueCollectionsDetailDto**: receiptVoucherId, voucherNumber, voucherDate, partyName, totalAmount, paymentMethod, depositSlipNumber?, depositSlipStatus?, lines: RevenueCollectionsLineDetailDto[], checks: CheckDetailDto[]
- **RevenueCollectionsLineDetailDto**: revenueAccountId, accountCode, accountName, amount
- **RevenueCollectionsReportDto / LineDto / TotalDto** — GAP-READ: main list columns to be read before implementation.

### API

GET `/api/Reporting/RevenueCollectionsReports` · GET `/{receiptVoucherId}/detail` · GET `/export`

### Permissions

| Code | Status |
|---|---|
| Reporting.ViewRevenueCollections | ✓ |
| Reporting.ExportReports | ✓ |

### Success Criteria

- **SC-001**: Filters load in under 2 seconds.
- **SC-002**: Detail view shows all three sections (lines, checks, deposit card) completely.

### Tests

T1 Filters · T2 Three-part detail · T3 Cancelled voucher marking · T4 Export · T5 Empty state

### Open Questions

- **OQ-N1** (Engineering): ReportDto/LineDto/TotalDto (GAP-READ) — main list columns.

---

## Sub-Module: RPT-03 — Disbursement Register Report

### User Scenarios

#### US1 (P1) — Register with Status Counters

As a payment controller, I need a disbursement register with status counters and amounts at a glance.

**Acceptance Scenarios**:

1. **Given** a period, **When** the user opens the register, **Then** totals display (5 status counters: draft/submitted/approved/paid/rejected + totalRequests) and lines.
2. **Given** a period with no data, **When** the report loads, **Then** an explicit empty state renders (RC-3).

#### US2 (P1) — Order Detail

As a payment controller, I need to drill into any payment order to see its actual payments.

**Acceptance Scenarios**:

1. **Given** a payment order, **When** the user clicks it, **Then** a detail view shows payments[], approverName, and paidAt.

#### US3 (P2) — Export

**Acceptance Scenarios**:

1. **Given** a filtered report on screen, **When** the user exports, **Then** the exported file matches the screen exactly (RC-2).

### Functional Requirements

- **FR-001**: Status counter cards (draft/submitted/approved/paid/rejected + totalRequests).
- **FR-002**: Lines: orderNumber, orderDate, payeeName, amount, status, fund, approver, paidAt.
- **FR-003**: Detail shows order payments.
- **FR-004**: Rejection reason is NOT displayed (not in contract — do not invent).
- **FR-005**: Inherit RC-1..RC-6.

### Data Contract

- **DisbursementRegisterDto**: `{fiscalYearId, fiscalYearName, lines[], totals}`
- **DisbursementRegisterLineDto**: paymentOrderId, orderNumber, orderDate, payeeName, amount, status, fundId, fundCode, fundName, approverId?, approverName?, paidAt?
- **DisbursementRegisterTotalDto**: totalRequests, draftCount, submittedCount, approvedCount, paidCount, rejectedCount, totalAmount, paidAmount
- **DisbursementRegisterDetailDto**: paymentOrderId, orderNumber, orderDate, payeeName, amount, status, fundCode, approverName?, paidAt?, payments: PaymentDetailDto[]

### API

GET `/api/Reporting/DisbursementRegisterReports` · GET `/{paymentOrderId}/detail` · GET `/export`

### Permissions

| Code | Status |
|---|---|
| Reporting.ViewDisbursementRegister | ✓ |
| Reporting.ExportReports | ✓ |

### Success Criteria

- **SC-001**: Status counters match the lines (verified by a comparison test).
- **SC-002**: Detail shows all payments for the order.

### Tests

T1 Counters · T2 Filters · T3 Detail · T4 Export · T5 Empty state

### Open Questions

- **OQ-N1** (User): Is rejection reason required in the register? ⇒ Requires backend contract addition (do not invent screen-side).

---

## Sub-Module: RPT-04 — Availability Snapshot Report

### User Scenarios

#### US1 (P1) — Snapshot with Control State

As a budget controller, I need a snapshot of budget items with their control state and colored indicators.

**Acceptance Scenarios**:

1. **Given** a fiscal year / organizational unit, **When** the user opens the snapshot, **Then** lines display with controlState (colored indicator) and financial amounts.
2. **Given** a fiscal year with no data, **When** the report loads, **Then** an explicit empty state renders (RC-3).

#### US2 (P1) — Item Detail (Triple View)

As a budget controller, I need to drill into any item to see its full appropriation-encumbrance-payment triple.

**Acceptance Scenarios**:

1. **Given** a budget item, **When** the user clicks it, **Then** a detail view shows appropriations[], encumbrances[], and payments[].

#### US3 (P2) — Export

**Acceptance Scenarios**:

1. **Given** a filtered report on screen, **When** the user exports, **Then** the exported file matches the screen exactly (RC-2).

### Functional Requirements

- **FR-001**: controlState displayed with colored indicator (server values — OQ-N1).
- **FR-002**: Detail view is triple-group: appropriations + encumbrances + payments.
- **FR-003**: Breakdown/totals (GAP-READ — OQ-N2).
- **FR-004**: Inherit RC-1..RC-6.

### Data Contract

- **AvailabilitySnapshotDto**: budgetItemId, itemCode, itemName, fiscalYearId, fiscalYearName, controlState, breakdown: AvailabilitySnapshotLineDto[], totals
- **AvailabilitySnapshotDetailDto**: budgetItemId, itemCode, itemName, appropriations: AppropriationDetailDto[], encumbrances: EncumbranceDetailDto[], payments: PaymentDetailDto[]
- **Line/Total** — GAP-READ: to be read before implementation.

### API

GET `/api/Reporting/AvailabilitySnapshotReports` · GET `/{budgetItemId}/detail` · GET `/export`

### Permissions

| Code | Status |
|---|---|
| Reporting.ViewAvailabilitySnapshot | ✓ |
| Reporting.ExportReports | ✓ |

### Success Criteria

- **SC-001**: controlState is displayed 100% of the time.
- **SC-002**: Detail view shows all three groups completely.

### Tests

T1 Control state with indicator · T2 Filters · T3 Detail · T4 Export · T5 Empty state

### Open Questions

- **OQ-N1** (Engineering): controlState values (free string — document for indicators/colors).
- **OQ-N2** (Engineering): Line/Total (GAP-READ).

---

## Sub-Module: RPT-05 — Trial Balance Report

### User Scenarios

#### US1 (P1) — Trial Balance

As an accountant, I need to verify that the books balance — total debits equal total credits.

**Acceptance Scenarios**:

1. **Given** a fiscal year / period, **When** the user opens the trial balance, **Then** lines display (accountCode, accountName, accountType + openingBalance, debitTotal, creditTotal, closingBalance) + totals (4 aggregates).
2. **Given** Σdebits = Σcredits, **When** the report is displayed, **Then** a balance indicator is shown (computed by display layer, labeled "عرض").
3. **Given** Σdebits ≠ Σcredits, **When** the report is displayed, **Then** a red warning alert is shown (imbalance indicator).
4. **Given** an open (not closed) period, **When** the report is displayed, **Then** a warning states "أرقام قابلة للتغير" (figures subject to change).

#### US2 (P1) — Account Movement

As an accountant, I need to trace the source of any account balance by viewing its ledger entries.

**Acceptance Scenarios**:

1. **Given** an account, **When** the user opens its movement, **Then** entries and totals are displayed.

#### US3 (P2) — Export

**Acceptance Scenarios**:

1. **Given** a filtered report on screen, **When** the user exports, **Then** the exported file matches the screen exactly (RC-2).

### Functional Requirements

- **FR-001**: Five columns per account (opening, debit, credit, closing) + 4 aggregate totals.
- **FR-002**: Balance indicator computed by display layer (ΣtotalDebits = ΣtotalCredits), labeled "عرض" — **no isBalanced in this contract** (exists only in legacy TrialBalanceDto).
- **FR-003**: Account movement displays entries with totals.
- **FR-004**: Open period warning: "أرقام قابلة للتغير".
- **FR-005**: Inherit RC-1..RC-6.

### Data Contract

- **TrialBalanceReportDto**: `{fiscalYearId, fiscalYearName, fiscalPeriodId?, fiscalPeriodName?, lines[], totals}`
- **TrialBalanceLineDto**: accountId, accountCode, accountName, accountType, openingBalance, debitTotal, creditTotal, closingBalance
- **TrialBalanceTotalDto**: totalDebits, totalCredits, totalOpeningBalance, totalClosingBalance
- **LedgerMovementDto**: accountId, accountCode, accountName, entries: LedgerMovementLineDto[], totals
- **LedgerMovementLine/Total** — GAP-READ.

### API

GET `/api/Reporting/TrialBalanceReports` · GET `/{accountId}/ledger-movement` · GET `/export`

### Permissions

| Code | Status |
|---|---|
| Reporting.ViewTrialBalanceReport | ✓ |
| Reporting.ExportReports | ✓ |

### Success Criteria

- **SC-001**: Trial balance loads in under 3 seconds.
- **SC-002**: Balance indicator is 100% correct (verified by fixture test).
- **SC-003**: Account movement loads in under 2 seconds.

### Tests

T1 Columns + totals · T2 Balance / imbalance · T3 Movement · T4 Open period warning · T5 Export · T6 Empty state

### Open Questions

- **OQ-N1** (Engineering): LedgerMovementLine/Total (GAP-READ).

---

## Sub-Module: RPT-06 — Financial Statements

### User Scenarios

#### US1 (P1) — Balance Sheet

As a financial manager, I need the official balance sheet — the primary legal statement.

**Acceptance Scenarios**:

1. **Given** an asOfDate, **When** the user opens the balance sheet, **Then** assets / liabilities / equity (groups with dual title/titleAr) + liabilitiesAndEquity + balance indicator are displayed.
2. **Given** balanced=false, **When** the statement is displayed, **Then** a red warning alert is shown (data integrity issue).

#### US2 (P1) — Income Statement

As a financial manager, I need the income statement to see the period's financial result.

**Acceptance Scenarios**:

1. **Given** a period (startDate/endDate), **When** the user opens the income statement, **Then** revenue + expenses + netIncome are prominently displayed.

#### US3 (P1) — General Ledger

As an accountant, I need the detailed general ledger with server-side pagination.

**Acceptance Scenarios**:

1. **Given** an account / period, **When** the user browses the GL, **Then** lines display (documentDate, entryNumber, reference, narration, accountCode, accountName, debit, credit, runningBalance) with page/pageSize/totalLines.
2. **Given** a large ledger, **When** the user pages through, **Then** the UI does not freeze and each page loads independently.

> **Note**: The GL is a detailed ledger (potentially millions of entries) and uses server-side pagination by design — this is distinct from summary reports (RPT-01..05) which load the complete result set.

#### US4 (P2) — Cash Flow Statement

As a financial manager, I need the cash flow statement with bilingual section titles.

**Acceptance Scenarios**:

1. **Given** the cash flow statement, **When** it is displayed, **Then** sections (title/titleAr) with items (description/amount) and totals are shown.

#### US5 (P2) — Unified Export

**Acceptance Scenarios**:

1. **Given** any financial statement on screen, **When** the user exports via `/{reportType}/export`, **Then** the exported file matches the screen exactly (RC-2).

### Functional Requirements

- **FR-001**: Balance sheet: 3 groups + liabilitiesAndEquity + balanced (server-side).
- **FR-002**: Income statement: free period + netIncome.
- **FR-003**: GL is **server-side paginated** (page/pageSize/totalLines — distinct from other reports which load full result sets) + runningBalance computed per row.
- **FR-004**: Cash flow sections are **bilingual** (titleAr displayed in RTL).
- **FR-005**: Legacy trial balance (sections + isBalanced + totalDebit/Credit).
- **FR-006**: Unified export: `/{reportType}/export` for any statement.
- **FR-007**: Inherit RC-1..RC-6.

### Data Contract

- **BalanceSheetDto**: asOfDate, currency, assets/liabilities/equity: BalanceSheetGroup, liabilitiesAndEquity, balanced, generatedAt
- **BalanceSheetGroup**: title, titleAr, items, total
- **ReportLine**: accountCode, accountName, debit, credit, balance
- **IncomeStatementDto**: startDate, endDate, currency, revenue/expenses: IncomeStatementGroup, netIncome, generatedAt
- **GeneralLedgerDto**: currency, totalLines, page, pageSize, lines: GeneralLedgerLine[], totals, generatedAt
- **GeneralLedgerLine**: documentDate, entryNumber, reference?, narration?, accountCode, accountName, debit, credit, runningBalance
- **CashFlowStatementDto** — partial (GAP-READ): CashFlowSection {title, titleAr, items: CashFlowLineItem[] {description, amount}, total}
- **TrialBalanceDto (legacy)**: fiscalYearId, fiscalYearName, fiscalPeriodId, periodName, sections: ReportSection[], totalDebit, totalCredit, isBalanced, currency, generatedAt

### API

GET `/api/Reports/balance-sheet` · `/income-statement` · `/general-ledger` · `/cash-flow` · `/trial-balance` · GET `/api/Reports/{reportType}/export`

### Permissions

| Code | Status |
|---|---|
| Accounting.Reports | ✓ (existing) |
| Reporting.ExportReports | ✓ |

### Success Criteria

- **SC-001**: Balance sheet with its groups and balance indicator loads in under 3 seconds.
- **SC-002**: GL pages load independently without freezing the UI.
- **SC-003**: All titleAr fields render in RTL correctly.

### Tests

T1 Balance sheet + balanced · T2 Income statement + netIncome · T3 GL pagination + runningBalance · T4 Cash flow with sections · T5 Legacy trial balance · T6 Unified export · T7 Empty state

### Open Questions

- **OQ-N1** (Engineering): Full CashFlowStatementDto body (GAP-READ).
- **OQ-N2** (Engineering): Query filters for each statement (date/account/period) — from handler signatures.

---

## Requirements Summary

### Functional Requirements (cross-cutting)

- **FR-RC-001**: All reports MUST inherit shared requirements RC-1..RC-6.
- **FR-RC-002**: Every displayed and exported amount MUST come from the server; the frontend MUST NOT recompute financial amounts (RC-1, Constitution III).
- **FR-RC-003**: All reports MUST be strictly read-only — no mutations, no new transactional tables.
- **FR-RC-004**: Export format: Excel AND PDF, both matching the on-screen report exactly (rows, columns, order, RTL).
- **FR-RC-005**: Reports MUST load the complete result set — no client-side pagination. The server returns all rows; the frontend MUST NOT impose a row limit.
- **FR-RC-006**: Fiscal year filter auto-selects the currently open fiscal year on load; user may change it via filter.

### Key Entities

- **BudgetItem**: budget line being reported (code, name, fund/program/project dimensions).
- **Appropriation**: authorized amount per budget item for the fiscal year.
- **Encumbrance**: open commitments against the item.
- **Payment**: executed payments against the item.
- **ReceiptVoucher**: revenue collection voucher with deposit tracking.
- **PaymentOrder**: disbursement order with status lifecycle.
- **Account**: chart of accounts entry with type and balances.
- **JournalEntry**: posted ledger entry with debit/credit lines.
- **Report DTOs (binding contract)**: All DTOs listed per sub-module above are the published HTTP payload shape.

## Success Criteria

### Measurable Outcomes

- **SC-001**: All P1 reports load in under 3 seconds for a full fiscal year.
- **SC-002**: All totals rows equal the sum of displayed lines (verified by comparison tests).
- **SC-003**: All exported outputs match the on-screen reports exactly for the same filters.
- **SC-004**: Financial invariants hold: appropriated = encumbrances + payments + available per line (Constitution V); debits = credits in trial balance (Constitution IV).
- **SC-005**: GL pages load independently without freezing the UI.
- **SC-006**: All titleAr fields render in RTL correctly.

## Assumptions

- Reports load the complete result set — no client-side pagination. Server returns all rows; performance constraints are handled server-side.
- Fiscal year filter auto-selects the currently open fiscal year on load; user may change it via filter.
- Export format: Excel AND PDF, both matching the on-screen report exactly (rows, columns, order, RTL).
- Permissions confirmed as registered per sub-module tables. Note: endpoint-layer policies are currently open placeholders (registered exception 1) — use-case-layer authorization is the effective control until RBAC remediation.
- Out of scope: dimension pivoting/breakdown (CTRL-01 territory), any editing or mutation, legacy trial balance in RPT-06 is display-only (no new posting logic).

## Dependencies

- BGT: budget items and appropriations (existing).
- TRE/PAY: encumbrance and payment movements (existing).
- ACC-05: account balances and journal entries (existing).
- Existing read-only reporting groundwork (spec 020 established click-to-drill and reconciliation conventions).
