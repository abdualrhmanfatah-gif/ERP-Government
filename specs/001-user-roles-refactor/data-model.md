# Data Model: User-Roles Refactor

**Date**: 2026-09-02
**Branch**: `001-user-roles-refactor`

## Entity Changes

### User (MODIFIED)

Add `RoleId` foreign key to link directly to SecurityRole.

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK, Identity | Unchanged |
| Login | string(100) | NOT NULL, Unique | Unchanged |
| ExternalAuthId | string(200) | Nullable | Unchanged |
| IsActive | bool | NOT NULL, default true | Unchanged |
| **RoleId** | **int** | **NOT NULL, FK -> SecurityRoles.Restrict** | **NEW** |
| ... | ... | ... | All existing fields unchanged |
| RowVersion | byte[] | RowVersion | Unchanged |

**FK**: `FK_Users_RoleId` -> `SecurityRoles(Id)` ON DELETE Restrict

**Index**: `IX_Users_RoleId` (non-unique, for join performance)

### UserRole (DROPPED)

The entire `UserRoles` table is dropped. The composite PK `{UserId, RoleId, EffectiveFrom}` and the `EffectiveFrom`/`EffectiveTo` time-windowing are no longer needed.

**Migration action**: `DROP TABLE UserRoles`

### UserPermission (MODIFIED)

Change unique constraint from `{UserId, PermissionId, EffectiveFrom}` to `{UserId, PermissionId}` per clarified spec (one override per user-permission pair).

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK, Identity | Unchanged |
| UserId | int | NOT NULL, FK -> Users.Restrict | Unchanged |
| PermissionId | int | NOT NULL, FK -> SecurityPermissions.Restrict | Unchanged |
| IsGranted | bool | NOT NULL, default true | Unchanged |
| EffectiveFrom | DateTimeOffset? | Nullable | Unchanged |
| EffectiveTo | DateTimeOffset? | Nullable | Unchanged |
| Reason | string(500) | Nullable | Unchanged (but UI validates non-empty) |
| ApprovedById | int? | FK -> Users.Restrict | Unchanged |
| RowVersion | byte[] | RowVersion | Unchanged |

**Index change**: Replace `IX_UserPermissions_UserId_PermissionId_EffectiveFrom` (unique) with `IX_UserPermissions_UserId_PermissionId` (unique)

### SecurityRole (UNCHANGED)

No schema changes. Referenced by `Users.RoleId` FK.

### RolePermission (UNCHANGED)

No schema changes. Join table for role-to-permission mapping.

### SecurityPermission (UNCHANGED)

No schema changes. Reference table for permission definitions.

## Relationship Diagram (After Refactor)

```
SecurityRole (1) ──────< (N) User
                              │
                              │
User (1) ────────────────< (N) UserPermission
SecurityPermission (1) ───< (N) UserPermission

SecurityRole (1) ────────< (N) RolePermission
SecurityPermission (1) ──< (N) RolePermission
```

## Effective Permission Resolution Formula

```
Effective(User) = 
    RolePermissions(User.RoleId)      -- where Role.IsActive && EffectiveFrom/To
    ∪ UserPermissions(User) WHERE IsGranted = true
    \ UserPermissions(User) WHERE IsGranted = false
```

Where:
- `RolePermissions(RoleId)` = `RolePermission WHERE RoleId = @RoleId` → `SecurityPermission WHERE IsActive = true`
- `UserPermissions(UserId)` = `UserPermission WHERE UserId = @UserId` AND `EffectiveFrom/To` window active

## Migration Script Outline

```sql
-- Phase 1: Add RoleId column (nullable initially for safety)
ALTER TABLE Users ADD RoleId INT NULL;
ALTER TABLE Users ADD CONSTRAINT FK_Users_RoleId FOREIGN KEY (RoleId) REFERENCES SecurityRoles(Id);
CREATE INDEX IX_Users_RoleId ON Users(RoleId);

-- Phase 2: Set NOT NULL (after all users have RoleId assigned via API)
ALTER TABLE Users ALTER COLUMN RoleId INT NOT NULL;

-- Phase 3: Drop old join table
DROP TABLE UserRoles;

-- Phase 4: Update UserPermission unique index
DROP INDEX IX_UserPermissions_UserId_PermissionId_EffectiveFrom ON UserPermissions;
CREATE UNIQUE INDEX IX_UserPermissions_UserId_PermissionId ON UserPermissions(UserId, PermissionId);
```

Note: Since no legacy data exists, phases 1 and 2 can be combined directly as `RoleId INT NOT NULL` in the migration.
