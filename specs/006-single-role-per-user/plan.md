# Implementation Plan: Single Role Per User (Option A)

**Branch**: `006-single-role-per-user` | **Date**: 2026-09-02 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/006-single-role-per-user/spec.md`

## Summary

Replace the `UserRoles` M:N join table with a single `RoleId INT NOT NULL` FK on `Users`, allowing each user exactly one active role. Effective permissions become `(RolePermissions of Users.RoleId) UNION (UserPermissions grants) MINUS (UserPermissions revokes)`, computed server-side at transaction time. Provide a single-screen UI (role Select + inherited read-only grid + overrides grid) replacing the collection-based role endpoints. Two-phase migration: (1) ADD NULL RoleId, (2) precise mapping via manual SQL for CONFLICT_MULTI/ORPHAN_ZERO, (3) ALTER NOT NULL. Fail closed: NULL RoleId users denied access; revoke/grant validated strictly (revoke must be inherited, grant must not be); override removal soft-deletes via IsActive=false.

## Technical Context

**Language/Version**: .NET 10 / C# 12 (backend), TypeScript 5.9 (frontend)

**Primary Dependencies**: ASP.NET Core Minimal APIs, EF Core 10, FluentValidation, MediatR (Application layer), React 19.1 / Vite 8 / TanStack Query+Table / NSwag (frontend)

**Storage**: SQL Server via EF Core, versioned migrations only (Constitution VI)

**Testing**: xUnit (`tests/Application.UnitTests`, `tests/Application.FunctionalTests`, `tests/Web.AcceptanceTests`); frontend Vitest 4 installed but **no `test` script configured** — NEEDS CLARIFICATION in research

**Target Platform**: Linux-capable server (Web), modern browsers (SPA)

**Project Type**: Web application (backend layered API + React SPA)

**Performance Goals**: SetUserRole happy-path < 500ms p95 (SC-004); effective permission calc at transaction time (Constitution III)

**Constraints**: RowVersion concurrency (409) on Users & UserPermissions; Restrict FK everywhere; Arabic-first RTL + dark mode (Constitution X); OpenAPI regenerated via NSwag (Constitution IX)

**Scale/Scope**: ~7 dependency locations in Application/Web + ~12 frontend files; 59 UserRoles references removed; 1 entity removed, 1 entity modified (User), 1 endpoint added, 3 endpoints removed

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Check | Notes |
|-----------|-------|-------|
| I. Layered Integrity | PASS | Model change confined to Domain/Security + Application use cases; endpoints delegate to use cases |
| III. Server-Side Rules | PASS | Effective calc server-side only (FR-002), transaction-time evaluation, fail closed (FR-012) |
| VI. Data Integrity | PASS | Restrict FKs (FR-010), RowVersion (FR-011), no hard delete (FR-008), versioned migration |
| VII. Authorization & SoD | PASS | Named permission per endpoint/use case (FR-012), fail closed, override reason for SoD (Story 4) |
| VIII. Audit Immutability | PASS | INSERT-ONLY AuditTrail + SecurityAuditLog for every change (FR-008) |
| IX. API Contract | PASS | OpenAPI regen, uniform problem-details, RowVersion round-trip, remove 3 add 1 endpoint (FR-007) |
| X. UI Design System | PASS | RTL tokens, dark mode, permission-gated nav, shared Select/DataGrid components |
| XI. Testing | PASS | Unit tests for effective calc + master data; functional tests against real DB for concurrency/audit |
| XII. Controlled Change | PASS | Schema + contract change is the feature's ratified scope; no layer/module boundary change |

**Registered exceptions relevant**: #4 (frontend permission stub — must bind overrides UI permission gating to real constants once server permission endpoints exist; remediation is this feature's PermissionService work but the frontend `usePermission` stub remains tracked debt).

## Project Structure

### Documentation (this feature)

```text
specs/006-single-role-per-user/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (repository root)

```text
# Backend
src/Domain/Security/Entities/
├── User.cs                  # MODIFY: add RoleId (int? during migration, NOT NULL after)
├── UserRole.cs              # DELETE: M:N join entity
└── UserPermission.cs        # (exists) unchanged fields

src/Application/Security/
├── PermissionService.cs     # MODIFY: use Users.RoleId + UserPermissions (grants MINUS revokes)
├── Commands/Users/
│   ├── SetUserRoleCommand.cs      # ADD: PUT /users/{id}/role, RowVersion, IsActive check
│   ├── AssignRoleCommand.cs       # DELETE
│   ├── AssignRolePeriodCommand.cs # DELETE
│   ├── RemoveRoleCommand.cs       # DELETE
│   ├── CreateUserCommand.cs       # MODIFY: require RoleId
│   ├── DeactivateUserCommand.cs   # MODIFY: admin check via Users.RoleId
│   └── UpdateUserCommand.cs       # MODIFY: no UserRoles
├── Commands/UserPermissions/
│   ├── AssignUserPermissionCommand.cs # MODIFY: strict revoke-inherited / grant-not-inherited validation, soft-delete
│   └── RemoveUserPermissionCommand.cs # MODIFY: soft-delete (IsActive=false)
├── Commands/Roles/DeleteRoleCommand.cs  # MODIFY: Restrict check via Users.RoleId
└── Queries/Users/
    ├── GetUserByIdQuery.cs     # MODIFY: single RoleId + effective perms
    └── GetUserRolesQuery.cs    # DELETE (replaced by GetUserById)
    └── GetUserPermissionsQuery.cs # MODIFY: return grants+revokes for overrides grid

src/Infrastructure/Data/
├── ApplicationDbContext.cs                    # MODIFY: remove UserRoles DbSet, add User.RoleId nav
├── Configurations/Security/UserRoleConfiguration.cs   # DELETE
└── Configurations/Security/UserConfiguration.cs       # MODIFY: RoleId FK Restrict (NULL→NOT NULL migration)
└── Migrations/                                # ADD: two-phase migration

src/Web/Endpoints/Security/Users.cs            # MODIFY: remove GET/POST/DELETE roles; add PUT {id}/role; keep permissions endpoints

# Frontend
src/Web/ClientApp/src/features/security/users/
├── types.ts               # MODIFY: UserDto single RoleId, add SetUserRoleCommand, drop UserRoleDto collection role
├── client.ts              # MODIFY: usersRolesClient → setUserRoleClient(PUT /api/Users/{id}/role); keep usersPermissionsClient
├── hooks/useUserRoles.ts  # REPLACE: useSetUserRole (mutation only)
├── components/RolesTab.tsx  # MODIFY → RoleTab: single Select + inherited grid + overrides grid
├── components/PermissionsTab.tsx  # MODIFY: overrides grid with grant/revoke/remove + Reason
└── pages/UserDetailPage.tsx   # MODIFY: single screen layout

src/Web/ClientApp/src/features/security/rbac/  # MODIFY: remove user-roles clients/hooks/pages (UserRolesPage)
src/Web/ClientApp/src/app/routes.tsx            # MODIFY: remove /security/users/:id/roles route (now single detail screen)
src/Web/ClientApp/src/shared/constants/permissions.ts  # MODIFY: add Security.Users/ManageRoles policy constants
src/Web/ClientApp/src/shared/hooks/usePermission.ts    # MODIFY: extend PolicyString with security policies

tests/Application.UnitTests/Security/...  # ADD/MODIFY: SetUserRole, PermissionService effective calc, overrides validation
tests/Application.FunctionalTests/Security/...  # ADD/MODIFY: migration, concurrency, audit immutability
tests/Web.AcceptanceTests/...             # ADD/MODIFY: single-screen role+override journey
```

**Structure Decision**: Two-layer web application — layered backend (Domain→Application→Infrastructure→Web) under `src/` and React SPA under `src/Web/ClientApp`, mirroring the existing repo. This feature touches only the Security module of Domain/Application/Web plus the `features/security` frontend area; no module boundary or layer change. Account-groups (spec 005) structure reused as reference pattern.

## Complexity Tracking

> Constitution holds; no violations requiring justification.

## Phase 0: Research (output: research.md)

**Resolve NEEDS CLARIFICATION:**
- D-01: Two-phase migration mechanics in EF Core (nullable add → data fill → ALTER NOT NULL) and how ALTER NOT NULL aborts on NULL.
- D-02: `EffectiveFrom/To` type mismatch — UserRole uses `DateOnly`, UserPermission uses `DateTimeOffset?`. Unified effective window resolution semantics.
- D-03: Frontend test runner wiring — Vitest installed but no `test` script. Verify target command/CI.
- D-04: Localization pattern for Arabic validation + RTL Select/DataGrid from design tokens.
- D-05: RowVersion 409 surfacing — current handlers return 400 with error array; spec demands 409. Confirm mapping pattern (BackgroundJobs `Results.Conflict()` precedent).

## Phase 1: Design & Contracts (output: data-model.md, contracts/, quickstart.md)

Extract entities/metadata per spec; define OpenAPI contract for `PUT /api/Users/{id}/role` and updated `GET /api/Users/{id}` effective-permission payload; document overrides grid contract (grants+revokes), soft-delete semantics, strict validation rules; quickstart scenarios for migration + effective calc + single screen.
