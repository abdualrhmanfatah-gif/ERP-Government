# API Contracts: User Management

**Date**: 2026-09-02
**Feature**: 004-user-management-frontend-backend

**Note**: The backend OpenAPI spec is auto-generated at build time (`src/Web/wwwroot/openapi/v1.json`). The frontend TypeScript client is generated via NSwag. This document captures the key endpoint contracts for reference during implementation.

## Endpoints

### Users — CRUD

| Method | Path | Permission | Description |
|--------|------|------------|-------------|
| GET | `/api/Users` | UsersView | List users (query: search, status, department, page, pageSize) |
| GET | `/api/Users/{id}` | UsersView | Get user detail |
| POST | `/api/Users` | UsersCreate | Create user |
| PUT | `/api/Users/{id}` | UsersUpdate | Update user |

### Users — Lifecycle

| Method | Path | Permission | Description |
|--------|------|------------|-------------|
| POST | `/api/Users/{id}/deactivate` | UsersDeactivate | Deactivate user + revoke sessions |
| POST | `/api/Users/{id}/reactivate` | UsersDeactivate | Reactivate user |
| POST | `/api/Users/{id}/reset-failed-login-attempts` | UsersResetLogin | Reset lockout counter |

### Users — Roles

| Method | Path | Permission | Description |
|--------|------|------------|-------------|
| GET | `/api/Users/{id}/roles` | UsersView | List assigned roles |
| POST | `/api/Users/{id}/roles` | UsersManageRoles | Assign role (body: `{ roleId }`) |
| DELETE | `/api/Users/{id}/roles/{roleId}` | UsersManageRoles | Remove role |

### Users — Permissions

| Method | Path | Permission | Description |
|--------|------|------------|-------------|
| GET | `/api/Users/{id}/permissions` | UsersView | List direct permissions |
| POST | `/api/Users/{id}/permissions` | UsersManageRoles | Assign permission |
| DELETE | `/api/Users/{id}/permissions/{permissionId}` | UsersManageRoles | Remove permission |

### Users — Sessions

| Method | Path | Permission | Description |
|--------|------|------------|-------------|
| GET | `/api/Users/{id}/sessions` | UsersManageSessions | List active sessions |
| POST | `/api/Users/{id}/sessions/{sessionId}/revoke` | UsersManageSessions | Revoke one session |
| POST | `/api/Users/{id}/sessions/revoke-all` | UsersManageSessions | Revoke all sessions |

## Key DTOs

### UserDto (List View)
```json
{
  "id": 1,
  "login": "jsmith",
  "accountType": "Internal",
  "isActive": true,
  "departmentId": 5,
  "departmentName": "Finance",
  "mfaEnabled": false,
  "lastLoginAt": "2026-09-01T10:30:00Z",
  "failedLoginAttempts": 0,
  "isLocked": false,
  "createdAt": "2026-01-15T08:00:00Z"
}
```

### UserDetailDto
```json
{
  "id": 1,
  "login": "jsmith",
  "accountType": "Internal",
  "isActive": true,
  "departmentId": 5,
  "departmentName": "Finance",
  "mfaEnabled": false,
  "mustChangePassword": false,
  "lastLoginAt": "2026-09-01T10:30:00Z",
  "failedLoginAttempts": 0,
  "isLocked": false,
  "activeSessionCount": 2,
  "roles": [
    { "roleId": 3, "code": "FINANCE_ADMIN", "name": "Finance Admin", "effectiveFrom": "2026-01-15", "effectiveTo": null }
  ],
  "createdAt": "2026-01-15T08:00:00Z",
  "createdBy": "admin",
  "updatedAt": "2026-08-20T14:00:00Z",
  "updatedBy": "admin"
}
```

### CreateUserCommand
```json
{
  "login": "newuser",
  "name": "New User",
  "departmentId": 5,
  "accountType": "Internal",
  "password": "SecureP@ss123"
}
```

### UpdateUserCommand
```json
{
  "id": 1,
  "name": "Updated Name",
  "departmentId": 5,
  "accountType": "Internal",
  "rowVersion": "AAAAAAAAB9k="
}
```

### AssignRoleCommand
```json
{
  "userId": 1,
  "roleId": 3
}
```

## Error Responses

All errors use the problem-details contract (Principle IX):

| Status | Meaning |
|--------|---------|
| 400 | Validation error (field-level errors in `errors`) |
| 401 | Unauthenticated |
| 403 | Forbidden (permission denied) |
| 404 | User not found |
| 409 | Concurrency conflict (RowVersion mismatch) |
