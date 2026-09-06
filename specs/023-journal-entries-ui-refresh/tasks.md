# Tasks: Journal Entries UI Refresh

**Input**: Design documents from `/specs/023-journal-entries-ui-refresh/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Frontend tests required per constitution principle XI and spec. TDD approach: write tests first, observe red, implement, green.

**Organization**: Tasks grouped by user story for independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Remove broken Move-based files, update types and routing

- [x] T001 Delete old Move-based page files: `src/Web/ClientApp/src/features/accounting/pages/MovesListPage.tsx`, `MoveCreatePage.tsx`, `MoveDetailPage.tsx`
- [x] T002 Delete old Move-based component files: `src/Web/ClientApp/src/features/accounting/components/MovesGrid.tsx`, `MoveDetail.tsx`, `MoveForm.tsx`, `MoveLinesEditor.tsx`
- [x] T003 Delete old Move-based hook files: `src/Web/ClientApp/src/features/accounting/hooks/useMoves.ts`, `useMoveLines.ts`
- [x] T004 Delete hand-written client: `src/Web/ClientApp/src/features/accounting/client.ts`
- [x] T005 Update `src/Web/ClientApp/src/features/accounting/types.ts` — replace MoveDto/MoveLineDto types with JournalEntryDto/JournalEntryLineDto types matching data-model.md interfaces

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Shared components and hooks that ALL user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T006 [P] Create `src/Web/ClientApp/src/features/accounting/components/StatusBadge.tsx` — EntryStatus enum to colored badge mapping (Draft=gray, Submitted=blue, Approved=amber, Posted=green, Reversed=orange, Cancelled=red)
- [x] T007 [P] Create `src/Web/ClientApp/src/features/accounting/components/DimensionPickers.tsx` — reusable component rendering inline Select elements for fund, project, budgetItem, encumbrance, paymentOrder using existing hooks (`useFundsList`, `useProjects`, `useBudgetItemsTree`) and NSwag clients for encumbrance/paymentOrder
- [x] T008 [P] Create `src/Web/ClientApp/src/features/accounting/hooks/useJournalEntries.ts` — hooks: `useJournalEntriesList(filters)`, `useJournalEntry(id)`, `useCreateJournalEntry()`, `useUpdateJournalEntry()`, `useSubmitJournalEntry()`, `useApproveJournalEntry()`, `usePostJournalEntry()`, `useReverseJournalEntry()`, `useCancelJournalEntry()` — all using `JournalEntriesClient` from `web-api-client.ts`
- [x] T009 [P] Create `src/Web/ClientApp/src/features/accounting/hooks/useJournalEntryLines.ts` — hooks: `useCreateJournalEntryLine()`, `useUpdateJournalEntryLine()`, `useRemoveJournalEntryLine()` — using `JournalEntriesClient` from `web-api-client.ts`
- [x] T010 [P] Create `src/Web/ClientApp/src/features/accounting/components/BalanceIndicator.tsx` — update existing component to work with JournalEntryLineDto; show running debit/credit totals and balance status

**Checkpoint**: Foundation ready — user story implementation can begin

---

## Phase 3: User Story 1 — Browse Journal Entries (Priority: P1) ⭐ MVP

**Goal**: Accountant sees journal entries list with status badges and can filter by period, fund, status

**Independent Test**: Load list page, verify status badges render, apply each filter individually and in combination, verify AND logic

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T011 [P] [US1] Create test file `src/Web/ClientApp/src/features/accounting/__tests__/JournalEntriesListPage.test.tsx` — test: list renders entries with status badges, filter by status returns matching entries, filter by fund returns matching entries, empty state shows message, zero Move/MoveLine references in DOM
- [x] T012 [P] [US1] Create test file `src/Web/ClientApp/src/features/accounting/__tests__/StatusBadge.test.tsx` — test: each EntryStatus value renders correct color/label

### Implementation for User Story 1

- [x] T013 [P] [US1] Create `src/Web/ClientApp/src/features/accounting/components/JournalEntriesGrid.tsx` — DataGrid with columns: entryNumber, documentDate, statusBadge (using StatusBadge component), totalDebit, totalCredit, fund summary; sorted newest first by default
- [x] T014 [US1] Create `src/Web/ClientApp/src/features/accounting/pages/JournalEntriesListPage.tsx` — page with FilterBar (period, fund, status dropdowns with AND logic), JournalEntriesGrid, "New Entry" button linking to create page, empty state when no results
- [x] T015 [US1] Update `src/Web/ClientApp/src/app/routes.tsx` — swap MovesListPage import to JournalEntriesListPage for `/accounting/journal-entries` route
- [x] T016 [US1] Update `src/Web/ClientApp/src/features/accounting/index.ts` — export JournalEntriesListPage

**Checkpoint**: List page fully functional with badges and filters

---

## Phase 4: User Story 2 — Create Journal Entry (Priority: P1) ⭐ MVP

**Goal**: Accountant creates entry via dedicated page with line editor, dimension pickers, and balance guard

**Independent Test**: Create entry with balanced lines → submit succeeds; create entry with unbalanced lines → submit blocked with error

### Tests for User Story 2

- [x] T017 [P] [US2] Create test file `src/Web/ClientApp/src/features/accounting/__tests__/JournalEntryCreatePage.test.tsx` — test: form renders header fields and line editor, adding balanced lines enables submit, adding unbalanced lines blocks submit with imbalance error, dimension pickers load reference data, submit creates entry and redirects
- [x] T018 [P] [US2] Create test file `src/Web/ClientApp/src/features/accounting/__tests__/JournalEntryLinesEditor.test.tsx` — test: add line shows all fields (account, debit/credit XOR, amount, description, dimensions), debit XOR credit validation, remove line updates totals
- [x] T019 [P] [US2] Create test file `src/Web/ClientApp/src/features/accounting/__tests__/DimensionPickers.test.tsx` — test: each picker loads data from API, selecting value updates parent state, optional fields allow null

### Implementation for User Story 2

- [x] T020 [P] [US2] Create `src/Web/ClientApp/src/features/accounting/components/JournalEntryHeaderForm.tsx` — react-hook-form + zod: documentDate, journalId, periodId, fiscalYearId, baseCurrencyId, narration, ref, entryType; integrates FiscalYearIndicator for auto-resolution
- [x] T021 [US2] Create `src/Web/ClientApp/src/features/accounting/components/JournalEntryLinesEditor.tsx` — inline line editor: add/remove lines, each line has account picker, debit/credit fields (XOR enforced), amount, description, DimensionPickers; computes running totals and shows BalanceIndicator
- [x] T022 [US2] Create `src/Web/ClientApp/src/features/accounting/pages/JournalEntryCreatePage.tsx` — dedicated page composing JournalEntryHeaderForm + JournalEntryLinesEditor; submit calls create entry then create lines; redirects to detail on success
- [x] T023 [US2] Update `src/Web/ClientApp/src/app/routes.tsx` — swap MoveCreatePage import to JournalEntryCreatePage for `/accounting/journal-entries/create` route
- [x] T024 [US2] Update `src/Web/ClientApp/src/features/accounting/index.ts` — export JournalEntryCreatePage

**Checkpoint**: Create page fully functional with line editor, dimensions, and balance guard

---

## Phase 5: User Story 3 — Lifecycle Actions with Approval and Status Panels (Priority: P1) ⭐ MVP

**Goal**: Accountant views detail with lines, performs lifecycle actions, sees status/approval history panels

**Independent Test**: Open detail → perform submit/approve/post/cancel/reverse → verify status transitions and panels record each decision

### Tests for User Story 3

- [x] T025 [P] [US3] Create test file `src/Web/ClientApp/src/features/accounting/__tests__/JournalEntryDetailPage.test.tsx` — test: header shows entry number/date/status/totals, lines table renders with dimensions, lifecycle buttons appear based on status (Draft→Submit, Submitted→Approve, etc.), status history panel shows transitions, approval history panel shows decisions, Posted entries show no edit/delete
- [x] T026 [P] [US3] Create test file `src/Web/ClientApp/src/features/accounting/__tests__/JournalEntryDetail.test.tsx` — test: component renders header card, workflow action bar with correct buttons per status, lines table, status history, approval history

### Implementation for User Story 3

- [x] T027 [US3] Create `src/Web/ClientApp/src/features/accounting/components/JournalEntryDetail.tsx` — full detail view: header card (entryNumber, date, status badge, totals), lines table with dimensions, import and use LifecycleActions from budgeting, ApprovalsPanel and StatusLogPanel from documents, reversal link when applicable
- [x] T028 [US3] Create `src/Web/ClientApp/src/features/accounting/pages/JournalEntryDetailPage.tsx` — page loading journal entry by ID, composing JournalEntryDetail, handling loading/error states, passing documentType="JournalEntry" to shared panels
- [x] T029 [US3] Update `src/Web/ClientApp/src/app/routes.tsx` — swap MoveDetailPage import to JournalEntryDetailPage for `/accounting/journal-entries/:id` route
- [x] T030 [US3] Update `src/Web/ClientApp/src/features/accounting/index.ts` — export JournalEntryDetailPage

**Checkpoint**: Detail page fully functional with lifecycle actions and history panels

---

## Phase 6: User Story 4 — Reversed Entry Links (Priority: P2)

**Goal**: Bidirectional links between reversed entries and their reversal entries

**Independent Test**: Reverse an entry → verify original shows link to reversal, reversal shows link to original, clicking link navigates correctly

### Tests for User Story 4

- [x] T031 [P] [US4] Add test cases to `src/Web/ClientApp/src/features/accounting/__tests__/JournalEntryDetailPage.test.tsx` — test: reversed entry shows link to reversal, reversal entry shows link to original, clicking link navigates to linked entry

### Implementation for User Story 4

- [x] T032 [US4] Update `src/Web/ClientApp/src/features/accounting/components/JournalEntryDetail.tsx` — add reversal link section: if reversalOfId is set, show "Reversal of: JE-XXXXX" link; if entry has linked reversal, show "Reversed by: JE-XXXXX" link; links navigate to `/accounting/journal-entries/{linkedId}`

**Checkpoint**: Reversed entry links functional

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Cleanup, validation, and quality assurance

- [x] T033 [P] Audit entire `src/Web/ClientApp/src/features/accounting/` directory for any remaining Move/MoveLine references — replace or remove
- [x] T034 [P] Audit `src/Web/ClientApp/src/app/routes.tsx` for any remaining Move component imports — replace
- [x] T035 Run `npm run lint` in `src/Web/ClientApp/` — fix any lint errors
- [x] T036 Run `npm run test` in `src/Web/ClientApp/` — ensure all tests pass
- [x] T037 Run `npm run build` in `src/Web/ClientApp/` — ensure production build succeeds
- [x] T038 Run quickstart.md validation scenarios V1-V10 — verify all pass
- [x] T039 Verify RTL rendering: switch to Arabic locale, confirm all text right-to-left and layout mirrored
- [x] T040 Verify dark mode: switch theme, confirm all components render correctly with visible status badges

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — can start immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 completion — BLOCKS all user stories
- **Phase 3 (US1)**: Depends on Phase 2 — independent of US2/US3/US4
- **Phase 4 (US2)**: Depends on Phase 2 — independent of US1/US3/US4
- **Phase 5 (US3)**: Depends on Phase 2 — independent of US1/US2/US4
- **Phase 6 (US4)**: Depends on Phase 5 (needs detail page to exist)
- **Phase 7 (Polish)**: Depends on all desired user stories being complete

### User Story Dependencies

- **US1 (P1)**: Can start after Phase 2 — no dependencies on other stories
- **US2 (P1)**: Can start after Phase 2 — no dependencies on other stories
- **US3 (P1)**: Can start after Phase 2 — no dependencies on other stories
- **US4 (P2)**: Depends on US3 (detail page must exist for reversal links)

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- Components before pages
- Pages before route updates
- Core implementation before integration

### Parallel Opportunities

- T006, T007, T008, T009, T010 (all Foundation tasks) — different files, no dependencies
- T011, T012 (US1 tests) — parallel
- T013 (US1 grid) can run while tests are failing
- T017, T018, T019 (US2 tests) — parallel
- T020 (US2 header form) can run while tests are failing
- T025, T026 (US3 tests) — parallel
- US1, US2, US3 can all start in parallel after Phase 2 completes

---

## Parallel Example: User Story 1

```bash
# Launch all tests for US1 together:
Task: "T011 [US1] JournalEntriesListPage.test.tsx"
Task: "T012 [US1] StatusBadge.test.tsx"

# Launch implementation (grid component + page):
Task: "T013 [US1] JournalEntriesGrid.tsx"
Task: "T014 [US1] JournalEntriesListPage.tsx"
```

---

## Implementation Strategy

### MVP First (US1 + US2 + US3)

1. Complete Phase 1: Setup (delete old files, update types)
2. Complete Phase 2: Foundational (StatusBadge, DimensionPickers, hooks)
3. Complete Phase 3: US1 (list page with badges and filters)
4. Complete Phase 4: US2 (create page with line editor and balance guard)
5. Complete Phase 5: US3 (detail page with lifecycle and history panels)
6. **STOP and VALIDATE**: Run quickstart scenarios V1-V7
7. Deploy/demo — all P1 stories complete

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add US1 → List page works → Test independently → Deploy/Demo
3. Add US2 → Create page works → Test independently → Deploy/Demo
4. Add US3 → Detail page works → Test independently → Deploy/Demo
5. Add US4 → Reversal links work → Test independently → Deploy/Demo
6. Polish → All quality checks pass → Final deploy

### Parallel Team Strategy

With multiple developers:
1. Team completes Setup + Foundational together
2. Once Phase 2 is done:
   - Developer A: US1 (list page)
   - Developer B: US2 (create page)
   - Developer C: US3 (detail page)
3. US4 picks up after US3 completes
4. Polish is shared

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- TDD is mandatory per constitution principle XI — tests first, always
