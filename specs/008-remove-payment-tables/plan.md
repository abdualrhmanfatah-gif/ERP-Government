# Implementation Plan: Remove Payment Sub-Entity Tables and Convert PaymentMethod to Enum

**Branch**: `008-remove-payment-tables` | **Date**: 2026-09-03 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/008-remove-payment-tables/spec.md`

## Summary

Remove 4 payment tables (AdvancePayments, PaymentExecutions, PaymentAllocations, PaymentMethods) from the Payments module. Convert PaymentMethod from a database lookup table to an enum with 6 fixed values (Cash=0, BankTransfer=1, Check=2, CreditCard=3, WireTransfer=4, Other=5). The enum is stored as an integer column in PaymentOrders and RevenueReceipts. All related code (entities, commands, queries, DTOs, endpoints, configurations, seed data, permissions, frontend) is deleted. A EF Core migration handles schema changes with data backfill, audit logging, and full reversibility.

## Technical Context

**Language/Version**: C# 14 / .NET 10 (SDK 10.0.201, net10.0 target)

**Primary Dependencies**: EF Core 10.0.5, MediatR 14.1.0, FluentValidation 12.1.1, AutoMapper 16.1.1, Ardalis.GuardClauses 5.0.0

**Storage**: SQL Server via EF Core 10.0.5. Migrations only (Constitution Principle VI). No FK cascade deletes (Principle VI).

**Testing**: xUnit + FluentAssertions. Test projects: Application.FunctionalTests, Application.UnitTests, Domain.UnitTests, Infrastructure.IntegrationTests, Web.AcceptanceTests. Payment test directories exist but are empty.

**Target Platform**: Web application (ASP.NET Core backend + React/TypeScript SPA frontend via Aspire)

**Project Type**: Layered monolith: Domain → Application → Infrastructure → Web, orchestrated by Aspire AppHost

**Performance Goals**: N/A — schema deletion migration, not performance-critical

**Constraints**: warnings-as-errors per Constitution. Nullable reference types enabled. Central package management.

**Scale/Scope**: 8 domain entities in Payments module (5 kept, 3 deleted + 1 entity→enum). 26 command handlers, 12 query handlers, 5 DTOs, 8 EF configurations, 6 web endpoints. No frontend payment pages exist yet.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Layered Architectural Integrity | ✅ PASS | Deletion follows layer order: Domain entities → Application commands/queries → Infrastructure configs → Web endpoints. No cross-layer violations. |
| II. Bounded Contexts | ✅ PASS | All changes within Payments module. Revenue module's RevenueReceipt FK updated but no module boundary crossed. |
| III. Server-Side Business-Rule Integrity | ✅ PASS | No business rules added or modified. PaymentOrder FSM unchanged. PaymentMethod enum is passive data. |
| IV. Financial Integrity | ✅ PASS | No journal entry logic changed. PaymentMethod enum does not affect posting. Moves pipeline untouched. |
| V. Budget Control | ✅ PASS | No budget check logic changed. |
| VI. Data Integrity | ✅ PASS | Migration is versioned (EF Core). No runtime auto-deletion. FK restrictions reviewed. Audit logs written for deletions. Reversibility required. |
| VII. Authorization | ✅ PASS | 18 permissions deleted. PaymentOrder/RevenueReceipt permissions unchanged. PermissionCodes constants removed from code. |
| VIII. Approval Workflows | ✅ PASS | No approval logic changed. |
| IX. API Contract | ⚠️ BREAKING | Removing endpoints (PaymentMethods, PaymentExecutions, PaymentAllocations, AdvancePayments) is a breaking API change. Per Principle IX, requires decision record. However, these endpoints are being removed as part of a Constitution-compliant migration. Recommendation: register as exception DEP-021 before implementation. |
| X. UI/Design System | ✅ PASS | No frontend payment pages exist yet. No UI changes needed. |
| XI. Testing | ⚠️ DEBT | Payment test directories are empty. Existing stub tests must be updated or removed. No new test debt introduced (deletion feature). |
| XII. Controlled Change | ⚠️ ACTION | Removing 4 tables from Payments module is a module boundary change. Per Binding Constraints, "Adding, removing, splitting, or merging a module requires a decision record." However, this is table deletion within an existing module (Payments), not module removal. Decision record recommended but not strictly required. |

**Gate Result**: PASS with 2 actions required:
1. Register decision record DEP-021 for breaking API endpoint removal (Principle IX)
2. Decision record for table deletion within Payments module (Principle XII, recommended)

## Project Structure

### Documentation (this feature)

```text
specs/008-remove-payment-tables/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (empty — no external interfaces)
└── tasks.md             # Phase 2 output (NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
├── Domain/
│   └── Payments/
│       ├── Entities/          # DELETE: AdvancePayment.cs, PaymentExecution.cs, PaymentAllocation.cs, PaymentMethod.cs
│       │                      # KEEP: PaymentOrder.cs, PaymentOrderLine.cs, PaymentOrderDeduction.cs, BankAccount.cs
│       ├── Enums/             # DELETE: PaymentExecutionStatus.cs, PaymentAllocationStatus.cs, AdvancePaymentStatus.cs
│       │                      # ADD: PaymentMethod.cs (new enum)
│       │                      # KEEP: PaymentOrderStatus.cs, PaymentOrderLineType.cs, PaymentType.cs, BudgetCheckStatus.cs, DeductionType.cs, SettlementStatus.cs
│       └── Events/            # DELETE: AdvancePaymentCreated.cs, AdvancePaymentSettled.cs
│                              # KEEP: PaymentOrderApproved.cs, PaymentOrderExecuted.cs, PaymentOrderReversed.cs
├── Application/
│   └── Payments/
│       ├── Commands/
│       │   ├── AdvancePayments/     # DELETE entire folder
│       │   ├── PaymentAllocations/  # DELETE entire folder
│       │   ├── PaymentExecutions/   # DELETE entire folder
│       │   ├── PaymentMethods/      # DELETE entire folder
│       │   ├── BankAccounts/        # KEEP
│       │   └── PaymentOrders/       # KEEP (update PaymentMethod references)
│       ├── Queries/
│       │   ├── AdvancePayments/     # DELETE entire folder
│       │   ├── PaymentAllocations/  # DELETE entire folder
│       │   ├── PaymentExecutions/   # DELETE entire folder
│       │   ├── PaymentMethods/      # DELETE entire folder
│       │   ├── BankAccounts/        # KEEP
│       │   └── PaymentOrders/       # KEEP (update PaymentMethod references)
│       └── Common/
│           └── DTOs/
│               ├── AdvancePaymentDto.cs     # DELETE
│               ├── PaymentExecutionDto.cs   # DELETE
│               ├── PaymentMethodDto.cs      # DELETE
│               ├── BankAccountDto.cs        # KEEP
│               └── PaymentOrderDto.cs       # KEEP (update PaymentMethod field)
├── Infrastructure/
│   └── Data/
│       ├── Configurations/Payments/
│       │   ├── AdvancePaymentConfiguration.cs       # DELETE
│       │   ├── PaymentExecutionConfiguration.cs     # DELETE
│       │   ├── PaymentAllocationConfiguration.cs    # DELETE
│       │   ├── PaymentMethodConfiguration.cs        # DELETE
│       │   ├── PaymentOrderConfiguration.cs         # KEEP (drop PaymentMethodId index, add PaymentMethod int)
│       │   ├── PaymentOrderLineConfiguration.cs     # KEEP
│       │   ├── PaymentOrderDeductionConfiguration.cs # KEEP
│       │   └── BankAccountConfiguration.cs          # KEEP
│       ├── Seeds/
│       │   └── PaymentMethodSeedData.cs             # DELETE
│       └── ApplicationDbContext.cs                  # REMOVE 4 DbSet lines
├── Web/
│   └── Endpoints/Payments/
│       ├── AdvancePayments.cs       # DELETE
│       ├── PaymentExecutions.cs     # DELETE
│       ├── PaymentAllocations.cs    # DELETE
│       ├── PaymentMethods.cs        # DELETE
│       ├── PaymentOrders.cs         # KEEP
│       └── BankAccounts.cs          # KEEP
│   └── DependencyInjection.cs      # REMOVE 18 policy lines for deleted permissions
└── Shared/                          # No changes

tests/
├── Application.FunctionalTests/Payments/   # Empty — no changes needed
├── Application.UnitTests/Payments/          # Empty — no changes needed
└── Domain.UnitTests/Payments/               # Empty — no changes needed
```

**Structure Decision**: Existing layered architecture preserved. No structural changes — only file deletions and targeted modifications to PaymentOrder, RevenueReceipt, their configurations, and their DTOs.

### Post-Design Constitution Re-check

*Re-evaluated after Phase 1 design completion.*

| Principle | Pre-Design | Post-Design | Change |
|-----------|------------|-------------|--------|
| I. Layered Architectural Integrity | ✅ PASS | ✅ PASS | No change — deletion follows layer order |
| II. Bounded Contexts | ✅ PASS | ✅ PASS | No change — RevenueReceipt FK update is within contract |
| III. Server-Side Business-Rule Integrity | ✅ PASS | ✅ PASS | No change — enum is passive data |
| IV. Financial Integrity | ✅ PASS | ✅ PASS | No change — posting pipeline untouched |
| V. Budget Control | ✅ PASS | ✅ PASS | No change |
| VI. Data Integrity | ✅ PASS | ✅ PASS | Confirmed: FK constraints analyzed (R1), drop order correct, no cascade deletes, audit logging in migration |
| VII. Authorization | ✅ PASS | ✅ PASS | Confirmed: 19 permissions to delete (research corrected count from 18), PermissionCodes constants and DI policies removed |
| VIII. Approval Workflows | ✅ PASS | ✅ PASS | No change |
| IX. API Contract | ⚠️ BREAKING | ⚠️ BREAKING | Confirmed: 4 endpoint files deleted, breaking change requires DEP-021 |
| X. UI/Design System | ✅ PASS | ✅ PASS | Confirmed: no frontend payment pages exist (R8) |
| XI. Testing | ⚠️ DEBT | ⚠️ DEBT | Confirmed: test directories empty, no new debt |
| XII. Controlled Change | ⚠️ ACTION | ⚠️ ACTION | Confirmed: table deletion within existing module, decision record recommended |

**Post-Design Gate Result**: PASS — no new violations introduced by design. The PermissionCodes count is 19 (not 18 as stated in spec); the actual PermissionCodes file defines 5+5+4+5=19 permissions. Spec count is a minor documentation error; implementation uses the actual file as source of truth.

## Complexity Tracking

> No Constitution violations requiring justification. Two recommended actions noted in Constitution Check.

| Item | Action Required |
|------|----------------|
| Breaking API change (Principle IX) | Register DEP-021 decision record for endpoint removal |
| Table deletion within module (Principle XII) | Recommended decision record (not strictly required for table removal within existing module) |
