# Implementation Plan: Budgeting Backend Completion

**Branch**: `015-budgeting-backend-completion` | **Date**: 2026-09-05 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/015-budgeting-backend-completion/spec.md`

## Summary

Complete the budgeting rebuild left ~58% done by spec 013: (1) fix the critical sequence bug — wire BGT/APR/ENC number generation into budget/appropriation/encumbrance creation and make allocation atomic with typed failures; (2) deliver the full Encumbrance stack (queries, 13 commands incl. availability-gated create + reversal, endpoint group, permissions); (3) model Transfer as an atomic paired Appropriation (new `TargetBudgetItemId` FK, joint lifecycle, hard source-coverage validation); (4) expose the availability engine via `GET /api/BudgetItems/{id}/availability` and `GET /api/Appropriations/{id}/availability` incl. resolved EffectiveAllowOverrun; (5) wire Budget head fields (TotalAmount/AllowOverrun/EffectiveFrom/EffectiveTo/Description) into create/update and remove the stored `IsActive` flag; (6) add informational BudgetItemMonthlyPlans with GET/PUT batch-12 endpoints; (7) one migration fixing all schema drift (drops, adds, lengths, stale Suppliers FK); (8) frontend cleanup (regenerated NSwag client, wrapper alignment, 'Liquidation' option removal) and docs (database-schema budgeting section, feature map, registry, mark 013 superseded). TDD throughout (Constitution XI).

## Technical Context

**Language/Version**: C# / .NET 10.0 (`net10.0`), EF Core 10.0.5

**Primary Dependencies**: MediatR 14.1.0, FluentValidation 12.1.1, EF Core (SQL Server), minimal APIs (`IEndpointGroup` auto-discovery), NSwag 14.6.3 (TS client gen), Aspire (AppHost + SQL Server)

**Storage**: SQL Server (versioned migrations in `src/Infrastructure/Migrations/`; current: `20260904031800_InitialCreate` + snapshot)

**Testing**: NUnit (unit: mocked `IApplicationDbContext`/`IUser`; functional: `TestBase` → `TestApp.ResetState()` Respawn reset over Aspire test host). TDD mandatory — every behavior observed failing first (Constitution XI).

**Target Platform**: ASP.NET Core minimal API (`/api/{Group}`) + React/TS ClientApp (Arabic-first RTL)

**Performance Goals**: standard web expectations; availability computed per-request from current data (no caching of aggregates)

**Constraints**: warnings-as-errors; nullable enabled; Result<T> error contract; enums-as-int; decimal(23,2); Restrict FKs only; ApprovalHistory-only approvals; zero stored computed fields

**Scale/Scope**: 6 user stories; ~25 new/changed Application files, 1 endpoint group + 2 availability routes + monthly-plan routes, 1 migration, frontend cleanup, docs

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Check | Verdict |
|-----------|-------|---------|
| I. Layered integrity | New commands/queries in Application; endpoints thin (no business rules); Encumbrance DTO computation (IsReversed) in query layer, not endpoint | PASS |
| II. Bounded contexts | Sequence service consumed via existing Application-layer service (FinancialSettings module service accessed through Application — same solution, service owned by Application) | PASS |
| III. Server-side rules | Lifecycle guards (Draft-only update/delete, reversal from Active/Suspended only), availability gate at create, typed failures for sequence errors | PASS |
| IV. Financial integrity | Transfer pair net zero; reversal-only correction; posted rows never mutated; RowVersion verified on every update | PASS |
| V. Budget control | Availability gate at encumbrance create honoring None/Warning/Blocking; Warning overrides recorded in ApprovalHistory Reason | PASS |
| VI. Data integrity | ONE versioned migration; no cascades (new FK Restrict); RowVersion everywhere; precision explicit decimal(23,2); seeds unchanged (idempotent) | PASS |
| VII. Authorization | Every new endpoint `.RequireAuthorization(permission)`; every use case `[Authorize(Policy=...)]`; fail-closed policies; new permission codes added (EncumbrancesUpdate/Delete missing today) | PASS |
| VIII. Approval history | ApprovalHistory row on every encumbrance transition (current shape, no pre-implementation of the Action-enum refactor) | PASS |
| IX. API contract | NSwag client regenerated in normal build (`prestart`/`prebuild` → `nswag run`); wrappers consume generated client; problem-details status semantics preserved; RowVersion round-trips on mutations | PASS |
| X. UI/design | No new screens; only wrapper cleanup + one dropdown option removal — token/design rules untouched | PASS (N/A scope) |
| XI. Testing/TDD | Test-first for all behaviors: encumbrance stack, sequence concurrency, transfer pair, availability endpoints, authorization, appropriation edge gaps. Functional tests hit real DB with Respawn reset | PASS |
| XII. Controlled change | Contract additions are additive (no breaking reshape); spec 013 superseded explicitly; no layer/boundary changes | PASS |

**Registered exceptions referenced**: exception #1 (open endpoint policy placeholders) remains the standing deviation — endpoint policies stay stubbed pending RBAC wiring; use-case authorization remains the effective control (unchanged by this feature). Exception #5 (stubbed functional tests) — this feature ADDS real assertions, does not touch existing stubs.

**Post-design re-check**: after Phase 1 — no violations introduced; see "Post-Design Constitution Re-Check" below.

## Project Structure

### Documentation (this feature)

```text
specs/015-budgeting-backend-completion/
├── plan.md              # This file
├── research.md          # Phase 0 output — decisions & rationale
├── data-model.md        # Phase 1 output — entities, FSM, migration change list
├── contracts/
│   └── api-contracts.md # Phase 1 output — HTTP contract for new endpoints
├── quickstart.md        # Phase 1 output — end-to-end validation guide
└── tasks.md             # Phase 2 output (/speckit.tasks) — NOT created here
```

### Source Code (repository root)

```text
src/
├── Domain/
│   ├── Budgeting/
│   │   ├── Entities/            # Encumbrance (existing), Appropriation (+TargetBudgetItemId), Budget (−IsActive), BudgetItem (−OriginalAmount), NEW BudgetItemMonthlyPlan
│   │   └── Enums/               # EncumbranceStatus (+Suspended)
│   └── Security/Entities/       # ApprovalHistory (unchanged shape)
├── Application/
│   ├── Budgeting/
│   │   ├── Commands/
│   │   │   ├── Budgets/         # create/update accept head fields; Activate stops IsActive
│   │   │   ├── Appropriations/  # CreateTransferAppropriationCommand (pair), joint-lifecycle edits
│   │   │   └── Encumbrances/    # NEW: 2 queries + 13 commands (file-per-command, handler+validator co-located)
│   │   ├── Queries/
│   │   │   ├── BudgetItems/     # GetBudgetItemAvailabilityQuery
│   │   │   ├── Appropriations/  # GetAppropriationAvailabilityQuery
│   │   │   └── Encumbrances/    # list + by-id
│   │   └── Common/              # BudgetAvailabilityService (+summary method incl. effective AllowOverrun)
│   ├── FinancialSettings/Common/Services/  # DocumentSequenceService: atomic OUTPUT allocation + typed exceptions
│   └── Common/Security/PermissionCodes.cs  # + EncumbrancesUpdate/Delete
├── Infrastructure/
│   ├── Data/Configurations/Budgeting/      # MonthlyPlan config; FK/index fixes
│   └── Migrations/                          # NEW migration (drops/adds/lengths/FK drop)
├── Web/
│   ├── Endpoints/Budgeting/
│   │   ├── Encumbrances.cs       # NEW group /api/Encumbrances
│   │   ├── BudgetItems.cs        # NEW group /api/BudgetItems (availability + monthly-plan)
│   │   ├── Appropriations.cs     # + POST transfers, GET /{id}/availability
│   │   └── Budgets.cs            # head fields in request DTOs
│   ├── DependencyInjection.cs    # register new permission policies
│   └── ClientApp/src/
│       ├── features/budgeting/hooks/useEncumbrances.ts      # align to real endpoints
│       ├── features/budgeting/shared/client.ts              # wrappers → generated client
│       ├── features/financial/document-sequences/components/DocumentSequenceForm.tsx  # remove 'Liquidation'
│       └── web-api-client.ts                                # regenerated (nswag run)
tests/
├── Application.UnitTests/Budgeting/   # encumbrance command/query/validator/gate, transfer, budget head
└── Application.FunctionalTests/Budgeting/  # sequence concurrency, availability endpoints, authorization, lifecycle
docs/
├── database-schema.md                 # EXTEND (exists) with budgeting section
├── feature-architecture-map-v1.0.md   # FM-004 → REMOVED
└── final-business-feature-registry.md # BF-002/BF-003 notes
specs/013-budgeting-backend-rebuild/spec.md  # status line → superseded by 015
```

**Structure Decision**: existing layered layout (Domain → Application → Infrastructure → Web) with module folders Budgeting/FinancialSettings; no new projects. BudgetItems CRUD stays nested under `/api/Budgets`; a NEW `/api/BudgetItems` group hosts only availability + monthly-plan sub-resources (spec-mandated routes; see research.md D1).

## Complexity Tracking

> No Constitution violations to justify.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|

## Post-Design Constitution Re-Check

Re-evaluated after Phase 1 artifacts (research.md, data-model.md, contracts/api-contracts.md):

- All new endpoints + use cases declare named permissions (VII) — including two permission codes that must be ADDED (EncumbrancesUpdate/Delete).
- Transfer pair and reversal restore availability computationally; no stored aggregates added (V/VI) — BudgetItemMonthlyPlans.PlannedAmount is ENTERED, not computed.
- Contract additions only; NSwag regen path already in the standard build (IX).
- TDD task obligations will be enforced at /speckit.tasks (XI) — test tasks non-optional.
- **Result: PASS — no violations, no complexity entries.**
