# Implementation Plan: Payments Group (PAY-01..04)

**Branch**: `045-payments-group` | **Date**: 2026-09-08 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/045-payments-group/spec.md`

## Summary

Four payment sub-modules — payment orders (lifecycle + budget check + treasury trace + totals), disbursement requests (1:1 with order, dual signature, availability gate), payment execution (single immutable record closing the order-request-payment triad), and bank accounts (registry + activate/deactivate + default auto-switch). The backend contract is largely built (`src/Application/Payments`, `src/Web/Endpoints/Payments|DisbursementRequests`); this plan aligns the built surface with the clarified spec: void guard (unpaid-only), dual-signature step-2 role check, Failed-check override via a new `PaymentOrders.OverrideBudgetCheck` permission, automatic request invalidation on order cancel/void, appropriation-level budget check via `BudgetAvailabilityService`, and bank-account default auto-switch. Frontend adds `features/payments/` (4 entity folders, Arabic RTL).

## Technical Context

**Language/Version**: .NET 10 / C# 13 (backend); React 19 + TypeScript + Vite (frontend)

**Primary Dependencies**: EF Core + SQL Server, MediatR, FluentValidation, minimal APIs, .NET Aspire; TanStack Query + NSwag client, React Hook Form + Zod, Tailwind v3 + shadcn

**Storage**: SQL Server (EF Core migrations; decimal(23,2) money, explicit precision)

**Testing**: No automated tests. Backend quality gate: `dotnet build src/Web/Web.csproj`. Frontend gate: `npm run lint` + `npm run build`. Validation via quickstart scenarios (S1–S6).

**Target Platform**: Web (backend API + SPA), Arabic-first RTL

**Project Type**: layered web application (Domain → Application → Infrastructure → Web + ClientApp)

**Performance Goals**: order create <5 min task time; totals endpoint fresh per request (no caching, Constitution-consistent)

**Constraints**: Financial Law 8/1990 (dual signature, budget-before-expenditure); server-side rule enforcement (Constitution III); all numbers server-issued (CC-1)

**Scale/Scope**: 4 sub-modules, ~30 endpoints (mostly built), 4 frontend entity folders, no automated tests

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|---|---|---|
| I. Layered integrity | PASS | Work confined to existing Payments module folders (Domain/Application/Web) + ClientApp; no new cross-references. |
| II. Bounded contexts / events | PASS | Payment→ledger posting already via `PaymentRecordedEvent` domain event + outbox; no direct cross-module writes. |
| III. Server-side rules | PASS | All clarified rules (void guard, step-2 role, auto-invalidate, default auto-switch) enforced in handlers/validators. |
| IV. Financial integrity | PASS | No journal mutation; void of unpaid order needs no reversing entry (nothing posted); posting remains event-driven. |
| V. Budget control | PASS→ALIGN | Budget check upgraded from fund-level heuristic to `BudgetAvailabilityService` appropriation-level check; Failed blocks approval; Overridden explicit + permissioned + recorded (spec clarification Q3). |
| VI. Data integrity | PASS | No schema deletion; `Restrict` FKs unchanged; RowVersion round-trips on all mutations; new permission seed idempotent. |
| VII. Authorization | PASS | New permission code `PaymentOrders.OverrideBudgetCheck` added to registry + policy; existing GAP-ADD codes already present in `PermissionCodes` (spec note stale — verified). Endpoint policies remain open placeholders per registered exception #1. |
| VIII. Approval audit immutability | PASS | Dual-signature steps + override decision recorded in ApprovalHistory (with evaluation snapshot); no inline columns. |
| IX. API contract integrity | PASS | No endpoint renames/removals; behavior fixes only; OpenAPI regenerated after changes (`npm run generate-api`). |
| X. UI/design consistency | PASS | Frontend reuses tokens.ts SSOT, shared primitives, RTL logical properties, Arabic-only strings. |
| XI. Testing/TDD | EXCEPTION | No automated tests for this feature. Backend gate: build success. Frontend gate: lint + build. Validation via quickstart scenarios. |
| XII. Controlled change | PASS | No module boundary/layer changes; no decision record required. Partial-payment semantics deferred (see research.md D7) — no contract invented. |

**Post-Phase-1 re-check**: PASS — no violations introduced; see Complexity Tracking (empty).

## Project Structure

### Documentation (this feature)

```text
specs/045-payments-group/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── payments-api.md
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── Domain/Payments/
│   ├── Entities/            # PaymentOrder, PaymentOrderLine, PaymentOrderDeduction,
│   │                        # DisbursementRequest, Payment, BankAccount (exist)
│   ├── Enums/               # 9 payment enums (exist; no changes)
│   └── Events/              # PaymentRecordedEvent lives in Application (as-built)
├── Application/Payments/
│   ├── Commands/
│   │   ├── PaymentOrders/   # Create/Update/Submit/Approve/Reject/Cancel/
│   │   │                    # SendToTreasury/Void (exist; Submit/Approve/Void/Cancel modified)
│   │   ├── DisbursementRequests/  # Create/Submit/Approve/Reject/Cancel (Approve modified)
│   │   ├── Payments/        # RecordPayment (unchanged behavior; optional referenceNumber)
│   │   └── BankAccounts/    # Create/Update/Activate/Deactivate (default auto-switch added)
│   ├── Queries/Payments/    # list/by-id/totals (exist)
│   └── Common/DTOs/
├── Infrastructure/Data/Migrations/   # NEW migration only if seed/precision requires
└── Web/
    ├── Endpoints/Payments/           # PaymentOrders.cs, Payments.cs, BankAccounts.cs (exist)
    ├── Endpoints/DisbursementRequests/DisbursementRequests.cs (exist)
    └── DependencyInjection.cs        # register OverrideBudgetCheck policy
src/Web/ClientApp/src/
├── features/payments/
│   ├── payment-orders/     # pages/ hooks/ shared/
│   ├── disbursement-requests/
│   ├── payments/
│   ├── bank-accounts/
│   └── hooks/ shared/      # cross-entity
├── components/             # Payments*-prefixed feature components
└── routes/                 # /payments/* route additions
```

**Structure Decision**: Reuse the existing Payments module layout (verified as-built). No new projects, no new endpoints groups — endpoint deltas are behavior changes inside existing handlers. Frontend follows the `features/budgeting/` exemplar with a new `features/payments/` domain folder.

## Complexity Tracking

> Empty — no Constitution violations to justify.
