# Research: Single Role Per User (Option A)

**Feature**: `006-single-role-per-user` | **Date**: 2026-09-02
**Spec**: [spec.md](./spec.md)

Resolves every NEEDS CLARIFICATION from the plan's Technical Context and grounds design decisions in verified codebase facts.

---

## D-01: Two-Phase Migration Mechanics in EF Core

**Decision**: Add `RoleId int?` null FK to `Users` (phase 1), fill via migration data step + manual SQL (phase 2a), then `ALTER COLUMN RoleId INT NOT NULL` (phase 2b). The NOT-NULL ALTER must be a guarded step that fails with explicit remaining-NULL UserIds rather than silently truncating.

**Rationale**:
- Phase 2b `ALTER COLUMN ... NOT NULL` fails natively in SQL Server if any row has NULL, satisfying FR-001's "fail with explicit error listing remaining UserIds" requirement at the schema layer.
- EF Core migrations run sequentially in `Migrations/`; each phase is its own migration for reviewability (Constitution VI).
- `HasColumnType`/`IsRequired(false)`→`IsRequired(true)` in `UserConfiguration.cs`, expressed as two migrations.

**Alternatives considered**:
- Single migration doing data-fill + ALTER in one step: rejected because ORPHAN_ZERO/CONFLICT_MULTI rows cannot be auto-resolved; split enables the report→manual mapping→re-run loop.
- Runtime auto-create/auto-delete schema: rejected, prohibited by Constitution VI.

**Key facts verified**:
- `src/Infrastructure/Data/Configurations/Security/UserConfiguration.cs` maps `Users`; current schema has no `RoleId`.
- Existing migrations under `src/Infrastructure/Migrations/` are versioned, `ApplicationDbContextModelSnapshot.cs` auto-updated.

---

## D-02: EffectiveFrom/To Type Mismatch (UserRole DateOnly vs UserPermission DateTimeOffset?)

**Decision**: The single-role model drops `UserRole` entirely, so the `DateOnly` window on UserRole disappears. Remaining windows are:
- `SecurityRole.IsActive` (bool) — role must be active.
- `RolePermission` — has `IsActive` + (per report) EffectiveFrom/To; but **verified** `RolePermission.cs` has only `RoleId, PermissionId, RowVersion` — no window fields. So role-permission windows come from the parent `SecurityRole`.
- `UserPermission.IsActive` + `EffectiveFrom/EffectiveTo` (DateTimeOffset?) — grant/revoke windows.

**Resolution**: Effective permission evaluation uses **current time (`DateTimeOffset.UtcNow`) within [EffectiveFrom, EffectiveTo]** for UserPermissions and role-level `IsActive`, consistent with spec FR-002/FR-009 and Constitution III (transaction-time evaluation). Convert the `UserPermission` windows to `DateTimeOffset` (already the case) and compare against `DateTimeOffset.UtcNow` at evaluation time. No `DateOnly` remains in the effective-permission path.

**Rationale**: Single consistent time representation avoids the old dual-type ambiguity; only UserPermission has configurable windows now.

**Alternatives considered**:
- Normalize RolePermission to carry its own windows: rejected — out of scope and would expand surface; role-level `IsActive` + UserPermission windows cover the spec.

**Key facts verified**:
- `src/Domain/Security/Entities/UserPermission.cs` has `IsActive`? **No** — it has `EffectiveFrom/To (DateTimeOffset?)`, `IsGranted`, `Reason`. There is no `IsActive` field on UserPermission currently. The spec assumes `IsActive` (IsActive true=active). This is a **gap**: UserPermission must gain `IsActive bool = true` to support soft-delete of overrides (clarified: removal soft-deletes). Confirmed needed addition in data model.
- `src/Domain/Security/Entities/SecurityRole.cs` has `IsActive`.

---

## D-03: Frontend Test Runner Wiring

**Decision**: Use Vitest 4 (already installed) with the existing `vitest.config.ts` (`globals: true`, `environment: jsdom`, include `src/**/*.{test,spec}.{ts,tsx}`). Add a `"test": "vitest run"` script to `package.json` (`"test:watch": "vitest"`). Verify via `npm test`.

**Rationale**:
- Vitest is already a devDependency and config exists; no new framework needed (Constitution XI frontend tests).
- Adding the `test` script makes the runner invocable and CI-addressable.
- Per spec 005 precedent, frontend component logic (Select, DataGrid, overrides) is tested with Vitest + Testing Library (already devDeps).

**Alternatives considered**:
- Jest: rejected (Vitest already configured for Vite).
- Playwright for full browser: exists as `tests/Web.AcceptanceTests` (backend side) — the browser-level acceptance journey (SC-006) is planned there.

**Key facts verified**:
- `src/Web/ClientApp/package.json` — Vitest 4.1.11 devDep, no `test` script.
- `src/Web/ClientApp/vitest.config.ts` exists.

---

## D-04: Arabic Validation + RTL Select/DataGrid

**Decision**: Follow the shared design-token library and existing account-groups (spec 005) patterns: `Select` and `DataGrid` UI components already exist in `src/Web/ClientApp/src/components/ui/`. Backend FluentValidation messages in Arabic; frontend maps API validation (`problem-details`) to Arabic inline messages using the established localization approach in the repo.

**Rationale**:
- Reuse shared primitives (Constitution X — no per-feature duplicates).
- RTL handled by logical (start/end) properties already enforced in the design system; dark mode via tokens.
- Arabic-first: role/override labels and validation copy in Arabic.

**Key facts verified**:
- `src/components/ui/Select.tsx`, `src/components/ui/DataGrid.tsx` exist (from spec 005).
- FluentValidation validators in repo return Arabic messages (e.g. AccountGroup validators).

---

## D-05: RowVersion 409 Concurrency Surfacing

**Decision**: Surface stale RowVersion as HTTP **409 Conflict** (spec FR-005/SC-004), not the current 400-with-error-array pattern. Add a dedicated result mapping: handler catches `DbUpdateConcurrencyException` and returns a concurrency failure; endpoint maps it to `Results.Conflict()` with problem-details body. Precedent: `Results.Conflict()` used in `BackgroundJobsEndpoints.cs:148`.

**Rationale**:
- Spec FR-005 and Constitution IX require concurrency conflicts "surfaced distinctly" (400 vs 409).
- Frontend already checks for `409`/`Conflict` in error text (`UserDetailPage.tsx`), so aligning backend to 409 removes the mismatch.

**Alternatives considered**:
- Keep 400 with error array (current): rejected — violates FR-005 distinct 409.
- Return 409 only, no mapping layer: rejected — handler still catches `DbUpdateConcurrencyException`; only the status mapping changes.

**Key facts verified**:
- `BackgroundJobsEndpoints.cs:25,148` — `StatusCodes.Status409Conflict` / `Results.Conflict()` precedent.
- Security endpoints currently `Results.BadRequest(result.Errors)` on failure — will add a conflict branch.

---

## D-06: UserPermission Active/Soft-Delete Support

**Decision**: Add `bool IsActive { get; set; } = true` to `UserPermission`. Override removal sets `IsActive=false` (soft-delete, FR-008/Edge Cases). Effective calc and overrides grid filter `IsActive=true`.

**Rationale**:
- Spec Edge Cases + clarification mandate soft-delete; no hard delete (Constitution VI, VIII).
- `UserPermission` currently lacks `IsActive`; must be added to entity, configuration, DTO, migration.

**Key facts verified**:
- `UserPermission.cs` — no `IsActive`; has `EffectiveFrom/To`, `IsGranted`, `Reason`.
- `UserPermissionConfiguration.cs` unique index on `(UserId, PermissionId, EffectiveFrom)`.

---

## D-07: Permit on Removing UserRoles References (59 hits → 0)

**Decision**: Remove the `UserRoles` DbSet from `IApplicationDbContext`, `ApplicationDbContext`, delete `UserRoleConfiguration.cs` and the `UserRole` entity, and update every referencing use case to the new `Users.RoleId` model. Endpoint deletion + OpenAPI regen removed from the broke surface.

**Rationale**: FR-006 mandates zero UserRoles references remain in Application/Web/Frontend (43 in src + web-api-client auto-gen refresh). This is the core mechanical sweep.

**Key facts verified** (16 distinct src locations):
- `IApplicationDbContext.cs:34`, `ApplicationDbContext.cs:37-43`
- `PermissionService.cs:21`, `GetUserRolesQuery.cs:25`, `GetUserByIdQuery.cs:32`, `DeactivateUserCommand.cs:41,50`, `DeleteRoleCommand.cs:28`, `AssignRoleCommand.cs:38,52`, `AssignRolePeriodCommand.cs:38,55`, `RemoveRoleCommand.cs:27,33`
- `UserRoleConfiguration.cs:11`, `Users.cs:72-83,273-305` (GET/POST/DELETE roles)
- Frontend: `users/client.ts`, `users/hooks/useUserRoles.ts`, `rbac/client.ts`, `rbac/hooks/useUserRoles.ts`, `rbac/hooks/useAssignUserRole.ts`, `useRemoveUserRole.ts`, `rbac/pages/UserRolesPage.tsx`, `app/routes.tsx`, `web-api-client.ts` (auto-gen).

---

## Synthesis

Primary unknowns resolved. Remaining high-confidence mechanical sweep (UserRoles removal, entity field additions, endpoint/contract changes, UI consolidation) is a large but well-mapped refactor with clear test targets. No open NEEDS CLARIFICATION remains for plan/design.
