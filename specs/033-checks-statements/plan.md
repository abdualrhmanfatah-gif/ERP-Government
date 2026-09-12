# Implementation Plan: Checks & Monthly Statement (TRE-03)

**Branch**: `033-checks-statements` | **Date**: 2026-09-07 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/033-checks-statements/spec.md`

## Summary

Implement the post-approval check lifecycle (clear / bounce / replace) over the existing `Check` entity (created by TRE-01/TRE-02 flows), an under-collection checks list built from Check-method receipt vouchers, and the monthly-statement clearings tab (already present in TRE-02, verified). Clearing posts to the ledger through the established outbox pipeline (`CheckCleared` domain event → `AccountingEvent` → `PostingRule` → `JournalEntry`). A pre-existing partial implementation of the three check commands exists but **diverges from the binding contract** (see Divergence Inventory) — the plan rebuilds those handlers to match the contract rather than amending the contract.

## Technical Context

**Language/Version**: .NET 10 / C# 13 (verified: src/Web/Web.csproj, central package management)

**Primary Dependencies**: EF Core + SQL Server, MediatR, FluentValidation, minimal APIs (IEndpointGroup), .NET Aspire. Frontend: React 19 + TS + Vite, TanStack Query, React Hook Form + Zod, Tailwind v3 + shadcn, Arabic-only RTL.

**Storage**: SQL Server via EF Core (`src/Infrastructure`). Check/ReceiptVoucher tables exist (InitialCreate migration verified).

**Testing**: 5-project suite — Domain.UnitTests, Application.UnitTests, Application.FunctionalTests, Infrastructure.IntegrationTests, Web.AcceptanceTests. TDD mandatory (constitution XI). Frontend has no tests (governance override).

**Target Platform**: Web API + SPA, Arabic-first RTL.

**Project Type**: Layered web application (Domain → Application → Infrastructure → Web + ClientApp).

**Performance Goals**: Checks list and statement tabs are period-filtered server-side queries; no unbounded scans (date-range filter mandatory).

**Constraints**: Money decimal(23,2), no stored computed columns; append-only status history; optimistic-concurrency token round-trip on every mutation; posting only via outbox pipeline — never direct ledger writes.

**Scale/Scope**: Single-tenant government deployment; scope = 1 entity lifecycle + 2 read models + 1 posting rule seed + minimal frontend surfaces (list + action dialogs, statement tab already in TRE-02).

## Divergence Inventory (existing code vs binding contract)

Discovered during research — all verified by file:line. The contract wins; tasks rebuild handlers.

| # | Existing code | Contract requirement | File |
|---|---|---|---|
| D1 | `BounceCheckCommand` auto-creates a Draft replacement voucher at bounce time and returns its id | Bounce only transitions check → Bounced and locks the source voucher; replacement is a separate action (US3 scenarios 1–3) | src/Application/Revenue/Commands/Checks/BounceCheck/BounceCheckCommand.cs:46-62 |
| D2 | Bounce resets `originalVoucher.DepositSlipId = null` | Not in contract — no slip mutation on bounce | same, line 46 |
| D3 | Hand-rolled voucher numbering (`RCV-{n:D6}` parsed from last row) | Numbering MUST use `IDocumentSequenceService` allocated in-transaction (AGENTS.md binding) | same, lines 87-99 |
| D4 | `ReplaceCheckCommand` REQUIRES `ReplacementVoucherId` already set (created by bounce) and mutates that auto-voucher | Replace CREATES the replacement voucher and sets `ReplacementVoucherId`; a check with `ReplacementVoucherId` already set MUST be rejected (FR-005 — double replace) | src/Application/Revenue/Commands/Checks/ReplaceCheck/ReplaceCheckCommand.cs:31-32 |
| D5 | ReplaceCheck uses `PermissionCodes.ChecksClear` | Contract permission `Checks.Replace` (code absent from PermissionCodes.cs — add `ChecksReplace`) | src/Web/Endpoints/Revenue/Checks.cs:28 |
| D6 | No date validation on `ClearedAt`/`BouncedAt` | Date MUST be >= check date and <= today (FR-002/FR-003) | ClearCheckCommand.cs:82-83, BounceCheckCommand.cs:109-110 |
| D7 | No cancelled-source-voucher guard | A cancelled voucher's check cannot transition (Edge Cases) | both commands |
| D8 | No checks list / single-detail endpoints | FR-001 list + OQ1 detail (`GET /api/Revenue/Checks`, `GET /api/Revenue/Checks/{id}`) | src/Web/Endpoints/Revenue/Checks.cs |
| D9 | No posting rule seeded for `EventType.CheckCleared` | SC-003: every clearing pairs with a posting effect — seed rule required (mapper already maps `CheckCleared` → `EventType.CheckCleared` by name) | src/Infrastructure/Data/Seeds/PostingRuleSeedData.cs:10-24 |
| D10 | Concurrency handled by try/catch only, `RowVersion` assigned manually from request | Follow the established optimistic-concurrency pattern: set the property entry's original value so EF's token check fires; distinct failure message (FR-008) — align with TRE-02 command style | both commands |
| D11 | ReplaceCheck creates the new check only; for Cash it does nothing observable beyond mutating the auto-voucher's method | Replace creates a full replacement ReceiptVoucher (number via sequence service, amount = original check, user-supplied voucher date), links it via `ReplacementVoucherId`, and creates the new `Check` row when method = Check (FR-004) | ReplaceCheckCommand.cs:40-58 |

Existing pieces that ALREADY satisfy the contract (verified, keep as-is):

- `Check` entity + `CheckStatus` enum (src/Domain/Revenue/Entities/Check.cs, Enums/CheckStatus.cs) — field-for-field matches `CheckDto`.
- `CheckDto` / `CreateCheckDto` / `CheckClearingDto` (src/Application/Revenue/Common/DTOs/ReceiptVoucherDtos.cs) — match contract literals. `CheckDetailDto` must be added.
- `ClearCheck` emits `CheckCleared` domain event (IHasSourceEntity) → `DomainEventHandler` → `AccountingEvent(EventType.CheckCleared)` via `EventTypeMapper` name-mapping — pipeline wiring correct, only the PostingRule seed is missing.
- Monthly statement `Clearings[]` populated from cleared checks (src/Application/Revenue/Queries/Statements/GetMonthlyStatement/GetMonthlyStatementQuery.cs:83-89) — US4 read side done.
- `DocumentStatusLog` append on clear/bounce (FR-007).

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|---|---|---|
| I. Layered integrity | PASS | Handlers in Application; endpoints delegate only; Domain event in Domain/Events/Revenue. |
| II. Bounded contexts + event-carried integration | PASS | Clearing → ledger via `CheckCleared` outbox event; no direct cross-module writes. PostingRule seed lives in Infrastructure seeding (established pattern). |
| III. Server-side rule integrity | PASS | All guards server-side in handlers/validators (FR-006, lifecycle FR-011, date rules). |
| IV. Financial integrity | PASS | Posting rule produces a balanced JournalEntry via MoveGenerator from PostingRuleLines; no direct ledger write. |
| V. Budget control | N/A | Revenue collection path, not expenditure. |
| VI. Data integrity | PASS | No schema changes needed (Check table exists); RowVersion verified on every mutation; append-only DocumentStatusLog. |
| VII. Authorization & SoD | PASS | Every endpoint/use case declares a permission (`Checks.View/Clear/Bounce/Replace`); policies registered per repo pattern (placeholder `RequireAssertion` per registered exception #1 — unchanged behavior). |
| VIII. Approval workflows & audit immutability | PASS | Status transitions via append-only DocumentStatusLog; no inline-only columns. |
| IX. API/frontend contract | PASS | Contract is literal (spec); NSwag client regenerated after endpoint changes; RowVersion round-trips on every mutating call. |
| X. UI/design consistency | PASS | Frontend uses tokens, shared primitives, RTL logical properties; no new shared primitives required. |
| XI. Testing & TDD | PASS | TDD mandatory: every behavior red-green-refactor; functional tests cover lifecycle guards, concurrency, double-replace, posting-pairing against real DB. |
| XII. Controlled change | PASS | No layer/module/boundary change. Rebuilding non-conformant check handlers is contract enforcement, not architectural change. |

**Gate result: PASS** — re-checked after Phase 1 design (artifacts below introduce no new violation; D1–D11 rebuilds reduce existing deviation).

## Project Structure

### Documentation (this feature)

```text
specs/033-checks-statements/
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
├── Domain/
│   └── Revenue/
│       ├── Entities/Check.cs                 # exists — no change
│       └── Enums/CheckStatus.cs              # exists — no change
├── Application/
│   ├── Common/Security/PermissionCodes.cs    # + ChecksReplace
│   ├── Revenue/
│   │   ├── Common/DTOs/ReceiptVoucherDtos.cs # + CheckDetailDto
│   │   ├── Commands/Checks/ClearCheck/       # rebuild (D6, D7, D10)
│   │   ├── Commands/Checks/BounceCheck/      # rebuild (D1, D2, D6, D7, D10)
│   │   ├── Commands/Checks/ReplaceCheck/     # rebuild (D4, D5, D7, D11)
│   │   └── Queries/Checks/                   # NEW: GetChecks + GetCheckById
│   └── FinancialSettings/Common/Services/IDocumentSequenceService.cs  # used by replace
├── Infrastructure/
│   └── Data/Seeds/PostingRuleSeedData.cs     # + CheckCleared rule (D9)
└── Web/
    ├── Endpoints/Revenue/Checks.cs           # + GET list, GET {id}; fix replace policy (D5, D8)
    └── DependencyInjection.cs                # register Checks.Replace policy

tests/                                        # per-project suites (Domain/Application/Application.Functional/...)
src/Web/ClientApp/src/
├── features/treasury/                        # checks list page + action dialogs (entity folder per frontend layer map)
└── components/                               # feature-scoped components, domain-prefixed
```

**Structure Decision**: Reuse the established Revenue module layout exactly (Command+Handler+Validator per action folder; Queries/Checks mirrors Queries/DepositSlips). No new projects, no new modules.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| (none) | | |
