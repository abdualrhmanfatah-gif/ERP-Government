# Implementation Plan: Posting Pipeline

**Branch**: `002-posting-pipeline` | **Date**: 2026-09-02 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/002-posting-pipeline/spec.md`

## Summary

Automated accounting posting pipeline that captures domain events, matches them to PostingRules, and generates journal entry shells (Moves). Uses Outbox pattern for reliable delivery, structured logging with correlation IDs, and orphan recovery on startup. MoveLine generation deferred to FEATURE-027.

## Technical Context

**Language/Version**: C# / .NET 10.0 (SDK 10.0.201)

**Primary Dependencies**: MediatR (in-process pub/sub), Entity Framework Core (SQL Server), FluentValidation, AutoMapper, Microsoft.Extensions.Logging

**Storage**: SQL Server (Aspire.Microsoft.EntityFrameworkCore.SqlServer 13.2.0)

**Testing**: NUnit 4.5.1, Shouldly 4.3.0, Moq 4.20.72, Respawn 7.0.0, Reqnroll.NUnit 3.3.4

**Target Platform**: Linux/Docker container (Aspire orchestrated)

**Project Type**: Web service (ASP.NET Core + SPA)

**Performance Goals**: 100 events/minute, <30 second processing latency

**Constraints**: No SaveChangesAsync in event handlers (infinite recursion prevention); Restrict on all foreign keys; optimistic concurrency via RowVersion

**Scale/Scope**: Government ERP, 12 seed PostingRules, 6 event types

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Evidence |
|-----------|--------|----------|
| I. Layered Architectural Integrity | ✅ PASS | Handlers in Application layer, no Infrastructure references from Domain/Application |
| II. Bounded Contexts and Event-Carved Integration | ✅ PASS | AccountingEvent + Outbox pattern for cross-module integration |
| III. Server-Side Business-Rule Integrity | ✅ PASS | All validation in handlers/validators, no business logic in endpoints |
| IV. Financial Integrity | ✅ PASS | Moves start as Draft, posting validates balance via SPEC-001 |
| V. Budget Control | N/A | Out of scope |
| VI. Data Integrity | ✅ PASS | RowVersion on all entities, Restrict FKs, migrations only |
| VII. Authorization | ✅ PASS | PermissionCodes on all commands/queries |
| VIII. Approval Workflows | N/A | Out of scope |
| IX. API Contract Integrity | ✅ PASS | Minimal API endpoints, OpenAPI spec |
| X. UI Consistency | N/A | Backend only |
| XI. Testing | ✅ PASS | Unit tests for handlers, functional tests for pipeline |
| XII. Controlled Change | ✅ PASS | No deviations from Constitution |

**Gate Result**: PASS — no violations

## Project Structure

### Documentation (this feature)

```text
specs/002-posting-pipeline/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── api-contracts.md
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── Domain/Accounting/Entities/
│   ├── AccountingEvent.cs        # Existing — no changes
│   ├── PostingRule.cs            # Existing — add DeletePostingRule validation
│   └── PostingRuleLine.cs        # Existing — no changes
├── Application/Accounting/EventHandlers/
│   ├── DomainEventHandler.cs     # Existing — add structured logging
│   ├── PostingPipelineHandler.cs # Existing — add structured logging + orphan recovery
│   ├── PostingRuleMatcher.cs     # Existing — no changes
│   ├── MoveGenerator.cs          # Existing — add structured logging
│   ├── AccountingEventAuditor.cs # Existing — add orphan recovery method
│   └── RetryPolicy.cs            # Existing — no changes
├── Application/Accounting/Commands/AccountingEvents/
│   ├── ProcessEvent/ProcessAccountingEventCommand.cs  # Existing — no changes
│   └── RetryEvent/RetryAccountingEventCommand.cs      # Existing — no changes
├── Application/Accounting/Commands/PostingRules/
│   ├── DeletePostingRule/DeletePostingRuleCommand.cs  # NEW — with pending event guard
│   └── CreatePostingRule/CreatePostingRuleCommand.cs  # Existing — no changes
├── Infrastructure/Services/
│   └── OutboxProcessorService.cs  # Existing — add orphan recovery on startup
├── Web/Endpoints/Accounting/
│   ├── AccountingEvents.cs       # Existing — no changes
│   └── PostingRules.cs           # Existing — add DELETE endpoint
└── tests/
    ├── Application.UnitTests/EventHandlers/
    │   ├── PostingPipelineHandlerTests.cs   # Existing — add structured logging tests
    │   ├── MoveGeneratorTests.cs            # Existing — no changes
    │   └── AccountingEventAuditorTests.cs   # Existing — add orphan recovery tests
    └── Application.FunctionalTests/Accounting/
        └── PostingPipelineTests.cs          # NEW — end-to-end pipeline test
```

**Structure Decision**: Existing layered architecture (Domain → Application → Infrastructure → Web). All changes follow established patterns. No new projects or layers.

## Complexity Tracking

> No Constitution violations — no complexity tracking needed.

## Phase 0: Research

### Research Tasks

1. **Orphan Recovery Pattern** — How to detect and reset stale Processing events on OutboxProcessorService startup
   - Decision: Query AccountingEvents where Status=Processing AND UpdatedAt < UtcNow.AddMinutes(-5), reset to Pending
   - Rationale: Simple, reliable, no external dependencies
   - Alternatives: Timeout-based (more complex, requires additional polling)

2. **Structured Logging with Correlation IDs** — How to link OutboxMessage → AccountingEvent → Move in logs
   - Decision: Pass correlation ID through handler chain via ILogger structured properties
   - Rationale: Standard .NET logging pattern, compatible with OpenTelemetry in ServiceDefaults
   - Alternatives: Custom correlation context (overkill for in-process pipeline)

3. **DeletePostingRule Guard** — How to prevent deletion when pending events exist
   - Decision: Check for Pending/Processing AccountingEvents referencing the rule before delete
   - Rationale: Database-level integrity, consistent with Constitution Principle VI
   - Alternatives: Soft delete (adds complexity, not required by spec)

### Research Output

See `research.md` for full details.

## Phase 1: Design & Contracts

### Data Model

See `data-model.md` for entity definitions, relationships, and validation rules.

### API Contracts

See `contracts/api-contracts.md` for endpoint specifications.

### Quickstart Validation

See `quickstart.md` for runnable validation scenarios.

## Re-evaluation: Constitution Check (Post-Design)

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Layered Architectural Integrity | ✅ PASS | DeletePostingRule command in Application layer, no Infrastructure references |
| II. Bounded Contexts | ✅ PASS | No cross-module writes |
| III. Server-Side Business Rules | ✅ PASS | Delete guard in handler, not endpoint |
| IV. Financial Integrity | ✅ PASS | No changes to posting logic |
| VI. Data Integrity | ✅ PASS | RowVersion on new command, Restrict FKs |
| VII. Authorization | ✅ PASS | PermissionCodes on new endpoint |
| XI. Testing | ✅ PASS | New tests for orphan recovery, delete guard |

**Gate Result**: PASS — no violations
