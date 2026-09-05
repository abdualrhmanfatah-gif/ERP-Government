# Implementation Plan: User Management Frontend + Backend

**Branch**: `004-user-management-frontend-backend` | **Date**: 2026-09-02 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/004-user-management-frontend-backend/spec.md`

## Summary

Build a full user management feature (frontend + backend gaps) for the ERP-Government Security module. The backend already exposes user CRUD, role/permission assignment, session management, and audit endpoints. The frontend currently lacks user management pages. This plan covers completing any missing backend use cases and building the frontend pages (list, detail, create, edit) with role/permission/session/audit tabs, conforming to the existing design system and constitutional constraints.

## Technical Context

**Language/Version**: C# 13 / .NET 10.0 (backend), TypeScript / React 19 (frontend)

**Primary Dependencies**: ASP.NET Core, MediatR, FluentValidation, EF Core, NUnit, Vitest, shadcn/ui, TanStack React Query/Table, react-hook-form + Zod, NSwag (auto-generated API client)

**Storage**: SQL Server via EF Core (existing User entity with migrations)

**Testing**: NUnit (backend unit/integration/functional/acceptance), Vitest + Testing Library (frontend), Playwright (browser acceptance), Reqnroll (BDD)

**Target Platform**: Web application (SPA + API), Arabic-first RTL, dark mode

**Project Type**: Web application (Clean Architecture: Domain → Application → Infrastructure → Web)

**Performance Goals**: User list loads within 2 seconds for 10,000 users; search returns within 30 seconds

**Constraints**: Arabic-first RTL; all design tokens from central source; optimistic concurrency on all mutations; hand-written frontend clients must mirror OpenAPI contract exactly

**Scale/Scope**: ~7 pages (list, create, detail with 4 tabs), 21 functional requirements, 7 user stories

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Layered Architectural Integrity | ✅ PASS | Frontend consumes HTTP contract only; backend use cases in Application layer |
| II. Bounded Contexts | ✅ PASS | Security module owns User entities; no cross-module writes |
| III. Server-Side Business-Rule Integrity | ✅ PASS | All validation (SoD, self-deactivation, login uniqueness) enforced server-side in use cases |
| IV. Financial Integrity | N/A | No financial data involved |
| V. Budget Control | N/A | No spending documents involved |
| VI. Data Integrity | ✅ PASS | User entity already has RowVersion; migrations only; no cascading deletes |
| VII. Authorization and Separation of Duties | ✅ PASS | All endpoints declare permissions (UsersView, UsersCreate, etc.); SoD blocking via FR-021 |
| VIII. Approval Workflows and Audit Immutability | ✅ PASS | Audit trail already captured; INSERT-ONLY per Principle VIII |
| IX. API and Frontend Contract Integrity | ✅ PASS | OpenAPI auto-generated; NSwag client generation; optimistic concurrency round-trip |
| X. UI and Design System Consistency | ✅ PASS | shadcn/ui component library; design tokens; RTL logical properties |
| XI. Testing | ✅ PASS | Unit tests for use cases; browser acceptance tests required; frontend Vitest |
| XII. Controlled Architectural Change | ✅ PASS | No new modules; Security module already registered |

**Gate result**: PASS — no violations. No complexity tracking needed.

## Project Structure

### Documentation (this feature)

```text
specs/004-user-management-frontend-backend/
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
├── Domain/Security/Entities/
│   ├── User.cs                    # Existing entity (add fields if needed)
│   ├── UserRole.cs                # Existing
│   ├── UserPermission.cs          # Existing
│   └── UserSession.cs             # Existing
├── Application/Security/
│   ├── Commands/Users/            # Existing: Create, Update, Deactivate, Reactivate
│   ├── Commands/UserPermissions/  # Existing: Assign, Remove
│   ├── Commands/UserRoles/        # Existing: Assign, Remove
│   ├── Queries/Users/             # Existing: GetUsers, GetUserById, GetUserSessions
│   └── Common/DTOs/               # Existing: UserDto, UserDetailDto, etc.
├── Web/Endpoints/Security/
│   └── Users.cs                   # Existing: all CRUD + role/permission + session endpoints
├── Web/ClientApp/src/features/security/
│   └── users/                     # NEW: User management frontend feature
│       ├── pages/                 # UserListPage, UserDetailPage, CreateUserPage
│       ├── components/            # UserForm, RolesTab, PermissionsTab, SessionsTab, AuditTab
│       ├── hooks/                 # useUsers, useUserMutations
│       ├── types.ts               # TypeScript types matching OpenAPI contract
│       └── client.ts             # API client calls (generated or hand-written mirroring contract)
└── tests/
    ├── Application.UnitTests/     # Existing: use case unit tests
    ├── FunctionalTests/           # Existing: integration tests with real DB
    └── AcceptanceTests/           # Existing: Playwright browser tests
```

**Structure Decision**: Follow existing Clean Architecture layout. Frontend lives in `src/Web/ClientApp/src/features/security/users/` matching the established feature-folder pattern (see `features/security/rbac/`). No new projects required.

## Complexity Tracking

No violations. No complexity tracking needed.
