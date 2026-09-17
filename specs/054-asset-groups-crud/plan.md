# Implementation Plan: Asset Groups CRUD

**Branch**: `054-asset-groups-crud` | **Date**: 2026-09-14 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/054-asset-groups-crud/spec.md`

## Summary

Implement full vertical slice (CQRS → API → React UI) for Asset Groups management with hierarchical tree display, GL account linking, and depreciation parameters. The `AssetGroup` domain entity, EF configuration, and seed data already exist. Missing: Application layer (Commands/Queries), Web Endpoints, React feature folder, and routes.

## Technical Context

**Language/Version**: C# 13 / .NET 9, TypeScript 5.x / React 19

**Primary Dependencies**: MediatR (CQRS), FluentValidation, EF Core 9, ASP.NET Core Minimal APIs, React Query (TanStack), react-hook-form + zod, Arabic RTL design system

**Storage**: SQL Server via EF Core (existing `AssetGroups` table with 3 seeded rows)

**Testing**: xUnit + FluentAssertions (backend), Vitest (frontend)

**Target Platform**: Web application (Blazor Server-like SPA backend + React SPA frontend)

**Project Type**: Web application (backend + frontend)

**Performance Goals**: Tree view loads ≤100 groups in <1s; search results in <500ms

**Constraints**: Arabic-first RTL; layered architecture (Domain → Application → Infrastructure → Web); no business logic in endpoints

**Scale/Scope**: ~100 asset groups max; single-tenant government ERP

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Layered Architecture | ✅ PASS | Vertical slice pattern: Domain → Application → Infrastructure → Web. Endpoints delegate to use cases. |
| II. Bounded Contexts | ✅ PASS | Assets module owns its entities and use cases. No cross-module writes. |
| III. Server-Side Rules | ✅ PASS | All validation in use cases/handlers. Cycle prevention, depth limit, duplicate code check server-side. |
| IV. Financial Integrity | ✅ N/A | No journal entries in this feature. GL accounts are linked by ID only. |
| V. Budget Control | ✅ N/A | No budget operations. |
| VI. Data Integrity | ✅ PASS | Restrict FK on ParentAssetGroupId (existing). Optimistic concurrency via RowVersion. Idempotent seed. |
| VII. Authorization | ⚠️ GAP | `AssetGroups.Deactivate` and `AssetGroups.Activate` permissions not in PermissionCodes.cs. Must add. |
| VIII. Audit | ✅ PASS | BaseAuditableEntity provides Created/CreatedBy/LastModified/LastModifiedBy. |
| IX. API Contract | ✅ PASS | OpenAPI contract. Frontend uses generated client or manual api.* calls mirroring contract. |
| X. UI Consistency | ✅ PASS | Arabic-first RTL. Design tokens from shared system. Reuse shared UI components. |
| XI. Testing | ✅ PASS | TDD required. Unit tests for handlers, functional tests for API, frontend tests. |
| XII. Controlled Change | ✅ PASS | No architectural deviations. New module files only. |
| XIII. Error Handling | ✅ PASS | Structured result failures with error codes and Arabic messages. Problem-details contract. |

**Gate Result**: PASS with one remediation required — add `AssetGroups.Deactivate` and `AssetGroups.Activate` permission constants.

## Project Structure

### Documentation (this feature)

```text
specs/054-asset-groups-crud/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks - NOT created here)
```

### Source Code (repository root)

```text
src/
├── Domain/Assets/Entities/
│   └── AssetGroup.cs                    # EXISTS — no changes needed
├── Application/Assets/
│   └── AssetGroups/
│       ├── Common/
│       │   └── MappingExtensions.cs     # NEW — response records + mapping
│       ├── Commands/
│       │   ├── CreateAssetGroup/
│       │   │   └── CreateAssetGroupCommand.cs
│       │   ├── UpdateAssetGroup/
│       │   │   └── UpdateAssetGroupCommand.cs
│       │   └── ToggleAssetGroupActive/
│       │       └── ToggleAssetGroupActiveCommand.cs
│       └── Queries/
│           ├── GetAssetGroups/
│           │   └── GetAssetGroupsQuery.cs
│           ├── GetAssetGroupById/
│           │   └── GetAssetGroupByIdQuery.cs
│           └── GetAssetGroupDetail/
│               └── GetAssetGroupDetailQuery.cs
├── Web/Endpoints/Assets/
│   └── AssetGroups.cs                   # NEW — endpoint group
├── Web/ClientApp/src/features/assets/
│   └── asset-groups/
│       ├── types.ts
│       ├── hooks/
│       │   ├── useAssetGroupsList.ts
│       │   ├── useAssetGroupDetail.ts
│       │   ├── useCreateAssetGroup.ts
│       │   ├── useUpdateAssetGroup.ts
│       │   └── useToggleAssetGroupActive.ts
│       ├── pages/
│       │   ├── AssetGroupsListPage.tsx
│       │   ├── AssetGroupCreatePage.tsx
│       │   └── AssetGroupDetailPage.tsx
│       └── shared/
│           └── schemas.ts
└── Web/ClientApp/src/app/routes.tsx     # MODIFY — add asset group routes

tests/
├── Unit/Assets/
│   └── AssetGroups/                     # NEW — handler unit tests
└── Functional/Assets/
    └── AssetGroups/                     # NEW — API functional tests
```

**Structure Decision**: Follows existing vertical slice pattern (Inventory Items as reference). Backend: `Application/Assets/AssetGroups/` with Commands/Queries subfolders. Web: `Endpoints/Assets/AssetGroups.cs`. Frontend: `features/assets/asset-groups/` with hooks/pages/shared subfolders.

## Complexity Tracking

No violations to justify. All principles pass.

## Research Decisions

See `research.md` for detailed findings.

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Permission model | Add `AssetGroups.Deactivate` + `AssetGroups.Activate` | Constitution VII requires declared permissions on every endpoint |
| Entity field mapping | Use existing entity fields (AccountAssetId, etc.) | Entity already exists; spec GL names map to existing FK fields |
| Parent selection | Dropdown of all active groups (filtered) | Simpler than tree picker for v1; consistent with Account Groups pattern |
| Code generation | Manual entry with uniqueness check | Spec clarified; seed uses semantic codes |
| Hierarchy depth | Enforce 3-level max in Create/Update handlers | Spec clarified; validated server-side |
