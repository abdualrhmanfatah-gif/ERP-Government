# Implementation Plan: Asset Register CRUD (سجل الأصول)

**Branch**: `055-asset-register-crud` | **Date**: 2026-09-15 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/055-asset-register-crud/spec.md`

## Summary

Implement full CRUD for the Asset Register — the core entity (35 fields) that all other asset features (F3–F9) depend on. Backend follows MediatR CQRS vertical slices; frontend follows the inventory items pattern with React Query, Zod, and reusable form component. Auto-generated AssetCode via existing DocumentSequenceService ("AST" prefix). Deactivation (Disposed status) replaces hard-delete.

## Technical Context

**Language/Version**: C# 13 / .NET 9, TypeScript 5.x / React 18

**Primary Dependencies**: MediatR, FluentValidation, AutoMapper, Entity Framework Core 9, React Query, Zod, react-hook-form

**Storage**: SQL Server via EF Core (existing migrations, `Assets` table with indexes on Status, LocationId, CustodianId, FundId, CostCenterId)

**Testing**: xUnit + FluentAssertions (backend), Vitest (frontend — pending project test runner confirmation)

**Target Platform**: Web application (SPA + REST API)

**Project Type**: Web application (layered: Domain → Application → Infrastructure → Web)

**Performance Goals**: List loads < 2s for 10k assets (SC-003), create in < 3min (SC-001)

**Constraints**: Arabic-first RTL, shared design tokens, no cascade deletes (Restrict on all FKs), optimistic concurrency via RowVersion

**Scale/Scope**: Government asset register — up to 10,000 assets, ~50 concurrent users

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Layered Architecture | ✅ PASS | Commands/Queries in Application, Endpoints in Web, Entity in Domain. No Infrastructure references from Application. |
| II. Bounded Contexts | ✅ PASS | Assets module is self-contained. Cross-module reads via shared persistence abstraction only. |
| III. Server-Side Business Rules | ✅ PASS | Status transition validation, field locking, uniqueness checks all server-side in Handlers/Validators. |
| IV. Financial Integrity | ✅ PASS | F2 is data-only — no journal entries. OriginalValue locking after activation protects future F3/F4. |
| V. Budget Control | N/A | No expenditure in F2. |
| VI. Data Integrity | ✅ PASS | Migrations only, Restrict FKs, RowVersion optimistic concurrency, explicit decimal precision (decimal(23,2)). |
| VII. Authorization | ⚠️ NOTE | `AssetsDelete` permission does not exist in PermissionCodes — deactivation uses `AssetsUpdate`. This is acceptable: soft-delete is an update operation. |
| VIII. Audit Immutability | ✅ PASS | FR-020 logs all create/update/deactivate. BaseAuditableEntity provides Created/CreatedBy/LastModified/LastModifiedBy. |
| IX. API Contract | ✅ PASS | Endpoints return OpenAPI-documented responses. Error responses follow problem-details contract. |
| X. UI/Design System | ✅ PASS | Arabic-first RTL, shared design tokens, shared UI components (DataGrid, FilterBar, Page, etc.). |
| XI. Testing | ⚠️ NOTE | TDD is NON-NEGOTIABLE for new features per Principle XI. All use cases must have tests covering success and failure paths. Test tasks are mandatory. |
| XII. Controlled Change | ✅ PASS | No layer/module boundary changes. New files within existing Assets module structure. |
| XIII. Error Handling | ✅ PASS | Result<T> with ErrorCategory, structured error codes, problem-details API responses. |

**Gate Result**: PASS with notes. No violations requiring justification.

## Project Structure

### Documentation (this feature)

```text
specs/055-asset-register-crud/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── Domain/Assets/Entities/
│   └── Asset.cs                          # EXISTING — no changes
├── Application/Assets/Assets/
│   ├── Commands/
│   │   ├── CreateAsset/
│   │   │   └── CreateAssetCommand.cs
│   │   ├── UpdateAsset/
│   │   │   └── UpdateAssetCommand.cs
│   │   └── DeactivateAsset/
│   │       └── DeactivateAssetCommand.cs
│   ├── Queries/
│   │   ├── GetAssetById/
│   │   │   └── GetAssetByIdQuery.cs
│   │   └── GetAssets/
│   │       └── GetAssetsQuery.cs
│   └── Common/
│       ├── AssetResponse.cs
│       └── MappingExtensions.cs
├── Web/Endpoints/Assets/
│   └── Assets.cs                         # NEW — endpoint group
├── Web/ClientApp/src/features/assets/
│   ├── assets/
│   │   ├── shared/
│   │   │   ├── types.ts
│   │   │   └── schemas.ts
│   │   ├── hooks/
│   │   │   └── useAssets.ts
│   │   ├── components/
│   │   │   └── AssetForm.tsx
│   │   └── pages/
│   │       ├── AssetsListPage.tsx
│   │       ├── AssetCreatePage.tsx
│   │       ├── AssetDetailPage.tsx
│   │       └── AssetEditPage.tsx
│   └── (routes added to app/routes.tsx)
```

**Structure Decision**: Follows the established inventory items pattern exactly. Backend vertical slices under `Application/Assets/Assets/`, frontend feature folder under `features/assets/assets/`.

## Complexity Tracking

> No Constitution violations requiring justification.

No entries.
