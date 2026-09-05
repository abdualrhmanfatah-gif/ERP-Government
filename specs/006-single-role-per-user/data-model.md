# Data Model: Single Role Per User (Option A)

**Feature**: `006-single-role-per-user` | **Date**: 2026-09-02
**Spec**: [spec.md](./spec.md) | **Research**: [research.md](./research.md)

## Overview

Replaces the `UserRoles` M:N join with a single `RoleId` FK on `User`. Effective permissions = role permissions (via `User.RoleId` → `RolePermission` → `SecurityPermission`) **UNION** `UserPermission` grants **MINUS** `UserPermission` revokes, computed server-side at transaction time.

---

## Entity Changes

### User (MODIFY) — `Users` table

| Field | Type | Notes |
|-------|------|-------|
| Id | int | PK (inherited BaseAuditableEntity) |
| RoleId | int? → **int NOT NULL** | NEW. FK → SecurityRoles.Restrict. NULL during phase 1, NOT NULL after phase 2b. |
| Login | string | unique index |
| RowVersion | byte[] | rowversion, concurrency |
| ...existing | | unchanged |

**FK**: `RoleId → SecurityRoles.Id` `DeleteBehavior.Restrict` (FR-010).
**Migration**: phase 1 adds nullable FK; phase 2 fills + ALTTERs NOT NULL (FR-001).

*Note*: `DepartmentId` (FK) FK also Restrict per existing config.

### UserRole (DELETE) — `UserRoles` table

M:N join `(UserId, RoleId, EffectiveFrom)` — removed entirely. Composite PK, `UserConfiguration`-analog `UserRoleConfiguration.cs` deleted. All references removed (FR-006).

### UserPermission (MODIFY) — `UserPermissions` table

| Field | Type | Notes |
|-------|------|-------|
| Id | int | PK |
| UserId | int | FK → Users.Restrict |
| PermissionId | int | FK → SecurityPermissions.Restrict |
| IsGranted | bool | true=grant, false=revoke (default true) |
| **IsActive** | **bool (NEW)** | =true default. Soft-delete: override removal sets false. |
| EffectiveFrom | DateTimeOffset? | window start |
| EffectiveTo | DateTimeOffset? | window end |
| Reason | string (max 500) | REQUIRED for grant/revoke (FR-003). Currently nullable → make required (non-empty validated). |
| ApprovedById | int? | FK → Users.Restrict (existing) |
| RowVersion | byte[] | concurrency (FR-011) |

**Unique index**: `(UserId, PermissionId, EffectiveFrom)` — but with soft-delete, a re-grant after deactivation hits the unique constraint. **Research note**: need to decide index strategy. For soft-delete + re-insert, the unique index must account for `IsActive` (e.g., filtered index `WHERE IsActive = 1`, or null EffectiveFrom for inactive). **Open in implementation** — see Edge Cases.

**Restrict FKs**: all `DeleteBehavior.Restrict` (FR-010).

### SecurityRole — `SecurityRoles` table (UNCHANGED)

`Id, Code, Name, Description, RoleLevel, IsMutuallyExclusive, ExclusiveWithRoleId, RequiresMfa, MaxSessionDuration, IsSystem, IsAdmin, IsActive, RowVersion`. Referenced by `User.RoleId`; `DeleteRoleCommand` Restrict-check against `Users.RoleId` (FR-006).

### RolePermission — `RolePermissions` table (UNCHANGED)

`RoleId, PermissionId, RowVersion`. Links role → base permission set. Evaluated via `User.RoleId`.

### SecurityPermission — `SecurityPermissions` table (UNCHANGED)

`Id, Module, Action, Code, Name, PermissionLevel, IsSensitive, DataScope, IsActive, RowVersion`. Named permission strings (e.g., `Accounting.ChartOfAccounts.Read`).

---

## Effective Permission Calculation (FR-002)

Computed in `PermissionService.GetPermissionsAsync(userId)` at transaction time:

```
rolePermissions = RolePermissions(RoleId = User.RoleId)
                  join SecurityRole(IsActive=true)
                  join SecurityPermission(IsActive=true)
                  → code set

grants  = UserPermissions where UserId=X AND IsGranted=true  AND IsActive=true AND EffectiveFrom<=now AND (EffectiveTo>=now OR null) → code set
revokes = UserPermissions where UserId=X AND IsGranted=false AND IsActive=true AND EffectiveFrom<=now AND (EffectiveTo>=now OR null) → code set

effective = (rolePermissions ∪ grants) \ revokes
```

**Fail closed** (FR-012): if `User.RoleId` is NULL, deny all (empty set) — migration window / orphan.
**Not computed in frontend** — server sole authority (FR-002).

---

## Validation Rules (server-side, FluentValidation)

| Rule | FR | Message (Arabic) |
|------|----|------|
| User creation requires RoleId (no default) | FR-001 | "يجب تحديد دور المستخدم" |
| SetUserRole: role exists + Role.IsActive=true | FR-005 | "الدور غير نشط" / "الدور غير موجود" |
| SetUserRole: RowVersion matches (else 409) | FR-005 | "تعارض التزامن" |
| Grant: permission not already inherited via role → else 400 | FR-003 | "الصلاحية موروثة بالفعل من الدور" |
| Revoke: permission IS inherited via role → else 400 | FR-003 | "الصلاحية ليست موروثة من الدور" |
| Grant/Revoke: Reason non-empty (max 500) | FR-003 | "السبب مطلوب" |
| EffectiveFrom <= EffectiveTo if both set | FR-009 | "النطاق الزمني غير صالح" |

---

## State / Lifecycle Transitions

- **User role**: no soft-delete interplay; setting a role just replaces `RoleId` (atomic, FR-005). Deactivate preserves `RoleId` (FR-006); reactivation keeps it.
- **Override lifecycle**: active → soft-delete (IsActive=false) on removal. Re-grant creates a new active row or reactivates (unique-index strategy open — see Edge Cases).
- **Role deletion**: blocked with 409/400 if any `Users.RoleId` references (Restrict, FR-006).

---

## Migration (FR-001/FR-004)

1. **Migration M1**: add `RoleId int?` FK Restrict to `Users`.
2. **Report**: categorize OK_SINGLE / CONFLICT_MULTI / ORPHAN_ZERO from pre-migration UserRoles data (script).
3. **PreciseMapping**: manual SQL updates `Users.RoleId` for CONFLICT_MULTI/ORPHAN_ZERO (clarified: SQL script, no app API).
4. **Migration M2**: `ALTER COLUMN RoleId INT NOT NULL` + add `IsActive` to UserPermission + make Reason required. Fails if any NULL remains listing UserIds (FR-001).
5. Drop `UserRoles` table/entity.

---

## Audit (FR-008)

- **SecurityAuditLog**: every role change (oldRoleId→newRoleId) and every grant/revoke/override-removal (permission, IsGranted old/new, Reason, actor, timestamp).
- **AuditTrail**: INSERT-ONLY entity-change diffs on User (RoleId) and UserPermission changes.
