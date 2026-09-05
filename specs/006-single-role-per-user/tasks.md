---

description: "Task list for Single Role Per User (Option A) feature implementation"
---

# Tasks: Single Role Per User (Option A)

**Input**: Design documents from `/specs/006-single-role-per-user/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Functional/unit tests are required by Constitution XI (business rules proven by executable tests) and the spec's measurable SCs. Frontend Vitest wiring is a research decision (D-03).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Backend**: `src/Domain`, `src/Application`, `src/Infrastructure`, `src/Web`
- **Frontend**: `src/Web/ClientApp/src/features/security/`
- **Tests**: `tests/Application.UnitTests/Security/`, `tests/Application.FunctionalTests/Security/`, `tests/Web.AcceptanceTests/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Frontend test wiring, verification of the Pet Shop UserRoles reference sweep baseline, and build baseline.

- [ ] T001 Add `"test": "vitest run"` and `"test:watch": "vitest"` scripts to `src/Web/ClientApp/package.json`
- [ ] T002 [P] Verify frontend Vitest runs: `cd src/Web/ClientApp && npm test` on existing spec files (confirm 0 tests currently or pass)
- [ ] T003 Verify backend full-solution baseline build: `dotnet build ERP-Government.slnx` (record pre-existing failures)

**Checkpoint**: Baseline established; frontend Vitest invocable; backend build state recorded.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Schema + domain model changes that ALL user stories depend on (RoleId on User, UserRole removal prep, UserPermission.IsActive). This phase is CRITICAL — no user story can begin until the single-role schema foundation is in place.

**CRITICAL**: No user story work can begin until this phase is complete

- [ ] T004 Add `int? RoleId` property + `SecurityRole Role` navigation to `src/Domain/Security/Entities/User.cs` (null during migration, NOT NULL after phase 2)
- [ ] T005 [P] Add `bool IsActive { get; set; } = true` to `src/Domain/Security/Entities/UserPermission.cs` (supports soft-delete, D-02/D-06)
- [ ] T006 Update `src/Infrastructure/Data/Configurations/Security/UserConfiguration.cs` to map `RoleId` FK to SecurityRoles with `DeleteBehavior.Restrict` (nullable for phase 1)
- [ ] T007 Update `src/Infrastructure/Data/Configurations/Security/UserPermissionConfiguration.cs` to configure `IsActive` and add filtered unique index `WHERE IsActive = 1` handling soft-delete + re-grant (resolve D-02 index strategy)
- [ ] T008 Create migration M1 (add nullable RoleId FK to Users) via `dotnet ef migrations add AddSingleRole_NullableRoleId` in `src/Infrastructure/Migrations/`
- [ ] T009 Create migration M2 (data-fill + `ALTER COLUMN RoleId INT NOT NULL` + add UserPermission.IsActive + make Reason required) guarded against NULL remains
- [ ] T010 Add `Users.RoleId` FK enforcement to test host seeding (idempotent, Constitution VI) in `tests/TestAppHost/`

**Checkpoint**: Foundation ready — Users schema has RoleId, UserPermission has IsActive, migrations M1/M2 present. User story implementation can now begin.

---

## Phase 3: User Story 1 - Precise Migration from M:N to Single Role (Priority: P1) MVP

**Goal**: Two-phase migration converting every user to exactly one RoleId with no data loss, categorizing OK_SINGLE/CONFLICT_MULTI/ORPHAN_ZERO and requiring manual PreciseMapping (SQL script, clarified) for conflicts/orphans.

**Independent Test**: Seed OK_SINGLE/CONFLICT_MULTI/ORPHAN_ZERO; run report, apply manual SQL mapping, run phase 2, verify 100% NOT NULL and zero pending (SC-001/SC-002).

### Tests for User Story 1 (REQUIRED — Constitution XI, SC-001/SC-002)

- [ ] T011 [P] [US1] Functional test: OK_SINGLE auto-migrates; CONFLICT_MULTI/ORPHAN_ZERO reported with exact RoleIds and NULL RoleId in `tests/Application.FunctionalTests/Security/MigrationReportTests.cs`
- [ ] T012 [P] [US1] Functional test: manual SQL PreciseMapping resolves conflicts/orphans; phase 2 leaves zero NULL RoleId; NOT NULL enforced in `tests/Application.FunctionalTests/Security/MigrationReportTests.cs`
- [ ] T013 [US1] Functional test: phase 2 fails with explicit remaining-UserIds list and does NOT ALTER schema when a user remains NULL in `tests/Application.FunctionalTests/Security/MigrationReportTests.cs`

### Implementation for User Story 1

- [ ] T014 [P] [US1] Implement migration report query producing OK_SINGLE/CONFLICT_MULTI/ORPHAN_ZERO lists from pre-migration UserRoles data (report script/query)
- [ ] T015 [US1] Implement phase-2 ALTER NOT NULL guard step that enumerates remaining NULL RoleIds and aborts before ALTER (in Migration M2, `src/Infrastructure/Migrations/`)
- [ ] T016 [US1] Implement manual PreciseMapping SQL template/script for CONFLICT_MULTI/ORPHAN_ZERO resolution (no app API, clarified Option C)

**Checkpoint**: User Story 1 fully functional — migration produces 100% NOT NULL, correct categorization, and guarded failure. MVP candidate.

---

## Phase 4: User Story 2 - Set Single Role for a User (Priority: P1)

**Goal**: Single `PUT /users/{id}/role` replacing the old role-collection endpoints, with IsActive check and RowVersion 409 concurrency.

**Independent Test**: Change role, save, reopen, verify only new role effective; stale rowVersion → 409; inactive role → 400; unauthorized → 403 (SC-004).

### Tests for User Story 2 (REQUIRED — Constitution XI, SC-004)

- [ ] T017 [P] [US2] Unit test SetUserRoleCommand success path (role replaces, RowVersion advances) in `tests/Application.UnitTests/Security/SetUserRoleCommandTests.cs`
- [ ] T018 [P] [US2] Unit test SetUserRoleCommand failure paths: 400 inactive role, 400 role not found, 404 user not found in `tests/Application.UnitTests/Security/SetUserRoleCommandTests.cs`
- [ ] T019 [P] [US2] Unit test SetUserRoleCommand stale RowVersion → concurrency failure (409) in `tests/Application.UnitTests/Security/SetUserRoleCommandTests.cs`
- [ ] T020 [US2] Unit test SetUserRoleCommand requires `Security.UsersManageRoles` permission (FR-012) in `tests/Application.UnitTests/Security/SetUserRoleCommandTests.cs`
- [ ] T021 [US2] Functional test: PUT /api/Users/{id}/role happy path + 409 stale + 400 inactive + 403 unauthorized against real DB in `tests/Application.FunctionalTests/Security/SetUserRoleTests.cs`
- [ ] T022 [US2] Functional test: role change creates exactly one AuditTrail + one SecurityAuditLog with old/new + actor (FR-008, SC-007) in `tests/Application.FunctionalTests/Security/SetUserRoleTests.cs`

### Implementation for User Story 2

- [ ] T023 [P] [US2] Create `SetUserRoleCommand.cs` in `src/Application/Security/Commands/Users/` (RoleId, RowVersion; guards: role exists + IsActive; 409 on concurrency; Arabic FluentValidation)
- [ ] T024 [US2] Add `SetUserRoleCommand` endpoint `PUT /api/Users/{id}/role` with 200/400/403/404/409 mapping (Conflict pattern from BackgroundJobsEndpoints.cs:148) in `src/Web/Endpoints/Security/Users.cs`
- [ ] T025 [P] [US2] Delete old `AssignRoleCommand.cs`, `AssignRolePeriodCommand.cs`, `RemoveRoleCommand.cs` from `src/Application/Security/Commands/Users/`
- [ ] T026 [US2] Remove `GET/POST/DELETE /api/Users/{id}/roles` endpoints from `src/Web/Endpoints/Security/Users.cs` (return 404 semantics, FR-007)
- [ ] T027 [US2] Modify `CreateUserCommand.cs` to require RoleId (no default, FR-001) in `src/Application/Security/Commands/Users/`

**Checkpoint**: Users 1 AND 2 functional — role assignment via single endpoint with full concurrency/validation/audit.

---

## Phase 5: User Story 3 - Effective Permission Calculation (Priority: P1)

**Goal**: PermissionService computes effective = (role perms of RoleId) ∪ grants − revokes, server-side at transaction time, fail closed (NULL RoleId = deny).

**Independent Test**: User with Role R {P1,P2} + UserPermissions {grant P3, revoke P2} → {P1,P3}, denies P2; expired/future filtered (SC-003).

### Tests for User Story 3 (REQUIRED — Constitution XI, SC-003)

- [ ] T028 [P] [US3] Unit test PermissionService: role-only, grant-only, revoke-only, grant+revoke combinations in `tests/Application.UnitTests/Security/PermissionServiceTests.cs`
- [ ] T029 [P] [US3] Unit test PermissionService: expired/inactive/future-EffectiveFrom filtered; NULL RoleId → deny empty set (fail closed, FR-012) in `tests/Application.UnitTests/Security/PermissionServiceTests.cs`
- [ ] T030 [US3] Functional test: effective-permission matrix over real DB (SC-003) in `tests/Application.FunctionalTests/Security/PermissionServiceTests.cs`

### Implementation for User Story 3

- [ ] T031 [P] [US3] Rewrite `PermissionService.GetPermissionsAsync` to use `Users.RoleId` → RolePermission → SecurityPermission plus UserPermissions (grants MINUS revokes, IsActive + window) in `src/Application/Security/PermissionService.cs`
- [ ] T032 [US3] Update `GetUserByIdQuery.cs` to return single `roleId`/`roleName` + server-computed `effectivePermissions` (remove UserRoleDto collection) in `src/Application/Security/Queries/Users/GetUserByIdQuery.cs`
- [ ] T033 [US3] Update `DeactivateUserCommand.cs` admin/IsAdmin check via `Users.RoleId` (preserve role) in `src/Application/Security/Commands/Users/DeactivateUserCommand.cs`
- [ ] T034 [US3] Update `DeleteRoleCommand.cs` Restrict check against `Users.RoleId` (409/400 if assigned) in `src/Application/Security/Commands/Roles/DeleteRoleCommand.cs`
- [ ] T035 [US3] Delete `GetUserRolesQuery.cs` in `src/Application/Security/Queries/Users/` (replaced by GetUserById)

**Checkpoint**: Effective permission calculation correct and fail-closed; all users/roles use cases migrated off UserRoles.

---

## Phase 6: User Story 4 - Grant / Revoke Individual Permission Overrides (Priority: P2)

**Goal**: Grant extra / revoke inherited permissions with non-empty Reason, audited; strict validation (revoke must be inherited, grant must not be — clarified Option A); soft-delete removal.

**Independent Test**: Revoke inherited P2 with reason, grant extra P4, verify effective + audit + overrides grid (SC-003, SC-007).

### Tests for User Story 4 (REQUIRED — Constitution XI, SC-003)

- [ ] T036 [P] [US4] Unit test AssignUserPermissionCommand: grant extra (IsGranted=true) with Reason creates row in `tests/Application.UnitTests/Security/AssignUserPermissionCommandTests.cs`
- [ ] T037 [P] [US4] Unit test AssignUserPermissionCommand: revoke inherited (IsGranted=false) with Reason creates row in `tests/Application.UnitTests/Security/AssignUserPermissionCommandTests.cs`
- [ ] T038 [P] [US4] Unit test strict validation: revoke non-inherited → 400 "not inherited"; grant already-inherited → 400 "already inherited" (FR-003) in `tests/Application.UnitTests/Security/AssignUserPermissionCommandTests.cs`
- [ ] T039 [P] [US4] Unit test Reason required (empty → 400) in `tests/Application.UnitTests/Security/AssignUserPermissionCommandTests.cs`
- [ ] T040 [US4] Unit test RemoveUserPermissionCommand soft-deletes via IsActive=false (no hard delete, FR-008) in `tests/Application.UnitTests/Security/RemoveUserPermissionCommandTests.cs`
- [ ] T041 [US4] Functional test: grant/revoke/remove overrides against real DB; effective permissions update; audit rows recorded in `tests/Application.FunctionalTests/Security/UserPermissionOverridesTests.cs`

### Implementation for User Story 4

- [ ] T042 [P] [US4] Modify `AssignUserPermissionCommand.cs` in `src/Application/Security/Commands/UserPermissions/` to add strict inherited/not-inherited validation + Reason required + audit
- [ ] T043 [US4] Modify `RemoveUserPermissionCommand.cs` in `src/Application/Security/Commands/UserPermissions/` to soft-delete (IsActive=false) + audit
- [ ] T044 [P] [US4] Modify `GetUserPermissionsQuery.cs` to return active grants+revokes with IsActive for overrides grid in `src/Application/Security/Queries/Users/GetUserPermissionsQuery.cs`
- [ ] T045 [US4] Ensure `POST/DELETE /api/Users/{id}/permissions` endpoints map 400 validation + 409 concurrency + audit correctly in `src/Web/Endpoints/Security/Users.cs`

**Checkpoint**: Overrides fully functional — least-privilege exceptions via grants/revokes with strict validation, soft-delete, audit.

---

## Phase 7: User Story 5 - Single-Screen User Role & Overrides Management (Priority: P2)

**Goal**: Single `/security/users/{id}` screen: role Select + inherited read-only grid + overrides grid, replacing `UserRolesPage`. Old endpoints 404; new PUT /role used (SC-005/SC-006).

**Independent Test**: Navigate to detail screen, change role via Select, revoke one inherited, grant one extra, save, verify all reflected without navigation; zero 404s (SC-006).

### Tests for User Story 5 (REQUIRED — Constitution XI, SC-005/SC-006)

- [ ] T046 [P] [US5] Frontend unit test: single-screen role Select + inherited grid + overrides grid render from `UserDetailDto`/`UserPermissionDto` in `src/Web/ClientApp/src/features/security/users/__tests__/RoleScreen.test.tsx`
- [ ] T047 [P] [US5] Frontend unit test: revoke button disabled for already-revoked; remove override reflects soft-delete in `src/Web/ClientApp/src/features/security/users/__tests__/RoleScreen.test.tsx`
- [ ] T048 [P] [US5] Frontend unit test: strict validation errors (400) shown as Arabic inline; no API call on client-side failure in `src/Web/ClientApp/src/features/security/users/__tests__/RoleScreen.test.tsx`
- [ ] T049 [US5] Acceptance test: full single-screen journey (create user with role → revoke → grant → verify effective, <3 min, zero old-404) in `tests/Web.AcceptanceTests/Security/SingleRoleScreenTests.cs`

### Implementation for User Story 5

- [ ] T050 [P] [US5] Update `src/Web/ClientApp/src/features/security/users/types.ts`: UserDetailDto single roleId/roleName/effectivePermissions; SetUserRoleCommand; drop UserRoleDto collection; UserPermissionDto + IsActive
- [ ] T051 [P] [US5] Update `src/Web/ClientApp/src/features/security/users/client.ts`: replace usersRolesClient with `setUserRole` (PUT /api/Users/{id}/role); keep usersPermissionsClient
- [ ] T052 [P] [US5] Replace `hooks/useUserRoles.ts` with `useSetUserRole` mutation (invalidate user detail) in `src/Web/ClientApp/src/features/security/users/hooks/`
- [ ] T053 [P] [US5] Update `components/RolesTab.tsx` → RoleTab: single Select (required) + inherited read-only DataGrid (revoke per row, disabled if revoked) in `src/Web/ClientApp/src/features/security/users/components/`
- [ ] T054 [US5] Update `components/PermissionsTab.tsx` → overrides DataGrid showing grants+revokes with Reason, add grant/revoke/remove actions in `src/Web/ClientApp/src/features/security/users/components/`
- [ ] T055 [P] [US5] Update `pages/UserDetailPage.tsx` to the single-screen layout (role Select + both grids) in `src/Web/ClientApp/src/features/security/users/pages/`
- [ ] T056 [P] [US5] Remove `rbac/pages/UserRolesPage.tsx`, `rbac/hooks/useUserRoles.ts`, `useAssignUserRole.ts`, `useRemoveUserRole.ts`, user-roles client in `src/Web/ClientApp/src/features/security/rbac/`
- [ ] T057 [US5] Update `src/Web/ClientApp/src/app/routes.tsx`: remove `/security/users/:id/roles` route; point to single detail screen
- [ ] T058 [P] [US5] Add `Security.UsersView`/`Security.UsersManageRoles` to `src/Web/ClientApp/src/shared/constants/permissions.ts` and extend `PolicyString` in `src/Web/ClientApp/src/shared/hooks/usePermission.ts` (Constitution X)
- [ ] T059 [US5] Regenerate OpenAPI contract via `cd src/Web/ClientApp && npm run generate-api`; verify old role endpoints absent, PUT /role + modified detail present (SC-005)

**Checkpoint**: Users 1-5 functional — single-screen management complete, old endpoints 404, contract regenerated.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Removal of all UserRoles references, contract regen, security, docs.

- [ ] T060 [P] Remove `DbSet<UserRole> UserRoles` from `src/Application/Common/Interfaces/IApplicationDbContext.cs` and `src/Infrastructure/Data/ApplicationDbContext.cs`
- [ ] T061 [P] Delete `src/Infrastructure/Data/Configurations/Security/UserRoleConfiguration.cs` and `src/Domain/Security/Entities/UserRole.cs`
- [ ] T062 Delete migration dropping UserRoles table/add NOT NULL; regen `ApplicationDbContextModelSnapshot.cs` via EF
- [ ] T063 Verify zero `UserRoles` references remain in `src/` and frontend (FR-006): `rg "UserRoles" src/`
- [ ] T064 [P] Update frontend design tokens / RTL / dark-mode audit for new grids in `src/Web/ClientApp/src/features/security/` (Constitution X)
- [ ] T065 [P] Security verification: every new/modified endpoint + use case declares named permission; fail-closed on subsystem error; denial logged (FR-012)
- [ ] T066 [P] Documentation: update OpenAPI/typed client and any affected docs in `specs/006-single-role-per-user/`
- [ ] T067 Run full backend build `dotnet build ERP-Government.slnx` (0 errors/warnings) and frontend `npm run build` + `npm test`
- [ ] T068 Run `quickstart.md` validation scenarios end-to-end (migration, effective calc, role set, single screen, audit)

**Checkpoint**: Feature complete and verified against quickstart; all UserRoles references removed (FR-006).

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup — BLOCKS all user stories
- **User Stories (Phases 3-7)**: All depend on Foundational
  - US1 → US2 → US3 share the migration/effective-calc foundation and must follow order (US2 relies on M1 schema; US3 relies on RoleId)
  - US1, US2, US3 are P1 and sequential (migration first, then role set, then calc)
  - US4 (P2) depends on US3 (effective calc + UserPermission)
  - US5 (P2) depends on US2 (PUT role) + US3 (effective) + US4 (overrides)
- **Polish (Phase 8)**: Depends on all user stories complete

### User Story Dependencies

- **User Story 1 (P1)**: Migration foundation — no dependency on other stories
- **User Story 2 (P1)**: Depends on Foundational (M1 nullable RoleId); no dependency on US1 report (can share phase order)
- **User Story 3 (P1)**: Depends on Foundational + US2 (RoleId populated, calc reads it) — calc is the security core
- **User Story 4 (P2)**: Depends on US3 (effective calc consumes UserPermissions)
- **User Story 5 (P2)**: Depends on US2 + US3 + US4 (single screen needs all)

### Within Each User Story

- Tests MUST be written and FAIL before implementation (red-green where TDD applies)
- Models before services before endpoints
- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- Setup tasks [P]: T002
- Foundational: T005 parallel with T004/T006
- US1: T011/T012/T013 tests parallel; T014/T015/T016
- US2: T017-T022 tests parallel; T023/T025/T027 parallel edits
- US3: T028/T029/T030 tests parallel; T031/T035 parallel
- US4: T036-T041 tests parallel; T042/T044 parallel
- US5: T046-T049 tests parallel; frontend T050-T058 largely parallel (different files)
- Polish: T060/T061/T064/T065/T066 parallel

---

## Parallel Example: User Story 5

```bash
# Launch all tests for User Story 5 together:
Task: "Frontend unit test: RoleScreen render/revoke/validation in ...__tests__/RoleScreen.test.tsx"
Task: "Acceptance test: single-screen journey in tests/Web.AcceptanceTests/Security/SingleRoleScreenTests.cs"

# Launch frontend implementation files together:
Task: "Update types.ts (single roleId + SetUserRoleCommand)"
Task: "Update client.ts (setUserRole PUT /role)"
Task: "Replace useUserRoles with useSetUserRole"
Task: "Update RolesTab → RoleTab (Select + inherited grid)"
Task: "Update PermissionsTab → overrides grid"
Task: "Remove rbac UserRoles page/hooks/client"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: User Story 1 (migration)
4. **STOP and VALIDATE**: Migration report + NOT NULL + zero pending
5. Deploy/demo migration if ready

### Incremental Delivery

1. Setup + Foundational → schema foundation ready
2. US1 migration → test → MVP
3. US2 role set → test → deploy
4. US3 effective calc → test (security core)
5. US4 overrides → test
6. US5 single screen → test (zero old-404)
7. Polish: UserRoles sweep + regen + full verify

### Parallel Team Strategy

- Team A: US1 migration (P1)
- Team B: US2 role set (P1) after M1
- Team C: US3 effective calc (P1) after US2
- Then US4/US5 after calc/overrides foundation

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- US1-3 are P1 (spec priorities); US4-5 are P2 but must land for complete single-role feature
- Clarified decisions baked in: PreciseMapping = manual SQL (Option C); revoke/grant strict 400 (Option A); override removal soft-delete IsActive=false; NULL RoleId → deny access
- UserPermission needs `IsActive` field added (D-02/D-06 research gap — currently absent)
- 409 concurrency surfaced distinctly per FR-005, not 400 (D-05)
- Commit after each task or logical group
