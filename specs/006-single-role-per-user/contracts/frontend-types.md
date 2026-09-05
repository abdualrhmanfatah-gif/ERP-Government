# Frontend Contract: TypeScript Types for Security Single-Role

**Feature**: `006-single-role-per-user` | **Date**: 2026-09-02
**Source**: mirrors published OpenAPI (Constitution IX). Frontend modules must conform; divergence is a defect.

Located under `src/Web/ClientApp/src/features/security/users/types.ts` (and regenerated `web-api-client.ts`).

## User detail (single role) — replaces `UserRoleDto[]`

```ts
export interface UserDetailDto {
  id: number;
  login: string;
  externalAuthId?: string | null;
  accountType: string;
  isActive: boolean;
  departmentId?: number | null;
  departmentName?: string | null;
  mfaEnabled: boolean;
  mustChangePassword: boolean;
  lastLoginAt?: string | null;
  failedLoginAttempts: number;
  isLocked: boolean;
  // role — single, replaces `roles: UserRoleDto[]`
  roleId?: number | null;
  roleName?: string | null;
  effectivePermissions: string[]; // server-computed
  activeSessionCount: number;
  createdAt: string;
  // ...audit fields
}
```

## Set User Role command

```ts
export interface SetUserRoleCommand {
  roleId: number;       // required
  rowVersion: string;   // required (base64) — round-trip for 409
}
```

## UserPermission override (overrides grid)

```ts
export interface UserPermissionDto {
  id: number;
  userId: number;
  permissionId: number;
  permissionCode: string;
  permissionName: string;
  isGranted: boolean;   // true=grant, false=revoke
  isActive: boolean;    // active vs soft-deleted
  reason?: string | null;
  effectiveFrom?: string | null;
  effectiveTo?: string | null;
  approvedById?: number | null;
}

export interface AssignUserPermissionCommand {
  permissionId: number;
  isGranted: boolean;   // true=extra grant, false=inherited revoke
  effectiveFrom?: string | null;
  effectiveTo?: string | null;
  reason: string;       // required non-empty
  rowVersion: string;   // round-trip
}
```

## Client functions (`client.ts`)

- `getUserDetail(id)` → `UserDetailDto` (now carries `roleId`, `effectivePermissions`)
- `setUserRole(id, body: SetUserRoleCommand)` → `PUT /api/Users/{id}/role`
- `getUserPermissions(id)` → `UserPermissionDto[]`
- `assignUserPermission(id, body)` → `POST /api/Users/{id}/permissions`
- `removeUserPermission(id, permissionId)` → `DELETE /api/Users/{id}/permissions/{permissionId}` (soft-delete)

Removed: `getUserRoles`, `assignUserRole`, `removeUserRole` (and `UserRoleDto[]` collection usage).

## Policy constants (`shared/constants/permissions.ts`)

Add `Security.UsersView`, `Security.UsersManageRoles` to `PERMISSIONS` and extend `PolicyString` in `shared/hooks/usePermission.ts` so nav + gating matches backend policies (Constitution X, X).
