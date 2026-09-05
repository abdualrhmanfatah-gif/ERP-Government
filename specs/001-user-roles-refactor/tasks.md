# Tasks: User-Roles Refactor

**Input**: Design documents from `/specs/001-user-roles-refactor/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Not explicitly requested in the feature specification. Test tasks are included where constitutionally required (Principle XI).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Foundational - Schema Deployment (US4) ⚠️ CRITICAL

**Goal**: Add RoleId INT NOT NULL FK to Users, drop UserRoles, update UserPermission index

**Independent Test**: Migration runs against database, Users has RoleId column, UserRoles table gone

- [x] T001 [US4] Add `RoleId` property (int, NOT NULL) to `src/Domain/Security/Entities/User.cs`
- [x] T002 [US4] Add `RoleId` FK configuration to `src/Infrastructure/Data/Configurations/Security/UserConfiguration.cs` (Restrict delete, index)
- [x] T003 [US4] Update `UserPermissionConfiguration.cs` unique index from `{UserId, PermissionId, EffectiveFrom}` to `{UserId, PermissionId}` in `src/Infrastructure/Data/Configurations/Security/UserPermissionConfiguration.cs`
- [x] T004 [US4] Remove `UserRoleConfiguration.cs` from `src/Infrastructure/Data/Configurations/Security/`
- [x] T005 [US4] Remove `UserRoles` DbSet from `src/Application/Common/Interfaces/IApplicationDbContext.cs`
- [x] T006 [US4] Remove `UserRoles` DbSet from `src/Infrastructure/Data/ApplicationDbContext.cs`
- [x] T007 [US4] Generate EF Core migration: `dotnet ef migrations add AddRoleIdDropUserRoles --project src/Infrastructure`
- [x] T008 [US4] Verify migration SQL: RoleId NOT NULL with FK Restrict, UserRoles DROP, UserPermissions index update

**Checkpoint**: Schema ready. Users.RoleId exists. UserRoles table gone. UserPermission unique constraint updated.

---

## Phase 2: User Story 1 - Assign a Single Role (Priority: P1) 🎯 MVP

**Goal**: Admin assigns exactly one role to a user via PUT /{id}/role with RowVersion concurrency

**Independent Test**: PUT /{id}/role sets Users.RoleId, rejects inactive roles, rejects stale RowVersion

### Implementation for User Story 1

- [x] T009 [P] [US1] Create `SetUserRoleCommand.cs` in `src/Application/Security/Commands/Users/` with `[Authorize(Policy = PermissionCodes.UsersManageRoles)]`, RowVersion concurrency, Role.IsActive check
- [x] T010 [US1] Create `SetUserRoleCommandValidator.cs` in `src/Application/Security/Commands/Users/` with FluentValidation rules (UserId > 0, RoleId > 0)
- [x] T011 [US1] Update `GetUserByIdQuery.cs` in `src/Application/Security/Queries/Users/` to read `Users.RoleId` instead of `UserRoles` join
- [x] T012 [US1] Update `UserDetailDto` in `src/Application/Security/Common/DTOs/SecurityDtos.cs`: replace `List<UserRoleDto> Roles` with `UserRoleDto? Role`
- [x] T013 [US1] Update `UserRoleDto` in `src/Application/Security/Common/DTOs/SecurityDtos.cs`: remove `EffectiveFrom`/`EffectiveTo` fields
- [x] T014 [US1] Add `PUT /{id}/role` endpoint mapping in `src/Web/Endpoints/Security/Users.cs` with `.RequireAuthorization(PermissionCodes.UsersManageRoles)`
- [x] T015 [US1] Remove `GET /{id}/roles` endpoint from `src/Web/Endpoints/Security/Users.cs`
- [x] T016 [US1] Remove `POST /{id}/roles` endpoint from `src/Web/Endpoints/Security/Users.cs`
- [x] T017 [US1] Remove `DELETE /{id}/roles/{roleId}` endpoint from `src/Web/Endpoints/Security/Users.cs`
- [x] T018 [US1] Remove `GetUserRolesQuery.cs` from `src/Application/Security/Queries/Users/`
- [x] T019 [US1] Remove `AssignRoleCommand.cs` from `src/Application/Security/Commands/Users/`
- [x] T020 [US1] Remove `RemoveRoleCommand.cs` from `src/Application/Security/Commands/Users/`

**Checkpoint**: PUT /{id}/role works. GET /{id} returns single role. Old role endpoints removed.

---

## Phase 3: User Story 2 - Effective Permission Resolution (Priority: P1)

**Goal**: PermissionService computes RolePermissions ∪ UserPermissions(true) \ UserPermissions(false)

**Independent Test**: Set role + create overrides → effective permissions match union-minus formula

### Implementation for User Story 2

- [x] T021 [US2] Rewrite `PermissionService.cs` in `src/Application/Security/` to read `Users.RoleId` + `UserPermissions` with union-minus formula, respect `Role.IsActive` and `EffectiveFrom/To`
- [x] T022 [US2] Create `EffectivePermissionDto` in `src/Application/Common/DTOs/SecurityDtos.cs` with fields: PermissionId, Code, Name, Source ("role"|"override"), IsGranted, Reason
- [x] T023 [US2] Update `GetUserPermissionsQuery.cs` in `src/Application/Security/Queries/Users/` to return effective permissions (role + overrides) instead of direct overrides only
- [x] T024 [US2] Update `IPermissionService.cs` XML doc to reflect new union-minus semantics (interface unchanged, doc only)

**Checkpoint**: PermissionService returns correct effective permissions for all scenarios (role-only, role+grants, role+revokes, inactive role, future role).

---

## Phase 4: User Story 3 - Override Individual Permissions (Priority: P2)

**Goal**: Grant/revoke individual permissions with mandatory Reason, audited

**Independent Test**: POST /{id}/permissions with Reason creates override, audit records generated

### Implementation for User Story 3

- [x] T025 [US3] Update `AssignUserPermissionCommand.cs` in `src/Application/Security/Commands/UserPermissions/` to require `Reason` (non-empty string) via FluentValidation
- [x] T026 [US3] Update `AssignUserPermissionCommandHandler` to write `SecurityAuditLog` entry on override create/update with EventCategory="PermissionOverride", OldValues, NewValues
- [x] T027 [US3] Update `RemoveUserPermissionCommand.cs` in `src/Application/Security/Commands/UserPermissions/` to write `SecurityAuditLog` entry on override removal

**Checkpoint**: Permission overrides require Reason. Both create and remove generate audit records.

---

## Phase 5: User Story 4 - Unified Role and Overrides Screen (Priority: P2)

**Goal**: Single screen: role dropdown + inherited permissions (read-only, revoke button) + overrides (editable, Reason)

**Independent Test**: Load screen → see role, inherited permissions, overrides. Revoke inherited → override created. Change role → inherited updates.

### Implementation for User Story 4

- [x] T028 [P] [US4] Update `src/Web/ClientApp/src/features/security/users/client.ts`: replace `usersRolesClient` with `usersRoleClient` (single PUT), update types
- [x] T029 [P] [US4] Update `src/Web/ClientApp/src/features/security/users/types.ts`: change `UserRole` type to single role object
- [x] T030 [US4] Rewrite `src/Web/ClientApp/src/features/security/users/components/RolesTab.tsx`: two-column layout — inherited permissions DataGrid (right in RTL, read-only with revoke button) + overrides DataGrid (left in RTL, editable with Reason field) + role Select dropdown at top
- [x] T031 [US4] Update `src/Web/ClientApp/src/features/security/users/pages/UserDetailPage.tsx`: pass single role data to RolesTab, update RolesTab props
- [x] T032 [US4] Update `src/Web/ClientApp/src/features/security/rbac/hooks/`: remove `useUserRoles`, `useAssignUserRole`, `useRemoveUserRole` hooks, add `useSetUserRole` hook for PUT endpoint
- [x] T033 [US4] Delete `src/Web/ClientApp/src/features/security/rbac/pages/UserRolesPage.tsx` (replaced by unified RolesTab)
- [x] T034 [US4] Update route config in `src/Web/ClientApp/src/app/routes.tsx`: remove `/security/users/:id/roles` route if present

**Checkpoint**: Unified screen loads. Role dropdown works. Inherited permissions read-only with revoke. Overrides editable with Reason. Two-column RTL layout.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Cleanup, security hardening, validation

- [x] T035 Remove `src/Domain/Security/Entities/UserRole.cs` (entity no longer needed)
- [x] T036 Remove `src/Infrastructure/Data/Configurations/Security/UserRoleConfiguration.cs`
- [x] T037 Search codebase for remaining `UserRoles` references (59 hits per spec) and update all to use `Users.RoleId`
- [x] T038 Run `dotnet build` — verify zero compilation errors
- [x] T039 Run `dotnet test` — verify all existing tests pass
- [x] T040 Run quickstart.md validation scenarios
- [x] T041 Address security review findings SEC-001 through SEC-004 from `specs/001-user-roles-refactor/security-review-plan.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Schema/US4)**: No dependencies — start immediately. BLOCKS all other phases.
- **Phase 2 (US1)**: Depends on Phase 1 completion (needs RoleId on User entity)
- **Phase 3 (US2)**: Depends on Phase 1 (needs User.RoleId) AND Phase 2 (needs SetUserRoleCommand to exist for testing)
- **Phase 4 (US3)**: Depends on Phase 1 (needs UserPermission unique constraint) AND Phase 2 (needs role assignment working)
- **Phase 5 (US4 UI)**: Depends on Phase 2 (needs PUT endpoint) AND Phase 3 (needs effective permissions query)
- **Phase 6 (Polish)**: Depends on all previous phases

### User Story Dependencies

- **US4 (Schema)**: Foundation — no dependencies
- **US1 (Assign Role)**: Depends on US4
- **US2 (Permission Resolution)**: Depends on US4, benefits from US1
- **US3 (Override Permissions)**: Depends on US4, benefits from US2
- **US4 (UI)**: Depends on US1 + US2 + US3

### Within Each User Story

- Entity changes before service changes
- Service changes before endpoint changes
- Endpoint changes before frontend changes
- Core implementation before audit/logging

### Parallel Opportunities

- T001-T004 can run in parallel (different entity/config files)
- T015-T017 can run in parallel (removing different endpoints from same file — sequential in practice)
- T028-T029 can run in parallel (different frontend files)
- T035-T036 can run in parallel (different files)

---

## Parallel Example: Phase 1 (Schema)

```bash
# Launch schema entity and config changes together:
Task: "T001 Add RoleId to User.cs"
Task: "T002 Add RoleId FK to UserConfiguration.cs"
Task: "T003 Update UserPermissionConfiguration.cs unique index"
Task: "T004 Remove UserRoleConfiguration.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Schema Deployment (US4) — foundation
2. Complete Phase 2: User Story 1 — single role assignment
3. **STOP and VALIDATE**: PUT /{id}/role works, GET /{id} returns role
4. Deploy/demo if ready

### Incremental Delivery

1. Phase 1 → Schema ready
2. Phase 2 → Role assignment works (MVP!)
3. Phase 3 → Effective permissions correct
4. Phase 4 → Overrides audited
5. Phase 5 → Unified UI complete
6. Phase 6 → Polish and validation

### Parallel Team Strategy

With multiple developers:
1. Developer A: Phase 1 (schema) → Phase 2 (US1 command/endpoints)
2. Developer B: Phase 3 (US2 PermissionService) → Phase 4 (US3 overrides)
3. Developer C: Phase 5 (US4 UI) — after backend endpoints ready

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Security review findings (SEC-001 through SEC-004) should be addressed in Phase 6
