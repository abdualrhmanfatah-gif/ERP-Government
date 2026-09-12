# API Contract: Budget Execution Report (RPT-01)

**Date**: 2026-09-08 | Constitution IX: the backend-generated OpenAPI document is SSOT; this file documents the endpoint shapes the implementation MUST produce/consume. After implementation: `npm run generate-api` regenerates `web-api-client.ts` and this file must be re-verified against it.

## Endpoints (existing — deltas marked)

Base: `/api/Reporting/BudgetExecutionReports`

### GET `/api/Reporting/BudgetExecutionReports`
Budget execution lines for a fiscal year.
- **Auth**: `Reporting.ViewBudgetExecution` (endpoint policy + query-level `[Authorize]`)
- **Query parameters**: `fiscalYearId` (required, int) · `fundId?` · `programId?` · `projectId?` · `fiscalPeriodId?` · `budgetItemId?` (all int, optional — OQ-N1 resolved)
- **200**: `BudgetExecutionReportDto` (see data-model.md — binding)
- **Deltas**: `programId`/`projectId` filters now APPLIED (subtree-aware, research D1); `programCode`/`projectCode` populated; status semantics per research D3.

### GET `/api/Reporting/BudgetExecutionReports/{budgetItemId}/detail`
Detail for one budget line (tabs data).
- **Auth**: `Reporting.ViewBudgetExecution`
- **200**: `BudgetExecutionDetailDto` — `encumbrances[]` + `payments[]`
- **404**: unknown budgetItemId (problem-details per Constitution IX)

### GET `/api/Reporting/BudgetExecutionReports/export`
Export the report using the SAME query/filters/ordering as the list endpoint (RC-2).
- **Auth**: `Reporting.ExportReports` (separate permission — RC-6)
- **Query parameters**: same as list + `format` (`xlsx` | `pdf`, default `xlsx`)
- **200**: file stream
  - `xlsx`: `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`, filename `BudgetExecution-{yyyyMMdd}.xlsx`
  - `pdf`: `application/pdf`, filename `BudgetExecution-{yyyyMMdd}.pdf`
- **Deltas**: 4-amount columns + per-column totals row + Arabic title "تقرير تنفيذ الموازنة" + RTL sheet/page direction + logical numeric alignment (research D4). Partial-data marker ("بيانات جزئية") rendered in export header when the report range includes the currently open fiscal period (RC-4).
- **Partial-data signal for UI**: the export download is decorated by the frontend with the same warning banner shown on screen; server embeds the marker inside the document header.

## Frontend Consumption Contract

- Generated NSwag client (regenerated post-implementation) — no hand-written divergence (Constitution IX / AGENTS.md API Strategy).
- Query keys: `['reporting','budget-execution', filters]` and `['reporting','budget-execution-detail', budgetItemId]` under `shared/api` keys factory.
- Errors: `Result<T>.Errors` → `result-to-ui.ts` mapping (4xx inline / 5xx toast) per RC-3.
- Client-side pagination over `lines[]` at threshold 500 (research D2); totals row always from server `totals` (never re-summed client-side — RC-1).
- Usage ratio computed client-side for display only (FR-003).
