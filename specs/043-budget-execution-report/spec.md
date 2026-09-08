# Feature Specification: Budget Execution Report (RPT-01)

**Feature Branch**: `rpt01-budget-execution-report`

**Created**: 2026-09-08

**Status**: Draft

**Input**: User description: "المجموعة: التقارير (RPT) — SPEC RPT-01 تقرير تنفيذ الموازنة (budget-execution-report). متخذ القرار يرى تنفيذ كل بند: تفويض/التزام/صرف/متاح، مع تفصيل بند (التزامات/مدفوعات) وتصدير مطابق للشاشة. يعتمد على بنود الموازنة (BGT) وحركات الالتزام/الصرف (TRE/PAY). الصلاحيات مؤكدة: Reporting.ViewBudgetExecution + Reporting.ExportReports."

## Clarifications

### Session 2026-09-08

- Q: What export format(s) must this report's export feature support? → A: Excel + PDF both.
- Q: When pagination activates (beyond the assumed 500-row threshold), should the totals row be computed over all filtered rows or only the displayed page? → A: Totals over all filtered rows.
- Q: Should the fiscal year filter be mandatory before showing any data, or should the report auto-select the currently open fiscal year? → A: Auto-select the currently open fiscal year; changeable via filter.

> **Scope note**: The user input describes a reports group (RPT) with shared requirements (RC-1..RC-6) inherited by RPT-01..06, plus SPEC RPT-02 (revenue collections report) which was provided only partially. Per Spec Kit rules, ONE feature per invocation — this spec covers **RPT-01 only**, absorbing the shared RC requirements it must inherit. RPT-02 requires its own `/speckit.specify` invocation with its full text.
>
> **CONTRACT NOTE**: The data contract in this spec is binding verbatim (web-api-client.ts). It documents the published HTTP payload shape — per Constitution IX the backend-generated OpenAPI document remains the single source of truth for the HTTP contract.

## User Scenarios & Testing

### User Story 1 - Budget Execution Overview (Priority: P1)

As a decision maker, I need to see the execution status of every budget item — appropriated, encumbered, paid, and available — so I can monitor budget consumption across the fiscal year and make informed spending decisions.

**Why this priority**: This is the core oversight view of the feature. Without it, no budget consumption visibility exists.

**Independent Test**: Can be fully tested by opening the report for a fiscal year with known data and verifying each line's four amounts and the totals row against ledger-derived figures.

**Acceptance Scenarios**:

1. **Given** a fiscal year with appropriations, encumbrances, and payments, **When** the user opens the report with filters (fiscal year / fund / program / project), **Then** the report displays one line per budget item with columns: item code, item name, fund, program, project, appropriated, encumbered, paid, available — plus a totals row with the four amounts fixed at the bottom of the table.
2. **Given** budget items across multiple funds, **When** the user filters by a specific fund, **Then** only lines belonging to that fund are displayed and the totals row recomputes over the filtered set.
3. **Given** budget items across programs/projects, **When** the user filters by program or project, **Then** only matching lines are shown.
4. **Given** the report is displayed, **When** the user reads the usage-ratio column, **Then** it shows paid/appropriated computed by the display layer, clearly marked as a display-only column (not a server-issued field).
5. **Given** a fiscal year with no data for the selected filters, **When** the report loads, **Then** an explicit empty state renders with Arabic artwork and the message "لا توجد بيانات للفترة المحددة" (per RC-3).
6. **Given** the report is loading or fails to load, **Then** the user sees a loading skeleton, or an error state with retry respectively (RC-3).

---

### User Story 2 - Budget Item Detail (Priority: P1)

As a budget officer, I need to drill from any summary line into its constituent movements, so I can trace what encumbrances and payments compose the summary numbers.

**Why this priority**: Traceability from summary to source is required for audit and decision confidence; builds directly on US1.

**Independent Test**: Can be fully tested by clicking a budget item line and verifying the detail view lists exactly the encumbrances and payments behind that item's summary figures.

**Acceptance Scenarios**:

1. **Given** a budget item line in the report, **When** the user clicks the line, **Then** a detail view displays the item's encumbrances and payments in two tabs (التزامات / مدفوعات).
2. **Given** a budget item with no encumbrances but with payments, **When** the detail opens, **Then** the encumbrances tab shows an explicit empty state and the payments tab lists the payments.
3. **Given** the detail view, **When** the user switches tabs, **Then** the previously loaded tab's state is preserved without re-fetching (or refetches consistently — either is acceptable, but behavior must not flicker or lose data).

---

### User Story 3 - Export (Priority: P2)

As a decision maker, I need to export the report to a paper/electronic output that matches the screen, so I can share and archive the execution snapshot.

**Why this priority**: Valuable for distribution and audit, but the on-screen report delivers the core value first.

**Independent Test**: Can be fully tested by applying filters, exporting, and comparing the exported file's rows/columns/order to the on-screen table.

**Acceptance Scenarios**:

1. **Given** a filtered report on screen, **When** the user exports, **Then** the exported file contains exactly the same rows, columns, and ordering as the screen — the filters apply to the same query used for export (RC-2).
2. **Given** the current (open) fiscal period, **When** the user exports data that includes it, **Then** the export is delivered with a warning that the data is partial ("بيانات جزئية") (RC-4).
3. **Given** an exported file, **When** opened, **Then** it renders fully right-to-left with logical start/end alignment for numeric columns (RC-5).

---

### Edge Cases

- What happens when the result set exceeds the pagination threshold (assumed 500 rows — to be documented in plan)? → Pagination activates (RC-4); the threshold assumption is confirmed or corrected at plan time.
- What happens when the user lacks the view permission? → Unauthorized state rendered; server remains the sole authority (RC-3, Constitution VII).
- What happens when appropriated = 0 for an item? → Usage-ratio column must not divide by zero; display a defined representation (e.g., dash) rather than an error.
- What happens when a filter combination yields totals that differ from Σlines? → This is a defect; totals MUST equal the sum of displayed lines (SC-002).
- What happens when the fiscal year is lapsed/closed? → The report is read-only by nature; lapsed items still display their historical execution figures.

## Requirements

### Functional Requirements

- **FR-001**: The report MUST display a table with columns: itemCode, itemName, fund (number + name), program (code), project (code), appropriated, encumbered, paid, available.
- **FR-002**: The report MUST display a totals row with the four amounts (appropriated, encumbered, paid, available) fixed at the bottom of the table. When pagination is active, totals MUST be computed over ALL filtered rows — never only the displayed page.
- **FR-003**: The usage ratio (paid/appropriated) MUST be computed by the display layer as an additional column labeled "عرض" — it MUST NOT be issued as a server field.
- **FR-004**: The budget item detail MUST present encumbrances and payments in two tabs.
- **FR-005**: The report MUST inherit all shared report requirements RC-1..RC-6: server-sourced numbers only (RC-1); filters applied to the same query used for export so export matches screen (RC-2); loading skeleton, error+retry, explicit Arabic empty state, and unauthorized states (RC-3); pagination above the threshold and partial-data warning on current-period export (RC-4); full RTL in view and export with logical numeric column alignment (RC-5); separate view and export permissions (RC-6).
- **FR-006**: Every displayed and exported amount MUST come from the server; the frontend MUST NOT recompute financial amounts (RC-1, Constitution III).
- **FR-007**: The report MUST be strictly read-only — no mutations, no new transactional tables.

### Key Entities *(include if data involves existing entities — queries only, no new entities)*

- **BudgetItem**: the budget line being reported (code, name, fund/program/project dimensions).
- **Appropriation**: the authorized amount per budget item for the fiscal year.
- **Encumbrance**: open commitments against the item.
- **Payment**: executed payments against the item.
- **Report DTOs (binding contract)**: BudgetExecutionReportDto {fiscalYearId, fiscalYearName, lines[], totals}; BudgetExecutionLineDto {budgetItemId, itemCode, itemName, fundId, fundNumber, fundName, programId?, programCode?, projectId?, projectCode?, appropriatedAmount, encumberedAmount, paidAmount, availableAmount}; BudgetExecutionTotalDto {appropriatedAmount, encumberedAmount, paidAmount, availableAmount}; BudgetExecutionDetailDto {budgetItemId, itemCode, itemName, fundId, fundNumber, encumbrances: EncumbranceDetailDto[], payments: PaymentDetailDto[]}.

## Success Criteria

### Measurable Outcomes

- **SC-001**: A full fiscal year's report loads in under 3 seconds.
- **SC-002**: Totals row equals the sum of displayed lines (verified by a comparison test).
- **SC-003**: The exported output matches the on-screen report exactly for the same filters (rows, columns, order).
- **SC-004**: For any fiscal year, appropriated = encumbrances + payments + available per line (financial invariant, Constitution V).

## Assumptions

- Pagination threshold assumed at 500 rows — to be documented and confirmed in plan (RC-4).
- Fiscal year filter auto-selects the currently open fiscal year on load; user may change it via filter. Every query still carries an explicit fiscal year (fiscalYearId in contract) — clarified 2026-09-08. Fund/program/project filters optional — exact query filter signature is an engineering (non-blocking) question resolved from the handler signature at plan time (OQ-N1).
- Export format: Excel AND PDF, both matching the on-screen report exactly (rows, columns, order, RTL) — clarified 2026-09-08.
- Permissions confirmed as registered: Reporting.ViewBudgetExecution (view) and Reporting.ExportReports (export), separate per RC-6. Note: endpoint-layer policies are currently open placeholders (registered exception 1) — use-case-layer authorization is the effective control until RBAC remediation.
- Out of scope: dimension pivoting/breakdown (CTRL-01 territory), any editing or mutation.

## Dependencies

- BGT: budget items and appropriations (existing).
- TRE/PAY: encumbrance and payment movements (existing).
- Existing read-only reporting groundwork (spec 020 established click-to-drill and reconciliation conventions).
