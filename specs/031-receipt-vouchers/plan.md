# Implementation Plan: Receipt Vouchers (TRE-01)

**Branch**: `031-receipt-vouchers` | **Date**: 2026-09-07 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/031-receipt-vouchers/spec.md`

## Summary

Receipt-voucher backend already exists (spec 017): entity, enums, DTOs, 4 lifecycle commands, 4 queries, endpoints, permissions `ReceiptVouchers.*` (exact contract codes). This plan closes the gaps between that implementation and the TRE-01 binding contract: DSL number prefix, self-approval allowance, cancel lifecycle restriction (Draft/PendingReview only), cancel FromStatus logging bug, Σ(checks) > total server-side rejection, `receivedFrom` made optional, create response returning the full DTO (number + total displayed immediately), and the full frontend feature (create form with conditional checks section, filtered list, detail with lifecycle actions).

## Technical Context

**Language/Version**: C# 13 / .NET 10, TypeScript 5.9 (frontend)

**Primary Dependencies**: EF Core, MediatR, FluentValidation, NSwag, Vite + React 19

**Storage**: SQL Server via EF Core

**Testing**: xUnit, FluentAssertions, Testcontainers (IntegrationTests), Vitest (frontend)

**Target Platform**: Web application (.NET Aspire orchestrator)

**Performance Goals**: Standard ERP — no special performance targets for voucher management

**Constraints**: Arabic-first RTL UI; decimal(23,2) for money; all FKs Restrict; no cascading deletes; binding data contract is literal (see spec §Data Contract)

**Scale/Scope**: Government ERP — hundreds of vouchers per day at peak; one frontend feature folder + backend gap fixes

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Layered Architecture | ✅ | Handlers in Application, entities in Domain, endpoints in Web — existing pattern followed |
| II. Bounded Contexts | ✅ | All within Revenue module; party read via shared persistence abstraction |
| III. Server-Side Business Rules | ⚠️ | Gaps: Σ(checks) > total not enforced; cancel guard too permissive (allows Approved); cancel logs wrong FromStatus |
| IV. Financial Integrity | ⚠️ | Number prefix deviates from binding contract (RCV vs DSL); create response hides the number |
| V. Budget Control | N/A | Receipt vouchers are revenue-side; no budget chain interaction |
| VI. Data Integrity | ✅ | RowVersion present on all three entities, migration-based, Restrict FKs; no hard delete |
| VII. Authorization | ✅ | Permission codes match contract exactly (View/Create/Submit/Approve/Cancel); dev-mode open policies = known tracked exception |
| VIII. Approval & Audit | ⚠️ | ApprovalHistory + DocumentStatusLog wired; SoD check present but contradicts clarified decision (self-approval allowed) |
| IX. API Contract | ⚠️ | Create returns bare id, not the contracted display payload; `receivedFrom` enforced required vs contract optional |
| X. UI Consistency | ⚠️ | Frontend feature not built yet (US1–US3); must use tokens.ts, RTL, shared primitives |
| XI. Testing | ⚠️ | Existing ReceiptVoucherTests must be extended for new rules; TDD mandatory for every gap |
| XII. Architectural Change | ✅ | No new modules or layer violations |

**Pre-Phase-0 Gate**: CONDITIONAL PASS — violations tracked below, each resolved by a planned gap fix.

### Violations / Gaps to Resolve

| Gap | Contract Ref | Resolution |
|-----|-------------|------------|
| Voucher number prefix is `RCV-`, contract mandates `DSL-{D6}` | FR-001 | Change `PrefixMap["ReceiptVoucher"]` to `"DSL"` (see research R1 — deliberate shared prefix with DepositSlip, independent counters) |
| Approve rejects submitter == approver (SoD) — contradicts clarified decision | Clarification Q1 | Remove the SoD guard in `ApproveReceiptVoucherCommandHandler` (approval history still recorded) |
| Cancel allowed on Approved vouchers (when not on a slip) — contract: Draft/PendingReview only, Approved terminal | Lifecycle | Guard: reject cancel unless status is Draft or PendingReview; cancel of Approved rejected even without deposit slip |
| Cancel logs `FromStatus = voucher.Status` AFTER mutation → always logs "Cancelled" | VIII (audit accuracy) | Capture fromStatus before mutation |
| Σ(checks) > total not validated | FR-008 | Handler + validator rule at create and submit |
| `receivedFrom` required in validator — contract: optional | Data Contract | Drop NotEmpty rule; DTO field nullable; UI optional free text |
| Create returns only `Result<int>` — FR-001 requires number displayed immediately | FR-001 | Create handler returns created DTO (voucherNumber, totalAmount, status, rowVersion); endpoint 201 with DTO |
| Submit on zero-line voucher possible only defensively | US1-3 | Guard in submit handler: reject when no lines |
| Reviewer name not in DTO (contract literal lacks `reviewedByName`) | SC-002 | Display via frontend users lookup cache (see research R3) |
| List query lacks PaymentMethod filter — US3 requires method filtering | US3, FR-002 | Add `PaymentMethod?` to `GetReceiptVouchersQuery` (+ by-party/by-period where reused) |
| No frontend feature | US1–US3 | Build `src/Web/ClientApp/src/features/treasury/` (pages/components/hooks/shared) |

### Post-Design Constitution Re-Check

| Principle | Pre-Design | Post-Design | Notes |
|-----------|------------|-------------|-------|
| III. Server-Side Rules | ⚠️ | ✅ | Σ-checks rule, cancel guard, FromStatus fix planned with TDD tests |
| IV. Financial Integrity | ⚠️ | ✅ | DSL prefix per contract; number returned in create response |
| VIII. Approval & Audit | ⚠️ | ✅ | SoD guard removal is a clarified spec decision, not a violation; history still recorded |
| IX. API Contract | ⚠️ | ✅ | Create returns full DTO; receivedFrom optional per contract |
| X. UI Consistency | ⚠️ | ✅ | Frontend plan conforms: tokens, RTL, shared primitives, money display |
| XI. Testing | ⚠️ | ✅ | Every gap ships test-first (red → green) |

**Post-Design Gate**: PASS — all gaps have concrete resolutions.

## Project Structure

### Documentation (this feature)

```text
specs/031-receipt-vouchers/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output (NOT created by /speckit.plan)
```

### Source Code (existing + changes)

```text
src/Application/FinancialSettings/Common/Services/
└── DocumentSequenceService.cs                # MODIFIED: ReceiptVoucher prefix → DSL

src/Application/Revenue/
├── Commands/ReceiptVouchers/
│   ├── CreateReceiptVoucher/CreateReceiptVoucherCommand.cs   # MODIFIED: Σ-checks rule, optional receivedFrom, returns DTO
│   ├── SubmitReceiptVoucher/SubmitReceiptVoucherCommand.cs   # MODIFIED: Σ-checks rule + zero-line guard
│   ├── ApproveReceiptVoucher/ApproveReceiptVoucherCommand.cs # MODIFIED: remove SoD guard
│   └── CancelReceiptVoucher/CancelReceiptVoucherCommand.cs   # MODIFIED: lifecycle guard + FromStatus fix
├── Common/DTOs/ReceiptVoucherDtos.cs         # MODIFIED: ReceivedFrom nullable
└── Queries/ReceiptVouchers/                  # MODIFIED: GetReceiptVouchersQuery adds PaymentMethod filter

src/Web/Endpoints/Revenue/
└── ReceiptVouchers.cs                        # MODIFIED: create endpoint returns DTO (201)

src/Web/ClientApp/src/features/treasury/     # NEW: pages/ components/ hooks/ shared/
tests/Application.FunctionalTests/Revenue/
└── ReceiptVoucherTests.cs                    # MODIFIED: tests for each gap
```

**Structure Decision**: Existing Revenue module layout extended in place; frontend gets a new `treasury` feature folder following the `budgeting` exemplar.
