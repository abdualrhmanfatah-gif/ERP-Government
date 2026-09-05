# Feature Specification: Single Role Per User (Option A)

**Feature Branch**: `006-single-role-per-user`

**Created**: 2026-09-02

**Status**: Draft

**Input**: User description: "Remove UserRoles join table and link Users directly to SecurityRoles - single NOT NULL RoleId with precise mapping and overrides in same screen (Option A) Context: UserRoles M:N with EffectiveFrom/To complicates SoD and permission resolution. 7 Application/Web/Frontend places depend on UserRoles (59 hits). Goal: Each User owns single RoleId INT NOT NULL (FK -> SecurityRoles Restrict) inheriting all RolePermissions, plus ability to grant/revoke individual permissions via UserPermissions IsGranted. Requirements: FR-001..FR-008, two-phase migration with precise mapping, effective = RolePermissions + grants - revokes, etc."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Precise Migration from M:N to Single Role (Priority: P1)

As a system administrator I execute the two-phase migration so every user ends with exactly one active RoleId and no data loss.

**Why this priority**: Without correct migration the NOT NULL constraint cannot be applied and the system is inconsistent. This is the foundation for all other stories and blocks deployment (Constitution VI - Restrict, no data loss).

**Independent Test**: Run migration report on a database containing OK_SINGLE, CONFLICT_MULTI, and ORPHAN_ZERO users. Verify report categorization, apply PreciseMapping for conflicts/orphans, run migration phase 2 and confirm all Users.RoleId NOT NULL and no UserRoles data remains referenced.

**Acceptance Scenarios**:

1. **Given** a user with exactly one active UserRoles row **When** phase 1 migration runs **Then** Users.RoleId is auto-set to that RoleId and user appears in OK_SINGLE list
2. **Given** a user with 2 active UserRoles rows **When** report is generated **Then** user appears in CONFLICT_MULTI with both RoleIds listed and Users.RoleId remains NULL
3. **Given** a user with zero active UserRoles rows **When** report is generated **Then** user appears in ORPHAN_ZERO and Users.RoleId remains NULL
4. **Given** CONFLICT_MULTI and ORPHAN_ZERO users with manual PreciseMapping (UserId->RoleId) provided **When** phase 2 runs **Then** mapped users get RoleId set, report shows zero pending, and no user has NULL RoleId
5. **Given** any user still has NULL RoleId **When** ALTER NOT NULL is attempted **Then** migration fails with explicit error listing remaining UserIds and does not alter schema

---

### User Story 2 - Set Single Role for a User (Priority: P1)

As an administrator I set or change a user's single role in one action with concurrency protection.

**Why this priority**: Core management operation; replaces old multi-select role assignment and must enforce invariants (Constitution VII, VI).

**Independent Test**: Open user detail, change role via single Select, save, reopen and verify only the new role is effective and history shows old/new.

**Acceptance Scenarios**:

1. **Given** a user with RoleId=A and RowVersion=V1 **When** I PUT /users/{id}/role with {roleId:B, rowVersion:V1} **Then** Users.RoleId becomes B, RowVersion advances, SecurityAuditLog records old=A new=B with actor/timestamp
2. **Given** a user with stale RowVersion=V1 (current is V2) **When** I attempt SetUserRole with V1 **Then** system returns 409 concurrency conflict and does not change role
3. **Given** Role B has IsActive=false **When** I attempt to assign B **Then** system returns 400 validation "Role is inactive" and does not change role
4. **Given** I have no permission to manage users **When** I call PUT /users/{id}/role **Then** system returns 403

---

### User Story 3 - Effective Permission Calculation (Priority: P1)

As the authorization subsystem I compute a user's effective permissions as union/minus to evaluate every protected action.

**Why this priority**: Authorization is NON-NEGOTIABLE (Constitution VII). Must fail closed and evaluate at transaction time, respecting IsActive and EffectiveFrom/To.

**Independent Test**: For a user with Role R (permissions P1,P2) plus UserPermissions (grant P3, revoke P2) verify PermissionService returns {P1, P3} and denies P2.

**Acceptance Scenarios**:

1. **Given** Role R has permissions {P1 (IsActive true), P2 (IsActive false), P3 (EffectiveFrom future)} **When** effective permissions are computed **Then** only P1 from role is included before user overrides
2. **Given** Role R has {P1, P2} and UserPermissions has {P3 IsGranted=true, P2 IsGranted=false} **When** effective permissions are computed **Then** result is {P1, P3} (P2 revoked)
3. **Given** PermissionService throws/error **When** authorization is evaluated **Then** decision is deny (fail closed) and denial is logged
4. **Given** GetUserById, DeactivateUser, DeleteRole use cases are called **When** they read permissions **Then** they use the new single-RoleId + UserPermissions path (no UserRoles reference remains)

---

### User Story 4 - Grant / Revoke Individual Permission Overrides (Priority: P2)

As an administrator I grant an extra permission or revoke an inherited one for a specific user with a reason, audited separately.

**Why this priority**: Enables least-privilege exceptions without creating new roles; required for SoD (Constitution VII).

**Independent Test**: Revoke inherited P2 with reason, verify P2 no longer effective; grant extra P4, verify effective, check both entries in AuditTrail and overrides grid.

**Acceptance Scenarios**:

1. **Given** inherited permission P2 from role **When** I revoke P2 with Reason="SoD conflict with P5" **Then** UserPermissions row created with IsGranted=false, Reason persisted, IsActive respected, audit recorded
2. **Given** permission P4 not in role **When** I grant P4 with Reason="temporary treasury access" **Then** UserPermissions IsGranted=true row created, effective permissions include P4
3. **Given** a previously revoked permission **When** I remove the revoke (delete UserPermissions row or set IsGranted handling per design) **Then** inherited permission becomes effective again (if still in role and active)
4. **Given** I attempt override without Reason or with empty reason **When** I submit **Then** system returns 400 validation

---

### User Story 5 - Single-Screen User Role & Overrides Management (Priority: P2)

As an administrator I manage a user's role and overrides from a single screen instead of separate role collection endpoints.

**Why this priority**: Consolidates UX, eliminates removed endpoints, ensures atomic perception (Constitution IX, X).

**Independent Test**: Navigate to /security/users/{id} (or equivalent single screen), change role via Select, revoke one inherited permission, grant one extra, save and verify all changes reflected without navigating to separate /roles collection page.

**Acceptance Scenarios**:

1. **Given** I am on the user detail screen **When** it loads **Then** single Select shows current RoleId (required, no empty default for existing user), inherited DataGrid shows read-only role permissions with revoke button per row, overrides DataGrid shows UserPermissions with grant/revoke state and Reason
2. **Given** I change the Select to a different role **When** I save **Then** PUT /{id}/role is called and inherited grid refreshes to new role's permissions
3. **Given** the old GET/POST/DELETE /{id}/roles endpoints are called **When** request is made **Then** system returns 404
4. **Given** user with validation errors (no role selected before save on orphan create) **When** I attempt save **Then** UI shows Arabic validation and does not call API

---

### Edge Cases

- Zero-role user after phase 1 blocks phase 2 NOT NULL conversion until precise mapping supplies a RoleId; migration must explicitly fail and report remaining NULL UserIds
- Multi-role user (2+ active UserRoles) must not auto-pick; CONFLICT_MULTI requires explicit PreciseMapping choice; if mapping missing, phase 2 fails
- Revoking a permission that is not inherited (not in role) MUST be rejected with 400 validation - no future-proof deny rows (Clarified 2026-09-02)
- Granting a permission already inherited via role MUST be rejected with 400 - no redundant IsGranted=true rows (Clarified 2026-09-02)
- EffectiveFrom/To on RolePermissions and UserPermissions: permissions with future EffectiveFrom or past EffectiveTo are not effective even if granted; server evaluates at transaction time (Constitution III)
- Concurrent role change + override change: RowVersion on Users covers role change; UserPermissions rows have their own concurrency if needed; role change does not silently discard pending override edits
- Deleting a Role that is assigned to users: FK Restrict blocks delete; DeleteRole must check assigned Users and return 409/400 with message
- Deactivating a user must preserve RoleId; reactivation keeps same role and overrides; no reset to NULL
- No default role assumption: creating a new user without RoleId must fail validation (400); no implicit assignment
- No hard delete of UserPermissions or AuditTrail; IsActive/deactivation pattern used (Constitution VI, VIII)
- Removing an existing override (revoke or grant row) MUST soft-delete via IsActive=false — no hard delete of UserPermissions regardless of action (Clarified 2026-09-02)
- During migration phase 1, any user whose RoleId is NULL MUST be denied login/access (fail closed) until migration completes; overrides alone do not grant login (Clarified 2026-09-02)

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST give every User a single `RoleId INT NOT NULL` FK -> SecurityRoles with `Restrict` delete behavior. Migration MUST be two-phase: (1) ADD `RoleId INT NULL` FK, (2) precise migration fills values, (3) ALTER to `NOT NULL`. No default role value; creation without RoleId MUST fail (400).
- **FR-002**: Effective permissions MUST be computed server-side as: `(RolePermissions of Users.RoleId where IsActive=true and EffectiveFrom/To window includes now) UNION (UserPermissions where IsGranted=true and IsActive and window) MINUS (UserPermissions where IsGranted=false)`. Computation MUST be the sole authority and evaluated at transaction time; frontend MUST NOT compute.
- **FR-003**: Revoking an inherited permission MUST create a `UserPermissions` row with `IsGranted=false` and non-empty `Reason`; granting an extra permission MUST create `IsGranted=true` with `Reason`. Both MUST be audited (AuditTrail + SecurityAuditLog) with actor, timestamp, old/new, context. `Reason` MUST be validated non-empty. Revoke MUST be rejected with 400 if permission is not currently in the user's effective role permissions; grant MUST be rejected with 400 if permission is already inherited via role (Clarified 2026-09-02).
- **FR-004**: Migration MUST produce three reporting lists before ALTER NOT NULL: `OK_SINGLE` (1 active UserRole -> auto-migrate), `CONFLICT_MULTI` (>1 active -> requires manual PreciseMapping), `ORPHAN_ZERO` (0 active -> requires manual PreciseMapping). CONFLICT/ORPHAN MUST be resolved only via explicit `PreciseMapping` dictionary `UserId->RoleId` supplied manually before phase 2 — applied via a manually-provided SQL script updating Users.RoleId, no application API code for mapping (Clarified 2026-09-02). If any `Users.RoleId IS NULL` remains after applying mappings, phase 2 MUST fail with list of remaining UserIds and MUST NOT execute ALTER.
- **FR-005**: `SetUserRole` (PUT /users/{id}/role) MUST replace the whole single role atomically, check `Role.IsActive=true`, verify `RowVersion` concurrency (409 on stale), and return 400 if Role inactive or not found, 404 if User not found, 409 on concurrency.
- **FR-006**: Every use case currently reading `UserRoles` MUST be updated to the new model: `PermissionService` (effective calc), `GetUserById` (return RoleId + effective), `DeactivateUser` (preserve role), `DeleteRole` (Restrict check against Users.RoleId). No reference to `UserRoles` may remain in Application/Web/Frontend (59 hits removed). All 7 dependent locations must be updated.
- **FR-007**: API MUST remove `GET /users/{id}/roles`, `POST /users/{id}/roles`, `DELETE /users/{id}/roles/{roleId}` and add `PUT /users/{id}/role` with body `{ roleId: int, rowVersion: string }`. OpenAPI MUST be regenerated. Frontend MUST provide a single screen: single `Select` for role (required) + inherited read-only `DataGrid` (role permissions with revoke button per row, disabled if already revoked) + overrides `DataGrid` (UserPermissions rows showing IsGranted, Reason, EffectiveFrom/To, IsActive, with actions to add grant/revoke and remove override). Screen MUST use RTL tokens, dark mode, and permission-gated navigation.
- **FR-008**: `AuditTrail` (entity-change, INSERT-ONLY, field diffs) and `SecurityAuditLog` (security decisions, actor, old/new, reason) MUST record every role assignment change and every UserPermissions grant/revoke. No hard delete of Users, Roles, or UserPermissions; deactivation via IsActive/soft delete only (Constitution VI, VIII). Removal of an existing override MUST soft-delete via `IsActive=false` and be audited identically to grant/revoke (Clarified 2026-09-02).
- **FR-009**: Permission evaluation MUST respect `IsActive` and `EffectiveFrom/To` windows on both `RolePermissions` and `UserPermissions`; expired/inactive entries MUST NOT be effective even if IsGranted=true (Constitution III).
- **FR-010**: All foreign keys introduced MUST use `DeleteBehavior.Restrict`; cascading deletes are prohibited (Constitution VI).
- **FR-011**: Every mutable record involved (Users, UserPermissions) MUST carry `RowVersion` concurrency token and verify it on update (Constitution VI).
- **FR-012**: Every endpoint and backing use case MUST declare a required named permission and fail closed on permission subsystem error with security audit denial logged (Constitution VII). During migration phase 1, any user whose `RoleId` is NULL MUST be denied access (fail closed); overrides alone do not grant access until a role is assigned (Clarified 2026-09-02).

### Key Entities

- **User**: Represents an authenticated actor. Attributes: Id, UserName, Email, IsActive, RoleId (FK -> SecurityRole, NOT NULL after phase 2), RowVersion, AuditTrail fields. Relationship: owns one Role, has many UserPermissions (overrides).
- **SecurityRole (SecurityRoles)**: Represents a collection of permissions. Attributes: Id, Name, NormalizedName, IsActive, RowVersion. Has many RolePermissions, referenced by many Users via RoleId.
- **RolePermission (RolePermissions)**: Links SecurityRole to Permission. Attributes: RoleId, PermissionId, IsActive, EffectiveFrom, EffectiveTo. Defines base permission set for the role.
- **UserPermission (UserPermissions)**: Override for a single user. Attributes: Id, UserId, PermissionId, IsGranted (true=grant, false=revoke), Reason (required), IsActive, EffectiveFrom, EffectiveTo, RowVersion. Revoke rows subtract from effective set; grant rows add.
- **Permission**: Named permission string (e.g., Accounting.ChartOfAccounts.Read). Referenced by RolePermissions and UserPermissions; evaluated by PermissionService.
- **AuditTrail**: INSERT-ONLY entity-change log. Attributes: actor, timestamp, action, entity, old/new diffs, request context. TRIGGER must reject UPDATE/DELETE.
- **SecurityAuditLog**: Security decision log. Attributes: actor, timestamp, decision (grant/deny), reason, oldRoleId/newRoleId or permission override diff. Used for FR-008.

## Clarifications

### Session 2026-09-02

- Q: Should the system reject a revoke for a permission not in the user's role and reject a grant for a permission already inherited, returning 400? → A: Option A - Reject revoke if not in role (400) and reject grant if already inherited via role (400).
- Q: How should an administrator provide the PreciseMapping (UserId → RoleId) for CONFLICT_MULTI and ORPHAN_ZERO users before phase 2? → A: Option C - SQL script manually updating Users.RoleId before ALTER, no app code for mapping.
- Q: When removing an existing override (restore inherited permission), should the system hard-delete or soft-delete the UserPermissions row? → A: Option B - Soft-delete via IsActive=false, preserving row and history.
- Q: What happens to a user's overrides and role during migration when RoleId may be NULL? → A: Option A - Deny login/access for any user whose RoleId is NULL (fail closed) until migration completes.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: After phase 2 migration, 100% of Users have RoleId NOT NULL; `SELECT COUNT(*) WHERE RoleId IS NULL` returns 0 and schema enforces NOT NULL constraint in production
- **SC-002**: Migration report correctly categorizes 100% of users: every user with 0/1/>1 active UserRoles appears in exactly one of ORPHAN_ZERO/OK_SINGLE/CONFLICT_MULTI with accurate RoleIds, verified by script count
- **SC-003**: Effective permission calculation matches specification for 100% of test matrix: role-only, grant-only, revoke-only, grant+revoke, expired/inactive filtered, future EffectiveFrom excluded – verified by unit + functional tests
- **SC-004**: SetUserRole rejects 100% of stale RowVersion attempts with 409 and 100% of inactive-role assignments with 400; happy-path completes in under 500ms p95 in functional tests
- **SC-005**: Old endpoints GET/POST/DELETE /{id}/roles return 404 and new PUT /{id}/role succeeds for authorized users; OpenAPI generation includes new endpoint and excludes old ones
- **SC-006**: Frontend single screen allows role change, revoke inherited, grant extra, and remove override all without navigation away; manual test completes full flow (create user with role -> revoke -> grant -> verify effective) in under 3 minutes with zero 404s for old endpoints
- **SC-007**: Every role change and permission override creates exactly one AuditTrail row and one SecurityAuditLog row with old/new values and actor; no hard deletes occur and INSERT-ONLY trigger rejects UPDATE/DELETE on AuditTrail

## Assumptions

- UserPermissions entity/table already exists with IsGranted, Reason, IsActive, EffectiveFrom/To, RowVersion; if not, it will be created as part of this feature with the same semantics
- PreciseMapping (UserId->RoleId for CONFLICT_MULTI and ORPHAN_ZERO) is provided manually by an administrator before running phase 2; the system does not auto-decide multi-role conflicts
- SecurityRoles referenced by Users.RoleId are pre-seeded and IsActive is maintained; no new role is auto-created for orphans
- RolePermissions already carry IsActive and EffectiveFrom/To; evaluation semantics are unchanged except for the new single-role source
- No default role is desired; product explicitly requires explicit role assignment on user creation
- Existing UserRoles EffectiveFrom/To windows define "active" as IsActive=true or soft-delete false and current time within window; this definition is reused for report categorization
- Single role is sufficient for SoD; any former need for simultaneous multi-roles is replaced by role + individual overrides model
