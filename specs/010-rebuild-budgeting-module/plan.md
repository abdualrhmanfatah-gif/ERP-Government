# Implementation Plan: Rebuild Budgeting Module from Scratch

**Branch**: `010-rebuild-budgeting-module` | **Date**: 2026-09-04 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/010-rebuild-budgeting-module/spec.md` (including 5 clarification answers in `## Clarifications`)

## Summary

Tear down and rebuild the Budgeting module: 7 tables (BudgetType, Fund, BudgetClassification, Budget, BudgetItem, Appropriation, Encumbrance), 8 enum statuses, zero stored derived/duplicate columns, a transaction-time availability engine, derived-value query projections, full Application/Web/frontend rebuild, and a single wipe-and-recreate EF Core migration. All existing Budgeting rows are wiped; reference data is reseeded idempotently. Clarifications settled: no Transfer type (Adjustment only), no Appropriation reversal operation or Reversed status, whole-BudgetItem net for encumbrance availability, Draft-only Appropriation Update/Delete, signed Adjustment amounts.

## Technical Context

**Language/Version**: C# on .NET 10.0 SDK (`global.json` pins `10.0.201`); nullable reference types enabled; `TreatWarningsAsErrors=true` (`Directory.Build.props`).

**Primary Dependencies**: ASP.NET Core minimal APIs with `IEndpointGroup` auto-discovery (`/api/{ClassName}`, OpenAPI `operationId` derived from handler method name); MediatR (commands/queries + `AuthorizationBehaviour` pipeline); FluentValidation; AutoMapper (`IMapper` + nested `Mapping : Profile` classes); EF Core 10 with SQL Server via Aspire; NSwag TS client generation (`src/Web/ClientApp/nswag.json`, `generate-api` script) from `wwwroot/openapi/v1.json` (`MapOpenApi` + Scalar).

**Storage**: SQL Server, single `ApplicationDbContext`, configuration-per-entity under `src/Infrastructure/Data/Configurations/{Module}/`. Enums stored as `int` columns (model snapshot shows `HasColumnType("int")`; `HasMaxLength` on enum mappings is vestigial). All FKs `Restrict`. `RowVersion` (`rowversion`) concurrency tokens on mutable entities. `BaseAuditableEntity`: `Created`, `CreatedBy`, `LastModified`, `LastModifiedBy`. Migrations in `src/Infrastructure/Migrations`; database migrated at startup in Development (`InitialiseDatabaseAsync` → `MigrateAsync`); seeds guarded by `Any()` checks (idempotent).

**Testing**: Backend NUnit (`[SetUpFixture]`, `TestBase` + Respawn `DatabaseResetter` per test); `tests/Application.UnitTests` (Moq + Shouldly, mocked `IApplicationDbContext`); `tests/Application.FunctionalTests` (Aspire `DistributedApplicationTestingBuilder` + `WebApiFactory` + real SQL Server, `TestApp.SendAsync` mediator access, `RunAsAdministratorAsync`/`RunAsUserAsync`); `tests/Domain.UnitTests`; `tests/Infrastructure.IntegrationTests`; `tests/Web.AcceptanceTests` (Playwright/Reqnroll). Frontend vitest + jsdom + Testing Library (`vitest.config.ts` includes `src/**/*.{test,spec}.{ts,tsx}`); `npm test` runs `vitest run`.

**Target Platform**: ASP.NET Core web server + React 19 SPA (Vite 8, React Router 7, TanStack Query 5, react-hook-form + zod, Tailwind 3.4, `next-themes` dark mode, `rtl.scss` + Arabic labels, design tokens in `src/design-system/` with `generate-tokens` script).

**Project Type**: Full-stack web application (Clean Architecture backend: Domain → Application → Infrastructure → Web; NSwag-generated TS client consumed by feature modules under `src/Web/ClientApp/src/features/`).

**Performance Goals**: No explicit latency/throughput targets in spec (clarify flagged NFR quantification as Outstanding, low impact). Correctness gates substitute: availability nets to 2 decimals, deep-hierarchy Level without overflow, per-test DB reset functional tests.

**Constraints**: Warnings-as-errors; migrations only (no auto-schema); Restrict FKs; RowVersion on all mutable records; enum-as-int storage (no string conversions); ProblemDetails contract (Validation→400, NotFound→404, Unauthorized→401, Forbidden→403 via `ProblemDetailsExceptionHandler`); every endpoint + use case declares a named permission (`RequireAuthorization(PermissionCodes.X)` + `[Authorize(Policy=...)]`, enforced fail-closed by MediatR `AuthorizationBehaviour` with SecurityAuditLog entries); Arabic-first RTL; design tokens only; TanStack Query hooks; NSwag client regen from OpenAPI.

**Scale/Scope**: 7 tables dropped + recreated, 8 enums, ~40 new/rewritten commands + ~12 queries + DTOs, 7 endpoint groups (+ items sub-routes, availability endpoint), `features/budgeting` frontend (7 pages, tree editor, availability indicator, approval panel), 1 migration, idempotent seeds, unit + functional + frontend tests, OpenAPI regen, `docs/database-schema.md` + feature registry updates.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Layered Architecture | ✅ PASS | Domain entities/enums → Application commands/queries/DTOs/availability service → Infrastructure configs/migration/seeds → Web endpoint groups; frontend consumes only the NSwag HTTP contract. |
| II. Bounded Contexts | ✅ PASS | All new types live in Budgeting; cross-module reads (FiscalYear, Account, CostCenter, Supplier, PurchaseOrder) go through shared `IApplicationDbContext`; no writes to other modules' tables; PostingPipeline integration explicitly out of scope. |
| III. Server-Side Rules | ✅ PASS | FSM guards, FluentValidation, RowVersion, transaction-time availability + FY/period gates in handlers; explicit `Result.Failure`; frontend validation is UX only. |
| IV. Financial Integrity | ✅ PASS | No journal entries in scope; nothing posted is mutated (Draft-only Update/Delete; post-submission via Adjustment/Cancel; encumbrance reversal rows); decimal(23,2) explicit precision. |
| V. Budget Control | ✅ PASS | Chain Budget→Appropriation→Encumbrance→Payment; availability checked on appropriation/encumbrance create + approve; None/Warning/Blocking honored; 3-level AllowOverrun chain; Warning path logs + surfaces + records decision. |
| VI. Data Integrity | ✅ PASS w/ justification | Single versioned migration; Restrict everywhere; RowVersion; idempotent seeds. Draft-only Update/Delete are pre-approval working-document operations (justified; post-submission corrections use Adjustment/Cancel, never hard-delete). No denormalized derived data (compliance table in plan per FR-017). |
| VII. Authorization | ✅ PASS | Named permission per endpoint + use case; anonymous denied; fail-closed `AuthorizationBehaviour`; grant/denial audit. Granular lifecycle permissions extend the existing `Budgets.*`-style scheme (see R8). |
| VIII. Audit Immutability | ✅ PASS | Approval decisions persisted to ApprovalHistory with evaluation snapshot at decision time; audit trails INSERT-ONLY; creator identity from `CreatedBy`. |
| IX. API Contract | ⚠️ DECISION RECORD REQUIRED | Removing/reshaping endpoints and payloads is a breaking change per IX → numbered decision record required before implementation (same record as XII). OpenAPI regen + NSwag client regen included. |
| X. UI Consistency | ✅ PASS | RTL logical properties, design tokens only, dark mode, shared UI primitives, permission-gated nav (`navigation.ts`), money/date formatting conventions. |
| XI. Testing | ✅ PASS | Unit per command (success + failure paths), real-DB functional tests with per-test reset, no stubs; frontend vitest coverage for pages/hooks/indicator/panel. |
| XII. Controlled Change | ⚠️ DECISION RECORD REQUIRED | New `DEP-0xx` record required: rationale, scope (≈7 tables/8 enums/≈50 commands+queries/7 pages), migration + wipe/remediation plan, contract-shape breakage (IX), permission additions. |

**Gate result**: PASS with condition — `DEP-0xx` decision record (covering IX + XII) must be accepted before implementation. No NON-NEGOTIABLE violations; VI Draft-only Update/Delete is justified above.

**Post-design re-check (2026-09-04)**: design confirms no new violations. Enum-as-`int` preserves existing storage (IV/VI); FY-status + period-lock gates satisfy III; ApprovalHistory reuse satisfies VIII with no new audit tables; granular lifecycle permissions extend the existing scheme under the already-required IX/XII decision record; wipe migration keeps historical migrations untouched (VI). Gate stands: PASS with the `DEP-0xx` condition.

## Project Structure

### Documentation (this feature)

```text
specs/010-rebuild-budgeting-module/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
│   ├── budgeting-api.md     # REST endpoints, operationIds, permissions, status codes
│   └── budgeting-dtos.md    # DTO shapes incl. computed/projected fields
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
├── Domain/Budgeting/
│   ├── Entities/        # BudgetType, Fund (unchanged), BudgetClassification, Budget, BudgetItem, Appropriation, Encumbrance
│   └── Enums/           # BudgetControlMethod, FundType, FundCategory, BudgetStatus, AppropriationType, AppropriationStatus, EncumbranceType, EncumbranceStatus
├── Application/Budgeting/
│   ├── Commands/        # BudgetTypes/, Funds/, BudgetClassifications/, Budgets/, BudgetItems/, Appropriations/, Encumbrances/
│   ├── Queries/         # ... + availability queries (GetAvailabilityForItem, GetAvailabilityForEncumbrance)
│   ├── Common/          # DTOs + Availability service (IBudgetAvailabilityService)
│   └── ...
├── Application/Common/Security/
│   └── PermissionCodes.cs   # extend lifecycle members per R8
├── Application/FinancialSettings/Common/Services/
│   └── DocumentSequenceService.cs  # PrefixMap: Budget→BGT, Appropriation→APR, Encumbrance→ENC
├── Infrastructure/Data/
│   ├── Configurations/Budgeting/   # 7 rewritten configurations
│   ├── Migrations/                 # single wipe-and-recreate migration
│   └── Seeds/                      # BudgetType/Fund/Classification/DocumentSequence seeds rewritten
├── Web/Endpoints/Budgeting/        # BudgetTypes, Funds, BudgetClassifications, Budgets, BudgetItems*, Appropriations, Encumbrances
└── Web/ClientApp/src/
    ├── features/budgeting/         # pages/, hooks/, components/, types/, __tests__/
    ├── app/routes.tsx              # /budgeting/* routes
    ├── layouts/navigation.ts       # الموازنة module group with permission identifiers
    └── web-api-client.ts           # regenerated via nswag
tests/
├── Domain.UnitTests/Budgeting/
├── Application.UnitTests/Budgeting/
├── Application.FunctionalTests/Budgeting/
└── (ClientApp) src/features/budgeting/__tests__/
docs/
├── database-schema.md
├── final-business-feature-registry.md
└── decision-records/DEP-0xx-rebuild-budgeting-module.md
```

**Structure Decision**: Reuse the established Clean Architecture layout and naming conventions (per-entity command/query folders, `Common` DTOs with nested AutoMapper `Mapping`, `IEndpointGroup` endpoint files, `features/{module}` frontend). No new projects. BudgetItems endpoints live under `/api/Budgets/{id}/items` per FR-013 (implemented as routes in `Budgets.cs` delegating to BudgetItems use cases, or a `BudgetItems` group with `RoutePrefix` override).

## Complexity Tracking

| Item | Why Needed | Simpler Alternative Rejected Because |
|------|-----------|-------------------------------------|
| `DEP-0xx` decision record before implementation | Constitution IX (breaking contract change) + XII (principles/module/contract/security change) | Proceeding without a record would be an unregistered deviation = defect |
| Draft-only Appropriation Update/Delete commands | Clarification Q4 requires edit/delete capability constrained to Draft | Post-submission mutation would violate VI (use Adjustment/Cancel instead) |
| Granular lifecycle PermissionCodes beyond the current set | VII requires a named permission per use case; SC-008 requires distinguishable 403s; Budgets.* precedent is per-transition | Coarse mapping (e.g., everything to `*.Approve`) would weaken least-privilege and lifecycle button gating |
| Application-side Level computation (no recursive SQL) | Provider-portable, unit-testable, reuses existing lookup+build-tree query pattern; cycle-guarded | Recursive CTEs are provider-specific and harder to unit test; stored Level is forbidden |
