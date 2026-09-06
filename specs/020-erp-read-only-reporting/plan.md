# Implementation Plan: ERP Read-Only Reporting & Oversight

**Branch**: `020-erp-read-only-reporting` | **Date**: 2026-09-06 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/020-erp-read-only-reporting/spec.md`

**Implementation Approach**: Read-only query layer — IEndpointGroup report endpoints (`/api/reports/...`), query handlers (no commands), `AsNoTracking`, dimension joins via JournalEntryLine dims + budget structures. No new tables, no mutations, no stored aggregates. Reconciliation tests against known ledger fixtures. Export = server-side file stream (Excel/PDF per existing IReportExporter pattern).

## Summary

Add five read-only financial reporting capabilities to the ERP: budget execution, revenue collections, disbursement register, budget availability snapshot, and trial balance/ledger movement reports. All reports query existing entities via MediatR handlers, filter by period/fund/dimensions, and support drill-down to detail records. Export uses the existing `IReportExporter` interface (ClosedXML for Excel, QuestPDF for PDF). No new entity tables or transactional mutations.

## Technical Context

**Language/Version**: C# 13 / .NET 10

**Primary Dependencies**: MediatR (query dispatch), EF Core (LINQ queries), AutoMapper (DTO mapping where used), ClosedXML (Excel export), QuestPDF (PDF export), FluentValidation (query validators)

**Storage**: SQL Server via EF Core — existing entities only, `AsNoTracking()` on all report queries

**Testing**: xUnit + Moq (unit), EF Core InMemory or Testcontainers (integration), existing test projects: `tests/Application.UnitTests`, `tests/Application.FunctionalTests`

**Target Platform**: ASP.NET Core minimal APIs (backend), React 19 + TypeScript (frontend)

**Project Type**: Web application (backend API + frontend SPA)

**Performance Goals**: All five reports complete within 2 seconds for a full fiscal year; export within 5 seconds

**Constraints**: Strictly read-only — no new entity tables, no mutations, no stored aggregates. Reconciliation to last riyal.

**Scale/Scope**: ~5 report types × ~2-3 endpoints each = ~12-15 endpoints total; queries over existing FY data (~50k-200k rows per report scope)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Layered Architectural Integrity | ✅ PASS | Reports: Web (endpoints) → Application (query handlers) → Domain (entities). No Infrastructure references from Application. |
| II. Bounded Contexts | ✅ PASS | Reports read cross-module via shared IApplicationDbContext. No cross-module writes. |
| III. Server-Side Business-Rule Integrity | ✅ PASS | No business rules in read-only queries. All filtering/sorting server-side. |
| IV. Financial Integrity | ✅ PASS | Base-currency totals only (clarified). Balancing verification in trial balance. |
| V. Budget Control | ✅ PASS | Availability snapshot reads existing control state. No mutations. |
| VI. Data Integrity | ✅ PASS | No schema changes. No new entities. |
| VII. Authorization | ⚠️ KNOW STATE | Endpoint policies registered as `RequireAssertion(_ => true)` (placeholder). New endpoints follow same pattern — real RBAC pending DEP-020 remediation. |
| VIII. Approval Workflows | N/A | No approvals in read-only reporting. |
| IX. API Contract | ✅ PASS | New endpoints follow OpenAPI conventions. `.Produces<T>()` on all routes. |
| X. UI/Design System | N/A | Frontend deferred to separate task. |
| XI. Testing | ✅ PASS | Reconciliation tests against known ledger fixtures planned. Functional tests per Constitution. |
| XII. Controlled Change | ✅ PASS | No architectural deviations. All patterns follow established codebase. |

**Gate Result**: PASS. No violations requiring justification.

## Project Structure

### Documentation (this feature)

```text
specs/020-erp-read-only-reporting/
├── spec.md              # Feature specification
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── report-endpoints.md
└── tasks.md             # Phase 2 output (via /speckit.tasks)
```

### Source Code (repository root)

```text
src/Application/Reporting/
├── Common/
│   ├── ReportFilterDto.cs              # Shared filter DTO (period, fund, dimensions)
│   └── IReportAuditLogger.cs           # Audit logging for report access
├── BudgetExecution/
│   ├── GetBudgetExecutionReport/
│   │   ├── GetBudgetExecutionReportQuery.cs
│   │   ├── GetBudgetExecutionReportQueryHandler.cs
│   │   ├── GetBudgetExecutionReportQueryValidator.cs
│   │   └── BudgetExecutionReportDto.cs
│   └── GetBudgetExecutionDetail/
│       ├── GetBudgetExecutionDetailQuery.cs
│       ├── GetBudgetExecutionDetailQueryHandler.cs
│       └── BudgetExecutionDetailDto.cs
├── RevenueCollections/
│   ├── GetRevenueCollectionsReport/
│   │   ├── GetRevenueCollectionsReportQuery.cs
│   │   ├── GetRevenueCollectionsReportQueryHandler.cs
│   │   ├── GetRevenueCollectionsReportQueryValidator.cs
│   │   └── RevenueCollectionsReportDto.cs
│   └── GetRevenueCollectionsDetail/
│       ├── GetRevenueCollectionsDetailQuery.cs
│       ├── GetRevenueCollectionsDetailQueryHandler.cs
│       └── RevenueCollectionsDetailDto.cs
├── DisbursementRegister/
│   ├── GetDisbursementRegisterQuery/
│   │   ├── GetDisbursementRegisterQuery.cs
│   │   ├── GetDisbursementRegisterQueryHandler.cs
│   │   ├── GetDisbursementRegisterQueryValidator.cs
│   │   └── DisbursementRegisterDto.cs
│   └── GetDisbursementRegisterDetail/
│       ├── GetDisbursementRegisterDetailQuery.cs
│       ├── GetDisbursementRegisterDetailQueryHandler.cs
│       └── DisbursementRegisterDetailDto.cs
├── AvailabilitySnapshot/
│   ├── GetAvailabilitySnapshotQuery/
│   │   ├── GetAvailabilitySnapshotQuery.cs
│   │   ├── GetAvailabilitySnapshotQueryHandler.cs
│   │   ├── GetAvailabilitySnapshotQueryValidator.cs
│   │   └── AvailabilitySnapshotDto.cs
│   └── GetAvailabilitySnapshotDetail/
│       ├── GetAvailabilitySnapshotDetailQuery.cs
│       ├── GetAvailabilitySnapshotDetailQueryHandler.cs
│       └── AvailabilitySnapshotDetailDto.cs
└── TrialBalance/
    ├── GetTrialBalanceReport/
    │   ├── GetTrialBalanceReportQuery.cs
    │   ├── GetTrialBalanceReportQueryHandler.cs
    │   ├── GetTrialBalanceReportQueryValidator.cs
    │   └── TrialBalanceReportDto.cs
    └── GetLedgerMovement/
        ├── GetLedgerMovementQuery.cs
        ├── GetLedgerMovementQueryHandler.cs
        ├── GetLedgerMovementQueryValidator.cs
        └── LedgerMovementDto.cs

src/Web/Endpoints/Reporting/
├── BudgetExecutionReports.cs           # IEndpointGroup: /api/BudgetExecutionReports
├── RevenueCollectionsReports.cs        # IEndpointGroup: /api/RevenueCollectionsReports
├── DisbursementRegisterReports.cs      # IEndpointGroup: /api/DisbursementRegisterReports
├── AvailabilitySnapshotReports.cs      # IEndpointGroup: /api/AvailabilitySnapshotReports
└── TrialBalanceReports.cs              # IEndpointGroup: /api/TrialBalanceReports

tests/Application.FunctionalTests/Reporting/
├── BudgetExecutionReportTests.cs
├── RevenueCollectionsReportTests.cs
├── DisbursementRegisterReportTests.cs
├── AvailabilitySnapshotReportTests.cs
└── TrialBalanceReportTests.cs
```

**Structure Decision**: Follows existing layered architecture. New `Reporting` module under Application for query handlers. New `Reporting` folder under Web/Endpoints for IEndpointGroup implementations. No new Domain entities — all queries read existing Budgeting, Revenue, Payments, and Accounting entities.

## Complexity Tracking

> No violations requiring justification. All patterns follow established codebase conventions.

| Item | Rationale |
|------|-----------|
| Cross-module reads | Existing pattern — IApplicationDbContext provides access across bounded contexts for read operations (Principle II allows this). |
| Reuse of IReportExporter | Existing infrastructure service — no new export abstractions needed. |
| Report-specific queries in Application layer | Follows established pattern in `src/Application/Accounting/Reports/` and `src/Application/Budgeting/Queries/FinancialControl/`. |
