# Tasks: EF Migration Baseline

**Input**: Design documents from `/specs/001-ef-migration-baseline/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: The examples below include test tasks. Tests are OPTIONAL - only include them if explicitly requested in the feature specification.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Single project**: `src/`, `tests/` at repository root
- **Web app**: `backend/src/`, `frontend/src/`
- **Mobile**: `api/src/`, `ios/src/` or `android/src/`
- Paths shown below assume single project - adjust based on plan.md structure

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Verify .NET SDK 10.0 installed and EF Core tools available
- [X] T002 Ensure project references EF Core SQL Server provider and Aspire packages
- [X] T003 [P] Configure design-time DbContext factory for Infrastructure project

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**?? CRITICAL**: No user story work can begin until this phase is complete

Examples of foundational tasks (adjust based on your project):

- [X] T004 Verify existing database schema is accessible via connection string
- [X] T005 [P] Ensure EF Core migrations directory exists (src/Infrastructure/Data/Migrations)
- [X] T006 [P] Create baseline validation script (scripts/validate-baseline.ps1)
- [X] T007 Document current EF Core version and compatibility matrix
- [X] T008 Setup test database for validation (Aspire hosting or local SQL Server)

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Establish Baseline Migration (Priority: P1) ?? MVP

**Goal**: Developers can generate an initial baseline migration from the EF model definition, reconciled with the existing database schema.

**Independent Test**: Run dotnet ef migrations add command and verify migration files are created with correct up/down operations.

### Implementation for User Story 1

- [X] T009 [P] [US1] Create design-time DbContext factory in src/Infrastructure/DesignTimeDbContextFactory.cs
- [X] T010 [US1] Generate baseline migration using dotnet ef migrations add InitialBaseline --context YourDbContext --output-dir Data/Migrations
- [X] T011 [US1] Verify migration file contains schema creation operations (up) and rollback operations (down)
- [X] T012 [US1] Ensure migration is idempotent and safe to apply multiple times
- [X] T013 [US1] Add migration to source control and verify it builds

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - Validate Baseline Against Existing Database (Priority: P2)

**Goal**: Developers can validate that the baseline migration accurately reflects the current database schema.

**Independent Test**: Apply baseline migration to test database and run validation script to compare schema.

### Implementation for User Story 2

- [X] T014 [P] [US2] Create validation script that compares EF model snapshot with database schema in scripts/compare-schema.ps1
- [X] T015 [US2] Implement schema comparison logic using EF Core metadata or SQL queries
- [X] T016 [US2] Add error reporting for discrepancies (list missing tables, columns, etc.)
- [X] T017 [US2] Test validation against existing database and baseline migration
- [X] T018 [US2] Document validation process in quickstart.md

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 3 - Document Baseline Process (Priority: P3)

**Goal**: Developers have clear documentation on how to create and manage the migration baseline.

**Independent Test**: New developer follows documentation to set up a baseline without assistance.

### Implementation for User Story 3

- [X] T019 [P] [US3] Update quickstart.md with step-by-step instructions for baseline generation
- [X] T020 [US3] Add troubleshooting section for common errors (connection, version mismatch)
- [X] T021 [US3] Include cross-version compatibility testing steps (EF Core 6.x, 7.x, 8.x)
- [X] T022 [US3] Create README.md in specs/001-ef-migration-baseline/ summarizing feature
- [X] T023 [US3] Add example commands and expected outputs

**Checkpoint**: All user stories should now be independently functional

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [X] T024 [P] Run quickstart.md validation scenarios end-to-end
- [X] T025 Code cleanup and refactoring (if any custom code added)
- [X] T026 Performance optimization (ensure migration generation is fast)
- [X] T027 [P] Additional unit tests (if requested) in tests/unit/
- [X] T028 Security hardening (ensure connection strings not exposed)
- [X] T029 Update documentation with final validation results

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 ? P2 ? P3)
- **Polish (Final Phase)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - May integrate with US1 but should be independently testable
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - May integrate with US1/US2 but should be independently testable

### Within Each User Story

- Tests (if included) MUST be written and FAIL before implementation
- Models before services
- Services before endpoints
- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- Once Foundational phase completes, all user stories can start in parallel (if team capacity allows)
- All tests for a user story marked [P] can run in parallel
- Models within a story marked [P] can run in parallel
- Different user stories can be worked on in parallel by different team members

---

## Parallel Example: User Story 1

```bash
# Launch all tests for User Story 1 together (if tests requested):
Task: "Contract test for [endpoint] in tests/contract/test_[name].py"
Task: "Integration test for [user journey] in tests/integration/test_[name].py"

# Launch all models for User Story 1 together:
Task: "Create [Entity1] model in src/models/[entity1].py"
Task: "Create [Entity2] model in src/models/[entity2].py"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Test User Story 1 independently
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational ? Foundation ready
2. Add User Story 1 ? Test independently ? Deploy/Demo (MVP!)
3. Add User Story 2 ? Test independently ? Deploy/Demo
4. Add User Story 3 ? Test independently ? Deploy/Demo
5. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1
   - Developer B: User Story 2
   - Developer C: User Story 3
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
