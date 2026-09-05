# Implementation Plan: Financial Reports Module

**Branch**: `007-financial-reports` | **Date**: 2026-09-03 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/007-financial-reports/spec.md`

## Summary

Build 4 financial reports (Balance Sheet, Income Statement, General Ledger, Cash Flow Statement) with a deterministic report engine, multi-format output (Screen/Excel/PDF/Print), server-side pagination for General Ledger, base currency only (v1), separate `accounting.reports.*` permissions, Arabic-first RTL, and full audit trail. Existing Trial Balance is preserved untouched.

## Technical Context

**Language/Version**: C# 13 / .NET 10 (SDK pinned in global.json)

**Primary Dependencies**:
- Backend: MediatR 13.x (CQRS), FluentValidation 11.x, EF Core 10.x, Minimal APIs
- Excel: ClosedXML 0.102.x (OpenXML spreadsheet generation)
- PDF: QuestPDF 2024.x (fluent PDF generation with RTL/Arabic support)
- Frontend: React 19, TanStack Query 5, TanStack Table 9, shadcn/ui, Tailwind CSS, Vite 8, TypeScript 5.9

**Storage**: SQL Server (via Aspire container for tests, existing production DB)

**Testing**: NUnit 4.5.1, Moq 4.20.72, Shouldly 4.3.0, EF Core InMemory (unit tests), Aspire + SQL Server + Respawn (functional tests), Playwright + Reqnroll (acceptance tests)

**Target Platform**: Web application (ASP.NET Core backend + React SPA frontend)

**Performance Goals**: All 4 reports <2 seconds for 100k MoveLines + 1k Accounts

**Constraints**: Base currency only v1; Arabic-first RTL; server-side aggregation only; no client-side recalculation; INSERT-ONLY audit trails; Principle IV (financial integrity) NON-NEGOTIABLE

**Scale/Scope**: 4 report endpoints + 1 export endpoint; 4 frontend pages; 6 new permissions; ~100k MoveLines performance target

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Layered Architectural Integrity | ✅ PASS | Reports live in Application (use cases), endpoints in Web, format providers in Infrastructure. No layer violations. |
| II. Bounded Contexts | ✅ PASS | Reports read from Accounting module's entities via shared IApplicationDbContext. No cross-module writes. |
| III. Server-Side Business-Rule Integrity | ✅ PASS | All report computation server-side. Date validation, permission checks, balancing assertions in use cases. |
| IV. Financial Integrity (NON-NEGOTIABLE) | ✅ PASS | Reports read posted-only moves. Balance Sheet enforces Assets=Liabilities+Equity. Reversals net to zero. Base currency only. |
| V. Budget Control | ✅ N/A | Reports are read-only; no budget operations. |
| VI. Data Integrity | ✅ PASS | New CashFlowMappingRule table uses migrations only. Seed data is idempotent. |
| VII. Authorization (NON-NEGOTIABLE) | ✅ PASS | Each endpoint declares required permission via RequireAuthorization. AuditTrail records grant/deny. |
| VIII. Audit Immutability | ✅ PASS | SecurityAuditLog is INSERT-ONLY. Report generation events logged. |
| IX. API Contract Integrity | ✅ PASS | OpenAPI auto-generated. ProblemDetails for errors. ETag support. |
| X. UI/Design System | ✅ PASS | Design tokens only. RTL logical properties. Tabular numerals. Dark mode. |
| XI. Testing | ✅ PASS | Unit tests for each use case, functional tests for financial invariants, acceptance tests for critical journeys. |
| XII. Controlled Change | ✅ PASS | No registered exceptions needed for this feature. |

**Gate Result**: PASS — all principles satisfied. No violations to justify.

## Project Structure

### Documentation (this feature)

```text
specs/007-financial-reports/
├── spec.md              # Feature specification
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (API contracts)
│   ├── reports-api.md   # Report endpoint contracts
│   └── report-dtos.md   # DTO shapes for all reports
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── Domain/
│   └── Accounting/
│       ├── Entities/
│       │   └── CashFlowMappingRule.cs          # NEW: classification rules
│       └── Enums/
│           └── CashFlowSectionType.cs          # NEW: Operating/Investing/Financing
├── Application/
│   ├── Common/
│   │   └── Interfaces/
│   │       └── IApplicationDbContext.cs        # ADD: CashFlowMappingRules DbSet
│   ├── Accounting/
│   │   ├── Reports/
│   │   │   ├── Common/                         # NEW: shared report abstractions
│   │   │   │   ├── ReportQuery.cs
│   │   │   │   ├── ReportResult.cs
│   │   │   │   ├── ReportSection.cs
│   │   │   │   ├── ReportLine.cs
│   │   │   │   └── IReportEngine.cs
│   │   │   ├── BalanceSheet/                   # NEW
│   │   │   │   ├── GetBalanceSheetQuery.cs
│   │   │   │   ├── GetBalanceSheetQueryValidator.cs
│   │   │   │   ├── BalanceSheetDto.cs
│   │   │   │   └── GetBalanceSheetQueryHandler.cs
│   │   │   ├── IncomeStatement/                # NEW
│   │   │   │   ├── GetIncomeStatementQuery.cs
│   │   │   │   ├── GetIncomeStatementQueryValidator.cs
│   │   │   │   ├── IncomeStatementDto.cs
│   │   │   │   └── GetIncomeStatementQueryHandler.cs
│   │   │   ├── GeneralLedger/                  # NEW
│   │   │   │   ├── GetGeneralLedgerQuery.cs
│   │   │   │   ├── GetGeneralLedgerQueryValidator.cs
│   │   │   │   ├── GeneralLedgerDto.cs
│   │   │   │   └── GetGeneralLedgerQueryHandler.cs
│   │   │   └── CashFlowStatement/              # NEW
│   │   │       ├── GetCashFlowStatementQuery.cs
│   │   │       ├── GetCashFlowStatementQueryValidator.cs
│   │   │       ├── CashFlowStatementDto.cs
│   │   │       └── GetCashFlowStatementQueryHandler.cs
│   │   └── Common/
│   │       └── AccountBalanceDto.cs            # EXISTING: reuse for Balance Sheet
│   └── Common/
│       └── Security/
│           └── PermissionCodes.cs              # ADD: 6 report permissions
├── Infrastructure/
│   ├── Data/
│   │   ├── ApplicationDbContext.cs              # ADD: CashFlowMappingRules DbSet
│   │   ├── Configurations/
│   │   │   └── CashFlowMappingRuleConfiguration.cs  # NEW
│   │   ├── Migrations/
│   │   │   └── AddFinancialReports.cs          # NEW: migration
│   │   └── Seeds/
│   │       └── CashFlowMappingRuleSeedData.cs  # NEW: default classification rules
│   ├── Services/
│   │   ├── ReportEngine.cs                     # NEW: IReportEngine implementation
│   │   ├── ExcelReportExporter.cs              # NEW: ClosedXML export
│   │   └── PdfReportExporter.cs               # NEW: QuestPDF export
│   └── DependencyInjection.cs                  # ADD: register report services
└── Web/
    ├── Endpoints/
    │   └── Reports/
    │       └── Reports.cs                      # NEW: IEndpointGroup for reports
    └── ClientApp/
        └── src/
            └── features/
                └── reports/                     # NEW: frontend feature folder
                    ├── pages/
                    │   ├── BalanceSheetPage.tsx
                    │   ├── IncomeStatementPage.tsx
                    │   ├── GeneralLedgerPage.tsx
                    │   └── CashFlowStatementPage.tsx
                    ├── components/
                    │   ├── ReportFilters.tsx
                    │   ├── ReportDataGrid.tsx
                    │   ├── ReportExportDropdown.tsx
                    │   ├── BalanceSheetGrid.tsx
                    │   ├── IncomeStatementGrid.tsx
                    │   ├── GeneralLedgerGrid.tsx
                    │   └── CashFlowStatementGrid.tsx
                    ├── hooks/
                    │   ├── useReport.ts
                    │   └── useReportExport.ts
                    └── types.ts

tests/
├── Application.UnitTests/
│   └── Accounting/
│       └── Reports/                            # NEW
│           ├── GetBalanceSheetQueryTests.cs
│           ├── GetIncomeStatementQueryTests.cs
│           ├── GetGeneralLedgerQueryTests.cs
│           └── GetCashFlowStatementQueryTests.cs
├── Application.FunctionalTests/
│   └── Accounting/
│       └── Reports/                            # NEW
│           ├── BalanceSheetFunctionalTests.cs
│           ├── IncomeStatementFunctionalTests.cs
│           ├── GeneralLedgerFunctionalTests.cs
│           ├── CashFlowStatementFunctionalTests.cs
│           └── ReportExportFunctionalTests.cs
└── Web.AcceptanceTests/
    └── Features/
        └── Reports/                            # NEW
            └── reports.feature
```

**Structure Decision**: Follows existing Clean Architecture layered pattern. Report use cases in Application/Accounting/Reports/. Format providers (Excel/PDF) in Infrastructure/Services/ per Principle I (Infrastructure implements Application interfaces). Single Reports endpoint group in Web. Frontend reports feature folder follows existing `features/` convention.

## Complexity Tracking

> No Constitution violations. No complexity tracking needed.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| (none) | — | — |
