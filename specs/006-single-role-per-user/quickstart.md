# Quickstart / Validation Guide: Single Role Per User (Option A)

**Feature**: `006-single-role-per-user` | **Date**: 2026-09-02
**Spec**: [spec.md](./spec.md) | **Data Model**: [data-model.md](./data-model.md) | **Contracts**: [contracts/http-api.md](./contracts/http-api.md)

Runnable validation scenarios proving the feature works end-to-end. Implementation details live in `tasks.md`.

---

## Prerequisites

- .NET SDK 10 (`global.json` pins 10.0.201).
- Node/npm in `src/Web/ClientApp`.
- SQL Server reachable; EF migrations apply via `dotnet ef` or startup host.
- Admin with `Security.UsersManageRoles` permission.

## Setup Commands

```bash
# backend build (warnings as errors)
dotnet build ERP-Government.slnx

# apply migrations to a scratch DB (test host dev DB)
dotnet ef database update -p src/Infrastructure -s src/Web

# frontend deps + API client regen
cd src/Web/ClientApp
npm install
npm run generate-api   # NSwag OpenAPI regen (verify old endpoints gone, PUT /role present)
cd ../..

# tests
dotnet test tests/Application.UnitTests
dotnet test tests/Application.FunctionalTests
dotnet test tests/Web.AcceptanceTests
```

---

## Scenario 1: Two-Phase Migration Leaves No NULL RoleId (FR-001/FR-004, SC-001/SC-002)

Seed scratch DB with: one OK_SINGLE user (1 active UserRole), one CONFLICT_MULTI user (2 active UserRoles as `Users.RoleId` conflict), one ORPHAN_ZERO user (0 active UserRoles).

1. Apply Migration M1 (adds nullable RoleId).
2. Run migration report script → verify three lists categorized correctly:
   - OK_SINGLE: auto-migrated RoleId.
   - CONFLICT_MULTI: both RoleIds listed, RoleId remains NULL.
   - ORPHAN_ZERO: RoleId remains NULL.
3. Provide PreciseMapping via **manual SQL** updating `Users.RoleId` for CONFLICT/ORPHAN.
4. Apply Migration M2 (`ALTER COLUMN int NOT NULL` + `UserPermissions.IsActive`).
5. Verify: `SELECT COUNT(*) FROM Users WHERE RoleId IS NULL` = 0; NOT NULL enforced.
6. Negative: attempt M2 while a user still NULL → migration fails listing remaining UserIds, no ALTER applied.

**Expected**: SC-001 (100% NOT NULL) and SC-002 (100% categorized) pass.

## Scenario 2: Effective Permission Calculation (FR-002/FR-009, SC-003)

User U with RoleId R; role R has RolePermissions {P1 active, P2 active}; UserPermissions row `{P3, IsGranted=true}` and `{P2, IsGranted=false}`.

1. Call PermissionService / a protected endpoint as U.
2. Verify effective = {P1, P3} (P2 revoked by MINUS; P3 added by UNION).
3. Negative matrix: expired P (EffectiveTo past) excluded; future EffectiveFrom excluded; P2 revoked.
4. Verify frontend never computes — server only (config check or code review).

**Expected**: SC-003 (100% of test matrix matches). Unit + functional tests assert each combination.

## Scenario 3: Set User Role w/ Concurrency (FR-005, SC-004)

1. `GET /api/Users/{id}` → note current `roleId` and `rowVersion`.
2. `PUT /api/Users/{id}/role` body `{roleId: B, rowVersion: <current>}` → 200; role becomes B; SecurityAuditLog old=A new=B.
3. Reuse stale `rowVersion` from step 1 → **409** conflict, role unchanged.
4. Assign inactive role → **400** "Role is inactive".
5. Untrusted actor → **403**.
6. Happy path completes < 500ms p95 (functional test timing).

**Expected**: SC-004 (100% stale rejected 409, 100% inactive rejected 400).

## Scenario 4: Single-Screen UI with Overrides (FR-003/FR-007, SC-005/SC-006)

On `/security/users/{id}` single screen:

1. Single `Select` shows current `roleId` (required, no empty default for existing user).
2. Inherited read-only grid lists role permissions with a revoke button per row (revoke disabled if already revoked).
3. Revoke inherited P2 with Reason → P2 leaves effective; override row appears (IsGranted=false, Reason, IsActive=true).
4. Grant extra P3 with Reason → P3 joins effective.
5. Attempt revoke of non-inherited P4 → 400 "الصلاحية ليست موروثة". Attempt grant of already-inherited P1 → 400 "الصلاحية موروثة بالفعل".
6. Remove an override → soft-delete (IsActive=false), inherited permission restores.
7. Change role via Select → save → inherited grid refreshes to new role's permissions.
8. Old endpoints GET/POST/DELETE `/api/Users/{id}/roles` → **404**; new `PUT /api/Users/{id}/role` works for authorized.
9. Full flow completes < 3 minutes manual, zero 404s for old endpoints.

**Expected**: SC-005 (OpenAPI regenerated, old 404, new works), SC-006 (single screen, zero navigation).

## Scenario 5: Audit Immutability (FR-008, SC-007)

1. Perform role change + one grant + one revoke.
2. Verify exactly one `AuditTrail` row and one `SecurityAuditLog` row per change, with old/new + actor.
3. Attempt UPDATE/DELETE on AuditTrail → rejected (INSERT-ONLY trigger).
4. Verify no hard delete of UserPermission (soft-delete only).

**Expected**: SC-007 (exactly one row each change; no hard delete).

---

## Verification Mapping

| Scenario | FRs | SCs | Test level |
|----------|-----|-----|-----------|
| 1 Migration | FR-001, FR-004 | SC-001, SC-002 | Functional (real DB) |
| 2 Effective calc | FR-002, FR-009 | SC-003 | Unit + Functional |
| 3 Set role + conflict | FR-005 | SC-004 | Functional |
| 4 Single-screen UI | FR-003, FR-007 | SC-005, SC-006 | Web.Acceptance + manual |
| 5 Audit immutability | FR-008 | SC-007 | Functional + integration |

See `contracts/http-api.md` for exact request/response shapes and `data-model.md` for entity/migration detail.
