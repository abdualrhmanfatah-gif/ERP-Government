# Tasks: Asset Attributes (خصائص الأصول)

**Input**: Design documents from `specs/059-asset-attributes/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Tests are included per constitution Principle XI (TDD non-negotiable for new features).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## User Stories

| Story | Title | Priority |
|-------|-------|----------|
| US1 | Manage Attribute Definitions | P1 |
| US2 | Bind Attributes to Asset Groups | P1 |
| US3 | Enter Attribute Values During Asset Registration | P1 |
| US4 | View Attribute Values in Asset Detail | P1 |
| US5 | Bulk Manage Definitions via List | P2 |

---

## Phase 1: Foundational — Data Type Expansion

**Purpose**: Expand AssetAttributeDataType from 4 to 5 types; add IntegerValue column; rename NumericValue to DecimalValue. This is a prerequisite for ALL user stories.

- [X] T001 [P] Expand AssetAttributeDataType enum to 5 members (Text=1, Integer=2, Decimal=3, Date=4, Boolean=5) in src/Domain/Assets/Enums/AssetAttributeDataType.cs
- [X] T002 [P] Add IntegerValue? int column to AssetAttributeValue entity in src/Domain/Assets/Entities/AssetAttributeValue.cs
- [X] T003 [P] Rename NumericValue to DecimalValue in AssetAttributeValue entity in src/Domain/Assets/Entities/AssetAttributeValue.cs
- [X] T004 Update EF configuration: add IntegerValue column, rename NumericValue in src/Infrastructure/Data/Configurations/Assets/AssetAttributeValueConfiguration.cs
- [X] T005 Update AttributeValueValidator: handle Integer type, check IntegerValue column in src/Application/Assets/AssetAttributes/Values/AttributeValueValidator.cs
- [X] T006 Update seed data: adjust enum values if needed in src/Infrastructure/Data/Seeds/AssetAttributeDefinitionSeedData.cs
- [X] T007 [P] Unit tests: AssetAttributeDataType enum has 5 members in tests/Domain.UnitTests/Assets/
- [X] T008 [P] Unit tests: AttributeValueValidator rejects wrong type for Integer and Decimal in tests/Application.UnitTests/Assets/AssetAttributes/

**Checkpoint**: 5 types work end-to-end; unit tests pass

---

## Phase 2: User Story 1 — Manage Attribute Definitions (Priority: P1)

**Goal**: Asset configuration administrator can create, view, update, and deactivate attribute definitions.

**Independent Test**: Can be fully tested by creating definitions of all 5 data types, verifying uniqueness of codes, blocking duplicate codes, deactivating a definition, and verifying it no longer appears in active lists.

### Tests for User Story 1

- [X] T009 [P] [US1] Unit tests: definitions query with search, filter by type/status, pagination in tests/Application.UnitTests/Assets/AssetAttributes/
- [X] T010 [P] [US1] Unit tests: FR-021 blocks DataType change when values exist in tests/Application.UnitTests/Assets/AssetAttributes/
- [ ] T011 [P] [US1] Functional test: definitions CRUD lifecycle with filters in tests/Application.FunctionalTests/Assets/AssetAttributes/

### Implementation for User Story 1

- [X] T012 Update CreateAssetAttributeDefinitionCommand: accept Description, Unit, SortOrder in src/Application/Assets/AssetAttributes/Definitions/Commands/CreateAssetAttributeDefinitionCommand.cs
- [X] T013 Update UpdateAssetAttributeDefinitionCommand: accept Description, Unit, SortOrder, IsActive + FR-021 validation in src/Application/Assets/AssetAttributes/Definitions/Commands/UpdateAssetAttributeDefinitionCommand.cs
- [X] T014 Update GetAssetAttributeDefinitionsQuery: add search, dataType filter, isActive filter, pagination, totalCount in src/Application/Assets/AssetAttributes/Definitions/Queries/GetAssetAttributeDefinitionsQuery.cs
- [X] T015 Create GetAssetAttributeDefinitionByIdQuery with linkedValueCount in src/Application/Assets/AssetAttributes/Definitions/Queries/GetAssetAttributeDefinitionByIdQuery.cs
- [X] T016 Add GET /api/AssetAttributes/definitions/{id} endpoint in src/Web/Endpoints/Assets/AssetAttributes.cs
- [X] T017 Update GET /api/AssetAttributes/definitions endpoint: add query parameters in src/Web/Endpoints/Assets/AssetAttributes.cs
- [ ] T018 [P] Create feature folder structure: src/Web/ClientApp/src/features/assets/asset-attributes/
- [ ] T019 [P] Create types.ts: definition types, request/response types in src/Web/ClientApp/src/features/assets/asset-attributes/shared/types.ts
- [ ] T020 [P] Create schemas.ts: Zod validation schemas in src/Web/ClientApp/src/features/assets/asset-attributes/shared/schemas.ts
- [ ] T021 Create useAssetAttributes.ts: React Query hooks for CRUD in src/Web/ClientApp/src/features/assets/asset-attributes/hooks/useAssetAttributes.ts
- [ ] T022 Create AssetAttributeForm.tsx: form component with all fields in src/Web/ClientApp/src/features/assets/asset-attributes/components/AssetAttributeForm.tsx
- [ ] T023 Create AssetAttributesListPage.tsx: list with search/filter/pagination in src/Web/ClientApp/src/features/assets/asset-attributes/pages/AssetAttributesListPage.tsx
- [ ] T024 Create AssetAttributeCreatePage.tsx: create form page in src/Web/ClientApp/src/features/assets/asset-attributes/pages/AssetAttributeCreatePage.tsx
- [ ] T025 Create AssetAttributeEditPage.tsx: edit form page in src/Web/ClientApp/src/features/assets/asset-attributes/pages/AssetAttributeEditPage.tsx
- [ ] T026 Add navigation entry: "تعريفات الخصائص" in src/Web/ClientApp/src/layouts/navigation.ts
- [ ] T027 Add routes in src/Web/ClientApp/src/routes.tsx
- [ ] T028 Regenerate web-api-client after contract changes

**Checkpoint**: Attribute definitions CRUD fully functional in UI

---

## Phase 3: User Story 5 — Bulk Manage Definitions via List (Priority: P2)

**Goal**: Administrator can see all attribute definitions in a filterable, sortable list with pagination.

**Independent Test**: Can be fully tested by creating 10+ definitions, filtering by name/code/type, sorting by name/type/status, and verifying pagination works correctly.

### Tests for User Story 5

- [ ] T029 [P] [US5] Unit tests: pagination returns correct totalCount and page in tests/Application.UnitTests/Assets/AssetAttributes/
- [ ] T030 [P] [US5] Acceptance test: definitions list displays with filters in tests/Web.AcceptanceTests/

### Implementation for User Story 5

- [ ] T031 Update AssetAttributesListPage.tsx: add pagination component in src/Web/ClientApp/src/features/assets/asset-attributes/pages/AssetAttributesListPage.tsx
- [ ] T032 Update AssetAttributesListPage.tsx: add sort by column header in src/Web/ClientApp/src/features/assets/asset-attributes/pages/AssetAttributesListPage.tsx
- [ ] T033 Update useAssetAttributes.ts: add pagination and sort state in src/Web/ClientApp/src/features/assets/asset-attributes/hooks/useAssetAttributes.ts

**Checkpoint**: Definitions list with search/filter/pagination fully functional

---

## Phase 4: User Story 2 — Bind Attributes to Asset Groups (Priority: P1)

**Goal**: Asset group manager can manage which attribute definitions are bound to a specific asset group, specifying required/optional and sort order.

**Independent Test**: Can be fully tested by navigating to a group's attribute management page, adding/removing bindings, setting required flags and sort order, and verifying the bindings are saved atomically.

### Tests for User Story 2

- [ ] T034 [P] [US2] Unit tests: group detail includes attributeBindings list in tests/Application.UnitTests/Assets/AssetGroups/
- [ ] T035 [P] [US2] Unit tests: SetGroupAttributeBindingsCommand saves atomically in tests/Application.UnitTests/Assets/AssetAttributes/
- [ ] T036 [P] [US2] Functional test: group bindings CRUD lifecycle in tests/Application.FunctionalTests/Assets/AssetGroups/

### Implementation for User Story 2

- [ ] T037 Update GetAssetGroupDetailQuery response: add attributeBindings list in src/Application/Assets/AssetGroups/Queries/GetAssetGroupDetail/GetAssetGroupDetailQuery.cs
- [ ] T038 Update AssetGroups.cs endpoint: include bindings in detail response in src/Web/Endpoints/Assets/AssetGroups.cs
- [ ] T039 Create AssetGroupAttributesPage.tsx: bindings table with add/remove/save in src/Web/ClientApp/src/features/assets/asset-groups/pages/AssetGroupAttributesPage.tsx
- [ ] T040 Update useAssetGroups.ts: add bindings API methods in src/Web/ClientApp/src/features/assets/asset-groups/hooks/useAssetGroups.ts
- [ ] T041 Add route: /assets/groups/:id/attributes in src/Web/ClientApp/src/routes.tsx
- [ ] T042 Add navigation link from group detail page in src/Web/ClientApp/src/features/assets/asset-groups/pages/AssetGroupDetailPage.tsx
- [ ] T043 Regenerate web-api-client after contract changes

**Checkpoint**: Group attributes management fully functional

---

## Phase 5: User Story 3 — Enter Attribute Values During Asset Registration (Priority: P1)

**Goal**: Asset registrar can enter typed attribute values when creating or editing an asset via a dynamic section that appears based on group selection.

**Independent Test**: Can be fully tested by selecting a group on the asset form, seeing the dynamic attributes section appear, filling values of the correct types, saving, and verifying the values persist.

### Tests for User Story 3

- [ ] T044 [P] [US3] Unit tests: asset detail includes attributeValues + currentDepartment in tests/Application.UnitTests/Assets/Assets/
- [ ] T045 [P] [US3] Unit tests: AssetAttributeValueValidator checks required from binding in tests/Application.UnitTests/Assets/AssetAttributes/
- [ ] T046 [P] [US3] Functional test: asset create with attribute values persists correctly in tests/Application.FunctionalTests/Assets/Assets/

### Implementation for User Story 3

- [ ] T047 Update GetAssetByIdQuery response: add attributeValues list + currentDepartment in src/Application/Assets/Assets/Queries/GetAssetById/GetAssetByIdQuery.cs
- [ ] T048 Update Assets.cs endpoint: include values in detail response in src/Web/Endpoints/Assets/Assets.cs
- [ ] T049 Create AssetAttributesSection.tsx: dynamic section component in src/Web/ClientApp/src/features/assets/assets/components/AssetAttributesSection.tsx
- [ ] T050 Update AssetForm.tsx: integrate dynamic section, handle group change in src/Web/ClientApp/src/features/assets/assets/components/AssetForm.tsx
- [ ] T051 Update useAssets.ts: add attribute values to create/update requests in src/Web/ClientApp/src/features/assets/assets/hooks/useAssets.ts
- [ ] T052 Update schemas.ts: dynamic Zod fields per group bindings in src/Web/ClientApp/src/features/assets/assets/shared/schemas.ts
- [ ] T053 Ensure data-attribute selectors match acceptance test expectations in src/Web/ClientApp/src/features/assets/assets/components/AssetAttributesSection.tsx
- [ ] T054 Acceptance test: select group → attributes appear, fill values, save in tests/Web.AcceptanceTests/

**Checkpoint**: Asset form shows dynamic attributes section with correct validation

---

## Phase 6: User Story 4 — View Attribute Values in Asset Detail (Priority: P1)

**Goal**: Asset reviewer can see all attribute values in the asset detail view with proper formatting and deactivated indicators.

**Independent Test**: Can be fully tested by navigating to an asset's detail page and verifying that all bound attributes are displayed with their values.

### Tests for User Story 4

- [ ] T055 [P] [US4] Acceptance test: asset detail shows attributes tab with values in tests/Web.AcceptanceTests/

### Implementation for User Story 4

- [ ] T056 Create AssetAttributesTab.tsx: values table with formatting in src/Web/ClientApp/src/features/assets/assets/components/AssetAttributesTab.tsx
- [ ] T057 Update AssetDetailPage.tsx: add attributes tab in src/Web/ClientApp/src/features/assets/assets/pages/AssetDetailPage.tsx

**Checkpoint**: Asset detail shows attribute values with correct formatting

---

## Phase 7: Tests + Acceptance Completion

**Purpose**: Complete test coverage and acceptance test execution.

- [ ] T058 Update AssetLifecycle.feature: remove placeholder steps for attribute binding in tests/Web.AcceptanceTests/Features/AssetLifecycle.feature
- [ ] T059 Update AssetLifecycleStepDefinitions.cs: implement WhenTheUserBindsAttribute with real UI interaction in tests/Web.AcceptanceTests/StepDefinitions/AssetLifecycleStepDefinitions.cs
- [ ] T060 Update AssetRegisterPage.cs: implement SetAttributeValue with real selector in tests/Web.AcceptanceTests/Pages/AssetRegisterPage.cs
- [ ] T061 Update AssetGroupPage.cs: add attribute management selectors in tests/Web.AcceptanceTests/Pages/AssetGroupPage.cs
- [ ] T062 Run full unit test suite: `dotnet test tests/Application.UnitTests/`
- [ ] T063 Run full functional test suite: `dotnet test tests/Application.FunctionalTests/`
- [ ] T064 Run acceptance tests and verify all pass
- [ ] T065 Run full build: `dotnet build`

**Checkpoint**: All tests green; acceptance tests pass

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Foundational)**: No dependencies — can start immediately. BLOCKS all user stories.
- **Phase 2 (US1)**: Depends on Phase 1 completion
- **Phase 3 (US5)**: Depends on Phase 2 completion (extends US1)
- **Phase 4 (US2)**: Depends on Phase 1 completion. Can run in parallel with Phase 2.
- **Phase 5 (US3)**: Depends on Phase 1 + Phase 4 completion (needs bindings to exist)
- **Phase 6 (US4)**: Depends on Phase 1 + Phase 5 completion (needs values to exist)
- **Phase 7 (Tests)**: Depends on all previous phases

### User Story Dependencies

- **US1 (P1)**: Can start after Phase 1 — No dependencies on other stories
- **US5 (P2)**: Extends US1 — depends on US1 completion
- **US2 (P1)**: Can start after Phase 1 — Independent of US1
- **US3 (P1)**: Depends on US2 (needs bindings to exist for dynamic section)
- **US4 (P1)**: Depends on US3 (needs values to exist for detail display)

### Within Each User Story

- Tests written and observed FAILING before implementation (TDD)
- Backend before frontend (endpoints needed for client)
- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- Phase 1 tasks T001-T003 can run in parallel
- Phase 1 tests T007-T008 can run in parallel
- Phase 2 tests T009-T011 can run in parallel
- Phase 2 frontend tasks T018-T020 can run in parallel
- Phase 2 and Phase 4 can run in parallel (US1 and US2 independent)
- Phase 4 tests T034-T036 can run in parallel
- Phase 5 tests T044-T046 can run in parallel

---

## Parallel Example: User Story 1

```bash
# Launch all tests for User Story 1 together:
Task: "Unit tests: definitions query with search, filter by type/status, pagination"
Task: "Unit tests: FR-021 blocks DataType change when values exist"
Task: "Functional test: definitions CRUD lifecycle with filters"

# Launch all frontend tasks for User Story 1 together:
Task: "Create feature folder structure"
Task: "Create types.ts"
Task: "Create schemas.ts"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Foundational (Data Type Expansion)
2. Complete Phase 2: User Story 1 (Manage Attribute Definitions)
3. **STOP and VALIDATE**: Test US1 independently
4. Deploy/demo if ready

### Incremental Delivery

1. Phase 1 → Foundation ready
2. Phase 2 → US1 complete → Test independently → Deploy/Demo (MVP!)
3. Phase 4 → US2 complete → Test independently → Deploy/Demo
4. Phase 5 → US3 complete → Test independently → Deploy/Demo
5. Phase 6 → US4 complete → Test independently → Deploy/Demo
6. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Phase 1 together
2. Once Phase 1 is done:
   - Developer A: Phase 2 (US1) + Phase 3 (US5)
   - Developer B: Phase 4 (US2)
3. Once Phase 4 is done:
   - Developer A: Phase 5 (US3)
   - Developer B: Phase 6 (US4)
4. Phase 7: Team completes tests together

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing (TDD)
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
