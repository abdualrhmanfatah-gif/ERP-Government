# Implementation Plan: User-Roles Refactor

**Branch**: `001-user-roles-refactor` | **Date**: 2026-09-02 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/001-user-roles-refactor/spec.md`

## Summary

Replace the M:N `UserRoles` join table with a single `RoleId INT NOT NULL` FK on the `Users` table. Each user owns one security role, inheriting all RolePermissions. Individual permission overrides (grant/revoke) are managed via the existing `UserPermission` table with create-or-update semantics and mandatory Reason. The effective permission formula is: RolePermissions ∪ UserPermissions(IsGranted=true) \ UserPermissions(IsGranted=false). A unified frontend screen shows the role dropdown, inherited permissions (read-only with revoke), and overrides (editable with Reason) in a two-column RTL layout.

## Technical Context

**Language/Version**: C# 14 / .NET 10.0 (SDK 10.0.201)

**Primary Dependencies**: ASP.NET Core 10, Entity Framework Core 10, MediatR 14, FluentValidation 12, AutoMapper 16

**Storage**: SQL Server via `Aspire.Microsoft.EntityFrameworkCore.SqlServer`

**Testing**: NUnit 4.5, Moq 4.20, Shouldly 4.3 (BE) / Vitest 4.1, Testing Library (FE) / Playwright 1.58 (E2E)

**Target Platform**: Web application (.NET Aspire orchestrated)

**Project Type**: Web application (backend + React SPA frontend)

**Performance Goals**: Standard web application expectations (sub-second page loads)

**Constraints**: Arabic-first RTL UI, RowVersion concurrency on all mutable entities, INSERT-only audit trails

**Scale/Scope**: ~23 security roles, ~62 permissions, 7 codebase locations with UserRoles references (59 hits)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Layered Architectural Integrity | ✅ | Changes in Domain (User entity), Application (commands/queries/services), Infrastructure (EF config, migration), Web (endpoints, frontend). Dependencies point inward. |
| II. Bounded Contexts | ✅ | Security module owns all entities. No cross-module side effects. |
| III. Server-Side Business-Rule Integrity | ✅ | Permission resolution is server-side only (PermissionService). Frontend permission hook remains a stub (out of scope). |
| IV. Financial Integrity | ✅ | No financial impact — this is a security/authorization change. |
| V. Budget Control | ✅ | No budget impact. |
| VI. Data Integrity | ✅ | FK Restrict on new RoleId. RowVersion on User. AuditTrail/SecurityAuditLog INSERT-only. Schema change via versioned migration. |
| VII. Authorization and SoD | ✅ | Effective permission formula enforces SoD via UserPermission overrides. Every auth decision logged. |
| VIII. Approval Workflows and Audit Immutability | ✅ | Role changes and overrides recorded in AuditTrail + SecurityAuditLog with actor, timestamp, old/new values. No hard deletes. |
| IX. API and Frontend Contract Integrity | ✅ | API contract changes documented in contracts/api-changes.md. OpenAPI regenerated after endpoint changes. |
| X. UI and Design System Consistency | ✅ | Unified screen uses existing DataGrid, Select, PageHeader components. RTL layout with two-column side-by-side. |
| XI. Testing, Verification, and Evidence | ✅ | Unit tests for PermissionService, SetUserRole, validators. Functional tests for API endpoints. |
| XII. Controlled Architectural Change | ✅ | Security model change documented in this plan. No registered exceptions needed for this feature. |

**Post-Phase-1 re-check**: No violations. All principles satisfied.

## Project Structure

### Documentation (this feature)

```text
specs/001-user-roles-refactor/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── api-changes.md
├── spec.md              # Feature specification
├── checklists/
│   └── requirements.md  # Quality checklist
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── Domain/Security/Entities/
│   ├── User.cs                          # MODIFY: add RoleId property
│   ├── UserRole.cs                      # DELETE: entity removed
│   └── UserPermission.cs                # UNCHANGED
├── Application/
│   ├── Common/Interfaces/
│   │   └── IApplicationDbContext.cs     # MODIFY: remove UserRoles DbSet
│   ├── Security/
│   │   ├── IPermissionService.cs        # UNCHANGED (interface)
│   │   ├── PermissionService.cs         # MODIFY: rewrite to use Users.RoleId + UserPermissions
│   │   ├── Commands/Users/
│   │   │   ├── SetUserRoleCommand.cs    # NEW: replaces AssignRoleCommand
│   │   │   ├── AssignRoleCommand.cs     # DELETE
│   │   │   └── RemoveRoleCommand.cs     # DELETE
│   │   ├── Commands/UserPermissions/
│   │   │   └── AssignUserPermissionCommand.cs  # MODIFY: add Reason validation
│   │   ├── Queries/Users/
│   │   │   ├── GetUserByIdQuery.cs      # MODIFY: read Users.RoleId instead of UserRoles
│   │   │   ├── GetUserRolesQuery.cs     # DELETE
│   │   │   └── GetUserPermissionsQuery.cs  # MODIFY: return effective permissions
│   │   └── Common/DTOs/
│   │       └── SecurityDtos.cs          # MODIFY: UserDetailDto.Role, UserRoleDto, new EffectivePermissionDto
│   └── Common/Behaviours/
│       └── AuthorizationBehaviour.cs    # UNCHANGED (already uses IPermissionService)
├── Infrastructure/
│   ├── Data/
│   │   ├── ApplicationDbContext.cs      # MODIFY: remove UserRoles DbSet
│   │   ├── Configurations/Security/
│   │   │   ├── UserConfiguration.cs     # MODIFY: add RoleId FK config
│   │   │   ├── UserRoleConfiguration.cs # DELETE
│   │   │   └── UserPermissionConfiguration.cs  # MODIFY: change unique index
│   │   ├── Interceptors/
│   │   │   └── AuditableEntityInterceptor.cs  # UNCHANGED (auto-captures User changes)
│   │   └── Migrations/                  # NEW: EF Core migration
│   └── Identity/                        # UNCHANGED
└── Web/
    ├── Endpoints/Security/
    │   └── Users.cs                     # MODIFY: remove role endpoints, add PUT /{id}/role
    ├── DependencyInjection.cs           # UNCHANGED
    └── ClientApp/src/
        ├── features/security/
        │   ├── rbac/
        │   │   ├── pages/UserRolesPage.tsx     # DELETE
        │   │   └── hooks/                      # MODIFY: remove useUserRoles, useAssignUserRole, useRemoveUserRole
        │   └── users/
        │       ├── pages/UserDetailPage.tsx    # MODIFY: update RolesTab
        │       ├── components/RolesTab.tsx     # REWRITE: unified role + overrides screen
        │       ├── components/PermissionsTab.tsx  # UNCHANGED
        │       ├── client.ts                   # MODIFY: update usersRolesClient
        │       └── hooks/                      # MODIFY: update useUserRoles hooks
        └── shared/types/                       # MODIFY: update UserRole type
```

**Structure Decision**: The existing layered structure (Domain → Application → Infrastructure → Web) is preserved. Changes flow inward: Domain entity change → Application commands/queries → Infrastructure EF config/migration → Web endpoints/frontend.

## Complexity Tracking

> No Constitution violations require justification for this feature.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| (none) | — | — |
