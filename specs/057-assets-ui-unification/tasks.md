---

description: "Task list for Assets Management UI Unification (Phase 3)"
---

# Tasks: Assets Management UI Unification (Phase 3)

**Input**: Design documents from `/specs/057-assets-ui-unification/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Tests**: No automated test tasks. This is a frontend-only batch; AGENTS.md governance override prohibits frontend test files. Verification is the structured manual review log required by FR-015 plus the existing automated gates.

**Organization**: Tasks are grouped by user story to enable independent implementation and validation of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1–US5)
- All paths are relative to the repository root

## Path Conventions

- Frontend source: `src/Web/ClientApp/src/`
- Feature specs and evidence: `specs/057-assets-ui-unification/`
- Commands run from `src/Web/ClientApp` unless stated otherwise

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Recording artifacts and baseline for this batch.

- [x] T001 [P] Create `specs/057-assets-ui-unification/review-evidence.md` using the spec 052 schema (eight screens × light/dark/RTL/320-css-px/400%-zoom/keyboard/grayscale rows + static-checks table)
- [x] T002 [P] Create `specs/057-assets-ui-unification/scope-inventory.md` per data-model.md §6 (resolved / deferred / behavior-changes tables with baseline rows)
- [x] T003 [P] Baseline scoped checks: run `npm run design:usage`, `npm run lint`, `npm run build`, plus the palette/direction scans from quickstart.md; record results in scope-inventory.md

**Checkpoint**: Baseline and recording artifacts exist.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: The shared status base-role map every story consumes.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [x] T004 Create `src/Web/ClientApp/src/features/assets/shared/status.ts` with the asset lifecycle base-role map (Draft→`draft`, Active→`active`, UnderMaintenance→`inactive`, Disposed→`closed`, WrittenOff→`closed`) with an unknown-value fallback (`draft` + raw label), plus the group `isActive`→`active`/`inactive` helper, per research R1 and screen-contract §2

**Checkpoint**: Status mapping exists and is importable by all eight screens.

---

## Phase 3: User Story 1 - Consistent assets list page (Priority: P1) 🎯 MVP

**Goal**: The assets register follows the list recipe with server-side pagination preserved: `Page` + `FilterBar` + typed `DataGrid`, correct status rendering, distinct states, and no palette styling.

**Independent Test**: Open `/assets` with real data; confirm one primary create, working filters (no duplicate status option), `StatusBadge` per row, correct totals and last page, distinct empty vs no-results messages, persistent error + retry, and keyboard row activation.

### Implementation for User Story 1

- [x] T005 [US1] Update `src/Web/ClientApp/src/features/assets/assets/pages/AssetsListPage.tsx` to the list recipe: `Page` (title `سجل الأصول`, description, primary create action `primary sm` + icon), `FilterBar` with `FilterSearch` + status `FilterSelect` whose options are deduplicated (explicit all-values option + one option per status), compact density
- [x] T006 [US1] Replace the raw table in `AssetsListPage.tsx` with a typed `DataGrid`: columns (code `dir="ltr"` + tabular, name, group, `StatusBadge` via the status map, original value via the shared money/number convention, asset code/tag identifiers direction-isolated), row activation → `/assets/{id}`, and server pagination mapped to the grid (`pageIndex` is 0-based; the server `page` is 1-based) with the shared `Pagination` and `totalCount`; remove the `items.length < pageSize` heuristic
- [x] T007 [US1] Wire the states in `AssetsListPage.tsx`: initial page-level loading vs grid-scope refresh/filter/page loading, `EmptyState` with dataset-empty vs no-results copy derived from active filters, error via `getQueryErrorMessage(error)` + `refetch`, and remove all palette classes and physical direction utilities
- [x] T008 [US1] Validate the list story on real data (filters, last full page, no-results vs empty, error + retry recovery, keyboard row activation) and record evidence rows per screen-contract §1/§3 in `specs/057-assets-ui-unification/review-evidence.md`

**Checkpoint**: Assets list is an independently validatable list reference.

---

## Phase 4: User Story 2 - Consistent asset create/edit form (Priority: P1) 🎯 MVP

**Goal**: One recipe-conformant asset form with shared controls, per-mode schemas, bound server errors, preserved values, and a clear save/cancel hierarchy; the edit page separates loading, not-found, and error states.

**Independent Test**: Create with invalid/duplicate data and edit with a rejected payload; confirm field/root error binding (single notification), preserved values, cancel behavior, and the edit-page state machine (loading ≠ not-found ≠ error).

### Implementation for User Story 2

- [x] T009 [US2] Update `src/Web/ClientApp/src/features/assets/assets/components/AssetForm.tsx`: replace native inputs with shared `Input`/`Select`/`Textarea` on the canonical field pattern (control owns label, required marker, error, ARIA), group fields into `Card` sections in task order, submit contract `onSubmit(values) => Promise<unknown>` + optional `onSuccess`, call `handleApiError(err, setError)` with a root `Alert`, submit loading/disabled state, correct the corrupted heading text, remove all palette classes
- [x] T010 [US2] Update `AssetForm.tsx` schema handling per mode: create validates with `createAssetSchema`; edit validates with `updateAssetSchema` and seeds the concurrency token from the loaded record (hidden/registered value; payload shape unchanged), per screen-contract §4
- [x] T011 [US2] Update `AssetForm.tsx` read-only mode for the detail page: when `readOnly` is set, render no action bar and no submit path (no operative save), per research R3
- [x] T012 [US2] Update `src/Web/ClientApp/src/features/assets/assets/pages/AssetCreatePage.tsx`: wrap in the form recipe (`Page` with `maxWidth md`, title `إنشاء أصل جديد`, `onBack`), wire the promise-based submit + `onSuccess` navigation + `onCancel`, remove ad-hoc error handling; no toasts
- [x] T013 [US2] Update `src/Web/ClientApp/src/features/assets/assets/pages/AssetEditPage.tsx`: `Page loading` while fetching, `Page error` + `refetch` on failure, not-found `EmptyState` only after a resolved miss, per-mode update schema with `rowVersion` preserved, `onCancel`, dynamic title `تعديل الأصل: {name}`; remove the custom header/breadcrumb and palette classes
- [x] T014 [US2] Validate the form story on real data (empty submit, duplicate/invalid server rejection with single notification + preserved values, unknown id not-found, cancel) and record evidence rows in `specs/057-assets-ui-unification/review-evidence.md`

**Checkpoint**: Create and edit are complete, independently validatable form references.

---

## Phase 5: User Story 3 - Consistent asset detail structure (Priority: P2)

**Goal**: The asset detail applies the detail recipe (identity, status toolbar, single primary edit, confirmed deactivate) and removes the no-op save presentation.

**Independent Test**: Open an asset in each lifecycle state; confirm header/toolbar, single primary edit, read-only presentation without an operative save, and a confirmed deactivate flow that surfaces failures safely.

### Implementation for User Story 3

- [x] T015 [US3] Update `src/Web/ClientApp/src/features/assets/assets/pages/AssetDetailPage.tsx`: `Page` title (asset name) + description + `Page.onBack`, toolbar with `StatusBadge` (status map) and meta items (code `dir="ltr"`, group, purchase date), edit as the single primary action navigating to the edit route, deactivate as a `destructive` action gated by the existing status rule and confirmed via the shared `ConfirmDialog` (inline bespoke modal removed) with `handleLifecycleError` on failure, read-only `AssetForm` (no action bar), loading/error/not-found through `Page`; remove palette classes
- [x] T016 [US3] Validate the detail story on real data (each lifecycle status, read-only no-submit, deactivate confirm/cancel/stale-token conflict, not-found) and record evidence rows in `specs/057-assets-ui-unification/review-evidence.md`

**Checkpoint**: Detail recipe applied; deactivate behavior evidenced (FR-006/FR-007).

---

## Phase 6: User Story 4 - Consistent asset groups screens (Priority: P2)

**Goal**: Asset group list/create/edit/detail follow the same recipes: functional filters, correct status rendering, bound form errors, primary edit, and confirmed lifecycle toggles.

**Independent Test**: Open `/assets/asset-groups`, its create/edit routes, and a group detail; confirm the FilterBar (active filter functional), `StatusBadge` base roles, page recipes, bound form errors, primary edit + confirmed toggles, and empty states for empty sections.

### Implementation for User Story 4

- [x] T017 [P] [US4] Update `src/Web/ClientApp/src/features/assets/asset-groups/pages/AssetGroupsListPage.tsx`: wire the existing `isActiveFilter` to a `FilterSelect` (no dead controls), `StatusBadge variant="active"/"inactive"` via the status helper (tree and grid views), typed `DataGrid` for the grid view, remove the unreachable local `Skeleton` path in favor of page-level loading, `EmptyState` dataset-empty vs no-results, error via `getQueryErrorMessage(error)` + `refetch`, view switcher buttons using shared `Button` variants, keep the tree variation and logical indentation
- [x] T018 [P] [US4] Update `src/Web/ClientApp/src/features/assets/asset-groups/pages/AssetGroupCreatePage.tsx`: replace the local `serverError` div with `handleApiError(err, setError)` + root `Alert`, keep the `Page` recipe and `Card` sections, submit loading, and correct the corrupted Arabic strings (`نسبة القيمة التخريدية`)
- [x] T019 [P] [US4] Update `src/Web/ClientApp/src/features/assets/asset-groups/pages/AssetGroupEditPage.tsx`: `Page loading` → `Page error` + `refetch` → not-found `EmptyState` only after a resolved miss, root `Alert` + `handleApiError`, `rowVersion` preserved through the update payload, corrected Arabic strings
- [x] T020 [P] [US4] Update `src/Web/ClientApp/src/features/assets/asset-groups/pages/AssetGroupDetailPage.tsx`: edit as `primary` (single), activate `outline` / deactivate `destructive` both with `ConfirmDialog`, `StatusBadge` active/inactive via the map (category stays metadata `Badge`), `EmptyState` for empty related sections, `handleLifecycleError` on toggle failures, corrected Arabic strings
- [x] T021 [US4] Validate the groups story on real data (active filter in both views, form rejection binding, deactivation rejection with active children, empty sections, toggles) and record evidence rows in `specs/057-assets-ui-unification/review-evidence.md`

**Checkpoint**: Groups list/detail/forms conform and are evidenced.

---

## Phase 7: User Story 5 - RTL, light/dark, responsive, keyboard (Priority: P2)

**Goal**: All eight screens meet the Phase 1 acceptance conditions with no physical-direction styling or mode regressions.

**Independent Test**: Run the eight screens through the review matrix; confirm reflow at 320 CSS px / 400% zoom, RTL correctness, keyboard-only workflows, and grayscale/forced-colors meaning.

### Implementation for User Story 5

- [x] T022 [US5] Audit the touched files for physical direction utilities and remaining palette classes; fix to logical equivalents (the quickstart scans must return zero matches in `features/assets/**`)
- [x] T023 [US5] Verify/fix responsive behavior at 320 CSS px / 400% zoom for the two lists (including the grid's compact/mobile representation), the forms, dialogs, and detail toolbars
- [x] T024 [US5] Verify/fix dark-mode and grayscale/forced-colors rendering of the eight screens (statuses, validation, focus rings, empty/error states) and fix findings
- [x] T025 [US5] Record all US5 evidence rows in `specs/057-assets-ui-unification/review-evidence.md`

**Checkpoint**: Mode/responsive conditions evidenced for this batch.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Backlog hygiene, the navigation correction, gates, and final validation pass.

- [x] T026 [P] Update `docs/ui-patterns.md` deferred backlog statuses for items resolved by this batch (status misuse, palette styling, custom back buttons, ad-hoc states, dead filters)
- [x] T027 [P] Append a "Phase 3 batch — assets" note to `specs/052-unified-ui-language/scope-inventory.md` deferred table (items now resolved / still deferred)
- [x] T028 Update `src/Web/ClientApp/src/layouts/navigation.ts`: correct the `/assets` permission identifier `Assets.Read` → `Assets.View` (FR-020) and record the behavior change in the scope inventory
- [x] T029 Finalize `specs/057-assets-ui-unification/scope-inventory.md` with resolved/deferred entries and the behavior-change register
- [x] T030 Run full frontend gates from `src/Web/ClientApp`: `npm run design:usage`, `npm run lint`, `npm run build`, `npm run design:lint`, `npm run design:check`, plus the palette/direction scans; record results
- [x] T031 Complete the `quickstart.md` validation pass and confirm every `review-evidence.md` row is filled or carries an accepted, documented deviation; re-check the backend regression gate status
- [x] T032 Confirm no screens outside the eight were modified and no shared component public props changed for other consumers

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Setup — BLOCKS all user stories
- **US1 (Phase 3)** and **US2 (Phase 4)**: Start after Foundational; both are MVP (P1)
- **US3 (Phase 5)**: After Foundational; consumes US2's read-only form mode
- **US4 (Phase 6)**: After Foundational; independent files, but consumes the shared status helper
- **US5 (Phase 7)**: After US1–US4 (validates their outputs)
- **Polish (Phase 8)**: After all stories

### User Story Dependencies

- US1 (P1): assets list page only
- US2 (P1): shared asset form + create/edit pages; T010/T011 touch the same file as T009 (sequential)
- US3 (P2): asset detail page + read-only form mode from US2
- US4 (P2): groups pages + list/grid presentation; uses the status helper but no code dependency on US1/US2
- US5 (P2): verification pass over US1–US4 outputs

### Within Each Story

- Shared component edits before page wiring
- Page wiring before validation/evidence tasks
- Evidence recorded before the story checkpoint

### Parallel Opportunities

- T001–T003 (setup) in parallel
- T009 → T010 → T011 are sequential (same file)
- T012/T013 different files, sequential only because both depend on the form contract
- T017–T020 are different files and can run in parallel
- T022–T024 audits sequential per finding, but independent of US4 evidence
- T026/T027 (polish docs) in parallel

---

## Parallel Example: User Story 4

```text
Task: "Update AssetGroupsListPage.tsx (active filter, StatusBadge, DataGrid grid view, states)"
Task: "Update AssetGroupCreatePage.tsx (Alert + handleApiError, strings)"
Task: "Update AssetGroupEditPage.tsx (state machine, Alert + handleApiError)"
Task: "Update AssetGroupDetailPage.tsx (primary edit, toggles, StatusBadge, empty states)"
```

---

## Implementation Strategy

### MVP First (US1 + US2)

1. Complete Setup + Foundational.
2. Complete US1 (list) and US2 (form) — the two mandatory validation targets for this batch.
3. **STOP and VALIDATE** on real data; record evidence.
4. Then US3 (detail), US4 (groups), US5 (modes/responsive), and Polish.

### Incremental Delivery

1. Foundation → status map.
2. List → validated register screen.
3. Form → validated create/edit screens.
4. Detail → recipe applied + deactivate confirmed.
5. Groups → list/detail/forms aligned.
6. Modes/responsive → acceptance conditions evidenced.
7. Polish → navigation fix, backlog/inventory hygiene, gates.

### Notes

- Every story ends with an evidence task; without recorded rows the story is not done (FR-015).
- Do not modify other asset sub-features (acquisition/activation 056, depreciation, disposal, movements, counts, revaluations), endpoints, DTOs, permissions (beyond the navigation identifier), or `routes.tsx`.
- Public props of shared `components/ui/` components stay unchanged; any additive prop must be justified in the task.
- The only behavior changes are the registered ones (screen-contract §5): palette→token presentation, pagination component + heuristic removal, duplicate option removal, functional active filter, status rendering, read-only no-submit, shared ConfirmDialog, outline activate, normalized errors, navigation identifier, corrected strings.
- [P] tasks touch different files; verify no overlaps before parallelizing.
