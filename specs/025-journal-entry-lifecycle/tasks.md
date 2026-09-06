# Tasks: Journal Entry Lifecycle Screens

**Input**: Design documents from `/specs/025-journal-entry-lifecycle/`
**Branch**: `025-journal-entry-lifecycle`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/journal-entry-api.md

**Tests**: TDD mandatory (Constitution XI). Tests written FIRST, observe RED, implement, observe GREEN.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to
- Include exact file paths in descriptions

---

## Phase 1: Setup

**Purpose**: Verify existing infrastructure and extend where needed

- [X] T001 Verify types completeness in `src/Web/ClientApp/src/features/accounting/types.ts` — confirm JournalEntryDto, JournalEntryLineDto, all command interfaces, EntryStatus enum match data-model.md
- [X] T002 [P] Verify route configuration in `src/Web/ClientApp/src/app/routes.tsx` — confirm `/accounting/journal-entries`, `/accounting/journal-entries/create`, `/accounting/journal-entries/:id` routes exist and point to correct page components
- [X] T003 [P] Verify permissions registered in `src/Web/ClientApp/src/shared/constants/permissions.ts` (lines 56-63) and `src/Web/ClientApp/src/shared/hooks/usePermission.ts` (lines 49-51, 94) — confirm all 8 Accounting.JournalEntries codes present

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Shared infrastructure that ALL user stories depend on

**CRITICAL**: No user story work can begin until this phase is complete

- [X] T004 Create shared client wrapper in `src/Web/ClientApp/src/features/accounting/shared/client.ts` — re-export JournalEntriesClient singleton with typed filter interface for list queries
- [X] T005 Rebuild `useJournalEntries` hook in `src/Web/ClientApp/src/features/accounting/hooks/useJournalEntries.ts` — add refetch capability for 409 conflict handling, ensure all mutations invalidate both `journalEntries` and `journalEntry` query keys, add error type for ProblemDetails
- [X] T006 Rebuild `useJournalEntryLines` hook in `src/Web/ClientApp/src/features/accounting/hooks/useJournalEntryLines.ts` — verify create/update/remove mutations use correct NSwag methods (linesPOST3, linesPUT, linesDELETE), add optimistic update option

**Checkpoint**: Foundation ready — user story implementation can begin

---

## Phase 3: User Story 1 — Create and Edit a Draft (Priority: P1) — MVP

**Goal**: Accountant can create draft journal entries with balanced lines, live totals, and system-generated read-only lock

**Independent Test**: Create draft entry, add/edit/remove lines, verify live balance totals, confirm draft persists, verify system-generated entries are read-only

### Tests for User Story 1

- [X] T007 [P] [US1] Write tests for `JournalEntryHeaderForm` in `src/Web/ClientApp/src/features/accounting/__tests__/JournalEntryHeaderForm.test.tsx` — test form renders all fields (documentDate, journal, entryType, narration, ref), test FiscalYearIndicator integration, test validation (required fields)
- [X] T008 [P] [US1] Write tests for `JournalEntryLinesEditor` in `src/Web/ClientApp/src/features/accounting/__tests__/JournalEntryLinesEditor.test.tsx` — test XOR validation (both zero = invalid, both non-zero = invalid, exactly one = valid), test line add/edit/remove, test balance computation, test DimensionPickers integration
- [X] T009 [P] [US1] Write tests for `JournalEntryCreatePage` in `src/Web/ClientApp/src/features/accounting/__tests__/JournalEntryCreatePage.test.tsx` — test two-step flow (header then lines), test redirect after create, test balance footer display, test save-draft button disabled when unbalanced

### Implementation for User Story 1

- [X] T010 [P] [US1] Rebuild `BalanceIndicator` in `src/Web/ClientApp/src/features/accounting/components/BalanceIndicator.tsx` — ensure default props = 0, green/red/unbalanced states, diff display, Arabic labels
- [X] T011 [P] [US1] Rebuild `FiscalYearIndicator` in `src/Web/ClientApp/src/features/accounting/components/FiscalYearIndicator.tsx` — show resolved fiscal year + period for a document date, loading/error states
- [X] T012 [P] [US1] Keep `DimensionPickers` in `src/Web/ClientApp/src/features/accounting/components/DimensionPickers.tsx` — verify cost center, fund, project, budget item, encumbrance, payment order pickers load from correct clients
- [X] T013 [P] [US1] Keep `StatusBadge` in `src/Web/ClientApp/src/features/accounting/components/StatusBadge.tsx` — verify all 6 status states render with correct colors/labels
- [X] T014 [US1] Rebuild `JournalEntryHeaderForm` in `src/Web/ClientApp/src/features/accounting/components/JournalEntryHeaderForm.tsx` — react-hook-form + zod validation, fields: documentDate (triggers FiscalYearIndicator), journal select, entryType select, narration textarea, ref input, baseCurrency display
- [X] T015 [US1] Rebuild `JournalEntryLinesEditor` in `src/Web/ClientApp/src/features/accounting/components/JournalEntryLinesEditor.tsx` — line table with inline edit, XOR validation per line, add/remove buttons, DimensionPickers per line, running balance totals, disabled when not Draft or isSystemGenerated
- [X] T016 [US1] Rebuild `JournalEntryCreatePage` in `src/Web/ClientApp/src/features/accounting/pages/JournalEntryCreatePage.tsx` — two-step flow: header form → line editor, redirect to detail after create, save-draft button with balance check, cancel returns to list

**Checkpoint**: Draft creation and line editing fully functional. System-generated entries display read-only.

---

## Phase 4: User Story 2 — Submit for Approval (Priority: P1)

**Goal**: Accountant can submit balanced drafts; unbalanced/empty drafts are blocked

**Independent Test**: Submit balanced draft → transitions to Submitted. Submit unbalanced draft → blocked with reason. Submit draft with zero lines → blocked with "must have at least one line"

### Tests for User Story 2

- [X] T017 [P] [US2] Write tests for `EntryLifecycleActions` (submit state) in `src/Web/ClientApp/src/features/accounting/__tests__/EntryLifecycleActions.test.tsx` — test Submit button visible for Draft with ≥1 balanced line, test Submit hidden for Submitted/Approved/Posted/Reversed/Cancelled, test Submit disabled when unbalanced or zero lines, test Cancel button visible for Draft

### Implementation for User Story 2

- [X] T018 [US2] Create `EntryLifecycleActions` component in `src/Web/ClientApp/src/features/accounting/components/EntryLifecycleActions.tsx` — POST-style action bar (NOT PATCH budgeting LifecycleActions), render Submit/Approve/Post/Reverse/Cancel buttons based on state machine, permission-gated via usePermission, loading/pending states, confirmation dialog before action

**Checkpoint**: Submit transitions work. Unbalanced/empty submissions blocked client-side.

---

## Phase 5: User Story 3 — Approve (Priority: P1)

**Goal**: Approver can approve Submitted entries; entry becomes eligible for posting

**Independent Test**: Approve Submitted entry → transitions to Approved. Approve button only visible for Submitted status.

### Tests for User Story 3

- [X] T019 [P] [US3] Extend `EntryLifecycleActions` tests in `src/Web/ClientApp/src/features/accounting/__tests__/EntryLifecycleActions.test.tsx` — test Approve button visible for Submitted only, test Approve hidden for Draft/Approved/Posted/Reversed/Cancelled, test loading state during approval

### Implementation for User Story 3

- [X] T020 [US3] Extend `EntryLifecycleActions` in `src/Web/ClientApp/src/features/accounting/components/EntryLifecycleActions.tsx` — add Approve button for Submitted state, confirmation dialog, loading state, permission check for Accounting.JournalEntries.Approve

**Checkpoint**: Approve transitions work. Full forward lifecycle (Draft→Submitted→Approved) complete.

---

## Phase 6: User Story 4 — Post with Fiscal-Period Guards (Priority: P1)

**Goal**: Accountant posts Approved entries; fiscal period lock/range guards enforced

**Independent Test**: Post Approved entry → transitions to Posted with postingDate/postedBy. Post with locked period → rejected. Post with date outside period → rejected.

### Tests for User Story 4

- [X] T021 [P] [US4] Extend `EntryLifecycleActions` tests in `src/Web/ClientApp/src/features/accounting/__tests__/EntryLifecycleActions.test.tsx` — test Post button visible for Approved only, test Post hidden for Draft/Submitted/Posted/Reversed/Cancelled, test error toast for fiscal period lock, test error toast for date out of range

### Implementation for User Story 4

- [X] T022 [US4] Extend `EntryLifecycleActions` in `src/Web/ClientApp/src/features/accounting/components/EntryLifecycleActions.tsx` — add Post button for Approved state, handle 400 errors (fiscal period locked, date outside range) with toast messages, display postingDate/postedBy on Posted entries

**Checkpoint**: Post transitions work with fiscal period guards. Full forward lifecycle (Draft→Submitted→Approved→Posted) complete.

---

## Phase 7: User Story 5 — Reverse a Posted Entry (Priority: P1)

**Goal**: Accountant reverses Posted entries with mandatory reason; counter-entry created; bidirectional linkage displayed

**Independent Test**: Reverse Posted entry → counter-entry Posted, original Reversed. Double reversal rejected. Locked period rejected. Reversal linkage shows both directions.

### Tests for User Story 5

- [X] T023 [P] [US5] Write tests for `ReverseDialog` in `src/Web/ClientApp/src/features/accounting/__tests__/ReverseDialog.test.tsx` — test modal renders with reason textarea (required), test counter-line preview shows mirrored amounts, test confirm button disabled when reason empty, test error display, test cancel closes dialog
- [X] T024 [P] [US5] Extend `EntryLifecycleActions` tests in `src/Web/ClientApp/src/features/accounting/__tests__/EntryLifecycleActions.test.tsx` — test Reverse button visible for Posted only, test Reverse hidden for Draft/Submitted/Approved/Reversed/Cancelled, test opens ReverseDialog on click

### Implementation for User Story 5

- [X] T025 [US5] Rebuild `ReverseDialog` in `src/Web/ClientApp/src/features/accounting/components/ReverseDialog.tsx` — proper modal (focus trap, Escape to close, backdrop click), mandatory reason textarea (max 500 chars), counter-line preview (mirrored debit↔credit), confirm/cancel buttons, loading state, error display
- [X] T026 [US5] Extend `EntryLifecycleActions` in `src/Web/ClientApp/src/features/accounting/components/EntryLifecycleActions.tsx` — add Reverse button for Posted state, open ReverseDialog on click, handle 400 errors (locked period, year not open, reversals cannot be reversed)

**Checkpoint**: Reverse transitions work with counter-entry creation. Bidirectional linkage displayed.

---

## Phase 8: User Story 6 — Cancel Before Commitment (Priority: P2)

**Goal**: Accountant cancels Draft/Submitted entries with confirmation; Cancel not offered for Approved/Posted

**Independent Test**: Cancel Draft → Cancelled. Cancel Submitted → Cancelled. Cancel not visible for Approved/Posted. CancelledBy/CancelledAt displayed.

### Tests for User Story 6

- [X] T027 [P] [US6] Extend `EntryLifecycleActions` tests in `src/Web/ClientApp/src/features/accounting/__tests__/EntryLifecycleActions.test.tsx` — test Cancel button visible for Draft and Submitted, test Cancel hidden for Approved/Posted/Reversed/Cancelled, test confirmation dialog before cancel

### Implementation for User Story 6

- [X] T028 [US6] Extend `EntryLifecycleActions` in `src/Web/ClientApp/src/features/accounting/components/EntryLifecycleActions.tsx` — add Cancel button for Draft and Submitted states, confirmation dialog, display cancelledBy/cancelledAt on Cancelled entries

**Checkpoint**: Cancel transitions work. Draft→Cancelled and Submitted→Cancelled complete.

---

## Phase 9: User Story 7 — Browse and Audit (Priority: P2)

**Goal**: Accountant browses entry list with status filters, search, and audit panels

**Independent Test**: Navigate list with status chips, search by entry number, open detail with approvals panel + status log, system badge on system-generated entries, reversal linkage displayed.

### Tests for User Story 7

- [X] T029 [P] [US7] Write tests for `JournalEntriesListPage` in `src/Web/ClientApp/src/features/accounting/__tests__/JournalEntriesListPage.test.tsx` — test 6 status filter chips render, test clicking chip filters list, test search by entry number, test system badge on system-generated entries, test "new entry" button navigates to create
- [X] T030 [P] [US7] Write tests for `JournalEntryDetailPage` in `src/Web/ClientApp/src/features/accounting/__tests__/JournalEntryDetailPage.test.tsx` — test detail renders header/lines/balance, test ApprovalsPanel renders for Submitted/Approved, test StatusLogPanel always renders, test reversal linkage displays, test system badge + read-only for system entries, test conflict toast on 409

### Implementation for User Story 7

- [X] T031 [US7] Rebuild `JournalEntriesGrid` in `src/Web/ClientApp/src/features/accounting/components/JournalEntriesGrid.tsx` — modern table with columns: entryNumber, documentDate, status (StatusBadge), journal, totalDebit, totalCredit, system badge, click to navigate to detail
- [X] T032 [US7] Rebuild `JournalEntryDetail` in `src/Web/ClientApp/src/features/accounting/components/JournalEntryDetail.tsx` — replace budgeting `LifecycleActions` import with new `EntryLifecycleActions`, add reversal linkage display (bidirectional), add conflict toast handler, keep ApprovalsPanel + StatusLogPanel
- [X] T033 [US7] Rebuild `JournalEntriesListPage` in `src/Web/ClientApp/src/features/accounting/pages/JournalEntriesListPage.tsx` — replace select dropdowns with clickable status filter chips (6 states), add search input for entry number, pass filters to useJournalEntriesList, keep "new entry" button
- [X] T034 [US7] Rebuild `JournalEntryDetailPage` in `src/Web/ClientApp/src/features/accounting/pages/JournalEntryDetailPage.tsx` — replace prompt() calls with proper ReverseDialog, add conflict toast (409) with refetch offer, integrate EntryLifecycleActions, keep ApprovalsPanel + StatusLogPanel

**Checkpoint**: All 7 user stories complete. List page with filters, detail with full lifecycle, create with balance.

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Final verification and cleanup

- [X] T035 Run frontend lint: `cd src/Web/ClientApp && npm run lint` — fix any lint errors
- [X] T036 Run frontend tests: `cd src/Web/ClientApp && npm run test` — all tests pass (RED→GREEN verified)
- [X] T037 Run frontend build: `cd src/Web/ClientApp && npm run build` — build succeeds with no errors
- [X] T038 Verify quickstart.md scenarios V1-V7 manually against running dev server

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — can start immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 — BLOCKS all user stories
- **Phases 3-9 (User Stories)**: All depend on Phase 2 completion
  - US1 (Phase 3) is MVP — must complete first
  - US2-5 (Phases 4-7) depend on US1 (need draft entry to transition)
  - US6 (Phase 8) depends on US1
  - US7 (Phase 9) can run parallel to US2-6 (read-only browsing)
- **Phase 10 (Polish)**: Depends on all user stories complete

### User Story Dependencies

- **US1 (P1)**: After Foundational — creates the draft entry
- **US2 (P1)**: After US1 — needs draft to submit
- **US3 (P1)**: After US2 — needs submitted entry to approve
- **US4 (P1)**: After US3 — needs approved entry to post
- **US5 (P1)**: After US4 — needs posted entry to reverse
- **US6 (P2)**: After US1 — needs draft/submitted to cancel
- **US7 (P2)**: After Foundational — independent read-only, can parallelize with US2-6

### Within Each User Story

- Tests (TDD) MUST be written and FAIL before implementation
- Components before pages (components compose into pages)
- Hooks already exist — verify and extend as needed
- Story complete before moving to next priority

### Parallel Opportunities

- T002 + T003 (route + permission verification) — parallel
- T007 + T008 + T009 (US1 tests) — parallel (different files)
- T010 + T011 + T012 + T013 (US1 shared components) — parallel
- T017 + T018 (US2 test + implementation) — sequential per TDD
- T019 + T020 (US3), T021 + T022 (US4), T023 + T024 + T025 + T026 (US5), T027 + T028 (US6) — sequential per TDD
- T029 + T030 (US7 tests) — parallel
- T031 + T032 (US7 components) — parallel

---

## Parallel Example: User Story 1

```bash
# Launch all US1 tests together (TDD — should FAIL):
Task: "Write tests for JournalEntryHeaderForm in __tests__/JournalEntryHeaderForm.test.tsx"
Task: "Write tests for JournalEntryLinesEditor in __tests__/JournalEntryLinesEditor.test.tsx"
Task: "Write tests for JournalEntryCreatePage in __tests__/JournalEntryCreatePage.test.tsx"

# Launch all US1 shared components together (different files):
Task: "Rebuild BalanceIndicator in components/BalanceIndicator.tsx"
Task: "Rebuild FiscalYearIndicator in components/FiscalYearIndicator.tsx"
Task: "Verify DimensionPickers in components/DimensionPickers.tsx"
Task: "Verify StatusBadge in components/StatusBadge.tsx"

# Then implement (sequential — header form → lines editor → create page):
Task: "Rebuild JournalEntryHeaderForm in components/JournalEntryHeaderForm.tsx"
Task: "Rebuild JournalEntryLinesEditor in components/JournalEntryLinesEditor.tsx"
Task: "Rebuild JournalEntryCreatePage in pages/JournalEntryCreatePage.tsx"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T003)
2. Complete Phase 2: Foundational (T004-T006)
3. Complete Phase 3: User Story 1 (T007-T016)
4. **STOP and VALIDATE**: Test draft creation, line editing, balance totals independently
5. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. US1 → Create & Edit Draft (MVP!) → Test independently → Deploy/Demo
3. US2-5 → Lifecycle Transitions → Test forward lifecycle end-to-end
4. US6 → Cancel → Test early-stage cancellation
5. US7 → Browse & Audit → Test list filtering and audit panels
6. Polish → Lint, test, build, quickstart validation

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Tests MUST fail before implementing (TDD — Constitution XI)
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Hooks (`useJournalEntries.ts`, `useJournalEntryLines.ts`) already exist and work — verify and extend, do not rewrite from scratch
- Types (`types.ts`) already complete — verify match with data-model.md
- Permissions already registered in `permissions.ts` and `usePermission.ts` — verify only
