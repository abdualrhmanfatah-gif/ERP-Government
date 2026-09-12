# Implementation Plan: Recurring Entries (ACC-04)

**Branch**: `029-recurring-entries` | **Date**: 2026-09-07 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/029-recurring-entries/spec.md`

## Summary

The recurring entries feature already has a significant partial implementation — entity, enums, commands, queries, endpoints, EF configuration, and background job processor all exist. This plan addresses the **gaps** between the current code and the spec: adding `Cancelled` status to the enum, integrating `IDocumentSequenceService` for entry numbering, adding document status logging for lifecycle transitions, enforcing endDate >= startDate validation, server-side fiscal period closure checks during creation, and ensuring the template amount fallback logic works correctly.

## Technical Context

**Language/Version**: C# 13 / .NET 10, TypeScript 5.9 (frontend)

**Primary Dependencies**: EF Core, MediatR, FluentValidation, NSwag, Vite + React 19

**Storage**: SQL Server via EF Core

**Testing**: xUnit, FluentAssertions, Testcontainers (IntegrationTests), Vitest (frontend)

**Target Platform**: Web application (.NET Aspire orchestrator)

**Project Type**: Web application (backend + SPA frontend)

**Performance Goals**: Standard ERP — no special performance targets for schedule management

**Constraints**: Arabic-first RTL UI; decimal(23,2) for money; all FKs Restrict; no cascading deletes

**Scale/Scope**: Government ERP — hundreds of schedules, not thousands per day

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Layered Architecture | ✅ | Handlers in Application, entities in Domain, endpoints in Web — pattern followed |
| II. Bounded Contexts | ✅ | All within Accounting module; processor in Infrastructure |
| III. Server-Side Business Rules | ⚠️ | Lifecycle guards exist but missing: (1) endDate >= startDate on create, (2) fiscal period closure check on create |
| IV. Financial Integrity | ⚠️ | Balance check exists in processor; entry numbering needs DocumentSequenceService integration |
| V. Budget Control | N/A | Recurring entries don't directly interact with budget control chain |
| VI. Data Integrity | ✅ | RowVersion present, migration-based, Restrict FKs |
| VII. Authorization | ⚠️ | PermissionCodes defined and used; dev-mode open policies (known exception DEP-020) |
| VIII. Approval & Audit | ⚠️ | Status logging not wired in lifecycle commands (Pause/Resume/Cancel) |
| IX. API Contract | ✅ | Endpoints use OpenAPI, typed clients via NSwag |
| X. UI Consistency | N/A | Plan phase; frontend not yet built |
| XI. Testing | ⚠️ | Existing tests need expansion; new gaps must be test-covered |
| XII. Architectural Change | ✅ | No new modules or layer violations |

**Pre-Phase-0 Gate**: CONDITIONAL PASS — violations tracked below, to be resolved in implementation.

### Violations / Gaps to Resolve (Pre-Phase-0)

| Gap | Spec Req | Resolution |
|-----|----------|------------|
| Cancelled status missing from enum | FR-016, OQ1 | Add `Cancelled = 3` to `RecurringEntryStatus` |
| Cancel sets `Completed` instead of `Cancelled` | FR-016 | Update `CancelRecurringEntryCommand` to use `Cancelled` |
| Entry number uses timestamp, not DocumentSequenceService | FR-008 | Integrate `IDocumentSequenceService` in `CreateRecurringEntryCommand` |
| No endDate >= startDate validation | FR-009 | Add FluentValidation rule in `CreateRecurringEntryCommandValidator` |
| No audit trail for lifecycle transitions | FR-007 | Inject `IDocumentStatusLogger` in Pause/Resume/Cancel handlers |
| No fiscal period closure check on schedule creation | FR-010 | ~~Check fiscal period open for StartDate in create handler~~ **Skipped** — processor handles at execution time (research R3) |
| No amount validation (schedule + template null) | FR-017 | Add validation rule: if TemplateId is null, Amount must be present |
| Resume command has no RowVersion in body | Concurrency | Already present in ResumeRecurringEntryCommand (verified) |

### Post-Design Constitution Re-Check

| Principle | Pre-Design | Post-Design | Notes |
|-----------|------------|-------------|-------|
| III. Server-Side Rules | ⚠️ | ✅ | endDate validation + amount validation added to plan; fiscal period check verified redundant |
| IV. Financial Integrity | ⚠️ | ✅ | DocumentSequenceService integration resolves numbering concern |
| VIII. Approval & Audit | ⚠️ | ✅ | IDocumentStatusLogger wired into all lifecycle handlers |
| XI. Testing | ⚠️ | ✅ | Test tasks included in plan scope |

**Post-Design Gate**: PASS — all gaps have concrete resolutions in the plan.

## Project Structure

### Documentation (this feature)

```text
specs/029-recurring-entries/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output (NOT created by /speckit.plan)
```

### Source Code (existing + changes)

```text
src/Domain/Accounting/
├── Entities/
│   ├── RecurringEntry.cs                    # MODIFIED: no schema changes needed
│   └── RecurringEntryExecutionLog.cs        # Existing, no changes
└── Enums/
    └── RecurringEntryStatus.cs              # MODIFIED: add Cancelled = 3

src/Application/Accounting/
├── Commands/RecurringEntries/
│   ├── CreateRecurringEntry/
│   │   └── CreateRecurringEntryCommand.cs   # MODIFIED: numbering + validation + fiscal check
│   ├── PauseRecurringEntry/
│   │   └── PauseRecurringEntryCommand.cs    # MODIFIED: add status logging
│   ├── ResumeRecurringEntry/
│   │   └── ResumeRecurringEntryCommand.cs   # MODIFIED: add status logging
│   └── CancelRecurringEntry/
│       └── CancelRecurringEntryCommand.cs   # MODIFIED: use Cancelled + status logging
├── Queries/RecurringEntries/                # Existing, no changes needed
└── Common/
    └── AccountingDtos.cs                    # Existing, no changes (Status maps via ToString)

src/Infrastructure/
├── Data/Configurations/Accounting/
│   └── RecurringEntryConfiguration.cs       # Existing, no schema changes
└── Services/
    └── RecurringEntryProcessor.cs           # MODIFIED: use Cancelled status in queries

src/Web/Endpoints/Accounting/
└── RecurringEntries.cs                      # Existing, no changes

tests/
├── Application.UnitTests/                   # NEW: unit tests for validation + lifecycle guards
└── Application.FunctionalTests/             # MODIFIED: expand recurring entry tests
```

**Structure Decision**: Existing codebase structure — all changes within established Accounting module boundaries.

## Complexity Tracking

> No Constitution violations requiring decision records. All gaps are within-module fixes following established patterns.

## Research Findings

See [research.md](./research.md) for detailed analysis of:
- DocumentSequenceService integration approach
- IDocumentStatusLogger usage patterns across lifecycle handlers
- Fiscal period check patterns from existing handlers
- Amount validation patterns from RecurringEntryProcessor
