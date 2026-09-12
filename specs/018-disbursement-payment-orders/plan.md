# Implementation Plan: Disbursement of Approved Payment Orders

**Branch**: `018-disbursement-payment-orders` | **Date**: 2026-09-09 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/018-disbursement-payment-orders/spec.md`

## Summary

Implement the request-first disbursement workflow in the existing Payment module. An accountant creates a **DisbursementRequest** (beneficiary, amount, currency, purpose, fiscal year). The request undergoes **dual-signature approval** (≥2 distinct approvers, ≥1 AccountsManager/AuthorizingOfficer). Each approver records an **approved amount** (≤ requested), **issuing authority name**, and **issuing authority capacity**. Upon second approval, a **PaymentOrder** is **auto-generated** as a Draft with the approved amount (FundId/AppropriationId deferred). The order follows its lifecycle: Draft → Submitted (budget check) → Approved → SentToTreasury → Paid. Payment execution records a **Payment** and triggers ledger posting.

## Technical Context

**Language/Version**: C# 13 / .NET 10
**Primary Dependencies**: EF Core, MediatR, FluentValidation, Minimal APIs
**Storage**: SQL Server
**Testing**: xUnit, FluentAssertions, Testcontainers (existing test projects)
**Target Platform**: Linux server (Docker)
**Project Type**: Web service (backend) + React frontend (existing pages)
**Performance Goals**: SC-004 <30s request creation, SC-008 <3s list/detail pages
**Constraints**: Single base currency, decimal(23,2), optimistic concurrency via RowVersion
**Scale/Scope**: Government ERP, moderate transaction volume

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Layered Architecture | PASS | Domain → Application → Infrastructure → Web. No cross-layer violations. |
| II. Bounded Contexts | PASS | DisbursementRequest, PaymentOrder, Payment live in Payments module. No cross-module writes. |
| III. Server-Side Business Rules | PASS | All validation in handlers. Dual-signature enforced server-side. Amount freeze after first approval. |
| IV. Financial Integrity | PASS | Ledger posting via event pipeline. Balanced journal entries. Net = Gross - Deductions. |
| V. Budget Control Before Expenditure | PASS (documented deviation) | Budget check at order submission, not request creation. Tender-law/monthly-plan gates excluded. |
| VI. Data Integrity | PASS | Migrations, Restrict FK, RowVersion, explicit decimal precision. |
| VII. Authorization & SoD | PASS | PermissionCodes on all endpoints. Dual-signature via ApprovalHistory. Separate edit/approve/cancel permissions. |
| VIII. Approval Workflows | PASS | ApprovalHistory records every decision with actor, timestamp, role, evaluation snapshot (amount + authority). |
| IX. API Contract | PASS | OpenAPI via NSwag. ProblemDetails error contract. |
| X. UI/Design System | N/A | Backend plan; frontend pages already exist. |
| XI. Testing | PASS | TDD mandatory. Functional tests for dual-sig, amount-freeze, budget check, posting. |
| XII. Controlled Change | PASS | Documented deviations: Principle V (budget check timing), dual-signature pattern. |

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

### Source Code (as-built, gaps to fill)

```text
src/
├── Domain/Payments/
│   ├── Entities/
│   │   ├── DisbursementRequest.cs    # EXISTS — no PaymentOrderId (ADR-001 D-2)
│   │   ├── PaymentOrder.cs           # EXISTS — has DisbursementRequestId UNIQUE
│   │   ├── Payment.cs                # EXISTS — links via PaymentOrderId UNIQUE
│   │   └── PaymentOrderDeduction.cs  # EXISTS
│   └── Enums/
│       ├── DisbursementRequestStatus.cs  # EXISTS
│       ├── PaymentOrderStatus.cs         # EXISTS
│       ├── PaymentStatus.cs              # EXISTS
│       ├── PaymentMethod.cs              # EXISTS
│       └── DeductionType.cs              # EXISTS
├── Application/Payments/
│   ├── Commands/DisbursementRequests/
│   │   ├── CreateDisbursementRequest/    # EXISTS
│   │   ├── SubmitDisbursementRequest/    # EXISTS
│   │   ├── ApproveDisbursementRequest/   # EXISTS — generates PaymentOrder on step 2
│   │   ├── RejectDisbursementRequest/    # EXISTS
│   │   └── CancelDisbursementRequest/    # EXISTS
│   ├── Commands/DisbursementRequests/UpdateDisbursementRequest/  # GAP — needs creation
│   ├── Commands/PaymentOrders/
│   │   ├── CreatePaymentOrder/           # EXISTS (order-first path)
│   │   ├── UpdatePaymentOrder/           # EXISTS
│   │   ├── SubmitPaymentOrder/           # EXISTS
│   │   ├── ApprovePaymentOrder/          # EXISTS
│   │   ├── RejectPaymentOrder/           # EXISTS
│   │   ├── CancelPaymentOrder/           # EXISTS
│   │   ├── SendToTreasury/               # EXISTS
│   │   └── VoidPaymentOrder/             # EXISTS
│   ├── Commands/Payments/
│   │   └── RecordPayment/                # EXISTS
│   ├── Queries/DisbursementRequests/
│   │   ├── GetDisbursementRequests/      # EXISTS
│   │   └── GetDisbursementRequestById/   # EXISTS
│   ├── Queries/PaymentOrders/
│   │   ├── GetPaymentOrders/             # EXISTS
│   │   ├── GetPaymentOrderById/          # EXISTS
│   │   └── GetPaymentOrderTotals/        # EXISTS
│   ├── Queries/Payments/
│   │   └── GetPayments/                  # EXISTS
│   └── Common/DTOs/                      # EXISTS
├── Web/Endpoints/
│   ├── DisbursementRequests/DisbursementRequests.cs  # EXISTS — needs Update endpoint
│   ├── PaymentOrders/PaymentOrders.cs                # EXISTS
│   └── Payments/Payments.cs                          # EXISTS
└── Infrastructure/Data/
    ├── ApplicationDbContext.cs                        # EXISTS
    └── Configurations/Payments/                       # EXISTS
```

### Key gaps to fill

| Gap | Description | Spec Reference |
|-----|-------------|----------------|
| UpdateDisbursementRequest | New command/handler/validator for Draft editing | US2, FR-003, FR-004 |
| Amount-freeze validation | PendingApproval requests reject amount edits | US2 scenario 5-6, FR-003 |
| DisbursementRequests.Update endpoint | PUT /api/DisbursementRequests/{id} | US2, FR-003 |
| Role check on step 2 approval | Currently step 1 only; step 2 needs role check | US3 scenario 3, FR-005 |

## Violation / Complexity Tracking

| Item | Principle | Status | Justification |
|------|-----------|--------|---------------|
| Budget check at order submit, not request creation | V | Documented deviation | Request approval gates creation; budget check gates order approval. Tender-law/monthly-plan excluded per user requirement. |
| Dual-signature pattern (≥2 approvers, same amount) | VIII | Domain-specific extension | Standard ApprovalHistory used; dual-sig counting is handler logic, not a new workflow engine. |
| Deferred FundId/AppropriationId on auto-generated orders | V | By design | Order generated as Draft with placeholders; user prepares before submit. Matches existing order-first preparation flow. |
