# Tasks: Asset Groups CRUD

**Input**: Design documents from `/specs/054-asset-groups-crud/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/api.md

**Tests**: Test tasks included per Constitution XI (TDD required for new features).

**Organization**: Tasks grouped by user story. US1+US2 combined as P1 MVP (co-dependent).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3, US4, US5)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Permission constants and shared Application infrastructure

- [x] T001 Add `AssetGroupsDeactivate` and `AssetGroupsActivate` permission constants in `src/Application/Common/Security/PermissionCodes.cs`
- [x] T002 [P] Create `src/Application/Assets/AssetGroups/Common/MappingExtensions.cs` with response records (`AssetGroupResponse`, `AssetGroupDetailResponse`) and `ToResponse()` / `ToDetailResponse()` extension methods

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Query handlers and shared validation logic that ALL user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T003 Create `src/Application/Assets/AssetGroups/Queries/GetAssetGroups/GetAssetGroupsQuery.cs` — record + handler returning flat list with `hasChildren` flag, supporting search/isActive/parentId filters
- [x] T004 Create `src/Application/Assets/AssetGroups/Queries/GetAssetGroupById/GetAssetGroupByIdQuery.cs` — record + handler returning basic group info
- [x] T005 Create `src/Application/Assets/AssetGroups/Queries/GetAssetGroupDetail/GetAssetGroupDetailQuery.cs` — record + handler returning full detail with GL accounts and audit fields

**Checkpoint**: Foundation ready — user story implementation can now begin

---

## Phase 3: User Story 1 + 2 — Create & View Asset Groups (Priority: P1) — MVP

**Goal**: Asset manager can create new groups with code, name, parent, depreciation params, and view them in tree + list with search

**Independent Test**: Create a group via API/UI, verify it appears in list and tree. Verify search filters correctly. Verify duplicate code is rejected.

### Tests for US1+US2

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T006 [P] [US1] Unit test for `CreateAssetGroupCommandHandler` in `tests/Unit/Assets/AssetGroups/CreateAssetGroupTests.cs` — test success, duplicate code, self-parent, cycle, depth limit
- [x] T007 [P] [US1] Unit test for `CreateAssetGroupCommandValidator` in `tests/Unit/Assets/AssetGroups/CreateAssetGroupValidatorTests.cs` — test required fields, max lengths
- [x] T008 [P] [US2] Unit test for `GetAssetGroupsQueryHandler` in `tests/Unit/Assets/AssetGroups/GetAssetGroupsTests.cs` — test search, filter, hasChildren flag
- [x] T009 [P] [US2] Unit test for `GetAssetGroupByIdQueryHandler` in `tests/Unit/Assets/AssetGroups/GetAssetGroupByIdTests.cs` — test found/not-found

### Implementation for US1+US2

- [x] T010 [US1] Create `src/Application/Assets/AssetGroups/Commands/CreateAssetGroup/CreateAssetGroupCommand.cs` — record with all fields, handler with cycle prevention + depth limit + duplicate code check, validator with FluentValidation
- [x] T011 [US2] [P] Create `src/Web/Endpoints/Assets/AssetGroups.cs` — endpoint group with `GET /` (list), `GET /{id}` (by id), `POST /` (create). Map to handlers. Apply permissions.
- [x] T012 [US2] [P] Create `src/Web/ClientApp/src/features/assets/asset-groups/types.ts` — TypeScript interfaces for `AssetGroupResponse`, `AssetGroupDetailResponse`, `CreateAssetGroupRequest`, `UpdateAssetGroupRequest`
- [x] T013 [US2] [P] Create `src/Web/ClientApp/src/features/assets/asset-groups/shared/schemas.ts` — zod schemas for create/update validation
- [x] T014 [US2] [P] Create `src/Web/ClientApp/src/features/assets/asset-groups/hooks/useAssetGroupsList.ts` — React Query hook for list with search/filter
- [x] T015 [US2] [P] Create `src/Web/ClientApp/src/features/assets/asset-groups/hooks/useCreateAssetGroup.ts` — React Query mutation hook
- [x] T016 [US1] [US2] Create `src/Web/ClientApp/src/features/assets/asset-groups/pages/AssetGroupsListPage.tsx` — DataGrid + TreeView + search + "إضافة مجموعة" button. Tree builds from flat list. Inactive groups show muted styling.
- [x] T017 [US1] [US2] Create `src/Web/ClientApp/src/features/assets/asset-groups/pages/AssetGroupCreatePage.tsx` — form with Code, Name, Parent (dropdown), DepreciationMethod, DefaultUsefulLifeYears, ResidualValuePercentage, AssetCategory, IsDepreciable
- [x] T018 [US2] Add asset group routes in `src/Web/ClientApp/src/app/routes.tsx` — `/assets/asset-groups` (list) and `/assets/asset-groups/create` (create)

**Checkpoint**: MVP complete — can create and view asset groups with tree display

---

## Phase 4: User Story 3 — Edit Asset Group (Priority: P2)

**Goal**: Asset manager can edit all group fields except Code (immutable)

**Independent Test**: Edit a group's name and depreciation params, verify changes persist and appear in detail view.

### Tests for US3

- [x] T019 [P] [US3] Unit test for `UpdateAssetGroupCommandHandler` in `tests/Unit/Assets/AssetGroups/UpdateAssetGroupTests.cs` — test success, inactive rejection, cycle, depth, concurrency conflict
- [x] T020 [P] [US3] Unit test for `UpdateAssetGroupCommandValidator` in `tests/Unit/Assets/AssetGroups/UpdateAssetGroupValidatorTests.cs`

### Implementation for US3

- [x] T021 [US3] Create `src/Application/Assets/AssetGroups/Commands/UpdateAssetGroup/UpdateAssetGroupCommand.cs` — record (no Code), handler with cycle prevention + depth limit + inactive guard + RowVersion check, validator
- [x] T022 [US3] Add `PUT /{id}` endpoint in `src/Web/Endpoints/Assets/AssetGroups.cs` — map to UpdateAssetGroupCommand
- [x] T023 [US3] [P] Create `src/Web/ClientApp/src/features/assets/asset-groups/hooks/useUpdateAssetGroup.ts` — React Query mutation hook
- [x] T024 [US3] Create `src/Web/ClientApp/src/features/assets/asset-groups/pages/AssetGroupDetailPage.tsx` — read-only view + edit mode toggle. Shows all fields. Code read-only. RowVersion round-tripped.
- [x] T025 [US2] [US3] Add detail route in `src/Web/ClientApp/src/app/routes.tsx` — `/assets/asset-groups/:id`

**Checkpoint**: US1+US2+US3 complete — full CRUD except activate/deactivate

---

## Phase 5: User Story 4 — Activate/Deactivate Asset Group (Priority: P2)

**Goal**: Asset manager can deactivate groups (with asset guard) and reactivate them via dedicated buttons

**Independent Test**: Deactivate a group, verify it disappears from asset creation dropdown. Reactivate, verify it reappears. Try deactivating group with assets — blocked.

### Tests for US4

- [x] T026 [P] [US4] Unit test for `ToggleAssetGroupActiveCommandHandler` in `tests/Unit/Assets/AssetGroups/ToggleAssetGroupActiveTests.cs` — test deactivate, activate, already-active, already-inactive, has-assets guard
- [x] T027 [P] [US4] Functional test for deactivate/activate endpoints in `tests/Functional/Assets/AssetGroups/ToggleActiveTests.cs`

### Implementation for US4

- [x] T028 [US4] Create `src/Application/Assets/AssetGroups/Commands/ToggleAssetGroupActive/ToggleAssetGroupActiveCommand.cs` — record with `IsActive` bool, handler with asset check + state guards + RowVersion
- [x] T029 [US4] Add `POST /{id}/deactivate` and `POST /{id}/activate` endpoints in `src/Web/Endpoints/Assets/AssetGroups.cs`
- [x] T030 [US4] [P] Create `src/Web/ClientApp/src/features/assets/asset-groups/hooks/useToggleAssetGroupActive.ts` — React Query mutation hook
- [x] T031 [US4] Update `AssetGroupsListPage.tsx` and `AssetGroupDetailPage.tsx` — add "تعطيل"/"تفعيل" buttons with confirmation dialog. Conditionally show based on IsActive.

**Checkpoint**: US1+US2+US3+US4 complete — full lifecycle management

---

## Phase 6: User Story 5 — View Asset Group Detail (Priority: P2)

**Goal**: View full group detail including all GL accounts and audit info

**Independent Test**: Navigate to detail page, verify all fields displayed including GL accounts and audit timestamps.

### Tests for US5

- [x] T032 [P] [US5] Unit test for `GetAssetGroupDetailQueryHandler` in `tests/Unit/Assets/AssetGroups/GetAssetGroupDetailTests.cs`

### Implementation for US5

- [x] T033 [US5] Add `GET /{id}/detail` endpoint in `src/Web/Endpoints/Assets/AssetGroups.cs` — map to GetAssetGroupDetailQuery
- [x] T034 [US5] [P] Create `src/Web/ClientApp/src/features/assets/asset-groups/hooks/useAssetGroupDetail.ts` — React Query hook for detail endpoint
- [x] T035 [US5] Update `AssetGroupDetailPage.tsx` — display GL accounts section (6 account fields), depreciation parameters, and audit fields (Created, CreatedBy, LastModified, LastModifiedBy)

**Checkpoint**: All user stories complete

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [x] T036 [P] Add error handling integration tests in `tests/Functional/Assets/AssetGroups/ErrorHandlingTests.cs` — verify problem-details contract for all error scenarios (EC-001 through EC-017)
- [ ] T037 [P] Verify RTL layout and dark mode correctness across all pages
- [ ] T038 Run quickstart.md validation scenarios end-to-end
- [x] T039 [P] Add empty state messages ("لا توجد مجموعات بعد") and loading skeletons to list page

---

## Phase 8: Convergence

**Purpose**: Gap remediation — requirements, spec violations, and quality issues found during convergence assessment

### CRITICAL

- [x] T040 Create `src/Web/ClientApp/src/features/assets/asset-groups/pages/AssetGroupEditPage.tsx` and add `/assets/asset-groups/:id/edit` route in `routes.tsx` — edit page with Code read-only, all other fields editable, RowVersion round-tripped (FR-004, gap: missing)
- [x] T041 Add TreeView component to `AssetGroupsListPage.tsx` with expand/collapse hierarchy, inactive groups shown with muted styling (gray text, reduced opacity) and inactive badge (FR-001, gap: missing)
- [x] T042 [P] Create unit test files: `CreateAssetGroupTests.cs`, `CreateAssetGroupValidatorTests.cs`, `GetAssetGroupsTests.cs`, `GetAssetGroupByIdTests.cs`, `UpdateAssetGroupTests.cs`, `UpdateAssetGroupValidatorTests.cs`, `ToggleAssetGroupActiveTests.cs`, `GetAssetGroupDetailTests.cs` in `tests/Unit/Assets/AssetGroups/` (Constitution XI, gap: missing)
- [x] T043 [P] Create functional test file `tests/Functional/Assets/AssetGroups/ToggleActiveTests.cs` and `tests/Functional/Assets/AssetGroups/ErrorHandlingTests.cs` (Constitution XI, gap: missing)

### HIGH

- [x] T044 Replace English error messages (`"Asset group with ID {id} not found."`) with Arabic error messages using `ErrorCodes` constants in `CreateAssetGroupCommand.cs`, `UpdateAssetGroupCommand.cs`, `ToggleAssetGroupActiveCommand.cs`, `GetAssetGroupByIdQuery.cs`, `GetAssetGroupDetailQuery.cs` (FR-013, gap: contradicts)
- [x] T045 Fix `GetAssetGroupByIdQuery.cs` to use `ToResponse()` instead of `ToDetailResponse()` — basic endpoint must not leak GL accounts or RowVersion (contracts/api.md, gap: contradicts)
- [x] T046 Add activate/deactivate toggle buttons per row in `AssetGroupsListPage.tsx` actions column with confirmation dialog (FR-002, gap: partial)

### MEDIUM

- [x] T047 Extract duplicated `WouldCreateCycleAsync` and `GetDepthAsync` from Create/Update handlers into a shared `AssetGroupValidationHelper.cs` (gap: unrequested/duplication)
- [x] T048 Document `MaintenanceExpenseAccount` (7th GL account) as known domain entity gap — entity lacks this FK; requires decision record per Constitution XII if adding (FR-007, gap: missing/partial)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 (MappingExtensions needed by queries)
- **US1+US2 (Phase 3)**: Depends on Phase 2 — BLOCKS all user stories
- **US3 (Phase 4)**: Depends on Phase 2 — can run parallel with US1+US2 if staffed, but sequential is simpler
- **US4 (Phase 5)**: Depends on Phase 2 — can run parallel with US3
- **US5 (Phase 6)**: Depends on Phase 2 — can run parallel with US3+US4
- **Polish (Phase 7)**: Depends on all user stories being complete

### User Story Dependencies

- **US1+US2 (P1)**: Can start after Phase 2 — No dependencies on other stories
- **US3 (P2)**: Can start after Phase 2 — Uses same endpoint file as US1+US2
- **US4 (P2)**: Can start after Phase 2 — Uses same endpoint file
- **US5 (P2)**: Can start after Phase 2 — Uses same endpoint file

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- Commands/Queries before Endpoints
- Hooks before Pages
- Core implementation before polish

### Parallel Opportunities

- T001 + T002 (Setup) — different files
- T006 + T007 + T008 + T009 (all tests) — different test files
- T011 + T012 + T013 + T014 + T015 (endpoint + frontend types/schemas/hooks) — different files
- T019 + T020 (US3 tests) — different test files
- T026 + T027 (US4 tests) — different test files
- All frontend hooks can be created in parallel once types are defined

---

## Parallel Example: US1+US2

```bash
# Launch all tests together:
Task: "Unit test for CreateAssetGroupCommandHandler in tests/Unit/Assets/AssetGroups/CreateAssetGroupTests.cs"
Task: "Unit test for CreateAssetGroupCommandValidator in tests/Unit/Assets/AssetGroups/CreateAssetGroupValidatorTests.cs"
Task: "Unit test for GetAssetGroupsQueryHandler in tests/Unit/Assets/AssetGroups/GetAssetGroupsTests.cs"
Task: "Unit test for GetAssetGroupByIdQueryHandler in tests/Unit/Assets/AssetGroups/GetAssetGroupByIdTests.cs"

# Launch frontend scaffolding together:
Task: "Create types.ts"
Task: "Create schemas.ts"
Task: "Create useAssetGroupsList.ts"
Task: "Create useCreateAssetGroup.ts"
```

---

## Implementation Strategy

### MVP First (US1+US2 Only)

1. Complete Phase 1: Setup (permissions + mapping)
2. Complete Phase 2: Foundational (queries)
3. Complete Phase 3: US1+US2 (create + view)
4. **STOP and VALIDATE**: Run quickstart scenarios V1, V2, V3, V10
5. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add US1+US2 → Test independently → Deploy/Demo (MVP!)
3. Add US3 → Test independently → Deploy/Demo
4. Add US4 → Test independently → Deploy/Demo
5. Add US5 → Test independently → Deploy/Demo
6. Polish → Final validation

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Verify tests fail before implementing (TDD)
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Entity (`AssetGroup.cs`) already exists — no domain changes needed
- No EF migration required — table and seed data already exist

---

## Phase 9: Convergence (Follow-up)

**Purpose**: Additional gaps found in second convergence pass — test compilation errors, missing 7th GL account field, frontend permission guards, and list-view toggle rowVersion

### CRITICAL

- [x] T049 Fix all 8 unit test files in `tests/Application.UnitTests/Assets/AssetGroups/` to use `BuildMockForAsync()` instead of non-existent `BuildMock()` — rest of codebase uses `BuildMockForAsync()` from `TestAsyncQueryProvider.cs`. Tests will not compile as-is. (Constitution XI, gap: contradicts)
- [x] T050 Fix `GetAssetGroupsQueryTests.cs` assertions — query returns `List<AssetGroupResponse>` directly, not `Result<T>`. Replace `result.Succeeded.ShouldBeTrue()` / `result.Value.Count()` with `result.Count()` / `result[0].Name`. (Constitution XI, gap: contradicts)

### HIGH

- [x] T051 Add `AccountDepreciationId` (int?) to `CreateAssetGroupCommand`, `UpdateAssetGroupCommand`, `MappingExtensions.cs` (`AssetGroupDetailResponse` + `ToDetailResponse()`), endpoint DTOs (`CreateAssetGroupRequest`, `UpdateAssetGroupRequest`), frontend types (`AssetGroupDetail`, request interfaces), schemas, and all 3 page components (Create, Edit, Detail). Entity field `AccountDepreciationId` exists in DB but is not exposed through any API or UI. (FR-007, gap: partial)

### MEDIUM

- [x] T052 Add `requiredPermission` to all 4 asset group routes in `src/Web/ClientApp/src/app/routes.tsx`: list → `AssetGroups.View`, create → `AssetGroups.Create`, detail → `AssetGroups.View`, edit → `AssetGroups.Update`. All other routes in the app set this field. (FR-011, gap: partial)
- [x] T053 Fix `AssetGroupsListPage.tsx` toggle to include `rowVersion` — either add `rowVersion` to `AssetGroupResponse` record / `AssetGroupResponse` TypeScript interface, or fetch group detail before toggle. Currently passes empty string which fails backend validation. (FR-012, gap: partial)

### LOW

- [x] T054 Add cycle detection test cases to `CreateAssetGroupCommandTests.cs` and `UpdateAssetGroupCommandTests.cs` — test `WouldCreateCycleAsync` path when setting a parent that would create a cycle. (EC-004, gap: missing)
- [x] T055 Add functional happy-path tests for Create, Update, Get, and GetDetail endpoints in `tests/Application.FunctionalTests/Assets/AssetGroups/`. Current coverage is limited to ToggleActive and ErrorHandling. (EC-001 through EC-017, gap: missing)
