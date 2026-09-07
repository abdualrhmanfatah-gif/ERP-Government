# Implementation Plan: ACC-05 — مراقبة المحاسبة (Accounting Monitoring)

**Branch**: `030-accounting-monitoring` | **Date**: 2026-09-07 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/030-accounting-monitoring/spec.md`

## Summary

إضافة وحدة مراقبة المحاسبة: عرض أرصدة الحسابات (خادمية) مع إغلاق/فتح فترة، فحص توافق وإعادة بناء، طابور أحداث ترحيل للقراءة فقط، وإدارة قواعد الترحيل (CRUD). يتصل بخط أنابيب الترحيل الموجود (AccountingEvent → JournalEntry).

## Technical Context

**Language/Version**: C# 13 / .NET 10

**Primary Dependencies**: EF Core, MediatR, FluentValidation, minimal APIs

**Storage**: SQL Server (EF Core migrations)

**Testing**: xUnit, FluentAssertions, Testcontainers (integration), Mock (unit)

**Target Platform**: Web service (ASP.NET Core) + React 19 SPA

**Project Type**: Web application (backend + frontend)

**Performance Goals**: Rebuild < 30s per period; query response < 500ms

**Constraints**: Arabic-first RTL UI; server-side business rules only; RowVersion concurrency

**Scale/Scope**: Government ERP — hundreds of accounts, thousands of journal entries per period

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Layered Architecture | ✅ PASS | New entities in Domain/Accounting; handlers in Application/Accounting; endpoints in Web/Endpoints/Accounting |
| II. Bounded Contexts | ✅ PASS | Accounting module owns all entities; reads via shared persistence abstraction |
| III. Server-Side Rules | ✅ PASS | All business rules (close guard, delete guard, rebuild logic) enforced server-side in handlers |
| IV. Financial Integrity | ✅ PASS | Balances computed from posted journal entries; rebuild reconciles against ledger; no stored computed columns |
| V. Budget Control | ✅ PASS | Out of scope — no payment/appropriation interactions |
| VI. Data Integrity | ✅ PASS | EF migrations; Restrict FK; RowVersion on AccountBalance, PostingRule; audit trail |
| VII. Authorization | ✅ PASS | PermissionCodes for all endpoints: Accounting.Balances.* / AccountingEvents.View / PostingRules.* |
| VIII. Approval Workflows | ✅ PASS | Period close/unclose recorded in DocumentStatusLog; no new approval workflows |
| IX. API Contract | ✅ PASS | Minimal APIs; OpenAPI spec auto-generated; NSwag client regeneration |
| X. UI Consistency | ✅ PASS | Arabic-first RTL; design tokens from tokens.ts; shared components |
| XI. Testing | ✅ PASS | TDD mandatory; functional tests for close/reconcile/rebuild; unit tests for handlers |
| XII. Controlled Change | ✅ PASS | New module within existing Accounting bounded context — no architectural deviation |

**Gate Result**: PASS — no violations. No complexity tracking needed.

## Project Structure

### Documentation (this feature)

```text
specs/030-accounting-monitoring/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── Domain/Accounting/Entities/        # AccountBalance, AccountingEvent, PostingRule, PostingRuleLine
├── Domain/Accounting/Enums/           # AccountingEventStatus, AccountSource, AmountSource, DebitOrCredit
├── Domain/Events/Accounting/          # Domain events (BalanceRebuiltEvent, PeriodClosedEvent)
├── Application/Accounting/
│   ├── Commands/
│   │   ├── AccountBalances/Finalize/  # FinalizePeriodCommand
│   │   ├── AccountBalances/Unfinalize/ # UnfinalizePeriodCommand
│   │   ├── AccountBalances/Rebuild/   # RebuildAccountBalancesCommand
│   │   └── PostingRules/Create/Update/Delete/
│   ├── Queries/
│   │   ├── AccountBalances/GetAccountBalances/ # Query with filters
│   │   ├── AccountBalances/Reconcile/
│   │   └── AccountingEvents/GetPendingEvents/
│   └── Common/                        # IDocumentStatusLogger, IAccountBalanceService
├── Infrastructure/Data/               # DbContext additions, migrations
├── Web/Endpoints/Accounting/          # IEndpointGroup implementations
└── Web/ClientApp/src/features/accounting-monitoring/  # Frontend pages/components/hooks/shared

tests/
├── Application.UnitTests/Accounting/  # Handler unit tests
├── Application.FunctionalTests/Accounting/  # Close/reconcile/rebuild functional tests
└── Web.AcceptanceTests/Accounting/    # Endpoint acceptance tests
```

**Structure Decision**: Follows existing layered architecture within Accounting bounded context. All new code stays within established module boundaries — no new projects or architectural deviations.

## Complexity Tracking

> No violations — section empty.
