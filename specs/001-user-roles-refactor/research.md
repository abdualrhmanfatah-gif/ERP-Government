# Research: User-Roles Refactor

**Date**: 2026-09-02
**Branch**: `001-user-roles-refactor`

## Decisions

### 1. Schema: Add RoleId directly to Users, drop UserRoles

**Decision**: Add `RoleId INT NOT NULL` with FK -> SecurityRoles (Restrict) to Users table. Drop UserRoles join table entirely.

**Rationale**: No legacy data exists. The schema is created directly. The M:N relationship via UserRoles (composite PK: UserId, RoleId, EffectiveFrom) is replaced by a 1:N relationship from SecurityRoles to Users. This eliminates the EffectiveFrom/EffectiveTo time-windowing complexity on role assignment.

**Alternatives considered**:
- Keep UserRoles but enforce single active role via trigger — rejected, adds complexity without simplifying the permission model.
- Nullable RoleId with default role — rejected per FR-001 ("No default role").

### 2. Permission resolution: Union-minus formula

**Decision**: Effective permissions = RolePermissions (of linked Role, where Role.IsActive and EffectiveFrom/To permit) UNION UserPermissions WHERE IsGranted=true MINUS UserPermissions WHERE IsGranted=false.

**Rationale**: The current `PermissionService` only resolves role-based permissions via `User -> UserRole -> RolePermission -> SecurityPermission`. After refactor, it must read `Users.RoleId` instead of `UserRoles` and include `UserPermission` overrides. The formula is simple, auditable, and consistent with the clarified spec.

**Alternatives considered**:
- Priority-based model (role < user override) — equivalent to union-minus for boolean grants.
- EffectiveFrom/EffectiveTo on role assignment — dropped per user clarification (no legacy data, no time-windowing on roles).

### 3. SetUserRole: Replace-all semantics with RowVersion

**Decision**: The new `SetUserRoleCommand` replaces the entire role assignment (not additive). Uses `RowVersion` for optimistic concurrency. Verifies `Role.IsActive` before allowing.

**Rationale**: FR-005 requires replace-all semantics. The existing `AssignRoleCommand` is additive (creates a new UserRole entry). The new command updates `Users.RoleId` directly with concurrency check.

### 4. Audit: Extend existing infrastructure

**Decision**: Role changes and permission overrides are recorded in both `AuditTrail` (entity change tracking via interceptor) and `SecurityAuditLog` (explicit security events). The existing `AuditableEntityInterceptor` automatically captures INSERT/UPDATE/DELETE on auditable entities. `SecurityAuditLog` entries are written explicitly from the use-case handler.

**Rationale**: Both audit systems already exist and are INSERT-ONLY (IImmutableEntity). The interceptor handles entity-change tracking; security-specific entries (role change, override with old/new values) are written explicitly for the security audit log.

### 5. Frontend: Unified screen replaces UserRolesPage

**Decision**: Replace the current `UserRolesPage` and `RolesTab` with a unified screen showing: single role dropdown, inherited permissions (read-only DataGrid with revoke button), and overrides (editable DataGrid with Reason). Two-column side-by-side layout (RTL-aware).

**Rationale**: FR-007 and the clarified spec require a single screen. The existing `UserRolesPage` at `/security/users/:id/roles` shows M:N role assignments. The new screen shows a single role with inherited and override permissions.

### 6. API contract changes

**Decision**: Remove `GET/POST/DELETE /{id}/roles`. Add `PUT /{id}/role`. Keep existing `GET/POST/DELETE /{id}/permissions` for overrides (the POST/DELETE already handle create-or-update semantics via the unique constraint).

**Rationale**: The old endpoints support M:N role assignment. The new PUT endpoint handles single-role assignment. The permission endpoints remain because they manage overrides (UserPermission), not roles.

## Codebase Findings

| Finding | Location | Impact |
|---------|----------|--------|
| PermissionService only resolves role-based | `PermissionService.cs:19-36` | Must add UserPermission resolution |
| UserRoles join table with composite PK | `UserRole.cs`, `UserRoleConfiguration.cs` | Will be dropped |
| AssignRoleCommand is additive | `AssignRoleCommand.cs:44-50` | Must replace with SetUserRole |
| Frontend UserRolesPage uses M:N pattern | `UserRolesPage.tsx` | Must replace with unified screen |
| UserDetailPage has hand-built tabs | `UserDetailPage.tsx` | RolesTab will become unified screen |
| Frontend permission hook is stub | `usePermission.ts:37-38` | Out of scope for this feature |
| Authorization policies are open | `Web.DependencyInjection.cs` | Out of scope for this feature |
