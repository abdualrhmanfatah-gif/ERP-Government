# Data Model: User Management

**Date**: 2026-09-02
**Feature**: 004-user-management-frontend-backend

## Entities

### User

Represents a system account in the Security module.

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK, auto-increment | |
| Login | string | Required, unique (case-insensitive) | Natural key, max 256 chars |
| Name | string | Required | Display name |
| ExternalAuthId | string? | Nullable | SSO/external auth identifier |
| IsActive | bool | Required, default true | Deactivation flag |
| PasswordHash | string? | Nullable | Hashed password (never exposed via API) |
| FailedLoginAttempts | int | Required, default 0 | |
| LockedUntil | DateTimeOffset? | Nullable | Account lock expiry |
| PasswordChangedAt | DateTimeOffset? | Nullable | |
| PasswordResetToken | string? | Nullable | |
| PasswordResetTokenExpiry | DateTimeOffset? | Nullable | |
| LastLoginAt | DateTimeOffset? | Nullable | |
| MfaEnabled | bool | Required, default false | |
| MfaMethod | string? | Nullable | |
| MustChangePassword | bool | Required, default false | |
| AccountType | AccountType (enum) | Required | Internal/External/etc. |
| DepartmentId | int? | Nullable, FK → Department | Cross-module reference |
| RowVersion | byte[] | Required, concurrency token | Optimistic concurrency |
| CreatedAt | DateTimeOffset | Audit field | From BaseAuditableEntity |
| CreatedBy | string? | Audit field | From BaseAuditableEntity |
| ModifiedAt | DateTimeOffset? | Audit field | From BaseAuditableEntity |
| ModifiedBy | string? | Audit field | From BaseAuditableEntity |

**State Transitions**:
- Active → Inactive (deactivate): Revokes all sessions, blocks authentication
- Inactive → Active (reactivate): Restores authentication capability

### UserRole

Assignment of a role to a user (many-to-many join).

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK | |
| UserId | int | FK → User, required | Restrict on delete |
| RoleId | int | FK → Role, required | Restrict on delete |
| AssignedAt | DateTimeOffset | Required | Assignment timestamp |

**Constraints**: Unique (UserId, RoleId) — duplicate assignment prevented (FR-011).

### UserPermission

Direct permission assignment to a user (bypasses roles).

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK | |
| UserId | int | FK → User, required | Restrict on delete |
| PermissionId | int | FK → Permission, required | Restrict on delete |
| AssignedAt | DateTimeOffset | Required | |

### UserSession

Active authenticated session for a user.

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK | |
| UserId | int | FK → User, required | Restrict on delete |
| DeviceInfo | string | Required | Browser/device identifier |
| IpAddress | string | Required | Client IP |
| LoginAt | DateTimeOffset | Required | Session start |
| RevokedAt | DateTimeOffset? | Nullable | Null = active; set on revoke |

### AuditEntry (Entity-Change Record)

Immutable record of changes to user accounts.

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK | |
| EntityName | string | Required | "User" |
| EntityId | int | Required | User.Id |
| Action | string | Required | Created/Updated/Deactivated/etc. |
| Actor | string | Required | Who performed the action |
| Timestamp | DateTimeOffset | Required | When the change occurred |
| OldValues | string (JSON) | Nullable | Before state (null on create) |
| NewValues | string (JSON) | Nullable | After state |
| RequestPath | string? | Nullable | HTTP request context |

**Constraints**: INSERT-ONLY per Constitution Principle VIII. No UPDATE or DELETE allowed.

## Relationships

```
User 1──N UserRole N──1 Role
User 1──N UserPermission N──1 Permission
User 1──N UserSession
User 1──N AuditEntry
User N──1 Department (cross-module read)
```

## Validation Rules (from FR)

- Login: required, unique case-insensitive (FR-005)
- Name: required
- Department: required on create (FR-004)
- AccountType: required on create (FR-004)
- Password: required on create, minimum complexity (FR-004)
- Deactivation: blocked if target is current user (FR-017)
- Role removal: blocked if removing own last admin role (FR-018)
- SoD: blocked if assignment creates conflict (FR-013/FR-021)
- Concurrency: RowVersion must match on update (FR-019/FR-020)
