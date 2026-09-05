# HTTP Contract: Security — Single Role & Overrides

**Feature**: `006-single-role-per-user` | **Date**: 2026-09-02
**Source of truth**: backend-generated OpenAPI (Constitution IX). This document describes the contract shape; actual serialization is regenerated via NSwag into `web-api-client.ts`.

**Endpoint base**: `/api/Users` (routed from class name `Users`, per `WebApplicationExtensions.cs`).

---

## Removed (FR-007) — return 404

| Method | Route | Old Handler |
|--------|-------|-------------|
| GET | `/api/Users/{id}/roles` | GetUserRoles |
| POST | `/api/Users/{id}/roles` | AssignUserRole |
| DELETE | `/api/Users/{id}/roles/{roleId}` | RemoveUserRole |

These are removed and return 404 (SC-005). Replaced by `PUT /api/Users/{id}/role`.

## Added — `PUT /api/Users/{id}/role`

Sets the user's single role atomically.

**Request**

```
PUT /api/Users/{id}/role
Authorization: Bearer <token>
Content-Type: application/json

{
  "roleId": 3,                       // required int
  "rowVersion": "/wGiT..."            // required string (base64 rowversion)
}
```

**Responses**

| Status | Condition | Body |
|--------|-----------|------|
| 200 | Role set successfully; history recorded old→new | `{ "succeeded": true }` |
| 400 | Role inactive ("Role is inactive") / Role not found / empty request | problem-details (Arabic) |
| 401 | Unauthenticated | problem-details |
| 403 | Missing `Security.UsersManageRoles` permission | problem-details |
| 404 | User not found | problem-details |
| 409 | Stale RowVersion concurrency conflict | problem-details (FR-005/SC-004) |

**Permission**: `Security.UsersManageRoles` (FR-012).

---

## Modified — `GET /api/Users/{id}` (detail)

`UserDetailDto.Roles` (list of `UserRoleDto`) is **replaced** by a single `RoleId` + `RoleName` + `EffectivePermissions` (computed server-side). New shape:

```json
{
  "id": 5,
  "login": "finance.lead@erp.gov",
  "roleId": 3,
  "roleName": "Finance Manager",
  "effectivePermissions": ["Accounting.ChartOfAccounts.Read", "Budgeting.Create"],
  "isActive": true,
  "...existing fields...": "..."
}
```

**Permission**: `Security.UsersView`.

## Modified — `UserPermissionDto` (overrides grid)

Adds `IsActive` (to distinguish active vs soft-deleted override) and `IsGranted`/`Reason` remain.

```json
{
  "id": 12,
  "userId": 5,
  "permissionId": 8,
  "permissionCode": "Budgeting.Grant",
  "permissionName": "منح الميزانية",
  "isGranted": true,
  "isActive": true,
  "reason": "تخويل مؤقت لفرق التمويل",
  "effectiveFrom": "2026-01-01T00:00:00Z",
  "effectiveTo": null,
  "approvedById": 1
}
```

## Unchanged (permissions sub-resource)

- `GET /api/Users/{id}/permissions` (list active grants+revokes) — `Security.UsersView`
- `POST /api/Users/{id}/permissions` (grant/revoke with Reason) — `Security.UsersManageRoles`
- `DELETE /api/Users/{id}/permissions/{permissionId}` (soft-delete override) — `Security.UsersManageRoles`

---

## Error/Status Semantics (Constitution IX)

- Uniform problem-details body throughout.
- 400 validation, 401 unauthenticated, 403 forbidden, 404 not found, 409 concurrency (distinct).
- Every mutating op round-trips `RowVersion` from frontend to backend.

## Regeneration

Run `npm run generate-api` (NSwag) in `src/Web/ClientApp` after backend OpenAPI changes; `web-api-client.ts` auto-generated (Constitution IX). The removed endpoints disappear from the client; new `PUT /role` and modified detail DTO appear.
