# Implementation Plan: Accounting Core Refactor — JournalEntry Rename, Analytic Dimensions, PaymentOrder Aggregate Strip

**Branch**: `014-accounting-core-refactor` | **Date**: 2026-09-05 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/014-accounting-core-refactor/spec.md`

## Summary

Rename Move → JournalEntry and MoveLine → JournalEntryLine across the entire solution, add five analytic dimension FKs to JournalEntryLine, convert EntryStatus from string to enum, extend AccountingEvent with JournalEntryId and structured enums, strip all stored aggregates and inline approval columns from the Payments domain, and add InKind to PaymentMethod. Executes after Command 1 (ApprovalHistory + Party exist).

## Technical Context

**Language/Version**: C# 13 / .NET 10 (SDK 10.0.201)

**Primary Dependencies**: Entity Framework Core 10.0.5, FluentValidation 12.1.1, MediatR 14.1.0, ASP.NET Core 10, Aspire 13.2.0

**Storage**: SQL Server via Aspire (EF Core migrations)

**Testing**: NUnit 4.5.1, Shouldly 4.3.0, Moq 4.20.72, Respawn 7.0.0, Reqnroll 3.3.4, Playwright 1.58.0

**Target Platform**: ASP.NET Core Web API (server-side)

**Project Type**: Web service (Clean Architecture: Domain / Application / Infrastructure / Web)

**Performance Goals**: No explicit targets — standard web-app latency expectations apply

**Constraints**: Single-entity deployment (NO EntityId), zero stored computed fields (Payments), ApprovalHistory only for approvals, FluentValidation + MediatR, EF migrations for all schema changes

**Scale/Scope**: Government ERP — medium-scale departmental use, not high-traffic public API

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

No constitution file exists (`.specify/memory/constitution.md` not found). Proceeding without constitution gates.

**Post-Phase 1 Re-check**: Design artifacts align with all spec constraints (single-entity, zero stored computed fields, ApprovalHistory-only approvals, reversal pattern).

## Project Structure

### Documentation (this feature)

```text
specs/014-accounting-core-refactor/
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
├── Domain/
│   ├── Accounting/
│   │   ├── Entities/
│   │   │   ├── JournalEntry.cs          # Renamed from Move.cs
│   │   │   ├── JournalEntryLine.cs      # Renamed from MoveLine.cs
│   │   │   └── AccountingEvent.cs       # Extended
│   │   └── Enums/
│   │       ├── EntryStatus.cs           # New enum (replaces string)
│   │       ├── EventCategory.cs         # New enum
│   │       ├── EventType.cs             # New enum
│   │       └── EventStatus.cs           # Renamed from AccountingEventStatus
│   └── Payments/
│       ├── Entities/
│       │   ├── PaymentOrder.cs          # Stripped of aggregates + approvals
│       │   ├── PaymentOrderLine.cs      # Stripped of computed fields
│       │   └── PaymentOrderDeduction.cs # Stripped of BaseAmount
│       └── Enums/
│           └── PaymentMethod.cs         # Added InKind
├── Infrastructure/
│   └── Data/
│       ├── Configurations/
│       │   ├── Accounting/
│       │   │   ├── JournalEntryConfiguration.cs    # Renamed from MoveConfiguration.cs
│       │   │   ├── JournalEntryLineConfiguration.cs # Renamed from MoveLineConfiguration.cs
│       │   │   └── AccountingEventConfiguration.cs # Extended
│       │   └── Payments/
│       │       ├── PaymentOrderConfiguration.cs     # Updated
│       │       ├── PaymentOrderLineConfiguration.cs # Updated
│       │       └── PaymentOrderDeductionConfiguration.cs # Updated
│       └── Migrations/
│           └── [timestamp]_AccountingCoreRefactor.cs # EF migration
├── Application/
│   └── [Handlers, Services, DTOs referencing renamed entities]
└── Web/
    └── [Controllers referencing renamed routes]
```

**Structure Decision**: Single-project Clean Architecture (existing layout). No structural changes — only file renames and entity modifications within existing layer boundaries.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | N/A |
