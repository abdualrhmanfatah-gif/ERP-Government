# Implementation Plan: Payments Group (PAY-01..04)

**Branch**: `045-payments-group` | **Date**: 2026-09-09 (amended per ADR-001 + request-first frontend requirements) | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/045-payments-group/spec.md` + [frontend-requirements.md](./frontend-requirements.md)

## Summary

Four payment sub-modules — payment orders (lifecycle + budget check + treasury trace + totals, two creation paths: order-first and request-first), disbursement requests (request-first initiator with independent creation, amount approval, issuing authority, atomic order generation), payment execution (single immutable record per order closing the triad), and bank accounts (registry + activate/deactivate + default auto-switch). **Schema simplified per ADR-001**: BeneficiaryName only (no Party ID), DisbursementRequestId UNIQUE on PaymentOrder (one-directional link), budget check at order submit only, PaymentOrderLine removed (AccountId added), approval data in ApprovalHistory only, PaymentOrderId UNIQUE on Payment. Frontend adds `features/payments/` (4 entity folders, Arabic RTL).

## Technical Context

**Language/Version**: .NET 10 / C# 13 (backend); React 19 + TypeScript + Vite (frontend)

**Primary Dependencies**: EF Core + SQL Server, MediatR, FluentValidation, minimal APIs, .NET Aspire; TanStack Query + NSwag client, React Hook Form + Zod, Tailwind v3 + shadcn

**Storage**: SQL Server (EF Core migrations; decimal(23,2) money, explicit precision)

**Testing**: No automated tests. Backend quality gate: `dotnet build src/Web/Web.csproj`. Frontend gate: `npm run lint` + `npm run build`. Validation via quickstart scenarios (S1–S6).

**Target Platform**: Web (backend API + SPA), Arabic-first RTL

**Project Type**: layered web application (Domain → Application → Infrastructure → Web + ClientApp)

**Performance Goals**: order create <5 min task time; totals endpoint fresh per request

**Constraints**: Financial Law 8/1990 (dual signature, budget-before-expenditure); server-side rule enforcement (Constitution III); all numbers server-issued (CC-1)

**Scale/Scope**: 4 sub-modules, ~30 endpoints (mostly built), 4 frontend entity folders, no automated tests

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-checked after Phase 1 design.*

| Principle | Status | Notes |
|---|---|---|
| I. Layered integrity | PASS | Work confined to existing Payments module folders + ClientApp; no new cross-references. |
| II. Bounded contexts / events | PASS | Payment→ledger posting via `PaymentRecordedEvent` + outbox; no direct cross-module writes. |
| III. Server-side rules | PASS | All rules (void guard, step-2 role, auto-invalidate, default auto-switch) enforced in handlers/validators. |
| IV. Financial integrity | PASS | No journal mutation; void of unpaid order needs no reversing entry; posting event-driven. |
| V. Budget control | PASS | Budget check at order submit via `BudgetAvailabilityService` (appropriation-level); Failed blocks approval; Overridden explicit + permissioned + recorded. Request submit is pure status transition (ADR-001 D-3). FundId/AppropriationId required at submit. |
| VI. Data integrity | PASS | ADR-001 migration: column drops/adds, unique indexes; Restrict FKs; RowVersion; idempotent permission seed. |
| VII. Authorization | PASS | `PaymentOrders.OverrideBudgetCheck` permission added; existing GAP-ADD codes verified. Endpoint policies remain open placeholders. |
| VIII. Approval audit immutability | PASS | Approval data (amount, issuing authority) in ApprovalHistory only — no duplicate columns on DisbursementRequest (ADR-001 D-5). |
| IX. API contract integrity | PASS | Breaking changes per ADR-001 (BeneficiaryPartyId removed, BeneficiaryId removed, PaymentOrderId removed from DisbursementRequest, PaymentOrderLine removed, AccountId added, approval columns removed from DisbursementRequest, PaymentOrderId UNIQUE on Payment). ADR-001 documents rationale. OpenAPI regenerated after changes. |
| X. UI/design consistency | PASS | Frontend reuses tokens.ts, shared primitives, RTL logical properties, Arabic-only strings. |
| XI. Testing/TDD | EXCEPTION | No automated tests. Backend gate: build success. Frontend gate: lint + build. Validation via quickstart scenarios. |
| XII. Controlled change | PASS | ADR-001 documents all breaking schema changes (rationale, scope, migration). No module boundary changes. |

**Post-Phase-1 re-check**: PASS — no violations introduced; ADR-001 covers all breaking changes.

## Project Structure

### Documentation (this feature)

```text
specs/045-payments-group/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── adr-001-schema-simplification.md   # NEW — breaking changes per ADR-001
├── frontend-requirements.md           # NEW — detailed screen/field specs for all UI screens
├── feature-context-request-first-disbursement.md  # Discovery input for request-first amendment
├── contracts/
│   └── payments-api.md
└── tasks.md
```

### Source Code (repository root)

```text
src/
├── Domain/Payments/
│   ├── Entities/            # PaymentOrder (no PaymentOrderLine), DisbursementRequest, Payment, BankAccount
│   ├── Enums/               # payment enums
│   └── Events/
├── Application/Payments/
│   ├── Commands/
│   │   ├── PaymentOrders/   # Create/Update/Submit/Approve/Reject/Cancel/SendToTreasury/Void
│   │   ├── DisbursementRequests/  # Create/Submit/Approve/Reject/Cancel
│   │   ├── Payments/        # RecordPayment
│   │   └── BankAccounts/    # Create/Update/Activate/Deactivate
│   ├── Queries/Payments/
│   └── Common/DTOs/
├── Infrastructure/Data/Migrations/   # ADR-001 migration
└── Web/
    ├── Endpoints/Payments/
    ├── Endpoints/DisbursementRequests/
    └── DependencyInjection.cs
src/Web/ClientApp/src/
├── features/payments/
│   ├── payment-orders/     # pages/ hooks/ shared/
│   ├── disbursement-requests/
│   ├── payments/
│   ├── bank-accounts/
│   └── hooks/ shared/
├── components/             # Payments*-prefixed
└── routes/                 # /payments/*
```

**Structure Decision**: Reuse existing Payments module layout. ADR-001 schema migration is the primary infrastructure change. Frontend follows `features/budgeting/` exemplar. Frontend screen-level specs are in `frontend-requirements.md` (linked from spec.md UI/UX sections).

## Complexity Tracking

> ADR-001 documents all breaking changes. No unresolved Constitution violations.

### Unresolved Decisions (from spec.md OQ-N5..N9)

These decisions are **not blocking** current implementation tasks but **block planning of future features** (e.g., payment gateway, Party linkage). They are tracked here for visibility.

| ID | Question | Impact on current scope |
|---|---|---|
| OQ-N5 | Beneficiary Party linkage: reintroduce in future version? | None — current model uses BeneficiaryName string only |
| OQ-N6 | Which signer's identity/capacity becomes the order's issuing authority? | None — both signatures recorded in ApprovalHistory; order authority is a display concern |
| OQ-N7 | Cancellation/re-authorization process after order generation? | None — current model: cancel + create new request |
| OQ-N8 | Zero-net balance scenario (deductions ≥ gross)? | None — server rejects deductionTotal > gross |
| OQ-N9 | Payment gateway integration or current Cash/Check model sufficient? | None — current enum is Cash/Check only |
