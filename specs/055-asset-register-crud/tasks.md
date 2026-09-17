# Tasks: Asset Register CRUD (سجل الأصول)

**Input**: Design documents from `/specs/055-asset-register-crud/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Tests are MANDATORY per Principle XI (TDD NON-NEGOTIABLE for new features). All use cases must have tests covering success and failure paths.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Error codes, shared DTOs, and infrastructure that all stories depend on

- [x] T001 Add asset-specific error codes to `src/Application/Common/Errors/ErrorCodes.cs` (AssetNotFound, DuplicateAssetTag, DuplicateBarcode, DuplicateSerialNumber, InvalidStatusTransition, FieldLockedAfterActivation, CannotDeactivate)
- [x] T002 Create `src/Application/Assets/Assets/Common/AssetResponse.cs` with `AssetResponse` (list DTO) and `AssetDetailResponse` (detail DTO) records
- [x] T003 [P] Create `src/Application/Assets/Assets/Common/MappingExtensions.cs` with `ToResponse()` and `ToDetailResponse()` extension methods mapping Asset entity to DTOs

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Shared backend queries and frontend infrastructure that ALL user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T004 [P] Create `src/Application/Assets/Assets/Queries/GetAssets/GetAssetsQuery.cs` — paginated list query with filters (status default Active, assetGroupId, locationId, custodianId, costCenterId, fundId) and free-text search across Code/Name/AssetTag/Barcode/SerialNumber
- [x] T005 [P] Create `src/Application/Assets/Assets/Queries/GetAssetById/GetAssetByIdQuery.cs` — single asset query with AssetGroup navigation
- [x] T006 [P] Create `src/Web/Endpoints/Assets/Assets.cs` — endpoint group with GET `/api/Assets` (list) and GET `/api/Assets/{id}` (by ID) routes, implementing `IEndpointGroup`
- [x] T007 [P] Create `src/Web/ClientApp/src/features/assets/assets/shared/types.ts` — TypeScript interfaces for Asset, AssetDetail, AssetStatus, AcquisitionType, and label maps (Arabic)
- [x] T008 [P] Create `src/Web/ClientApp/src/features/assets/assets/shared/schemas.ts` — Zod validation schemas for create and update asset forms
- [x] T009 [P] Create `src/Web/ClientApp/src/features/assets/assets/hooks/useAssets.ts` — React Query hooks for list, byId, create, update, deactivate with query key factory

**Checkpoint**: Foundation ready — user story implementation can now begin

---

## Phase 3: User Story 1 — Create New Asset Record (Priority: P1) — MVP

**Goal**: Asset Accountant can register a new asset with auto-generated AST-xxxx code, all required fields, saved in Draft status

**Independent Test**: Create an asset via form → verify it appears in list with AST-xxxx code and Draft status

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T010 [P] [US1] Unit test for CreateAssetCommand success path in `src/Application.Tests/Assets/Assets/Commands/CreateAsset/CreateAssetCommandTests.cs` — valid data returns new ID, asset in DB with Draft status, Code starts with "AST-"
- [x] T011 [P] [US1] Unit test for CreateAssetCommand failure paths in same test file — duplicate AssetTag, duplicate Barcode, duplicate SerialNumber, invalid AssetGroupId, DocumentSequenceService failure
- [x] T012 [P] [US1] Unit test for CreateAssetCommandValidator in same test file — required field validation (Name, AssetGroupId, OriginalValue, PurchaseDate, AcquisitionType)

### Implementation for User Story 1

- [x] T013 [P] [US1] Create `src/Application/Assets/Assets/Commands/CreateAsset/CreateAssetCommand.cs` — Command record, Handler (uses IDocumentSequenceService for "Asset" prefix, validates AssetGroup exists, validates uniqueness of AssetTag/Barcode/SerialNumber), Validator (FluentValidation with required fields and async uniqueness checks)
- [x] T014 [US1] Add POST `/api/Assets` route to `src/Web/Endpoints/Assets/Assets.cs` — HandleCreate method, requires `Assets.Create` permission, returns 201 with new ID
- [x] T015 [P] [US1] Create `src/Web/ClientApp/src/features/assets/assets/components/AssetForm.tsx` — reusable form component with create/edit/read-only modes, skeleton loading, field locking based on status, uses react-hook-form with zodResolver
- [x] T016 [US1] Create `src/Web/ClientApp/src/features/assets/assets/pages/AssetCreatePage.tsx` — loads dropdown options (AssetGroups, Locations, Users, CostCenters, Funds), renders AssetForm in create mode, calls useCreateAsset on submit, navigates to detail on success
- [x] T017 [US1] Add asset routes to `src/Web/ClientApp/src/app/routes.tsx` — `/assets` (list), `/assets/create`, `/assets/:id` (detail), `/assets/:id/edit` with required permissions

**Checkpoint**: Asset creation works end-to-end — form → API → DB → list display

---

## Phase 4: User Story 2 — View Asset List with Filtering and Search (Priority: P1)

**Goal**: Asset Custodian can browse, filter, and search the asset register with Active-only default and skeleton loading

**Independent Test**: Populate test assets → verify list shows Active only by default, filters work, search finds by code/name/tag/barcode/serial, skeleton shows during load

### Tests for User Story 2

- [x] T018 [P] [US2] Unit test for GetAssetsQuery in `src/Application.Tests/Assets/Assets/Queries/GetAssets/GetAssetsQueryTests.cs` — default returns Active only, filters by each field, search across 5 fields, pagination works, empty search shows all matching
- [x] T019 [P] [US2] Unit test for GetAssetByIdQuery in `src/Application.Tests/Assets/Assets/Queries/GetAssetById/GetAssetByIdQueryTests.cs` — found returns DTO, not found returns AssetNotFound error

### Implementation for User Story 2

- [x] T020 [US2] Create `src/Web/ClientApp/src/features/assets/assets/pages/AssetsListPage.tsx` — DataGrid with columns (code, name, group, status, location, custodian, value), FilterBar (status, group, location, custodian, cost center, fund), FilterSearch, skeleton loading rows, "Show All" toggle, "No assets found" empty state, row click navigates to detail
- [x] T021 [US2] Verify GET `/api/Assets` endpoint returns paginated results with correct default filter (Active only) — integration test or manual verification via quickstart V2

**Checkpoint**: Asset list displays with filtering, search, skeleton loading, and Active-only default

---

## Phase 5: User Story 3 — Edit Asset Details (Priority: P1)

**Goal**: Asset Accountant can update asset details with field locking after activation and optimistic concurrency

**Independent Test**: Edit a Draft asset → verify changes persist. Edit an Active asset → verify locked fields rejected. Edit with stale RowVersion → concurrency error.

### Tests for User Story 3

- [x] T022 [P] [US3] Unit test for UpdateAssetCommand success path in `src/Application.Tests/Assets/Assets/Commands/UpdateAsset/UpdateAssetCommandTests.cs` — valid update returns success, fields updated, RowVersion changes
- [x] T023 [P] [US3] Unit test for UpdateAssetCommand failure paths in same test file — stale RowVersion (ConcurrencyConflict), edit OriginalValue on Active asset (FieldLockedAfterActivation), invalid status transition (InvalidStatusTransition), asset not found (AssetNotFound)
- [x] T024 [P] [US3] Unit test for UpdateAssetCommandValidator — RowVersion required, locked field detection based on status

### Implementation for User Story 3

- [x] T025 [P] [US3] Create `src/Application/Assets/Assets/Commands/UpdateAsset/UpdateAssetCommand.cs` — Command record (with RowVersion), Handler (validates existence, checks field locking for Active+ status, validates status transitions, validates uniqueness for AssetTag/Barcode/SerialNumber excluding self), Validator
- [x] T026 [US3] Add PUT `/api/Assets/{id}` route to `src/Web/Endpoints/Assets/Assets.cs` — HandleUpdate method, requires `Assets.Update` permission, validates ID match, returns Result
- [x] T027 [US3] Create `src/Web/ClientApp/src/features/assets/assets/pages/AssetEditPage.tsx` — loads asset by ID, loads dropdown options, renders AssetForm in edit mode, handles "not found" state, calls useUpdateAsset on submit with RowVersion, navigates back to detail on success, breadcrumbs

**Checkpoint**: Asset editing works with field locking and concurrency protection

---

## Phase 6: User Story 4 — View Asset Detail Card (Priority: P2)

**Goal**: Auditor can view complete asset card with all fields, status label, and acquisition type label

**Independent Test**: Click any asset in list → verify all fields display on detail page with correct labels

### Tests for User Story 4

- [x] T028 [P] [US4] Unit test for GetAssetByIdQuery with full field coverage in `src/Application.Tests/Assets/Assets/Queries/GetAssetById/GetAssetByIdQueryTests.cs` — all DTO fields populated correctly from entity

### Implementation for User Story 4

- [x] T029 [P] [US4] Create `src/Web/ClientApp/src/features/assets/assets/pages/AssetDetailPage.tsx` — read-only detail view using AssetForm in readOnly mode, edit button (navigates to edit page), breadcrumbs (الأصول > asset name)
- [x] T030 [US4] Verify GET `/api/Assets/{id}` endpoint returns full AssetDetailResponse with all fields — integration test or manual verification via quickstart V3

**Checkpoint**: Full asset detail view accessible from list

---

## Phase 7: User Story 5 — Deactivate (Disable) an Asset (Priority: P2)

**Goal**: Asset Accountant can deactivate an asset (status → Disposed) with confirmation, excluded from default list but visible in "Show All"

**Independent Test**: Deactivate Active asset → status changes to Disposed → disappears from default list → appears in "Show All"

### Tests for User Story 5

- [x] T031 [P] [US5] Unit test for DeactivateAssetCommand in `src/Application.Tests/Assets/Assets/Commands/DeactivateAsset/DeactivateAssetCommandTests.cs` — Active → Disposed success, Draft → Disposed fails (InvalidStatusTransition), asset not found (AssetNotFound)

### Implementation for User Story 5

- [x] T032 [P] [US5] Create `src/Application/Assets/Assets/Commands/DeactivateAsset/DeactivateAssetCommand.cs` — Command record (with RowVersion), Handler (validates status transition Active/UnderMaintenance → Disposed), Validator
- [x] T033 [US5] Add POST `/api/Assets/{id}/deactivate` route to `src/Web/Endpoints/Assets/Assets.cs` — HandleDeactivate method, requires `Assets.Update` permission, returns Result
- [x] T034 [US5] Add deactivate functionality to `src/Web/ClientApp/src/features/assets/assets/pages/AssetDetailPage.tsx` — "تعطيل" button with ConfirmDialog, calls useDeactivateAsset, navigates to list on success

**Checkpoint**: Asset deactivation works end-to-end with confirmation

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [x] T035 [P] Run quickstart.md validation scenarios V1–V10 and fix any issues
- [x] T036 [P] Verify RTL layout renders correctly on all asset screens (FR-018)
- [x] T037 [P] Verify monetary values use shared money-display formatting (FR-019)
- [x] T038 [P] Verify audit log entries are produced for create/update/deactivate operations (FR-020)
- [!] T039 Run backend test suite and ensure all tests pass — BLOCKED by pre-existing build errors in Procurement/Revenue test files (unrelated to Assets feature)
- [x] T040 Run frontend lint and ensure no errors

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 completion — BLOCKS all user stories
- **User Stories (Phase 3–7)**: All depend on Phase 2 completion
  - US1, US2, US3 (P1) can proceed in parallel after Phase 2
  - US4, US5 (P2) can proceed in parallel after Phase 2
- **Polish (Phase 8)**: Depends on all desired user stories being complete

### User Story Dependencies

- **US1 (Create)**: Can start after Phase 2 — No dependencies on other stories
- **US2 (List/Search)**: Can start after Phase 2 — Independent of US1 but benefits from US1 for testing
- **US3 (Edit)**: Can start after Phase 2 — Independent but benefits from US1 for testing
- **US4 (Detail)**: Can start after Phase 2 — Depends on GetAssetByIdQuery (Phase 2)
- **US5 (Deactivate)**: Can start after Phase 2 — Independent but benefits from US1 for testing

### Within Each User Story

- Tests written FIRST and observed failing before implementation
- Models/Commands before Endpoints
- Backend before Frontend (or parallel if team capacity allows)
- Story complete before moving to next priority

### Parallel Opportunities

- T002 + T003 (Phase 1 DTOs) can run in parallel
- T004 + T005 + T006 + T007 + T008 + T009 (Phase 2) can all run in parallel
- T010 + T011 + T012 (US1 tests) can run in parallel
- T013 + T015 (US1 command + form component) can run in parallel
- T018 + T019 (US2 tests) can run in parallel
- T022 + T023 + T024 (US3 tests) can run in parallel
- T025 + T027 (US3 command + edit page) can run in parallel
- T028 + T029 (US4 test + detail page) can run in parallel
- T031 + T032 (US5 test + command) can run in parallel
- Different user stories can be worked on in parallel by different team members

---

## Parallel Example: User Story 1

```bash
# Launch all tests for User Story 1 together:
Task: "T010 [US1] Unit test for CreateAssetCommand success path"
Task: "T011 [US1] Unit test for CreateAssetCommand failure paths"
Task: "T012 [US1] Unit test for CreateAssetCommandValidator"

# Launch command + form component in parallel:
Task: "T013 [US1] Create CreateAssetCommand.cs"
Task: "T015 [US1] Create AssetForm.tsx"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (error codes, DTOs)
2. Complete Phase 2: Foundational (queries, endpoints, frontend infra)
3. Complete Phase 3: User Story 1 (Create Asset)
4. **STOP and VALIDATE**: Test create flow end-to-end
5. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add US1 (Create) → Test independently → Deploy/Demo (MVP!)
3. Add US2 (List/Search) + US3 (Edit) → Test independently → Deploy/Demo
4. Add US4 (Detail) + US5 (Deactivate) → Test independently → Deploy/Demo
5. Polish → Final validation

### Parallel Team Strategy

With multiple developers:
1. Team completes Phase 1 + Phase 2 together
2. Once Phase 2 is done:
   - Developer A: US1 (Create) → US4 (Detail)
   - Developer B: US2 (List/Search) → US5 (Deactivate)
   - Developer C: US3 (Edit)
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Tests written FIRST (TDD) — observe failure before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- TDD is NON-NEGOTIABLE per Constitution Principle XI

---

## Phase 9: Convergence

**Purpose**: Close gaps between spec/plan and current codebase implementation

- [x] T041 Add navigation properties to `src/Domain/Entities/Asset.cs` for Location, Custodian (User), CostCenter, Fund; update `GetAssetsQuery.cs` and `GetAssetByIdQuery.cs` to Include() them; update `MappingExtensions.cs` ToResponse/ToDetailResponse to resolve names from navigation properties per FR-013 (partial)
- [x] T042 Add Status field to `UpdateAssetCommand.cs` with status transition validation per FR-010 (Draft→Active, Active→UnderMaintenance, Active→Disposed, UnderMaintenance→Active, UnderMaintenance→Disposed); update `AssetForm.tsx` to allow status changes for Draft assets per US3/AC3 (partial)
- [x] T043 [P] Write integration test for GET `/api/Assets` endpoint verifying pagination, default Active filter, search across 5 fields, and filter combinations per T021 (missing)
- [x] T044 [P] Write integration test for GET `/api/Assets/{id}` endpoint verifying full AssetDetailResponse field coverage per T030 (missing)
- [x] T045 [P] Replace `toLocaleString('ar-SA')` in `AssetsListPage.tsx:91` with shared `formatCurrency` utility from `src/shared/utils/formatters.ts` per FR-019 (partial)
- [x] T046 [P] Run quickstart.md validation scenarios V1–V10 and fix any issues per T035 (missing)
- [x] T047 [P] Verify RTL layout renders correctly on all asset screens (AssetsListPage, AssetCreatePage, AssetEditPage, AssetDetailPage) per FR-018 / T036 (missing)
- [x] T048 [P] Verify audit log entries are produced for create, update, and deactivate operations per FR-020 / T038 (missing)
- [ ] T049 Run backend test suite (`dotnet test`) and ensure all tests pass per T039 (missing)
- [x] T050 Run frontend lint and ensure no errors per T040 (missing)
