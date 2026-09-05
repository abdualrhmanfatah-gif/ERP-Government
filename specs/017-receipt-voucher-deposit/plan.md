# Implementation Plan: Receipt Voucher & Deposit Slip Workflow

**Branch**: `017-receipt-voucher-deposit` | **Date**: 2026-09-05 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/017-receipt-voucher-deposit/spec.md`

## Summary

Replace the existing RevenueReceipt system with a unified ReceiptVoucher + DepositSlip workflow. Introduces two-stage revenue recognition: cash (Form 47) recognized on slip approval, checks (Form 48) recognized on bank clearing. Reuses existing Party, DocumentStatusLog, ApprovalHistory, and Attachment infrastructure from specs/016.

## Technical Context

**Language/Version**: C# 13 / .NET 9

**Primary Dependencies**: MediatR (CQRS), FluentValidation, EF Core 9, ASP.NET Core Minimal API

**Storage**: SQL Server (EF Core migrations)

**Testing**: xUnit, FluentAssertions, Testcontainers (SQL Server), WebApplicationFactory

**Target Platform**: Linux server (containerized)

**Project Type**: Web service (modular monolith)

**Performance Goals**: <200ms p95 for voucher creation, <500ms for monthly statement generation

**Constraints**: Arabic-first RTL UI, single base currency, append-only audit trails

**Scale/Scope**: ~50 concurrent cashiers, ~500 vouchers/day, ~10k checks/month under-collection

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Layered Architectural Integrity | ✅ PASS | Domain → Application → Infrastructure → Web. No cross-layer violations. |
| II. Bounded Contexts | ✅ PASS | Revenue module owns entities/use cases. Cross-module via events. |
| III. Server-Side Business-Rule Integrity | ✅ PASS | All rules enforced in use cases. Document state transitions guarded. |
| IV. Financial Integrity | ✅ PASS | Journal entries balanced. Posting gates enforced. Reversals for corrections. |
| V. Budget Control | ⚠️ N/A | Revenue collection does not involve budget control. |
| VI. Data Integrity | ✅ PASS | Migrations only, Restrict FKs, optimistic concurrency, explicit precision. |
| VII. Authorization | ✅ PASS | PermissionCodes declared on all endpoints and use cases. |
| VIII. Approval Workflows | ✅ PASS | Reviewer gate enforced. ApprovalHistory recorded. |
| IX. API Contract Integrity | ✅ PASS | OpenAPI contract. Problem details errors. Optimistic concurrency tokens. |
| X. UI Consistency | ⚠️ N/A | Backend only; frontend separately consumed. |
| XI. Testing | ✅ PASS | TDD required. Functional tests against real DB. |
| XII. Controlled Change | ✅ PASS | Schema changes via migrations. No silent deviations. |

**Gate Result**: PASS — proceed to Phase 0.

## Project Structure

### Documentation (this feature)

```text
specs/017-receipt-voucher-deposit/
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
├── Domain/
│   └── Revenue/
│       ├── Entities/
│       │   ├── ReceiptVoucher.cs          # NEW
│       │   ├── ReceiptVoucherLine.cs      # NEW
│       │   ├── Check.cs                   # NEW
│       │   └── DepositSlip.cs             # NEW
│       ├── Enums/
│       │   ├── ReceiptVoucherStatus.cs    # NEW (Draft=0, PendingReview=1, Approved=2, Cancelled=3)
│       │   ├── PaymentMethod.cs           # NEW (Cash=1, Check=2)
│       │   ├── CheckStatus.cs             # NEW (UnderCollection=1, Cleared=2, Bounced=3)
│       │   ├── DepositSlipStatus.cs       # NEW (Draft=0, Approved=1)
│       │   └── FormType.cs                # NEW (Form47=47, Form48=48)
│       └── Events/
│           ├── ReceiptVoucherCollected.cs  # NEW (replaces RevenueReceiptPosted)
│           └── CheckCleared.cs            # NEW
├── Application/
│   └── Revenue/
│       ├── Commands/
│       │   ├── ReceiptVouchers/
│       │   │   ├── CreateReceiptVoucher/
│       │   │   ├── SubmitReceiptVoucher/
│       │   │   ├── ApproveReceiptVoucher/
│       │   │   └── CancelReceiptVoucher/
│       │   ├── DepositSlips/
│       │   │   ├── CreateDepositSlip/
│       │   │   ├── AddVoucherToSlip/
│       │   │   ├── RemoveVoucherFromSlip/
│       │   │   └── ApproveDepositSlip/
│       │   └── Checks/
│       │       ├── ClearCheck/
│       │       ├── BounceCheck/
│       │       └── ReplaceCheck/
│       ├── Queries/
│       │   ├── ReceiptVouchers/
│       │   │   ├── GetReceiptVouchers/
│       │   │   ├── GetReceiptVoucherById/
│       │   │   ├── GetReceiptVouchersByParty/
│       │   │   └── GetReceiptVouchersByPeriod/
│       │   ├── DepositSlips/
│       │   │   ├── GetDepositSlips/
│       │   │   └── GetDepositSlipById/
│       │   └── Statements/
│       │       └── GetMonthlyStatement/
│       ├── EventHandlers/
│       │   └── ReceiptVoucherCollectedHandler.cs  # NEW
│       └── Common/
│           └── DTOs/
│               └── ReceiptVoucherDtos.cs
├── Infrastructure/
│   ├── Data/
│   │   ├── Configurations/
│   │   │   ├── ReceiptVoucherConfiguration.cs
│   │   │   ├── ReceiptVoucherLineConfiguration.cs
│   │   │   ├── CheckConfiguration.cs
│   │   │   └── DepositSlipConfiguration.cs
│   │   └── Migrations/
│   │       └── YYYYMMDDHHMMSS_AddReceiptVoucherDepositSlip.cs
│   └── Services/
│       └── (existing DocumentSequenceService — no changes needed)
├── Web/
│   └── Endpoints/
│       └── Revenue/
│           ├── ReceiptVouchers.cs         # NEW (IEndpointGroup)
│           ├── DepositSlips.cs            # NEW (IEndpointGroup)
│           └── Checks.cs                  # NEW (IEndpointGroup)
tests/
├── Application.FunctionalTests/
│   └── Revenue/
│       ├── ReceiptVoucherTests.cs
│       ├── DepositSlipTests.cs
│       └── CheckTests.cs
└── Domain.UnitTests/
    └── Revenue/
        └── ReceiptVoucherTests.cs
```

**Structure Decision**: Follows existing modular monolith layout. Revenue module extends with new entities, commands, queries, and endpoints. No new projects required.

## Complexity Tracking

> No Constitution violations requiring justification. All gates pass.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| (none) | — | — |
