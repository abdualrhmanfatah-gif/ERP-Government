# Implementation Plan: Rebuild Budgeting Module Backend

**Branch**: `013-budgeting-backend-rebuild` | **Date**: 2026-09-05 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/013-budgeting-backend-rebuild/spec.md`

## Summary

Tear down and rebuild the entire Budgeting module: 7 domain entities (BudgetType, Fund, BudgetClassification, Budget, BudgetItem, Appropriation, Encumbrance), 8 enums, computed availability engine, derived-value projections, all Application layer commands/queries/DTOs, REST endpoints with permissions, EF Core migration (wipe + recreate), unit + functional tests. All existing Budgeting data wiped. No stored snapshots or derived values — availability computed at transaction time from live data.

## Technical Context

**Language/Version**: C# / .NET 10.0 (SDK 10.0.201)

**Primary Dependencies**: ASP.NET Core 10 (Minimal APIs), Entity Framework Core 10.0.5, MediatR 14.1, FluentValidation 12.1, AutoMapper 16.1

**Storage**: SQL Server via EF Core (decimal(23,2) for money, RowVersion for concurrency)

**Testing**: NUnit 4.5, Moq 4.20, Shouldly 4.3, EF InMemory (unit), real DB via TestBase (functional)

**Target Platform**: Linux server (containerized via .NET Aspire)

**Project Type**: Web service (Clean Architecture: Domain / Application / Infrastructure / Web)

**Performance Goals**: Standard web API — no specific latency targets in spec; computed availability must handle concurrent transactions correctly

**Constraints**: Constitutional Principles III (computed at transaction time), V (no stored snapshots), VI (idempotent seeds), VII (auth audited), VIII (ApprovalHistory source of truth), XI (no stub tests)

**Scale/Scope**: 7 entities, ~40 commands/queries, ~12 REST endpoint groups, 5 seeded roles

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| III — No stored snapshots | PASS | All amounts computed at transaction time from live Appropriation/Encumbrance rows |
| V — No derived data stored | PASS | Level, IsReversed, contextual FKs computed via projections only |
| VI — Idempotent seeds | PASS | Seed data designed for INSERT IF NOT EXISTS |
| VII — Auth audited | PASS | Authorization decisions logged to audit log |
| VIII — ApprovalHistory source | PASS | No ApprovedById/At on entities; ApprovalHistory is truth |
| XI — No stub tests | PASS | All tests against real DB with success + failure paths |

No violations. No complexity tracking needed.

## Project Structure

### Documentation (this feature)

```text
specs/013-budgeting-backend-rebuild/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (OpenAPI spec)
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── Domain/Budgeting/
│   ├── Entities/          # BudgetType, Fund, BudgetClassification, Budget, BudgetItem, Appropriation, Encumbrance
│   └── Enums/             # BudgetControlMethod, BudgetStatus, FundType, FundCategory, AppropriationType, AppropriationStatus, EncumbranceType, EncumbranceStatus
├── Application/Budgeting/
│   ├── Commands/          # Budgets/, BudgetItems/, BudgetTypes/, Funds/, BudgetClassifications/, Appropriations/, Encumbrances/
│   ├── Queries/           # GetXxxById, GetXxxsList, GetBudgetItemsTree, GetBudgetClassificationsTree, GetAvailability
│   └── Common/            # DTOs, AvailabilityService, Validators
├── Infrastructure/Data/
│   ├── Configurations/Budgeting/  # EF entity configurations
│   ├── Seeds/                     # BudgetTypeSeedData, BudgetClassificationSeedData, etc.
│   └── Migrations/                # Single wipe+recreate migration
├── Web/Endpoints/Budgeting/       # Minimal API endpoint groups
tests/
├── Application.UnitTests/Budgeting/       # Unit tests per command
└── Application.FunctionalTests/Budgeting/ # Functional tests with real DB
```

**Structure Decision**: Existing Clean Architecture layout preserved. All budgeting code lives under `Domain/Budgeting/`, `Application/Budgeting/`, `Infrastructure/Data/Configurations/Budgeting/`, `Web/Endpoints/Budgeting/`. No new projects or folders needed.
