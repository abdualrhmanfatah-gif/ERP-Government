# Implementation Plan: Budget Preparation — BudgetItemAllocations Model

**Branch**: `046-budget-ledger-redesign` | **Date**: 2026-09-10 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/046-budget-ledger-redesign/spec.md`

## Summary

Introduce BudgetItemAllocations as the central entity for budget preparation. Each allocation links a BudgetItem to a Budget with ProposedAmount/ApprovedAmount, frozen at approval. BudgetTransactions become per-allocation (BudgetItemAllocationId replaces BudgetTransactionLines). ActualExpenditure is computed from posted JournalEntryLines via the GL account linked to the BudgetItem. Remaining = ApprovedAmount − ActualExpenditure. Available = Remaining − OutstandingEncumbrance. Transfer type is removed. PaymentOrders gain BudgetItemAllocationId for budget control linkage.

## Technical Context

**Language/Version**: C# 13 / .NET 10

**Primary Dependencies**: EF Core, SQL Server, MediatR, FluentValidation, minimal APIs, .NET Aspire

**Storage**: SQL Server via EF Core (migrations only, no auto-creation)

**Testing**: dotnet test — 5 projects: Domain.UnitTests, Application.UnitTests, Application.FunctionalTests, Infrastructure.IntegrationTests, Web.AcceptanceTests

**Target Platform**: Web service (backend) + React 19 SPA (frontend)

**Project Type**: Web application (backend + frontend)

**Performance Goals**: Budget availability queries < 1s (SC-006), blocking checks < 2s (SC-006)

**Constraints**: Arabic-only RTL frontend, TDD mandatory for new features, migration preserves existing data, shared GL account prevented per budget/fiscal year

**Scale/Scope**: ~12 domain entities, ~15 EF configurations, ~20 application commands/queries, ~7 endpoint groups, ~12 frontend components, ~18 test files

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Layered Architecture | PASS | Domain -> Application -> Infrastructure -> Web. Budgeting module owns its entities. No cross-layer violations. |
| II. Bounded Contexts | PASS | Budgeting integrates with Accounting via domain events only. ActualExpenditure reads JournalEntryLines through shared persistence abstraction — no direct writes. |
| III. Server-Side Business Rules | PASS | All validation, lifecycle transitions, availability checks, and GL account uniqueness enforced in use cases. |
| IV. Financial Integrity | PASS | Posted transactions immutable. Corrections via reversal only. Availability computed from posted data. No stored balances. |
| V. Budget Control | PASS | Control levels (None/Warning/Blocking) honored per budget type/item. AvailableAmount checked before posting. |
| VI. Data Integrity | PASS | Migrations only. FK Restrict. Optimistic concurrency. UNIQUE(BudgetId, BudgetItemId). No hard deletes. |
| VII. Authorization | PASS | New permission codes declared. Every endpoint requires authorization. |
| VIII. Approval Workflows | PASS | ApprovalHistory + DocumentStatusLog for every lifecycle transition. |
| IX. API Contract | PASS | OpenAPI regenerated. NSwag clients. |
| X. UI Consistency | PASS | Arabic RTL. Logical CSS. Design tokens. Money formatting. |
| XI. Testing | PASS | TDD mandatory. Functional tests against real DB. 5 existing test projects. |
| XII. Controlled Change | PASS | This spec documents the architectural change. No unregistered deviations. |

**Gate Result**: PASS — no violations. No justification table needed.

## Project Structure

### Documentation (this feature)

```text
specs/046-budget-ledger-redesign/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks — NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
├── Domain/Budgeting/
│   ├── Entities/         # Budget, BudgetItem, BudgetItemAllocation (NEW), BudgetTransaction, Encumbrance, EncumbranceLine, etc.
│   └── Enums/            # BudgetTransactionType (Transfer removed), BudgetTransactionStatus, TransactionDirection, etc.
├── Application/Budgeting/
│   ├── Commands/
│   │   ├── BudgetItemAllocations/   # Create, Update, Delete (Draft only)
│   │   ├── BudgetTransactions/      # Create, Submit, Approve, Post, Reverse
│   │   └── Budgets/                 # Submit, Approve, Activate, Close, Cancel
│   ├── Queries/
│   │   ├── BudgetItemAllocations/   # List, GetById, GetRemaining, GetAvailable
│   │   ├── BudgetTransactions/      # List, GetById
│   │   └── Budgets/                 # List, GetById
│   └── Common/                      # BudgetAvailabilityService (updated), IDocumentStatusLogger
├── Infrastructure/Data/             # DbContext, configurations, migrations
├── Web/Endpoints/Budgeting/
│   ├── BudgetItemAllocations.cs     # IEndpointGroup
│   ├── BudgetTransactions.cs        # IEndpointGroup
│   └── Budgets.cs                   # IEndpointGroup (lifecycle actions)
└── Web/ClientApp/src/features/budgeting/
    ├── budgets/                     # pages/ (list, detail with allocation grid)
    ├── budget-item-allocations/     # pages/, hooks/, shared/
    ├── budget-transactions/         # pages/, hooks/, shared/
    └── shared/                      # types.ts, client.ts

tests/
├── Domain.UnitTests/
├── Application.UnitTests/
├── Application.FunctionalTests/
├── Infrastructure.IntegrationTests/
└── Web.AcceptanceTests/
```

**Structure Decision**: Follows the existing layered architecture. New BudgetItemAllocation entity and commands/queries follow established patterns. BudgetTransactions simplified to per-allocation model.

## Complexity Tracking

No violations to justify. This plan is constitution-compliant.
