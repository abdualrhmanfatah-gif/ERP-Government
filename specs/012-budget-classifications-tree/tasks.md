# Tasks: Budget Classifications Tree #2 of 5

**Input**: Design documents from `/specs/012-budget-classifications-tree/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/frontend-api-contract.md

**Tests**: TDD required per Constitution Principle XI. Every behavior has a test task that must be observed failing before its implementation task begins.

**Organization**: Tasks grouped by user story for independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- **[UX]**: Behavior marker referencing test-list.md id — `/speckit.tdd.run` ticks this checkbox only when the behavior's test is green
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create directory structure and barrel exports for the classifications feature module

- [X] T001 Create directory structure: `src/Web/ClientApp/src/features/budgeting/classifications/hooks/`, `features/budgeting/classifications/pages/`
- [X] T002 [P] Create barrel file `src/Web/ClientApp/src/features/budgeting/classifications/index.ts` re-exporting from hooks and pages sub-modules

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Hooks that ALL user stories depend on

**CRITICAL**: No user story work can begin until this phase is complete

### User Story 1 - Classification Hooks (Priority: P1)

**Goal**: Provide query and mutation hooks for the classification tree

**Independent Test**: Import hooks, verify compilation, verify cache keys are stable

- [X] T003 [US1] [U20] [U21] [U22] Create `src/Web/ClientApp/src/features/budgeting/classifications/hooks/useClassifications.ts` — test: mutation hooks invalidate budgetClassifications.all on success. Implementation: useClassificationsTree query hook (uses budgetingKeys.budgetClassifications.tree), useClassificationDetail query hook, useCreateClassification mutation, useUpdateClassification mutation, useToggleClassificationActive mutation

**Checkpoint**: All hooks compile. Cache keys return stable arrays.

---

## Phase 3: User Story 1 - Classification Tree View (Priority: P1) — MVP

**Goal**: Budget administrator can view the full classification hierarchy as an expandable tree with search, filtering, and keyboard navigation

**Independent Test**: Navigate to /budgeting/budget-classifications, view tree, expand/collapse nodes, search, filter by active state

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T004 [P] [US1] [U1] [U2] [U3] [U4] Test tree normalization in `src/features/budgeting/__tests__/ClassificationsListPage.test.tsx` — orphaned parentId → root-level, cycle → root-level, warning toast on normalization, valid tree unchanged
- [X] T005 [P] [US1] [U5] [U6] [U7] [U8] [U9] [U10] Test expand/collapse in `src/features/budgeting/__tests__/ClassificationsListPage.test.tsx` — initial state (first level expanded), toggle expand/collapse, empty state, single-node tree
- [X] T006 [P] [US1] [U11] [U12] [U13] [U14] [U15] [U16] Test search in `src/features/budgeting/__tests__/ClassificationsListPage.test.tsx` — match code, match name, auto-expand ancestors, non-matching collapsed, clear restores state, no matches
- [X] T007 [P] [US1] [U17] [U18] [U19] Test filter in `src/features/budgeting/__tests__/ClassificationsListPage.test.tsx` — active filter, inactive filter, no filter

### Implementation for User Story 1

- [X] T008 [US1] Create `src/Web/ClientApp/src/features/budgeting/classifications/pages/ClassificationsListPage.tsx` with PageHeader (title: "التصنيفات المالية"), loading skeleton, error state with retry, empty state with create action
- [X] T009 [US1] [U1] [U2] [U3] [U4] Implement tree normalization function: orphaned parentId → root-level, cycle → root-level, warning toast, valid tree unchanged
- [X] T010 [US1] [U5] [U6] [U7] [U8] [U9] [U10] Implement TreeItem recursive component with expand/collapse toggle, Code, Name, Level badge, RTL indentation via padding-inline-start; initial state = first level expanded
- [X] T011 [US1] [U11] [U12] [U13] [U14] [U15] [U16] Add search: FilterSearch component, client-side filter on Code/Name, auto-expand ancestors, non-matching collapsed, clear restores state
- [X] T012 [US1] [U17] [U18] [U19] Add IsActive filter: FilterSelect with نشط/معطل options, filter nodes and show ancestors
- [X] T013 [US1] Add keyboard navigation: Right arrow expand, Left arrow collapse, Up/Down arrows move focus (WAI-ARIA Treeview)

**Checkpoint**: Tree view fully functional. Search, filter, keyboard nav work. Malformed data handled.

---

## Phase 4: User Story 2 - Create and Edit Dialog (Priority: P1)

**Goal**: Budget administrator can create/edit classifications via dialog with parent tree-select and cycle prevention

**Independent Test**: Create root classification, create child, edit classification, verify cycle prevention

### Tests for User Story 2

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T014 [P] [US2] [U23] [U24] [U25] [U26] Test cycle prevention utility `getExcludedDescendantIds` — leaf node returns empty, node with children returns self + descendants, no exclusion beyond self, inactive nodes NOT excluded
- [X] T015 [P] [US2] [U27] [U28] Test validation in `src/features/budgeting/__tests__/ClassificationsListPage.test.tsx` — empty Code blocks submission, empty Name blocks submission
- [X] T016 [P] [US2] [U29] [U30] Test permission gating in `src/features/budgeting/__tests__/ClassificationsListPage.test.tsx` — create button hidden without Create permission, edit button hidden without Update permission

### Implementation for User Story 2

- [X] T017 [P] [US2] [U23] [U24] [U25] [U26] Create `src/Web/ClientApp/src/features/budgeting/classifications/utils/classification-utils.ts` with getExcludedDescendantIds(tree, excludeId) — BFS to collect descendant IDs
- [X] T018 [US2] Add create/edit dialog to ClassificationsListPage: Dialog, fields Code (Input, required), Name (Input, required), ParentId (Combobox tree-select, optional), IsActive (Switch)
- [X] T019 [US2] Implement parent tree-select: render tree nodes inside Combobox dropdown, on create show all, on edit exclude self + descendants via getExcludedDescendantIds
- [X] T020 [US2] Wire create flow: open dialog → fill form → submit → close dialog → invalidate tree → success toast
- [X] T021 [US2] Wire edit flow: click edit → open dialog pre-filled → submit → close dialog → invalidate tree → success toast
- [X] T022 [US2] [U27] [U28] Add validation: Code and Name required, empty values block submission, show validation errors
- [X] T023 [US2] [U29] [U30] Add permission gating: BudgetClassifications.Create for create button, BudgetClassifications.Update for edit button

**Checkpoint**: Create/edit dialog fully functional. Cycle prevention works. Permissions enforced.

---

## Phase 5: User Story 3 - Toggle Active State (Priority: P2)

**Goal**: Budget administrator can toggle IsActive with confirmation dialog and RowVersion conflict handling

**Independent Test**: Toggle active state, verify tree reflects change, verify error handling

### Tests for User Story 3

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T024 [P] [US3] [A13] [A14] [A15] Test toggle flow in `src/features/budgeting/__tests__/ClassificationsListPage.test.tsx` — switch opens confirm dialog, confirm triggers mutation + toast, RowVersion conflict shows error toast + refetch
- [X] T025 [P] [US3] [U31] Test permission gating — IsActive switch hidden without Update permission

### Implementation for User Story 3

- [X] T026 [US3] [A13] Add IsActive switch to each tree node, permission-gated (BudgetClassifications.Update)
- [X] T027 [US3] [A14] Wire toggle flow: click switch → ConfirmDialog → confirm → toggle mutation → invalidate tree → success toast
- [X] T028 [US3] [A15] Handle RowVersion conflict: error toast + tree refetch on 409 response

**Checkpoint**: Toggle active state fully functional. Error handling works.

---

## Phase 6: User Story 4 - Route and Sidebar Navigation (Priority: P1)

**Goal**: Users can reach Budget Classifications page via sidebar link

**Independent Test**: Load app, verify sidebar link appears, click navigates to correct page

### Tests for User Story 4

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T029 [P] [US4] [U32] [U33] [U34] [U35] Test route and sidebar in `src/features/budgeting/__tests__/ClassificationsRoute.test.ts` — route maps to page, sidebar includes link with permission, active state detection

### Implementation for User Story 4

- [X] T030 [US4] [U32] Register route in `src/Web/ClientApp/src/app/routes.tsx`: /budgeting/budget-classifications → ClassificationsListPage
- [X] T031 [US4] [U33] [U34] Update sidebar "الموازنة" group in `src/Web/ClientApp/src/layouts/navigation.ts`: add "التصنيفات المالية" link with BudgetClassifications.View permission
- [X] T032 [US4] [U35] Verify sidebar renders link, active state detection works, RTL layout correct

**Checkpoint**: Route and sidebar functional. Link navigates correctly.

---

## Phase 7: Outer-Loop Acceptance Tests

**Purpose**: One test per acceptance criterion, exercising the real entry point (component render). Must be green before story is considered complete.

- [X] T033 [A1] [A2] [A6] Test tree view renders all classifications with Code, Name, Level, expand toggle, RTL indentation, 5+ levels — component test in vitest/jsdom
- [X] T034 [A3] Test collapse hides descendants and toggle icon updates — component test
- [X] T035 [A4] Test search auto-expands ancestors, non-matching collapsed — component test
- [X] T036 [A5] Test IsActive filter shows matching nodes and ancestors — component test
- [X] T037 [A7] [A8] [A9] Test create dialog fields, create with/without parent — component test
- [X] T038 [A10] [A11] [A12] Test edit dialog pre-fill, cycle prevention, validation, save flow — component test
- [X] T039 [A13] [A14] [A15] Test toggle confirmation, success toast, RowVersion conflict — component test
- [X] T040 [A16] [A17] [A18] Test sidebar link visible, navigates correctly, active state — component test

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Final quality pass across all user stories

- [X] T041 [P] Verify SC-001: run build, confirm zero errors and zero type-checking failures
- [X] T042 [P] Verify SC-002: create 5+ level hierarchy, confirm RTL indentation correct at every level
- [X] T043 [P] Verify SC-003: test cycle prevention UI-side + backend error surfacing
- [X] T044 [P] Verify SC-004: test RowVersion conflict handling
- [X] T045 [P] Verify SC-005: toggle dark mode on all pages, confirm zero hardcoded color/font values, keyboard nav works
- [ ] T046 Run quickstart.md validation scenarios V1-V14 against live backend

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — start immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 — BLOCKS all user stories
- **Phase 3 (US1)**: Depends on Phase 2 complete
- **Phase 4 (US2)**: Depends on Phase 2 complete — can run parallel with Phase 3
- **Phase 5 (US3)**: Depends on Phase 2 complete — can run parallel with Phase 3/4
- **Phase 6 (US4)**: Depends on Phase 3 complete (needs page component for route)
- **Phase 7 (Outer-loop)**: Depends on corresponding inner-loop phase complete
- **Phase 8 (Polish)**: Depends on all phases complete

### User Story Dependencies

- **US1 (Tree View)**: Phase 3 — depends on shared types/client only (from spec #1)
- **US2 (Create/Edit)**: Phase 4 — depends on US1 (needs tree rendering for parent select)
- **US3 (Toggle Active)**: Phase 5 — depends on US1 (needs tree rendering for switch)
- **US4 (Route/Sidebar)**: Phase 6 — depends on US1 (needs page component)

### Within Each User Story

- Tests FIRST (write, observe fail, then implement)
- Hooks before pages (pages consume hooks)
- Pages before routes (routes reference pages)
- Tree normalization before tree rendering
- Core implementation before polish

### Parallel Opportunities

- Phase 1: T001 and T002 can run in parallel
- Phase 3 tests: T004, T005, T006, T007 can run in parallel (different behaviors)
- Phase 4 tests: T014, T015, T016 can run in parallel
- Phase 5 tests: T024, T025 can run in parallel
- Phase 3 and Phase 4 can run in parallel (different concerns)
- Phase 5 can run in parallel with Phase 3/4
- Phase 7 tests: T033-T040 can run in parallel
- Phase 8 tasks: T041-T045 can run in parallel

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- [UX] behavior marker — `/speckit.tdd.run` reads this to tick checkbox when test is green
- Tests FIRST: every implementation task has a preceding test task with the same behavior markers
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
