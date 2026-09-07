# Implementation Plan: Deposit Slips (TRE-02)

**Branch**: `tre02-deposit-slips` | **Date**: 2026-09-07 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/032-deposit-slips/spec.md`

## Summary

Homogeneous deposit slips (Form47 cash / Form48 checks) that close a collection batch: create a slip with member vouchers, correct membership while Draft, approve once (Draft→Approved terminal — Form47 posts revenue via the event pipeline; Form48 confirms member checks UnderCollection), plus a per-fund monthly statement (six-figure summary + full activity).

**Key discovery — this is a gap-closure plan, not greenfield**: an earlier implementation (spec 017, commit d7ffd0f) already ships `DepositSlip` entity, all 4 commands, both queries, the endpoint group, the permission codes, and the migration. The plan therefore aligns the existing code to the binding TRE-02 contract and the clarified spec, adds the missing tests, and builds the missing frontend pages. Coordination note: TRE-01 (031) has plan/tasks but no implementation commits yet; both specs touch `ReceiptVoucher` — tasks must sequence against the current code and re-verify after TRE-01 lands.

## Technical Context

**Language/Version**: .NET 10 / C# 13 (backend), React 19 + TypeScript + Vite (frontend)

**Primary Dependencies**: EF Core + SQL Server, MediatR, FluentValidation, minimal APIs (.NET Aspire AppHost/ServiceDefaults); NSwag-generated API client

**Storage**: SQL Server (EF Core migrations; decimal(23,2) money; RowVersion concurrency)

**Testing**: xUnit — `tests/Domain.UnitTests`, `tests/Application.UnitTests`, `tests/Application.FunctionalTests` (real DB, per-test reset), `tests/Web.AcceptanceTests`; frontend `npm run test` (vitest)

**Target Platform**: Web (Arabic-first RTL), server-side API

**Project Type**: Web application (minimal API backend + SPA frontend)

**Performance Goals**: SC-001 — a full day's vouchers assembled into one slip in <5 min (UX/flow); statement query is bounded by one month of one fund's data

**Constraints**: Binding data contract (spec "Data Contract" section — literal); constitution principles I–XII; no editing applied migrations; Result<T> everywhere; DSL-{D6} slip numbering via IDocumentSequenceService in the creation transaction

**Scale/Scope**: 4 commands, 3 queries, 1 endpoint group, 1 migration-free alignment (no schema change required), frontend `treasury/deposit-slips` pages + monthly statement page

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|---|---|---|
| I. Layered integrity | ✅ | Gap-closure stays in `src/Application/Revenue` + `src/Web/Endpoints/Revenue`; no new cross-layer references |
| II. Bounded contexts / event-carried integration | ✅ | Form47 revenue posting rides the existing `ReceiptVoucherCollected` domain event → outbox → posting pipeline (assumption in spec). No direct cross-module writes |
| III. Server-side business rules | ⚠→✅ | Gaps to close: empty-slip approval guard (FR-005), cancelled-member guard, removal-reason enforcement. All enforced in handlers |
| IV. Financial integrity | ✅ | Posting happens only through posting pipeline; totals computed server-side from member lines (FR-007); no stored computed columns |
| V. Budget control | N/A | Revenue collection, no expenditure |
| VI. Data integrity | ✅ | No schema change needed; RowVersion verified on every mutation; no hard deletes (lifecycle terminal at Approved) |
| VII. Authorization / SoD | ⚠→✅ | Permission codes exist; `DepositSlipsManage` must be renamed to `DepositSlips.Update` (binding contract). Self-approval allowed per clarification FR-014. Open-placeholder policy registration (registered exception 1) — keep placeholders consistent |
| VIII. Approval workflows / audit immutability | ⚠→✅ | Approve currently hardcodes `RequiredRole="TreasuryManager"` without rule evaluation — must migrate to `IApprovalRuleEvaluationService` + `IDocumentStatusLogger` following the `ApprovePaymentOrder` exemplar (evaluation snapshot in ApprovalHistory) |
| IX. API/frontend contract integrity | ⚠→✅ | Routes must move from `/api/DepositSlips` to `/api/Revenue/DepositSlips` (RoutePrefix override on the endpoint group, mirroring the TRE-01 contract decision); NSwag client regenerated; problem-details semantics kept |
| X. UI/design tokens | ✅ | New pages use shared component library, tokens, RTL logical properties, money-display conventions; permission identifiers on nav entries |
| XI. Testing / TDD | ⚠→✅ | Only legacy `ReceiptVoucherTests` exist; new functional tests for every FR gap (red first), unit tests for handlers, frontend tests; no test weakening |
| XII. Controlled change | ✅ | No layer/module/contract changes beyond the spec's own binding contract; contract-conformant rename is spec-mandated, not silent divergence |

**Verdict**: PASS — all ⚠ items are the gap-closure work itself, tracked as tasks; no unjustified violations.

## Project Structure

### Documentation (this feature)

```text
specs/032-deposit-slips/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/
│   └── api.md           # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks — NOT created here)
```

### Source Code (repository root)

```text
src/
├── Domain/Revenue/
│   ├── Entities/            # DepositSlip (exists), ReceiptVoucher, Check
│   ├── Enums/               # FormType, DepositSlipStatus, CheckStatus, PaymentMethod (all exist)
│   └── Events/Revenue/      # ReceiptVoucherCollected (Form47 posting trigger, exists)
├── Application/Revenue/
│   ├── Commands/DepositSlips/   # Create/AddVoucher/RemoveVoucher/Approve (exist — align in place)
│   ├── Queries/                 # GetDepositSlips, GetDepositSlipById, Statements/GetMonthlyStatement (exist — align)
│   └── Common/DTOs/             # DepositSlipDto, MonthlyStatementDto (verify against contract)
├── Infrastructure/
│   ├── Data/Configurations/Revenue/  # DepositSlipConfiguration (exists)
│   └── FinancialSettings/Common/Services/DocumentSequenceService.cs  # DepositSlip → DSL (exists)
├── Web/Endpoints/Revenue/DepositSlips.cs   # Add RoutePrefix /api/Revenue/DepositSlips
└── Web/ClientApp/src/features/treasury/    # new deposit-slips pages/, components/, hooks/, shared/, __tests__/

tests/
├── Application.UnitTests/Revenue/       # handler unit tests (new)
├── Application.FunctionalTests/Revenue/ # DepositSlipLifecycleTests, MonthlyStatementTests (new)
└── Web.AcceptanceTests/                 # slip journey coverage
```

**Structure Decision**: Existing Revenue module layout extended in place (gap-closure); frontend extends the existing `treasury` feature folder with a `deposit-slips` area following the `budgeting` exemplar.

## Complexity Tracking

> No constitution violations require justification — gap-closure only. Coordination risks tracked here:

| Risk | Why Needed | Mitigation |
|-----------|------------|-------------------------------------|
| TRE-01 (031) rebuild pending on same files | `ReceiptVoucher`/`Check` semantics may shift when 031 lands | Sequence tasks against current code; after 031 merges, re-run this feature's full suite at converge gate |
| Shared `DSL` prefix across vouchers and slips (independent counters) | Both TRE-01 and TRE-02 contracts mandate `DSL-{D6}` | Already a documented TRE-01 research decision (R1); sequence table keyed by DocumentType keeps counters independent |
| Statement fund dimension missing on vouchers | `ReceiptVoucher` carries no `FundId` (TRE-01 scope) | v1 decision (research R5): single-fund cash box; explicit zeros when no activity; fund dimension is a TRE-01 follow-up |
