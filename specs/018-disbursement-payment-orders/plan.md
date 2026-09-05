# Implementation Plan: Disbursement of Approved Payment Orders

**Branch**: `018-disbursement-payment-orders` | **Date**: 2026-09-05 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/018-disbursement-payment-orders/spec.md`

## Summary

Add the payment-execution layer to the existing Payment module. An accountant creates a DisbursementRequest against an Approved PaymentOrder; budget availability is the sole gate. Dual-signature approval (≥2 distinct approvers, ≥1 AccountsManager/AuthorizingOfficer) is enforced via ApprovalHistory. On approval, a Payment is recorded, the payment order transitions to Paid, and a domain event triggers ledger posting.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: EF Core, MediatR, FluentValidation, Minimal APIs
**Storage**: SQL Server
**Testing**: xUnit, FluentAssertions, Testcontainers (existing test projects)
**Target Platform**: Linux server (Docker)
**Project Type**: Web service (backend only; frontend out of scope)
**Performance Goals**: SC-004 <30s request creation, SC-006 <5s report
**Constraints**: Single base currency, decimal(23,2), optimistic concurrency via RowVersion
**Scale/Scope**: Government ERP, moderate transaction volume

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Layered Architecture | PASS | Domain → Application → Infrastructure → Web. No cross-layer violations. |
| II. Bounded Contexts | PASS | DisbursementRequest and Payment live in Payments module. No cross-module writes. |
| III. Server-Side Business Rules | PASS | All validation in handlers. Dual-signature enforced server-side. |
| IV. Financial Integrity | PASS | Ledger posting via event pipeline. Balanced journal entries. |
| V. Budget Control Before Expenditure | PASS (documented deviation) | Availability check IS the gate. Tender-law and monthly-plan gates intentionally excluded. |
| VI. Data Integrity | PASS | Migrations, Restrict FK, RowVersion, explicit decimal precision. |
| VII. Authorization & SoD | PASS | PermissionCodes on all endpoints. Dual-signature via ApprovalHistory. |
| VIII. Approval Workflows | PASS | ApprovalHistory records every decision with actor, timestamp, role, snapshot. |
| IX. API Contract | PASS | OpenAPI via NSwag. ProblemDetails error contract. |
| X. UI/Design System | N/A | Backend only. |
| XI. Testing | PASS | TDD mandatory. Functional tests for budget check, dual-sig, posting. |
| XII. Controlled Change | PASS | No architectural deviations beyond documented ones. |

## Project Structure

### Documentation (this feature)

```text
specs/018-disbursement-payment-orders/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/
│   └── api-contracts.md # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── Domain/Payments/
│   ├── Entities/
│   │   ├── DisbursementRequest.cs    # NEW
│   │   └── Payment.cs                # NEW
│   └── Enums/
│       ├── DisbursementRequestStatus.cs  # NEW
│       └── PaymentStatus.cs              # NEW
├── Application/Payments/
│   ├── Commands/DisbursementRequests/
│   │   ├── CreateDisbursementRequest/
│   │   │   ├── CreateDisbursementRequestCommand.cs
│   │   │   ├── CreateDisbursementRequestCommandHandler.cs
│   │   │   └── CreateDisbursementRequestCommandValidator.cs
│   │   ├── ApproveDisbursementRequest/
│   │   │   ├── ApproveDisbursementRequestCommand.cs
│   │   │   ├── ApproveDisbursementRequestCommandHandler.cs
│   │   │   └── ApproveDisbursementRequestCommandValidator.cs
│   │   └── CancelDisbursementRequest/
│   │       ├── CancelDisbursementRequestCommand.cs
│   │       ├── CancelDisbursementRequestCommandHandler.cs
│   │       └── CancelDisbursementRequestCommandValidator.cs
│   ├── Commands/Payments/
│   │   └── RecordPayment/
│   │       ├── RecordPaymentCommand.cs
│   │       ├── RecordPaymentCommandHandler.cs
│   │       └── RecordPaymentCommandValidator.cs
│   ├── Queries/DisbursementRequests/
│   │   ├── GetDisbursementRequests/
│   │   │   └── GetDisbursementRequestsQuery.cs
│   │   └── GetDisbursementRequestById/
│   │       └── GetDisbursementRequestByIdQuery.cs
│   ├── Queries/Payments/
│   │   └── GetPayments/
│   │       └── GetPaymentsQuery.cs
│   └── Common/DTOs/
│       ├── DisbursementRequestDto.cs
│       └── PaymentDto.cs
├── Web/Endpoints/
│   ├── DisbursementRequests/
│   │   └── DisbursementRequests.cs    # IEndpointGroup
│   └── Payments/
│       └── Payments.cs                # IEndpointGroup
└── Infrastructure/Data/
    ├── ApplicationDbContext.cs        # Add DbSets
    └── Configurations/
        ├── DisbursementRequestConfiguration.cs  # NEW
        └── PaymentConfiguration.cs              # NEW

tests/
├── Application.UnitTests/Payments/
│   ├── CreateDisbursementRequestTests.cs
│   ├── ApproveDisbursementRequestTests.cs
│   └── RecordPaymentTests.cs
└── Application.FunctionalTests/Payments/
    └── DisbursementLifecycleTests.cs
```

**Structure Decision**: Follows existing Payments module layout. New entities in Domain/Payments/Entities, commands/queries in Application/Payments, endpoints in Web/Endpoints. One-file command/handler/validator pattern per AGENTS.md convention.
