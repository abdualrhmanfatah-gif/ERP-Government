---

description: "Task list for Accounts Management UI Unification (Phase 2)"
---

# Tasks: Accounts Management UI Unification (Phase 2)

**Input**: Design documents from `/specs/053-accounts-ui-unification/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Tests**: No automated test tasks. This is a frontend-only batch; AGENTS.md governance override prohibits frontend test files. Verification is the structured manual review log required by FR-015 plus the existing automated gates.

**Organization**: Tasks are grouped by user story to enable independent implementation and validation of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1–US5)
- All paths are relative to the repository root

## Path Conventions

- Frontend source: `src/Web/ClientApp/src/`
- Feature specs and evidence: `specs/053-accounts-ui-unification/`
- Commands run from `src/Web/ClientApp` unless stated otherwise

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Recording artifacts and baseline for this batch.

- [x] T001 [P] Create `specs/053-accounts-ui-unification/review-evidence.md` using the spec 052 schema (six screens × light/dark/RTL/320-css-px/400%-zoom/keyboard/grayscale rows + static-checks table)
- [x] T002 [P] Create `specs/053-accounts-ui-unification/scope-inventory.md` per data-model.md §6 (resolved / deferred / behavior-changes tables with baseline rows)
- [x] T003 [P] Baseline scoped checks: run `npm run design:usage` filtered to `features/accounting/**` and `components/Accounting*`, plus `npm run lint` and `npm run build`; record results in scope-inventory.md

**Checkpoint**: Baseline and recording artifacts exist.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Shared schema module and the token-reference defect that blocks all stories.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [x] T004 Create `src/Web/ClientApp/src/features/accounting/shared/schemas.ts` exporting the account schema (code required/read-only-on-edit, name, description, accountGroupId, parentId, normalBalance, isPostable, isReconcilable, currencyId) and the account-group schema (code/name/type/normalBalance with the type↔balance rule, description, parentId), preserving the current Arabic messages
- [x] T005 [P] Fix undefined `--color-border-container` → `--color-container-border` in `src/Web/ClientApp/src/components/AccountingAccountGrid.tsx`, `src/Web/ClientApp/src/components/AccountingAccountForm.tsx`, `src/Web/ClientApp/src/components/AccountingAccountDetail.tsx`
- [x] T006 Run `npm run design:usage` scoped to the batch and confirm zero unknown token references in the touched accounting files; record the result in scope-inventory.md

**Checkpoint**: Schemas exist and token integrity is proven for the batch.

---

## Phase 3: User Story 1 - Consistent accounts list page (Priority: P1) 🎯 MVP

**Goal**: The accounts list follows the approved list recipe with a justified tree-grid variation: correct status roles, scoped states, and token-clean presentation.

**Independent Test**: Open `/accounting/accounts` with real data; confirm single primary create, compact filters, tree keyboard navigation, `StatusBadge` active/inactive, distinct no-results vs empty messages, and a persistent retry state on API failure.

### Implementation for User Story 1

- [x] T007 [US1] Update `src/Web/ClientApp/src/components/AccountingAccountGrid.tsx`: `StatusBadge variant="active"/"inactive"` instead of `Badge`, `text-[var(--color-on-primary)]` header, `--color-focus-ring` focused-row outline, `ErrorState` + outline retry instead of the ad-hoc error block, and an additive `emptyMessage` prop
- [x] T008 [US1] Update `src/Web/ClientApp/src/features/accounting/pages/AccountsListPage.tsx`: derive `hasFilters` (search/group/active/postable), pass the no-results vs empty `emptyMessage`, pass `error`/`onRetry`, keep the fetch-all pattern with no pagination
- [x] T009 [US1] Direction-isolate identifiers (`dir="ltr"`) in the grid and list and confirm no `any` column typing remains in the touched files
- [x] T010 [US1] Validate the list story on real data against `contracts/screen-contract.md` §1 and record evidence rows (light, RTL, 320 CSS px) plus empty/no-results/error checks in `specs/053-accounts-ui-unification/review-evidence.md`

**Checkpoint**: Accounts list is an independently validatable list reference.

---

## Phase 4: User Story 2 - Consistent account create/edit form (Priority: P1) 🎯 MVP

**Goal**: One recipe-conformant account form with shared validation, bound server errors, and a clear save/cancel hierarchy; the edit page separates loading, not-found, and error states.

**Independent Test**: Create with invalid/duplicate code and edit with a rejected payload; confirm field/root error binding (single notification), preserved values, cancel behavior, and the edit-page state machine (loading ≠ not-found ≠ error).

### Implementation for User Story 2

- [x] T011 [US2] Update `src/Web/ClientApp/src/components/AccountingAccountForm.tsx`: import the shared schema, group fields into labeled sections (basic / classification / options), submit contract `onSubmit(values) => Promise<unknown>` + `onSuccess`, call `handleApiError(err, setError)` with a root `Alert`, add `onCancel` (ghost), keep code read-only on edit, comfortable density
- [x] T012 [US2] Update `src/Web/ClientApp/src/features/accounting/pages/AccountCreatePage.tsx`: wire the promise-based submit and `onSuccess` navigation, pass `onCancel`, remove ad-hoc toasts, set `maxWidth="sm"`
- [x] T013 [US2] Update `src/Web/ClientApp/src/features/accounting/pages/AccountEditPage.tsx`: `Page loading` while fetching, `Page error` + refetch on failure, not-found `EmptyState` only after a resolved miss; wire submit/onSuccess/onCancel like create; keep the payload and `rowVersion` round-trip unchanged
- [x] T014 [US2] Validate the form story on real data (empty submit, duplicate code, conflict, cancel) and record evidence rows in `specs/053-accounts-ui-unification/review-evidence.md`

**Checkpoint**: Create and edit are complete, independently validatable form references.

---

## Phase 5: User Story 3 - Consistent account detail structure with sub-accounts (Priority: P2)

**Goal**: The account detail applies the detail recipe (identity, status toolbar, single primary edit) and its sub-accounts tab lists direct children from existing data.

**Independent Test**: Open an account with children and a leaf account; confirm toolbar status/meta, single primary edit, working sub-accounts navigation, distinct empty/loading/error states, and no custom back button.

### Implementation for User Story 3

- [x] T015 [P] [US3] Update `src/Web/ClientApp/src/components/AccountingAccountDetail.tsx`: token fix (done in T005, verify), field presentation consistent with the detail recipe (no status duplication), direction-isolated identifiers
- [x] T016 [US3] Update `src/Web/ClientApp/src/features/accounting/pages/AccountDetailPage.tsx`: title = account name, `Page.onBack`, toolbar with `StatusBadge` + `MetaItem`s (code/group/level/balance), edit as single primary action, loading/error through `Page`, keep the details tab
- [x] T017 [US3] Add the sub-accounts tab in `src/Web/ClientApp/src/features/accounting/pages/AccountDetailPage.tsx`: derive direct children from `useAccountsList()` (`parentId === account.id`, sorted by `code`), typed `DataGrid` (code/name/group/state) with navigation, `EmptyState "لا توجد حسابات فرعية"`, loading/error states from the query
- [x] T018 [US3] Validate the detail and sub-accounts stories on real data and record evidence rows in `specs/053-accounts-ui-unification/review-evidence.md`

**Checkpoint**: Detail recipe applied; sub-accounts behavior evidenced (SC-013).

---

## Phase 6: User Story 4 - Consistent account groups list and detail (Priority: P2)

**Goal**: Groups list and detail follow the same recipes: shared filter search, correct status roles, real error states, and the clarified action hierarchy; dialogs stay as the editing surface.

**Independent Test**: Open `/accounting/account-groups` and a group detail; confirm FilterSearch, inactive role, persistent retry, primary create, `Page.onBack`, edit-primary + confirmed outline/destructive toggle, and `EmptyState` for empty children/accounts.

### Implementation for User Story 4

- [x] T019 [P] [US4] Update `src/Web/ClientApp/src/features/accounting/account-groups/pages/AccountGroupsListPage.tsx`: replace the raw `Input` with `FilterSearch`, pass `error`/`onRetry` to `Page`, map inactive to `StatusBadge variant="inactive"`, make the create button explicit `primary`/`sm` with the icon pattern, keep tree/grid modes and pagination
- [x] T020 [P] [US4] Update `src/Web/ClientApp/src/features/accounting/account-groups/pages/AccountGroupDetailPage.tsx`: `Page.onBack`, `StatusBadge` inactive role, explicit edit primary + toggle `outline`/`destructive` with `ConfirmDialog`, `EmptyState` for empty children/accounts, icon+text for the postable capability, loading/error handling
- [x] T021 [US4] Update `src/Web/ClientApp/src/components/AccountingAccountGroupForm.tsx`: import the shared schema, drop the "(20)/(200)" label hints and the double-spaced label, use `loading` on the save button, bind server errors via `handleApiError` with a root `Alert`, single-column on narrow dialogs, remove the duplicated inner padding
- [x] T022 [US4] Validate the groups stories on real data (create/edit dialog validation, deactivation confirmation, empty sections) and record evidence rows in `specs/053-accounts-ui-unification/review-evidence.md`

**Checkpoint**: Groups list/detail/dialog conform and are evidenced.

---

## Phase 7: User Story 5 - RTL, light/dark, responsive, keyboard (Priority: P2)

**Goal**: All six screens meet the Phase 1 acceptance conditions with no physical-direction styling or mode regressions.

**Independent Test**: Run the six screens through the review matrix; confirm reflow at 320 CSS px / 400% zoom, RTL correctness, keyboard-only workflows, and grayscale/forced-colors meaning.

### Implementation for User Story 5

- [x] T023 [US5] Audit the touched files for physical direction utilities and fix to logical equivalents (`ms/me/ps/pe`, `start/end`) in `src/Web/ClientApp/src/features/accounting/**` and `src/Web/ClientApp/src/components/Accounting*`
- [x] T024 [US5] Verify/fix responsive behavior of the tree grid (labeled horizontal-scroll region for the inherently 2-D table), the account form grids, dialogs, and the detail toolbar at 320 CSS px / 400% zoom
- [x] T025 [US5] Verify dark-mode and grayscale/forced-colors rendering of the touched screens (statuses, focus rings, empty/error states) and fix findings
- [x] T026 [US5] Record all US5 evidence rows in `specs/053-accounts-ui-unification/review-evidence.md`

**Checkpoint**: Mode/responsive conditions evidenced for this batch.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Backlog hygiene, gates, and final validation pass.

- [x] T027 [P] Update `docs/ui-patterns.md` deferred backlog statuses for items resolved by this batch (Badge misuse, custom back buttons, filter input, status roles, `DataGridColumn<any>` where fixed)
- [x] T028 [P] Append a "Phase 2 batch 1 — accounts" note to `specs/052-unified-ui-language/scope-inventory.md` deferred table (items now resolved / still deferred)
- [x] T029 Finalize `specs/053-accounts-ui-unification/scope-inventory.md` with resolved/deferred entries and the behavior-change register
- [x] T030 Run full frontend gates from `src/Web/ClientApp`: `npm run design:usage`, `npm run lint`, `npm run build`, `npm run design:lint`, `npm run design:check`; record results
- [x] T031 Complete the `quickstart.md` validation pass and confirm every `review-evidence.md` row is filled or carries an accepted, documented deviation; re-check the backend regression gate status (blocked pre-existing at Phase 1)
- [x] T032 Confirm no screens outside the six were modified (`git status`/diff review) and no public props of `Accounting*` components changed for other consumers

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Setup — BLOCKS all user stories
- **US1 (Phase 3)** and **US2 (Phase 4)**: Start after Foundational; both are MVP (P1)
- **US3 (Phase 5)**: After Foundational; touches the detail page only (independent of US1/US2 code paths)
- **US4 (Phase 6)**: After Foundational; independent files
- **US5 (Phase 7)**: After US1–US4 (validates their outputs)
- **Polish (Phase 8)**: After all stories

### User Story Dependencies

- US1 (P1): shared grid + list page only
- US2 (P1): shared form + create/edit pages; reuses US1's page-level error conventions but no code dependency
- US3 (P2): detail page + detail component + sub-accounts (uses `useAccountsList`, no dependency on US1/U2)
- US4 (P2): groups pages + group dialog; independent files
- US5 (P2): verification pass over US1–US4 outputs

### Within Each Story

- Shared component edits before page wiring
- Page wiring before validation/evidence tasks
- Evidence recorded before the story checkpoint

### Parallel Opportunities

- T001–T003 (setup) in parallel
- T005 independent of T004
- T007/T019/T020 are different files and can run in parallel across stories once Foundational is done
- T015 (detail component) parallel with T016/T017 planning
- T023–T025 audit tasks sequential per finding, but independent of US1–US4 evidence tasks
- T027/T028 (polish docs) in parallel

---

## Parallel Example: User Story 1

```text
Task: "Update AccountingAccountGrid.tsx statuses/states/tokens"
Task: "Update AccountsListPage.tsx filter/no-results/error wiring"
Task: "Direction-isolate identifiers and confirm typed columns"
```

## Parallel Example: User Story 4

```text
Task: "Update AccountGroupsListPage.tsx (FilterSearch, error, status, primary create)"
Task: "Update AccountGroupDetailPage.tsx (onBack, actions, empty states)"
Task: "Update AccountingAccountGroupForm.tsx (schema import, labels, error binding)"
```

---

## Implementation Strategy

### MVP First (US1 + US2)

1. Complete Setup + Foundational.
2. Complete US1 (list) and US2 (form) — the two mandatory validation targets for this batch.
3. **STOP and VALIDATE** on real data; record evidence.
4. Then US3 (detail/sub-accounts), US4 (groups), US5 (modes/responsive), and Polish.

### Incremental Delivery

1. Foundation → schemas + token integrity.
2. List → validated list screen.
3. Form → validated create/edit screens.
4. Detail → recipe applied + sub-accounts view.
5. Groups → list/detail/dialog aligned.
6. Modes/responsive → acceptance conditions evidenced.
7. Polish → backlog/inventory hygiene and gates.

### Notes

- Every story ends with an evidence task; without recorded rows the story is not done (FR-015).
- Do not modify journal entries, journals, templates, recurring entries, endpoints, DTOs, permissions, or `routes.tsx`.
- Public props of `Accounting*` components stay unchanged for other consumers.
- The only behavior changes are the registered ones (cancel action, no-results message, detail primary action, sub-accounts view, inactive role rendering, edit-page state machine, group label text).
- [P] tasks touch different files; verify no overlaps before parallelizing.

---

## Phase 9: Convergence

- [x] T033 Use `StatusBadge variant="inactive"` and the canonical `getActiveStatusLabel` text for inactive groups in `src/Web/ClientApp/src/components/AccountingGroupTree.tsx` (tree mode of the groups list) instead of `closed`/`معطل` per FR-002 and contract §1 (contradicts)
- [x] T034 Rebuild the `AccountEditPage` not-found branch as `<Page title="" onBack={...}><EmptyState message="الحساب غير موجود" /></Page>` with the shared `Button`, removing the `Page` nested inside `EmptyState.action` and the raw `<button>` per FR-005 and T013 (partial)
- [x] T035 Handle the `useAccountsList()` failure in the account detail sub-accounts tab: destructure `error`/`refetch` and render a persistent error + retry instead of the "لا توجد حسابات فرعية" empty state per FR-019, SC-013, and contract §2 (partial)
- [x] T036 Add navigation from sub-accounts rows to the child account detail (`onRowClick` or an actions column) in `src/Web/ClientApp/src/features/accounting/pages/AccountDetailPage.tsx` per FR-019, SC-013, and contract §2 (missing)
- [x] T037 Replace the `AccountEditPage` error-state `onRetry` navigation with the `useAccountDetail` query `refetch` per T013 and contract §1 (partial)
- [x] T038 Render a distinguishable not-found `EmptyState` (with message) on `AccountDetailPage` instead of an empty `Page` with only a back button per FR-005 and contract §1 (partial)
- [x] T039 Use `getQueryErrorMessage(error)` and a React Query `refetch` for the `AccountGroupsListPage` error state instead of the hard-coded "خطأ في التحميل" and `window.location.reload()` per FR-005, T019, and Principle XIII (partial)
- [x] T040 Use `getQueryErrorMessage(error)` and the query `refetch` for the `AccountGroupDetailPage` error state instead of the hard-coded message and navigate-away retry per FR-005, T020, and contract §1 (partial)
- [x] T041 Direction-isolate codes/paths (`dir="ltr"` or equivalent) in the groups list grid (`code`, `ancestorPath`), `AccountingGroupTree.tsx` (`code`), and the group detail children grid per FR-008 (partial)
- [x] T042 Give the accounts tree grid's horizontal-scroll wrapper a labeled, keyboard-reachable region (`role="region"`/`aria-label`/`tabIndex={0}` or equivalent) in `AccountingAccountGrid.tsx` per T024 and SC-006/SC-007 (partial)
- [x] T043 Render the linked accounts' postable capability as icon + text in the group detail accounts grid per contract §1 and T020 (partial)
- [x] T044 Remove the residual trailing/double whitespace in the `نوع الحساب` labels and validation messages (account and group forms, accounts grid header, detail field, `features/accounting/shared/schemas.ts`) per contract §4 and T021 (partial)
- [x] T045 Collapse the group form type/normal-balance pair to a single column at narrow dialog widths in `AccountingAccountGroupForm.tsx` per T021 (partial)
