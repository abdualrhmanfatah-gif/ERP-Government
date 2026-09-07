# Tasks: ACC-03 — Journals & Templates

**Input**: Design documents from `/specs/028-journals-templates/`

**Prerequisites**: plan.md, spec.md, data-model.md, contracts/, research.md

**Tests**: Test tasks included per spec test table (T-028-001 through T-028-012).

**Organization**: Tasks grouped by user story. Backend fully built — frontend-only.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story (US1, US2, US3)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Extend shared client, create directory structure

- [x] T001 [P] Extend `src/Web/ClientApp/src/features/accounting/shared/client.ts` — add JournalsClient and TemplatesClient instances
- [x] T002 [P] Create directory structure: `journals/pages/`, `journals/components/`, `templates/pages/`, `templates/components/` under `src/Web/ClientApp/src/features/accounting/`

---

## Phase 2: Foundational (Hooks)

**Purpose**: Shared hooks both US1 and US2 depend on

- [x] T003 Extend `src/Web/ClientApp/src/features/accounting/hooks/useJournalsList.ts` — add `type` filter parameter (JournalType)
- [x] T004 [P] Create `src/Web/ClientApp/src/features/accounting/hooks/useJournalById.ts` — GET /api/Journals/{id}
- [x] T005 [P] Create `src/Web/ClientApp/src/features/accounting/hooks/useCreateJournal.ts` — POST /api/Journals
- [x] T006 [P] Create `src/Web/ClientApp/src/features/accounting/hooks/useUpdateJournal.ts` — PUT /api/Journals/{id}
- [x] T007 [P] Create `src/Web/ClientApp/src/features/accounting/hooks/useTemplatesList.ts` — GET /api/Templates with filters
- [x] T008 [P] Create `src/Web/ClientApp/src/features/accounting/hooks/useTemplateById.ts` — GET /api/Templates/{id}
- [x] T009 [P] Create `src/Web/ClientApp/src/features/accounting/hooks/useCreateTemplate.ts` — POST /api/Templates
- [x] T010 [P] Create `src/Web/ClientApp/src/features/accounting/hooks/useUpdateTemplate.ts` — PUT /api/Templates/{id}

---

## Phase 3: User Story 1 — Manage Journals (Priority: P1) — MVP

**Goal**: List, create, and edit journals with type filter, duplicate code rejection, and used-journal field lock.

**Independent Test**: Create a journal of each type (7), verify list with filter, attempt duplicate code, attempt edit on used journal.

### Tests for User Story 1

- [ ] T011 [P] [US1] Test: Journal list renders columns and type filter — `src/Web/ClientApp/src/features/accounting/__tests__/JournalsListPage.test.tsx`
- [ ] T012 [P] [US1] Test: Journal create with all fields succeeds — `src/Web/ClientApp/src/features/accounting/__tests__/JournalCreatePage.test.tsx`
- [ ] T013 [P] [US1] Test: Duplicate code rejected with message — `src/Web/ClientApp/src/features/accounting/__tests__/JournalCreatePage.test.tsx`
- [ ] T014 [P] [US1] Test: Edit blocked for used journal with reason — `src/Web/ClientApp/src/features/accounting/__tests__/JournalEditPage.test.tsx`

### Implementation for User Story 1

- [x] T015 [P] [US1] Create `src/Web/ClientApp/src/features/accounting/journals/components/JournalGrid.tsx` — DataGrid with columns: code, name, type, sequence, approval, active. Row click → edit page.
- [x] T016 [P] [US1] Create `src/Web/ClientApp/src/features/accounting/journals/components/JournalForm.tsx` — Form for create/edit. Fields: code, name, type (enum select), accountId, suspenseAccountId, allowForeignCurrency (checkbox), sequenceId, requireApprovalBeforePosting (checkbox). Used-journal lock: code/name/type/accountId/suspenseAccountId/allowForeignCurrency/sequenceId read-only when journal has entries.
- [x] T017 [US1] Create `src/Web/ClientApp/src/features/accounting/journals/pages/JournalsListPage.tsx` — PageHeader (title="دفاتر اليومية", create button), FilterBar with type filter (7 options from JournalType enum) + active filter, JournalGrid. Empty state: "لا توجد دفاتر".
- [x] T018 [US1] Create `src/Web/ClientApp/src/features/accounting/journals/pages/JournalCreatePage.tsx` — JournalForm in create mode. On success → redirect to list. On 400 → display server error (duplicate code message).
- [x] T019 [US1] Create `src/Web/ClientApp/src/features/accounting/journals/pages/JournalEditPage.tsx` — Load journal by id (useJournalById). JournalForm in edit mode with RowVersion. Used-journal response → lock fields + display reason. On success → redirect to list. On 409 → conflict toast.

**Checkpoint**: Journals list/create/edit fully functional. Type filter works. Duplicate code rejected. Used-journal lock enforced.

---

## Phase 4: User Story 2 — Manage Templates (Priority: P2)

**Goal**: List, create, and edit templates (header-only). System template delete hidden.

**Independent Test**: Create a template linked to a journal, verify list with journal name. System template delete not offered.

### Tests for User Story 2

- [ ] T020 [P] [US2] Test: Template list displays with journal name — `src/Web/ClientApp/src/features/accounting/__tests__/TemplatesListPage.test.tsx`
- [ ] T021 [P] [US2] Test: Template create with templateName, journalId, templateType succeeds — `src/Web/ClientApp/src/features/accounting/__tests__/TemplateCreatePage.test.tsx`
- [ ] T022 [P] [US2] Test: System template delete action hidden — `src/Web/ClientApp/src/features/accounting/__tests__/TemplatesListPage.test.tsx`

### Implementation for User Story 2

- [x] T023 [P] [US2] Create `src/Web/ClientApp/src/features/accounting/templates/components/TemplateGrid.tsx` — DataGrid with columns: templateName, journalName, templateType, isActive. No delete column (no DELETE endpoint). Row click → edit page.
- [x] T024 [P] [US2] Create `src/Web/ClientApp/src/features/accounting/templates/components/TemplateForm.tsx` — Form for create/edit. Fields: templateName, description (textarea), journalId (picker from useJournalsList), templateType (enum select). isSystemTemplate NOT in form.
- [x] T025 [US2] Create `src/Web/ClientApp/src/features/accounting/templates/pages/TemplatesListPage.tsx` — PageHeader (title="قوالب القيود", create button), FilterBar with journal filter + type filter + active filter, TemplateGrid. Empty state: "لا توجد قوالب".
- [x] T026 [US2] Create `src/Web/ClientApp/src/features/accounting/templates/pages/TemplateCreatePage.tsx` — TemplateForm in create mode. On success → redirect to list.
- [x] T027 [US2] Create `src/Web/ClientApp/src/features/accounting/templates/pages/TemplateEditPage.tsx` — Load template by id (useTemplateById). TemplateForm in edit mode with RowVersion. On success → redirect to list.

**Checkpoint**: Templates list/create/edit fully functional. System templates show no delete action.

---

## Phase 5: User Story 3 — Feed Entry Creation Pickers (Priority: P2)

**Goal**: Active-only journals and templates exposed for spec 023 entry creation pickers.

**Independent Test**: Disable a journal, verify it doesn't appear in the entry creation picker. Same for templates.

### Tests for User Story 3

- [ ] T028 [P] [US3] Test: Only active journals appear in picker — `src/Web/ClientApp/src/features/accounting/__tests__/JournalPicker.test.tsx`
- [ ] T029 [P] [US3] Test: Only active templates appear in picker — `src/Web/ClientApp/src/features/accounting/__tests__/TemplatePicker.test.tsx`

### Implementation for User Story 3

- [ ] T030 [US3] Verify `useJournalsList({ isActive: true })` returns only active journals — document in spec 023 integration point
- [ ] T031 [US3] Verify `useTemplatesList({ isActive: true })` returns only active templates — document in spec 023 integration point
- [x] T032 [US3] Register journals/templates routes in `src/Web/ClientApp/src/App.tsx` (or router config) under `/accounting/journals` and `/accounting/templates`

**Checkpoint**: Active-only filtering verified. Routes registered.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final quality pass

- [x] T033 [P] Run `npm run lint` in `src/Web/ClientApp` — fix any lint errors
- [x] T034 [P] Run `npm run build` in `src/Web/ClientApp` — verify build succeeds
- [ ] T035 Run full test suite: `npm run test -- --run` in `src/Web/ClientApp`
- [ ] T036 Run quickstart.md validation scenarios V1–V6 manually
- [ ] T037 Verify Arabic RTL layout renders correctly on all 6 pages
- [ ] T038 Verify dark mode renders correctly on all 6 pages

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — start immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 — BLOCKS all user stories
- **Phase 3 (US1)**: Depends on Phase 2
- **Phase 4 (US2)**: Depends on Phase 2 — can run parallel with US1
- **Phase 5 (US3)**: Depends on Phase 3 (uses useJournalsList from US1 hooks)
- **Phase 6 (Polish)**: Depends on all user stories

### User Story Dependencies

- **US1 (Journals P1)**: Starts after Phase 2 — independent
- **US2 (Templates P2)**: Starts after Phase 2 — independent of US1
- **US3 (Pickers P2)**: Starts after US1 hooks are done (uses useJournalsList)

### Parallel Opportunities

- Phase 1: T001 and T002 parallel
- Phase 2: T004–T010 all parallel (different hook files)
- Phase 3 tests: T011–T014 all parallel
- Phase 3 impl: T015–T016 parallel (Grid + Form), then T017–T019 sequential (pages depend on components)
- Phase 4 tests: T020–T022 all parallel
- Phase 4 impl: T023–T024 parallel (Grid + Form), then T025–T027 sequential
- Phase 5: T028–T029 parallel
- Phase 6: T033–T034 parallel

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational hooks
3. Complete Phase 3: Journals (US1)
4. **STOP and VALIDATE**: Test journals list/create/edit independently
5. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add Journals (US1) → Test → Deploy (MVP!)
3. Add Templates (US2) → Test → Deploy
4. Add Pickers integration (US3) → Test → Deploy
5. Polish → Final deploy

---

## Notes

- Backend fully built — no new C#, no migrations, no new endpoints
- Binding data contract from web-api-client.ts — do not rename fields
- Arabic-first RTL throughout — all page titles, labels, empty states in Arabic
- Dark mode via design tokens — no hardcoded colors
- `isSystemTemplate` hidden from UI create form — server defaults to false
- No DELETE endpoints — no delete buttons in UI for either entity
