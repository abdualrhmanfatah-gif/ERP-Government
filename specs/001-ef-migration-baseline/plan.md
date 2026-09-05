# Implementation Plan: EF Migration Baseline

**Branch**: `001-ef-migration-baseline` | **Date**: 2026-09-02 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-ef-migration-baseline/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command; its definition describes the execution workflow.

## Summary

Create a baseline Entity Framework Core migration that captures the current database schema using a code‑first approach with mandatory reconciliation. The baseline must support SQL Server only, work with all currently supported EF Core versions (6.x, 7.x, 8.x), and integrate with the existing project structure. The migration will be generated from the EF model definition, reconciled against the existing database, and include up/down operations to align differences.

## Technical Context

**Language/Version**: C# (.NET 10.0)

**Primary Dependencies**: ASP.NET Core 10, Entity Framework Core 10, MediatR 14.1, FluentValidation 12.1, AutoMapper 16.1, ASP.NET Identity, Azure Identity, .NET Aspire 13.2

**Storage**: SQL Server (via EF Core Aspire integration)

**Testing**: NUnit 4.5, Moq, Shouldly, Reqnroll.NUnit, Microsoft.Playwright, Microsoft.AspNetCore.Mvc.Testing, Respawn

**Target Platform**: net10.0 (cross‑platform: Linux, Windows, macOS)

**Project Type**: web‑service (Clean Architecture ERP system with Aspire orchestration)

**Performance Goals**: Standard web application performance (no specific targets defined)

**Constraints**: Must adhere to ERP‑Government Constitution (NON‑NEGOTIABLE layered architecture, data integrity, authorization, etc.)

**Scale/Scope**: Enterprise ERP system (multiple bounded contexts, high data volume)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re‑check after Phase 1 design.*

| Principle | Compliance | Notes |
|-----------|------------|-------|
| I. Layered Architectural Integrity | ✅ PASS | Migrations reside in Infrastructure layer; dependencies point inward. |
| II. Bounded Contexts and Event‑Carried Integration | ✅ PASS | Baseline migration is per‑module; no cross‑module writes. |
| III. Server‑Side Business‑Rule Integrity | ✅ N/A | No business rules involved in schema migration. |
| IV. Financial Integrity | ✅ N/A | Schema changes do not affect financial invariants. |
| V. Budget Control Before Expenditure | ✅ N/A | Not applicable. |
| VI. Data Integrity | ✅ PASS | Schema changes will ship as versioned, reviewable migrations. Runtime auto‑creation prohibited. |
| VII. Authorization and Separation of Duties | ✅ N/A | Migration commands are developer‑only; no business endpoints. |
| VIII. Approval Workflows and Audit Immutability | ✅ N/A | Not applicable. |
| IX. API and Frontend Contract Integrity | ✅ N/A | No API changes. |
| X. UI and Design System Consistency | ✅ N/A | No UI changes. |
| XI. Testing, Verification, and Evidence | ✅ PASS | Baseline migration must be validated against database; tests required. |
| XII. Controlled Architectural Change | ✅ PASS | Baseline migration is a one‑time architectural change; decision record may be required if it alters module boundaries. |

**Result**: All applicable gates pass. No violations require justification.

## Project Structure

### Documentation (this feature)

```text
specs/001-ef-migration-baseline/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
├── Domain/
├── Application/
├── Infrastructure/
│   ├── Data/
│   │   ├── Migrations/          # Existing EF migrations
│   │   └── Configurations/      # EF entity configurations
│   └── DependencyInjection.cs
├── Web/
├── AppHost/
├── ServiceDefaults/
└── Shared/

tests/
├── Unit/
├── Integration/
└── Acceptance/
```

**Structure Decision**: The feature will be implemented within the existing Infrastructure layer, specifically in the `Infrastructure/Data/Migrations` directory. The baseline migration will be added alongside existing migrations, following the established pattern.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| (none) | | |