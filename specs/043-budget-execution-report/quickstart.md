# Quickstart: Budget Execution Report (RPT-01)

Validation guide — proves the feature end-to-end. Implementation detail lives in tasks.md/plan.md.

## Prerequisites

- .NET 10 SDK, SQL Server (per repo dev config), Node.js for ClientApp
- Seeded dev database with at least one fiscal year + budget with appropriations, encumbrances, payment orders

## Build & Run

```bash
dotnet build src/Web/Web.csproj
dotnet run --project src/AppHost        # Aspire orchestration (or src/Web directly)
cd src/Web/ClientApp && npm run dev
```

## Backend validation (tests are the evidence — Constitution XI)

```bash
dotnet test tests/Application.FunctionalTests --filter BudgetExecutionReport
dotnet test tests/Infrastructure.IntegrationTests --filter ReportExporter
dotnet test tests/Web.AcceptanceTests
```

### Scenario checks (map to spec tests T1–T5)

| # | Scenario | Expected |
|---|---|---|
| T1 | Query with `fiscalYearId` + `fundId`/`programId`/`projectId` | Only matching lines; program/project subtree items included; `programCode`/`projectCode` populated |
| T2 | Totals vs lines | `totals.X == Σ lines.X` for all 4 amounts, on filtered sets too |
| T3 | `GET /{id}/detail` | `encumbrances[]` + `payments[]` match the item's summary figures; empty lists are explicit |
| T4 | `GET /export?format=xlsx` and `?format=pdf` with same filters | File opens RTL; 4 amount columns + totals row + Arabic title; rows/order identical to screen query |
| T5 | Empty fiscal year / filters with no data | Empty `lines[]` with zeroed totals (frontend renders the explicit Arabic empty state) |
| — | Paid semantics | Draft/submitted/rejected payment orders do NOT count in `paidAmount` (research D3) |

## Frontend validation (manual — no frontend test suite by governance decision)

1. Open the budget execution report page → current open fiscal year auto-selected, report loads with skeleton first (RC-3).
2. Apply fund/program/project filters → lines + totals recompute; totals row spans all filtered rows even when paginated (clarified).
3. Exceed 500 rows (seed volume or lower threshold in dev) → pagination activates; totals unchanged across pages.
4. Click a line → detail opens with tabs التزامات / مدفوعات; tab with no data shows explicit empty state.
5. Item with `appropriated = 0` → usage-ratio column shows "—" (no error).
6. Export Excel and PDF with filters applied → file matches screen rows/columns/order; includes partial-data warning when current period included.
7. Load a year with no data → Arabic empty state "لا توجد بيانات للفترة المحددة"; kill the API → error state with retry.
8. RTL + dark mode visual pass: logical properties only, money via shared money-display, keyboard-reachable rows/tabs, ARIA labels.

## Definition of Done gates

- Full 5-project backend suite green (session preflight / pre-merge): Domain.UnitTests, Application.UnitTests, Application.FunctionalTests, Infrastructure.IntegrationTests, Web.AcceptanceTests
- `npm run lint` + `npm run build` in ClientApp
- `npm run generate-api` run after endpoint changes; contract re-verified against `web-api-client.ts`
- TDD evidence: filtered tests observed red before green (backend); no assertions weakened
