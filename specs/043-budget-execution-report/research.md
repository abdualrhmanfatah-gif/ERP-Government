# Research: Budget Execution Report (RPT-01)

**Date**: 2026-09-08 | **Branch**: `043-budget-execution-report`

Existing-state audit performed against the codebase before decisions (per AGENTS.md: specs can lag code). The Reporting slice from spec 020 already ships: `GetBudgetExecutionReportQuery/Handler`, `GetBudgetExecutionDetailQuery/Handler`, DTOs matching the binding contract, endpoints in `src/Web/Endpoints/Reporting/BudgetExecutionReports.cs`, exporters (`ExcelReportExporter`/`PdfReportExporter` via `IReportExporter`), functional tests in `tests/Application.FunctionalTests/Reporting/BudgetExecutionReportTests.cs`.

## D1 — Program/Project filters and dimensions

**Decision**: Resolve program/project through `BudgetItem.BudgetClassificationId` → `BudgetClassification` tree (`ParentId` chain). Filtering by ProgramId/ProjectId means "classification node = the filter node OR any descendant of it". Populate `ProgramCode`/`ProgramId`/`ProjectId`/`ProjectCode` on each line by walking the item's classification ancestry.

**Rationale**: There are no separate Program/Project entities — spec 012 established `BudgetClassification` as the single tree (Code/Name/ParentId/IsActive). Budget item dimensions live on the item via `BudgetClassificationId`; `Budget` carries `FundId` only. Any filter must therefore be subtree-aware, else filtering a parent program silently drops child-classified items.

**Alternatives considered**:
- Direct `ProgramId`/`ProjectId` FKs on BudgetItem — rejected: schema change, violates FR-007 read-only/no-migrations scope.
- Exact-node match only (no descendants) — rejected: breaks user expectation that filtering a program returns its projects' items.

**Resolved OQ-N1 (query filter signature)**: `FiscalYearId` required; `FundId`, `ProgramId`, `ProjectId`, `FiscalPeriodId`, `BudgetItemId` optional (all present on the existing query record — confirmed from handler signature).

## D2 — Pagination strategy (RC-4, threshold 500)

**Decision**: Client-side pagination in the frontend over the full `lines[]`; backend keeps returning the complete filtered set. Threshold: 500 rows confirmed (assumption ratified — full fiscal-year budget item counts in this deployment are in the low hundreds; SC-001 <3s covers delivery of the full set).

**Rationale**: The binding contract (`BudgetExecutionLineDto[]`, totals) has no paging envelope fields; adding them would be a contract-breaking change requiring a decision record (Constitution IX). Totals must span ALL filtered rows regardless of page (clarified 2026-09-08) — server returns totals over the whole set already, so client paging preserves SC-002 trivially. Export also uses the full set, keeping RC-2 (export matches screen) exact.

**Alternatives considered**:
- Server-side paging (new contract fields `page/pageSize/totalCount`) — rejected: breaking contract change + decision record overhead for no current scale need.
- Virtualized table without pagination — rejected: RC-4 mandates pagination past threshold; virtualization can complement later without contract impact.

## D3 — Status semantics for "paid" and "encumbered"

**Decision**: Align the report handler with `BudgetAvailabilityService` conventions:
- Appropriated: sum appropriations with `Status != Cancelled` per line (report keeps cancelled-excluded appropriation totals as shipped; availability-vs-report distinction documented).
- Encumbered: encumbrances with status in {Active, PartiallyReleased, PartiallyLiquidated} — sum outstanding amounts.
- Paid: payment orders with status in {Approved, SentToTreasury, Paid, PartiallyPaid} — sum paid/executed amounts; Draft/Submitted/Rejected/Voided/Cancelled excluded.

**Rationale**: Current handler counts every non-cancelled payment order as "paid" — a Draft order would consume "paid" budget without existing. That contradicts the spec's meaning of "صرف" (executed payment) and diverges from BudgetAvailabilityService's open-commitment status set, breaking reconciliation between report and availability checks (SC-004 / Constitution V spirit).

**Alternatives considered**: Keep non-cancelled (as-is) — rejected: overstates execution; auditors comparing report to ledger would find unexecuted orders counted as paid.

## D4 — Export fidelity (RC-2, RC-5, clarification: Excel + PDF)

**Decision**: Extend `ReportResultMapper.ToReportResult(BudgetExecutionReportDto)` with a dedicated 4-amount column layout (appropriated/encumbered/paid/available), per-column totals, Arabic section title ("تقرير تنفيذ الموازنة"), and the partial-data marker when the report range includes the currently open fiscal period. Extend `ExcelReportExporter`/`PdfReportExporter` for RTL sheet direction and logical start/end numeric alignment. Export handler reuses the SAME `GetBudgetExecutionReportQuery` as the list endpoint (already true) — filters and ordering identical by construction.

**Rationale**: Current mapping flattens the report to Debit/Credit/Balance ledger columns — loses encumbered column and per-column totals; English titles violate Arabic-first (Constitution X / RC-5). Exporter enhancements are shared infrastructure reused by all RPT reports.

**Alternatives considered**: Frontend-generated export (client-side XLSX/PDF) — rejected: RC-2 requires export identical to screen query; server-side from the same query result is the only auditable path, and permission separation (Reporting.ExportReports) already gates the endpoint.

## D5 — Fiscal year auto-select (clarified 2026-09-08)

**Decision**: Frontend loads fiscal years via the existing `FinancialSettings` fiscal-years endpoint, auto-selects the year with `Status == Open` (fallback: `IsActive`, then latest `YearNumber`), then queries the report. Every request still carries explicit `fiscalYearId`.

**Rationale**: Clarified behavior; no backend change needed — list endpoint already exists.

**Alternatives considered**: New "current fiscal year" endpoint — rejected: redundant; list + status filter suffices.

## D6 — Usage ratio (FR-003)

**Decision**: Frontend computes `paid/appropriated` per row, renders as percentage column labeled "عرض" (display marker). `appropriated == 0` → renders "—" (dash); negative available → red money representation via shared money-display conventions.

**Rationale**: Explicitly display-side per FR-003; server must never issue the field (contract binding). Dash convention avoids divide-by-zero without a server round trip.

**Alternatives considered**: Server-computed ratio — prohibited by FR-003/RC-1.

## D7 — Frontend placement

**Decision**: New domain folder `src/Web/ClientApp/src/features/reporting/budget-execution-report/` (pages/hooks/shared); feature-scoped components `ReportingBudgetExecutionFilters.tsx` + `ReportingBudgetExecutionDetail.tsx` in `src/components/`; query keys under `shared/api`; errors via `result-to-ui.ts`. Client wrapper only if composition demands it (header comment per AGENTS.md API Strategy).

**Rationale**: AGENTS.md layer map; reporting is cross-cutting — nesting under `features/budgeting/` would misrepresent its data sources (payments, treasury) and block RPT-02..06 reuse.

**Alternatives considered**: `features/budgeting/budget-execution-report/` — rejected: domain misplacement; future RPT specs would have no home.

## Open items → deferred to tasks

- None blocking. Exporter RTL enhancements may reveal library-level constraints (font embedding for Arabic in PDF) — implementation task includes a spike-with-fallback (render Arabic numerals/labels via existing exporter font stack; verify with Infrastructure.IntegrationTests).
