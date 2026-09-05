# Tasks: BF-001 Journal Entries Frontend

**Input**: Design documents from `/specs/003-bf001-journal-entries-frontend/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Tests are NOT explicitly requested in the spec. Test tasks omitted per task generation rules.

**Organization**: Tasks grouped by user story. All 4 stories are P1 — executed sequentially for clarity.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3, US4)
- Exact file paths included

## Phase 1: Setup

**Purpose**: Verify existing structure, no new directories needed

- [x] T001 Verify `src/Web/ClientApp/src/features/accounting/` structure exists with pages/, components/, hooks/ subdirectories
- [x] T002 [P] Verify shared UI components available: DataGrid, StatusBadge, MoneyDisplay, Button, Input, Select, Combobox, DatePicker, Dialog, ConfirmDialog, Loading, ErrorState, EmptyState, PageHeader, Pagination
- [x] T003 [P] Verify React Query configured in `src/Web/ClientApp/src/main.tsx` with QueryClient
- [x] T004 [P] Verify routes defined in `src/Web/ClientApp/src/app/routes.tsx` for `/accounting/journal-entries`, `/accounting/journal-entries/create`, `/accounting/journal-entries/:id`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: New hooks and components that ALL user stories depend on

**CRITICAL**: No user story work can begin until this phase is complete

- [x] T005 [P] Create `useFiscalYearByDate` hook in `src/Web/ClientApp/src/features/accounting/hooks/useFiscalYearByDate.ts` — React Query hook calling `GET /api/FiscalYears/by-date?date=`, returns `FiscalYearPeriodResult`, enabled only when date is non-empty, 60s stale time
- [x] T006 [P] Create `useExchangeRateLookup` hook in `src/Web/ClientApp/src/features/accounting/hooks/useExchangeRateLookup.ts` — React Query hook calling `GET /api/ExchangeRates/lookup`, returns `ExchangeRateLookupDto | null`, enabled only when baseCurrencyId, currencyId, date are non-empty and currencyId !== baseCurrencyId, 60s stale time
- [x] T007 [P] Create `BalanceIndicator` component in `src/Web/ClientApp/src/features/accounting/components/BalanceIndicator.tsx` — accepts `lines: { debit: number; credit: number }[]`, displays totalDebit, totalCredit, difference (red when unbalanced), uses MoneyDisplay for formatting, Arabic labels
- [x] T008 Modify `useMoves` hook in `src/Web/ClientApp/src/features/accounting/hooks/useMoves.ts` — add pagination state support to `useMovesList` (accept `page`, `pageSize` params, return `totalItems`)

**Checkpoint**: Foundation ready — new hooks and BalanceIndicator available for all stories

---

## Phase 3: User Story 1 — List and Filter Journal Entries (Priority: P1) — MVP

**Goal**: Accountant can view all journal entries with filtering by status, date range, and journal

**Independent Test**: Navigate to `/accounting/journal-entries`. Verify entries load in DataGrid. Filter by status. Filter by date range. Filter by journal. Click entry number to navigate to detail.

### Implementation for User Story 1

- [x] T009 [P] [US1] Create `MovesFilters` component in `src/Web/ClientApp/src/features/accounting/components/MovesFilters.tsx` — status dropdown (6 options with Arabic labels), date range (from/to DatePicker), journal dropdown (from useJournalsList), clear filters button, reads/writes URL search params via `useSearchParams`
- [x] T010 [US1] Modify `MovesGrid` component in `src/Web/ClientApp/src/features/accounting/components/MovesGrid.tsx` — replace `<table>` with `<DataGrid>` component, define columns (Entry Number as link, Document Date, Journal Name, System-generated badge, Status badge), add client-side pagination, row click navigates to detail
- [x] T011 [US1] Modify `MovesListPage` in `src/Web/ClientApp/src/features/accounting/pages/MovesListPage.tsx` — integrate MovesFilters, integrate upgraded MovesGrid with DataGrid pagination, read filters from URL search params, pass filters to useMovesList, add "قيد جديد" button navigating to create page

**Checkpoint**: User Story 1 fully functional — list with filters, pagination, navigation

---

## Phase 4: User Story 2 — Create Journal Entry with Lines (Priority: P1)

**Goal**: Accountant can create a new journal entry with header form (RHF) and lines editor (manual state)

**Independent Test**: Navigate to create page. Fill header. Add 2+ lines. Verify balance indicator. Verify FY auto-detection. Submit. Verify redirect to detail.

### Implementation for User Story 2

- [x] T012 [P] [US2] Create `FiscalYearIndicator` component in `src/Web/ClientApp/src/features/accounting/components/FiscalYearIndicator.tsx` — accepts `date: string`, uses `useFiscalYearByDate` hook, displays FY name and period name when found, displays error message when not found, loading state
- [x] T013 [US2] Refactor `MoveCreatePage` header form in `src/Web/ClientApp/src/features/accounting/pages/MoveCreatePage.tsx` — replace manual `useState` header with React Hook Form + Zod schema (documentDate required, journalId optional, entryType optional, ref max 100, narration max 1000), integrate FiscalYearIndicator below date field, disable submit when no FY found
- [x] T014 [US2] Refactor `MoveCreatePage` lines editor in `src/Web/ClientApp/src/features/accounting/pages/MoveCreatePage.tsx` — keep manual `useState` for lines, integrate `useExchangeRateLookup` hook for auto-fetching rates, integrate `BalanceIndicator` component below lines table, disable submit when lines unbalanced or empty
- [x] T015 [US2] Add Zod validation schema for move header in `src/Web/ClientApp/src/features/accounting/pages/MoveCreatePage.tsx` — documentDate required, narration max 1000, ref max 100, Arabic error messages

**Checkpoint**: User Story 2 fully functional — create with RHF header, manual lines, FY detection, exchange rate, balance indicator

---

## Phase 5: User Story 3 — View Journal Entry Detail (Priority: P1)

**Goal**: Accountant can view full entry details with header info, status badge, lines table, audit info

**Independent Test**: Navigate to detail page. Verify header card, status badge, lines table, audit info. Verify read-only for non-Draft. Verify inline edit for Draft.

### Implementation for User Story 3

- [x] T016 [US3] Modify `MoveDetail` component in `src/Web/ClientApp/src/features/accounting/components/MoveDetail.tsx` — improve header card layout (entry number, status badge, dates, journal, reference, period/FY, narration), add audit info section for Posted/Cancelled entries, add reversal reference display
- [x] T017 [US3] Modify `MoveLinesEditor` in `src/Web/ClientApp/src/features/accounting/components/MoveLinesEditor.tsx` — integrate `BalanceIndicator` component, ensure read-only mode for non-Draft entries, verify inline edit flow for Draft entries
- [x] T018 [US3] Modify `MoveDetailPage` in `src/Web/ClientApp/src/features/accounting/pages/MoveDetailPage.tsx` — ensure proper loading/error states, pass all required props to MoveDetail

**Checkpoint**: User Story 3 fully functional — detail view with header, lines, audit info

---

## Phase 6: User Story 4 — Workflow Transitions (Priority: P1)

**Goal**: Accountant/Approver can transition entries through lifecycle with confirmation dialogs

**Independent Test**: Create Draft. Submit. Approve. Post (with confirm). Cancel (with confirm). Reverse (with dialog). Verify all transitions and toasts.

### Implementation for User Story 4

- [x] T019 [US4] Add Cancel confirmation dialog in `src/Web/ClientApp/src/features/accounting/components/MoveDetail.tsx` — use `ConfirmDialog` component, show before calling `cancel.mutateAsync`, Arabic message "هل أنت متأكد من إلغاء القيد؟", confirm label "إلغاء", destructive style
- [x] T020 [US4] Add Post confirmation dialog in `src/Web/ClientApp/src/features/accounting/components/MoveDetail.tsx` — use `ConfirmDialog` component, show before calling `post.mutateAsync`, Arabic message "هل أنت متأكد من ترحيل القيد؟", confirm label "ترحيل"
- [x] T021 [US4] Verify ReverseDialog integration in `src/Web/ClientApp/src/features/accounting/components/ReverseDialog.tsx` — ensure reversal reason field (required, max 500), confirm creates reversal and navigates to new entry
- [x] T022 [US4] Add concurrency conflict handling in `src/Web/ClientApp/src/features/accounting/components/MoveDetail.tsx` — catch 409 errors on workflow actions, show warning toast "تم تعديل القيد من مستخدم آخر", refetch entry data
- [x] T023 [US4] Add Arabic toast messages for all workflow transitions — submit: "تم الإرسال للاعتماد", approve: "تم الاعتماد", post: "تم الترحيل", cancel: "تم الإلغاء", reverse: "تم عكس القيد"

**Checkpoint**: User Story 4 fully functional — all workflow transitions with confirmations and toasts

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Final validation and cleanup

- [x] T024 [P] Verify all pages render correctly in RTL — check `dir="rtl"` on root divs, verify logical properties (ms-/ps-/pe-), verify Arabic labels
- [x] T025 [P] Verify dark mode support — check all new/modified components use design tokens (no hardcoded colors), verify StatusBadge variants work in dark mode
- [x] T026 Run quickstart.md validation scenarios 1-10 — verify each scenario passes end-to-end
- [x] T027 Verify build succeeds — run `npm run build` in `src/Web/ClientApp/`, confirm zero errors
- [x] T028 Verify lint passes — run `npm run lint` in `src/Web/ClientApp/`, confirm zero errors

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **US1 (Phase 3)**: Depends on Foundational (Phase 2)
- **US2 (Phase 4)**: Depends on Foundational (Phase 2) — can run parallel with US1
- **US3 (Phase 5)**: Depends on Foundational (Phase 2) — can run parallel with US1/US2
- **US4 (Phase 6)**: Depends on US3 (needs MoveDetail component) — sequential after US3
- **Polish (Phase 7)**: Depends on all user stories being complete

### User Story Dependencies

- **US1 (List)**: Can start after Foundational — no dependencies on other stories
- **US2 (Create)**: Can start after Foundational — no dependencies on other stories
- **US3 (Detail)**: Can start after Foundational — no dependencies on other stories
- **US4 (Workflow)**: Depends on US3 — workflow actions are on detail page

### Parallel Opportunities

- T002, T003, T004 (Setup verification) — parallel
- T005, T006, T007 (New hooks + BalanceIndicator) — parallel
- T009 (MovesFilters) — can start immediately after T008
- T012 (FiscalYearIndicator) — can start immediately after T005
- US1 and US2 can run in parallel after Foundational
- T024, T025 (RTL/dark mode verification) — parallel

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (hooks + BalanceIndicator)
3. Complete Phase 3: User Story 1 (List with filters)
4. **STOP and VALIDATE**: List page works with DataGrid, filters, URL params
5. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add US1 (List) → Test independently → Deploy/Demo (MVP!)
3. Add US2 (Create) → Test independently → Deploy/Demo
4. Add US3 (Detail) → Test independently → Deploy/Demo
5. Add US4 (Workflow) → Test independently → Deploy/Demo
6. Polish → Final validation

### Parallel Team Strategy

With multiple developers:
1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: US1 (List)
   - Developer B: US2 (Create)
   - Developer C: US3 (Detail)
3. US4 depends on US3 — sequential
4. Polish together

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- All user stories are P1 — all are required for MVP
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
