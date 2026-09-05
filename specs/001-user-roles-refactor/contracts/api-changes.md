# API Contracts: User-Roles Refactor

**Date**: 2026-09-02

## Endpoints Removed

| Method | Path | Reason |
|--------|------|--------|
| GET | `/api/Users/{id}/roles` | Replaced by single role in UserDetailDto |
| POST | `/api/Users/{id}/roles` | Replaced by PUT /{id}/role |
| DELETE | `/api/Users/{id}/roles/{roleId}` | Replaced by PUT /{id}/role |

## Endpoints Added

### PUT /api/Users/{id}/role

Set the user's single security role. Replaces any prior assignment.

**Request**:
```json
{
  "roleId": 5
}
```

**Response**: `204 No Content`

**Errors**:
- `400` — User not found, Role not found, Role is not active
- `409` — Concurrency conflict (RowVersion mismatch)

**Handler**: `SetUserRoleCommand`

### GET /api/Users/{id}/permissions (MODIFIED)

Now returns effective permissions (role-inherited + overrides) rather than just direct overrides.

**Response**:
```json
[
  {
    "permissionId": 12,
    "code": "Accounting.JournalEntries.Post",
    "name": "Post Journal Entries",
    "source": "role",
    "isGranted": true,
    "reason": null
  },
  {
    "permissionId": 45,
    "code": "Users.ManageRoles",
    "name": "Manage User Roles",
    "source": "override",
    "isGranted": false,
    "reason": "SoD restriction — user should not manage roles"
  }
]
```

**`source` values**: `"role"` (inherited from assigned role) | `"override"` (UserPermission entry)

## Endpoints Modified

### GET /api/Users/{id} (MODIFIED)

The `roles` field in `UserDetailDto` is replaced by a single `role` object.

**Before**:
```json
{
  "roles": [
    { "roleId": 5, "code": "ACCT_SR", "name": "Senior Accountant", "effectiveFrom": "...", "effectiveTo": null }
  ]
}
```

**After**:
```json
{
  "role": {
    "roleId": 5,
    "code": "ACCT_SR",
    "name": "Senior Accountant"
  }
}
```

### POST /api/Users/{id}/permissions (MODIFIED)

Now requires `reason` field (clarified: required for both grants and revocations).

**Request**:
```json
{
  "permissionId": 45,
  "isGranted": false,
  "reason": "SoD restriction — user should not manage roles"
}
```

**Errors**:
- `400` — Missing reason, user not found, permission not found

### GET /api/Users/{id}/audit (UNCHANGED)

Returns audit trail entries including role changes and permission overrides.

## DTO Changes

### UserDetailDto (MODIFIED)

```csharp
public class UserDetailDto
{
    // ... existing fields unchanged ...
    public UserRoleDto? Role { get; init; }    // Changed: single role (was List<UserRoleDto> Roles)
    // ...
}
```

### UserRoleDto (MODIFIED)

Remove `EffectiveFrom`/`EffectiveTo` (no longer applicable):

```csharp
public class UserRoleDto
{
    public int RoleId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    // EffectiveFrom/EffectiveTo removed
}
```

### EffectivePermissionDto (NEW)

```csharp
public class EffectivePermissionDto
{
    public int PermissionId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty; // "role" | "override"
    public bool IsGranted { get; init; }
    public string? Reason { get; init; }
}
```
