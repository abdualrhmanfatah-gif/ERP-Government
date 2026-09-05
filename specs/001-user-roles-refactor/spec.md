# Feature Specification: User-Roles Refactor

**Feature Branch**: `001-user-roles-refactor`

**Created**: 2026-09-02

**Status**: Draft

**Input**: User description: "Remove UserRoles join table and link Users directly to SecurityRoles - single NOT NULL RoleId with precise mapping and overrides in same screen (Option A)"

## Clarifications

### Session 2026-09-02

- Q: How should the UserPermission table enforce uniqueness — one override per user-permission pair, or can multiple overrides for the same user-permission coexist? → A: Unique constraint on (UserId, PermissionId) — one override per pair, create-or-update semantics.
- Q: Is a Reason required for every permission override (both grants and revocations), or only when revoking an inherited permission? → A: Reason is required for both grants and revocations — consistent audit trail.
- Q: How should the inherited permissions and overrides tables be arranged on the unified role screen? → A: Two-column side-by-side: inherited (right in RTL) and overrides (left in RTL) — instant comparison.
- Q: What should happen if phase 2 of the migration fails partway through — for example, after some users have been mapped but before the NOT NULL constraint is applied? → A: No legacy data exists. The migration creates the final schema directly; no phased data migration is required.
- Q: When a user's role is deactivated, should the inherited permissions in the unified screen show as disabled/greyed-out to indicate they are not effective, or should the inherited permissions table simply not load? → A: Show inherited permissions as disabled/greyed-out with a deactivation indicator.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Assign a Single Role to a User (Priority: P1)

As a security administrator, I want to assign exactly one security role to each user so that the user inherits all permissions from that role as their baseline access. The role assignment replaces any prior role — there can be no ambiguity about which role a user holds.

**Why this priority**: This is the foundational change. Every downstream permission check depends on Users having a single RoleId. Without this, no other stories work.

**Independent Test**: Can be fully tested by assigning a role to a user via the API and verifying the user's RoleId is set and the role's inherited permissions appear in their effective permission set.

**Acceptance Scenarios**:

1. **Given** a user with no role assigned, **When** an admin assigns them a role via PUT /{id}/role, **Then** the user's RoleId is set to the assigned role and all RolePermissions for that role become part of the user's effective permissions.
2. **Given** a user already assigned to role A, **When** the admin assigns them to role B, **Then** the user's RoleId changes to B, role A permissions are removed from effective permissions, and role B permissions are added.
3. **Given** a user assigned to a role, **When** the admin attempts to assign a role that is not active, **Then** the assignment is rejected with an explicit error.
4. **Given** a concurrent update to the same user, **When** two admins attempt to assign roles simultaneously, **Then** the second update is rejected with a concurrency conflict (RowVersion mismatch).

---

### User Story 2 - Effective Permission Resolution (Priority: P1)

As the system, when evaluating whether a user has a specific permission, I must compute the effective permission set as: RolePermissions of the linked role (where Role.IsActive and EffectiveFrom/To permit) PLUS UserPermissions where IsGranted=true MINUS UserPermissions where IsGranted=false. This must be enforced server-side only.

**Why this priority**: This is the core business rule. Without correct permission resolution, authorization breaks across the entire system.

**Independent Test**: Can be tested by setting a user's role, creating user-level overrides, and verifying the effective permission set matches the union-minus formula.

**Acceptance Scenarios**:

1. **Given** a user with role R (having permissions P1, P2, P3) and no overrides, **When** the system evaluates permissions, **Then** effective permissions are {P1, P2, P3}.
2. **Given** a user with role R (having P1, P2) and UserPermission P3 IsGranted=true, **When** the system evaluates, **Then** effective permissions are {P1, P2, P3}.
3. **Given** a user with role R (having P1, P2, P3) and UserPermission P2 IsGranted=false, **When** the system evaluates, **Then** effective permissions are {P1, P3}.
4. **Given** a user with role R where Role.IsActive=false, **When** the system evaluates, **Then** no permissions from role R are included in effective permissions.
5. **Given** a user with role R where Role.EffectiveFrom is in the future, **When** the system evaluates, **Then** role permissions are not included.

---

### User Story 3 - Override Individual Permissions (Priority: P2)

As a security administrator, I want to grant or revoke individual permissions for a user on top of their role's inherited permissions, so that exceptions to the role baseline are explicitly controlled and audited.

**Why this priority**: After the core role-assignment works, administrators need fine-grained override capability. This is essential for real-world SoD and special-access scenarios.

**Independent Test**: Can be tested by granting and revoking specific permissions for a user and verifying the effective permission set changes accordingly, with audit records created.

**Acceptance Scenarios**:

1. **Given** a user with role R (having P1, P2), **When** the admin revokes P1 for this user, **Then** a UserPermission record IsGranted=false with a Reason is created, P1 is excluded from effective permissions, and an audit record is created.
2. **Given** a user with role R (having P1, P2), **When** the admin grants P3 (not in role) for this user with a Reason, **Then** a UserPermission record IsGranted=true is created, P3 is included in effective permissions, and an audit record is created.
3. **Given** a user with an existing override (P1 IsGranted=false), **When** the admin revokes P1 again, **Then** the system updates the existing override and the audit trail records both old and new states.
4. **Given** a user with an existing grant override (P3 IsGranted=true), **When** the admin revokes P3, **Then** the UserPermission record is updated to IsGranted=false and an audit record is created.

---

### User Story 4 - Schema Deployment (Priority: P1)

As a system operator, I need a database migration that adds RoleId INT NOT NULL (FK -> SecurityRoles) to the Users table and drops the UserRoles join table so that the new single-role model is in place. No legacy data migration is required — the schema is created directly.

**Why this priority**: Without the schema change, the system cannot function. This is a prerequisite to deploying the new model.

**Independent Test**: Can be tested by running the migration against a clean database and verifying Users has a non-NULL RoleId column with FK to SecurityRoles, and UserRoles table no longer exists.

**Acceptance Scenarios**:

1. **Given** the Users table without RoleId, **When** the migration runs, **Then** Users gains a RoleId INT NOT NULL column with FK -> SecurityRoles (Restrict), and the UserRoles table is dropped.
2. **Given** the migration has completed, **When** the system starts, **Then** all new users are created with RoleId set via the PUT /{id}/role endpoint, and no NULL RoleId is possible.

---

### User Story 5 - Unified Role and Overrides Screen (Priority: P2)

As a security administrator, I want a single screen that shows a user's assigned role (via a dropdown), displays the inherited permissions from that role as a read-only table with a revoke button, and displays user-specific overrides (grants and revocations) as an editable table with a Reason field, so that I can manage a user's complete permission picture in one place. The inherited and overrides tables are arranged in a two-column side-by-side layout (inherited on the right in RTL, overrides on the left in RTL) for instant visual comparison.

**Why this priority**: The UI consolidates what was previously scattered across multiple screens. It improves administrator efficiency and reduces errors.

**Independent Test**: Can be tested by loading the screen for a user, verifying the role dropdown shows the current role, the inherited permissions table is read-only, and the overrides table allows adding/editing entries with Reason.

**Acceptance Scenarios**:

1. **Given** a user with role R and two overrides, **When** an admin opens the user's role screen, **Then** the dropdown shows role R selected, the inherited permissions table shows all of R's permissions, and the overrides table shows the two override entries.
2. **Given** a user with role R, **When** the admin clicks revoke on an inherited permission P1, **Then** an override entry IsGranted=false is created with a Reason field, and P1 is removed from the effective permission display.
3. **Given** the admin changes the user's role from R to S, **When** the change is saved, **Then** the inherited permissions table updates to show S's permissions, existing overrides against R's permissions are preserved, and the effective set is recalculated.
4. **Given** the admin attempts to save without providing a Reason for a new override, **When** the admin clicks save, **Then** the save is rejected and an error prompts for a Reason.

---

### Edge Cases

- What happens when a user's sole role is deactivated? The user's effective permissions become only those from overrides (IsGranted=true). The system must not crash or return all permissions. The unified screen must show inherited permissions as disabled/greyed-out with a deactivation indicator to preserve diagnostic context.
- What happens when an override targets a permission the user's new role does not include? The override is still persisted and applies if the role later includes that permission.
- What happens when a user has overrides but their role is changed? Overrides are independent of role and persist; effective permissions recalculated immediately.
- What happens when an admin attempts to set a role that does not exist? The system rejects with a clear error.
- What happens when RowVersion is stale on role assignment? The system rejects with a concurrency conflict error.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Users MUST have RoleId INT NOT NULL (FK -> SecurityRoles) at all times. The schema migration adds this column directly; no phased approach is needed. No default role is assigned automatically.
- **FR-002**: Effective permissions MUST be computed as: RolePermissions of the linked Role (respecting Role.IsActive and EffectiveFrom/To) UNION UserPermissions WHERE IsGranted=true MINUS UserPermissions WHERE IsGranted=false. Computation is server-side only.
- **FR-003**: Revoking an inherited permission MUST create a UserPermissions record with IsGranted=false and a Reason. Granting a non-inherited permission MUST create a UserPermissions record with IsGranted=true and a Reason. A Reason MUST be provided for every override operation — the system MUST reject saves without one. Both actions MUST be recorded in the audit trail and security audit log.
- **FR-004**: The database migration MUST add RoleId INT NOT NULL with FK -> SecurityRoles (Restrict) to the Users table and drop the UserRoles join table. No data migration is required — the schema is created directly.
- **FR-005**: SetUserRole MUST replace the user's entire role assignment (not additive). MUST use RowVersion for optimistic concurrency. MUST verify the target Role.IsActive is true before allowing assignment.
- **FR-006**: Every use case or service that previously read from UserRoles MUST be updated to read from Users.RoleId instead. This includes: PermissionService, GetUserById, DeactivateUser, DeleteRole.
- **FR-007**: The API MUST remove the following endpoints: GET /{id}/roles, POST /{id}/roles, DELETE /{id}/roles. The API MUST add: PUT /{id}/role. The frontend MUST present a single screen with: a single Select for the role, a read-only DataGrid for inherited permissions with a revoke button, and a DataGrid for overrides (grant/revoke) with a Reason field.
- **FR-008**: AuditTrail and SecurityAuditLog MUST record: role change (oldRoleId, newRoleId, actor, timestamp), permission override creation/update (permission, old value, new value, actor, timestamp). No hard deletes of audit records.

### Key Entities

- **User**: Represents a system user. Key attribute: RoleId (INT NOT NULL, FK -> SecurityRoles) replacing the M:N UserRoles relationship.
- **SecurityRole**: Represents a named role with a set of permissions. Key attributes: IsActive, EffectiveFrom, EffectiveTo.
- **RolePermission**: Join entity linking a SecurityRole to its permissions. Each entry grants a permission to the role.
- **UserPermission**: Join entity linking a User to a specific permission override. Key attributes: IsGranted (boolean), Reason (text), EffectiveFrom, EffectiveTo. Unique constraint on (UserId, PermissionId) — one override per pair with create-or-update semantics. Enables fine-grained grant/revoke on top of role-inherited permissions.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users table has RoleId INT NOT NULL with FK to SecurityRoles after schema deployment.
- **SC-002**: Effective permission calculation matches the union-minus formula for all test scenarios (role-only, role+grants, role+revokes, role+both).
- **SC-003**: Role assignment via the API completes in a single request (PUT /{id}/role) without needing separate calls.
- **SC-004**: The unified role-and-overloads screen loads and displays all data (role, inherited permissions, overrides) without requiring navigation to other screens.
- **SC-005**: Every role change and permission override generates exactly one audit trail entry and one security audit log entry with actor, timestamp, and old/new values.
- **SC-006**: Zero instances of the removed endpoints (GET/POST/DELETE /{id}/roles) exist in the codebase after the feature is complete.

## Assumptions

- UserPermissions table already exists in the database schema and supports IsGranted, Reason, EffectiveFrom, EffectiveTo fields.
- No legacy user-role data exists — the schema is created directly without a data migration phase.
- All 7 places in Application/Web/Frontend that depend on UserRoles (59 hits) are identified and will be updated.
- The security audit log infrastructure already exists and can accept role-change and override-change records.
- RowVersion (optimistic concurrency token) is already present on the User entity.
- The RolePermissions join table is not being modified — only the User-to-Role link is changing.
- The frontend uses a component library with Select and DataGrid components available.
