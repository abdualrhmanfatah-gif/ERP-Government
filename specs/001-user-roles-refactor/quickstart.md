# Quickstart Validation: User-Roles Refactor

**Date**: 2026-09-02
**Branch**: `001-user-roles-refactor`

## Prerequisites

- .NET 10 SDK installed
- SQL Server available (via Aspire or local)
- `dotnet restore` completed at repo root
- Frontend dependencies installed (`cd src/Web/ClientApp && npm install`)

## Validation Scenarios

### Scenario 1: Schema Deployment (FR-001, FR-004)

**Goal**: Verify Users table has RoleId NOT NULL with FK, UserRoles table is dropped.

```bash
dotnet test tests/Infrastructure.IntegrationTests --filter "FullyQualifiedName~Security"
```

**Expected**: All security integration tests pass. The migration creates `RoleId INT NOT NULL` on Users with FK to SecurityRoles. The `UserRoles` table no longer exists.

### Scenario 2: SetUserRole Command (FR-005)

**Goal**: Verify role assignment replaces the entire role with RowVersion concurrency.

```bash
dotnet test tests/Application.UnitTests --filter "FullyQualifiedName~SetUserRole"
```

**Expected tests**:
- Assigning a role sets Users.RoleId
- Assigning a new role replaces the old one
- Concurrent update (stale RowVersion) returns concurrency error
- Inactive role is rejected

### Scenario 3: PermissionService Resolution (FR-002)

**Goal**: Verify effective permissions = RolePermissions ∪ UserPermissions(IsGranted=true) \ UserPermissions(IsGranted=false).

```bash
dotnet test tests/Application.UnitTests --filter "FullyQualifiedName~PermissionService"
```

**Expected tests**:
- Role-only permissions returned correctly
- UserPermission IsGranted=true adds to effective set
- UserPermission IsGranted=false removes from effective set
- Inactive role contributes no permissions
- Future EffectiveFrom role contributes no permissions

### Scenario 4: API Endpoint Contract (FR-007)

**Goal**: Verify removed and added endpoints.

```bash
dotnet test tests/Web.AcceptanceTests --filter "FullyQualifiedName~Users"
```

**Expected**:
- `GET /api/Users/{id}/roles` returns 404 (removed)
- `POST /api/Users/{id}/roles` returns 404 (removed)
- `DELETE /api/Users/{id}/roles/{roleId}` returns 404 (removed)
- `PUT /api/Users/{id}/role` accepts `{ roleId: N }` and returns 204

### Scenario 5: Override with Reason (FR-003)

**Goal**: Verify override operations require Reason and are audited.

```bash
dotnet test tests/Application.UnitTests --filter "FullyQualifiedName~UserPermission"
```

**Expected**:
- Creating override without Reason returns validation error
- Creating override with Reason succeeds
- Override is recorded in SecurityAuditLog

### Scenario 6: Frontend Unified Screen

**Goal**: Verify the unified role + overrides screen loads and functions.

Manual validation:
1. Navigate to `/security/users/{id}`
2. Click the "الأدوار" (Roles) tab
3. Verify: single role dropdown shows current role
4. Verify: inherited permissions table (right column in RTL) shows role permissions, read-only
5. Verify: overrides table (left column in RTL) shows direct overrides
6. Click revoke on an inherited permission → Reason dialog appears → save creates override
7. Change role via dropdown → inherited permissions update → overrides persist

### Scenario 7: Audit Trail (FR-008)

**Goal**: Verify role changes and overrides generate audit records.

```bash
dotnet test tests/Application.FunctionalTests --filter "FullyQualifiedName~AuditTrail"
```

**Expected**:
- Role change creates AuditTrail entry (oldRoleId, newRoleId, actor, timestamp)
- Role change creates SecurityAuditLog entry
- Override create/update creates both audit records

## Running All Tests

```bash
dotnet test
```

**Expected**: All tests pass, zero failures.
