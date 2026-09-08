# Research: Reports Group (RPT-01..06)

**Date**: 2026-09-08

## Research Summary

No research tasks required. All technical unknowns resolved through codebase exploration.

## Findings

### Backend Status: Fully Implemented

All 6 reports have complete backend implementations:

| Report | Endpoint Group | Handlers | DTOs | Permissions |
|---|---|---|---|---|
| RPT-01 Budget Execution | `src/Web/Endpoints/Reporting/BudgetExecutionReports.cs` | ✅ GetBudgetExecutionReport, GetBudgetExecutionDetail | ✅ | ✅ Reporting.ViewBudgetExecution |
| RPT-02 Revenue Collections | `src/Web/Endpoints/Reporting/RevenueCollectionsReports.cs` | ✅ GetRevenueCollectionsReport, GetRevenueCollectionsDetail | ✅ | ✅ Reporting.ViewRevenueCollections |
| RPT-03 Disbursement Register | `src/Web/Endpoints/Reporting/DisbursementRegisterReports.cs` | ✅ GetDisbursementRegister, GetDisbursementRegisterDetail | ✅ | ✅ Reporting.ViewDisbursementRegister |
| RPT-04 Availability Snapshot | `src/Web/Endpoints/Reporting/AvailabilitySnapshotReports.cs` | ✅ GetAvailabilitySnapshot, GetAvailabilitySnapshotDetail | ✅ | ✅ Reporting.ViewAvailabilitySnapshot |
| RPT-05 Trial Balance | `src/Web/Endpoints/Reporting/TrialBalanceReports.cs` | ✅ GetTrialBalanceReport, GetLedgerMovement | ✅ | ✅ Reporting.ViewTrialBalanceReport |
| RPT-06 Financial Statements | `src/Web/Endpoints/Reports/Reports.cs` (legacy) | ✅ BalanceSheet, IncomeStatement, GeneralLedger, CashFlow, TrialBalance | ✅ | ✅ Accounting.Reports.* |

**Export**: All endpoints support Excel + PDF export via `IReportExporter`.

**Common Infrastructure**: `ReportFilterDto`, `ReportResultMapper`, `IReportAuditLogger` exist in `src/Application/Reporting/Common/`.

### Frontend Status: Only RPT-01 Implemented

| Report | Frontend Page | Route | Nav Item |
|---|---|---|---|
| RPT-01 Budget Execution | ✅ `features/reporting/budget-execution-report/` | ✅ `/reporting/budget-execution` | ✅ "تقرير تنفيذ الموازنة" |
| RPT-02 Revenue Collections | ❌ | ❌ | ❌ |
| RPT-03 Disbursement Register | ❌ | ❌ | ❌ |
| RPT-04 Availability Snapshot | ❌ | ❌ | ❌ |
| RPT-05 Trial Balance | ❌ | ❌ | ❌ |
| RPT-06 Financial Statements | ❌ | ❌ | ❌ |

### NSwag-Generated Clients

All 6 report API clients are generated in `web-api-client.ts`:
- `BudgetExecutionReportsClient`
- `RevenueCollectionsReportsClient`
- `DisbursementRegisterReportsClient`
- `AvailabilitySnapshotReportsClient`
- `TrialBalanceReportsClient`
- Legacy `ReportsClient` (BalanceSheet, IncomeStatement, GeneralLedger, CashFlow, TrialBalance)

### RPT-01 Exemplar Pattern

The existing RPT-01 implementation establishes the pattern for all reports:
- **Page**: `pages/XxxReportPage.tsx` — filter bar + DataGrid + export buttons + empty/error states
- **Hooks**: `hooks/useXxxReport.ts` — TanStack Query hooks for list + detail
- **Shared**: `shared/types.ts` (re-exported DTOs + filter types), `shared/schemas.ts` (Zod), `shared/client.ts` (export blob download)
- **Components**: `components/ReportingXxxFilters.tsx` + `components/ReportingXxxDetail.tsx`
- **Query Keys**: `reportingKeys.xxx(filters)` + `reportingKeys.xxxDetail(id)`

## Decisions

1. **No backend work needed** — all endpoints, handlers, DTOs, and permissions exist.
2. **Follow RPT-01 pattern exactly** — same folder structure, same component conventions, same query key pattern.
3. **RPT-06 uses legacy `/api/Reports/*` endpoints** — different namespace from RPT-01..05 which use `/api/Reporting/*`.
4. **No pagination for RPT-01..05** — full result set loaded. RPT-06 GL uses server-side pagination.
5. **No frontend tests** — governance decision (AGENTS.md Frontend Architecture).
