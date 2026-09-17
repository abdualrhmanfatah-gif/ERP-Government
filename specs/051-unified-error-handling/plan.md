# Implementation Plan: Unified Error Handling

**Branch**: `051-unified-error-handling` | **Date**: 2026-09-14 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/051-unified-error-handling/spec.md`

## Summary

Unify error handling across all layers of ERP-Government per Constitution XIII and DEP-029. Extend `Result<T>` with structured error fields (Code, Category, Message, Target), establish a shared ProblemDetails contract at the Web boundary, normalize all frontend clients into a single error representation, and add Outbox claim recovery with heartbeat-based lease management. Migration follows the incremental sequence in `docs/error-handling.md`: normalize consumers first, introduce shared conversion, then migrate producers.

## Technical Context

**Language/Version**: C# 13 / .NET 10 (backend), TypeScript / React 19 (frontend)

**Primary Dependencies**: ASP.NET Core minimal APIs, EF Core + SQL Server, MediatR, FluentValidation, NSwag (client generation), TanStack Query, React Hook Form + Zod, Zustand

**Storage**: SQL Server (existing schema; Outbox claim recovery may require `LeaseExpiry` column on `OutboxMessages`)

**Testing**: xUnit + FluentAssertions (backend: Domain.UnitTests, Application.UnitTests, Application.FunctionalTests, Infrastructure.IntegrationTests, Web.AcceptanceTests); ESLint + Vite build only (frontend — no automated tests per AGENTS.md override)

**Target Platform**: Web application (SPA + API), Arabic-first RTL

**Project Type**: Web application (backend API + frontend SPA)

**Performance Goals**: Error response latency indistinguishable from success path; Outbox polling interval configurable (existing default); no measurable regression on existing endpoints

**Constraints**: Must preserve all existing success contracts and status codes during migration; no frontend automated tests; DEP-027 exception for Spec 045 unchanged; .NET 10 diagnostic suppression must be validated

**Scale/Scope**: ~15 business modules, ~100+ endpoints, single Outbox processor (current), Arabic-only UI

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Layered Architectural Integrity | ✅ PASS | Application defines error contract; Web converts to HTTP; Domain remains HTTP-free |
| II. Bounded Contexts | ✅ PASS | Error codes use `Module.Cause` format; catalog in Application/Common is shared, not module-crossing |
| III. Server-Side Business-Rule Integrity | ✅ PASS | Structured Result failures for expected rejection; unexpected faults propagate for central translation |
| IV. Financial Integrity | ✅ PASS | Outbox recovery preserves idempotency; duplicate posting prevention enforced at effect boundary |
| V. Budget Control | ✅ PASS | Budget availability error codes included in catalog |
| VI. Data Integrity | ⚠️ CONDITIONAL | Outbox claim recovery may require `LeaseExpiry` column migration; must follow migration-only rule |
| VII. Authorization | ✅ PASS | 401/403 classification preserved; auth state recovery does not weaken fail-closed |
| VIII. Approval Workflows | ✅ PASS | No changes to approval logic; error classification applies to approval rejections |
| IX. API Contract Integrity | ✅ PASS | ProblemDetails is the single error contract; OpenAPI schemas updated; breaking changes tracked |
| X. UI Consistency | ✅ PASS | Error presentation uses shared components; RTL-safe; Arabic messages |
| XI. Testing | ⚠️ CONDITIONAL | TDD mandatory for backend changes; frontend has no automated tests (AGENTS.md override); DEP-027 for Spec 045 unchanged |
| XII. Controlled Architectural Change | ✅ PASS | DEP-029 decision record exists; no unregistered deviations |
| XIII. Error Handling | ✅ PASS | This spec implements Constitution XIII requirements |

**Gate result**: PASS with conditions. Outbox migration must follow VI (migration-only). Testing follows XI with documented frontend override.

## Project Structure

### Documentation (this feature)

```text
specs/051-unified-error-handling/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   ├── api-error-contract.md
│   └── frontend-error-contract.md
└── tasks.md             # Phase 2 output (NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
├── Domain/                           # No changes (HTTP-free)
├── Application/
│   ├── Common/
│   │   ├── Models/Result.cs          # EXTEND: add Code, Category, Message, Target
│   │   ├── Errors/ErrorCodes.cs      # NEW: shared error code catalog
│   │   └── Behaviours/
│   │       ├── ValidationBehaviour.cs        # No change (preserved boundary)
│   │       ├── AuthorizationBehaviour.cs     # Minor: classify as Authorization category
│   │       └── UnhandledExceptionBehaviour.cs # Minor: distinguish expected vs unexpected
│   └── <Module>/Commands/.../        # Migrate to use structured Result failures
├── Infrastructure/
│   ├── Services/OutboxProcessorService.cs    # ADD: heartbeat lease, claim recovery
│   └── Data/                         # MIGRATION: add LeaseExpiry column if needed
├── Web/
│   ├── Infrastructure/
│   │   ├── ProblemDetailsExceptionHandler.cs # EXTEND: full ProblemDetails contract
│   │   └── ApiExceptionOperationTransformer.cs # UPDATE: error schemas in OpenAPI
│   ├── Endpoints/                    # Migrate to return Result (not ad-hoc 400/404)
│   └── ClientApp/src/
│       ├── shared/
│       │   ├── api/
│       │   │   ├── index.ts          # EXTEND: normalize errors, add cancellation
│       │   │   ├── query-error.ts    # REWRITE: unified NormalizedError
│       │   │   └── result-to-ui.ts   # REWRITE: field path canonicalization
│       │   └── utils/
│       │       ├── auth-fetch.ts     # ADD: 401 detection + session recovery
│       │       └── patch-fetch.ts    # ADD: 401 detection
│       └── components/               # ADD: ErrorBoundary component
└── tests/
    ├── Application.UnitTests/        # ADD: Result structured error tests
    ├── Application.FunctionalTests/  # ADD: ProblemDetails contract tests
    └── Web.AcceptanceTests/          # ADD: error response validation
```

**Structure Decision**: Existing layered architecture preserved. Changes are additive (extending existing files) or migratory (updating callers). No new projects or layer violations.

## Complexity Tracking

> No Constitution violations requiring justification. All changes are within existing architectural boundaries.
