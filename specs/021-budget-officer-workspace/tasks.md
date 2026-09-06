# Tasks: Budget Officer Workspace — Full Budget Lifecycle UI

**Input**: Design documents from `/specs/021-budget-officer-workspace/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Included — TDD mandatory per constitution Principle XI.

**Organization**: Tasks grouped by user story for independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Routes, navigation, shared client extensions — must complete before any user story.

- [x] T001 Register budget list, detail, and create routes in src/Web/ClientApp/src/app/routes.tsx with Arabic labels and `protected: true`
- [x] T002 Add "الموازنات" nav item to the "الموازنة" group in src/Web/ClientApp/src/layouts/navigation.ts with permission `Budgets.View`
- [x] T003 [P] Extend budgetingKeys in src/Web/ClientApp/src/features/budgeting/shared/client.ts with budgetItems, monthlyPlan, and execution keys
- [x] T004 [P] Verify BUDGET_PERMISSIONS already covers all needed codes in src/Web/ClientApp/src/shared/constants/permissions.ts — no changes expected

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Hooks and shared utilities that ALL user stories depend on.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [x] T005 Add useBudgetItemsTree(budgetId) and useCreateBudgetItem, useUpdateBudgetItem, useDeleteBudgetItem hooks in src/Web/ClientApp/src/features/budgeting/hooks/useBudgetItems.ts using BudgetsClient from web-api-client.ts
- [x] T006 Create useMonthlyPlan(budgetItemId) hook in src/Web/ClientApp/src/features/budgeting/hooks/useMonthlyPlan.ts — read/write to localStorage key `monthly-plan-{budgetItemId}`, return { plan, setMonth, save, total, variance }
- [x] T007 [P] Extend appropriationsClient in src/Web/ClientApp/src/features/budgeting/shared/client.ts with full CRUD + lifecycle methods (create, update, delete, transition) if not already present

**Checkpoint**: Foundation ready — user story implementation can now begin.

---

## Phase 3: User Story 1 — Budget Draft & Item Tree (Priority: P1) 🎯 MVP

**Goal**: Budget officer creates a budget draft, populates the item tree, edits items, and progresses through lifecycle (Draft → Submitted → Approved → Active).

**Independent Test**: Create a budget, add/edit/remove items in the tree, submit → approve → activate. Verify status transitions and tree integrity. No appropriation or encumbrance needed.

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T008 [P] [US1] Write BudgetsListPage.test.tsx — render list, verify filters, empty state, click navigates to detail in src/Web/ClientApp/src/features/budgeting/__tests__/BudgetsListPage.test.tsx
- [x] T009 [P] [US1] Write BudgetCreatePage.test.tsx — render form, fill fields, submit calls createBudget, validation errors in src/Web/ClientApp/src/features/budgeting/__tests__/BudgetCreatePage.test.tsx
- [x] T010 [P] [US1] Write BudgetDetailPage.test.tsx — render detail, lifecycle buttons conditional on status, item tree renders, approval history panel in src/Web/ClientApp/src/features/budgeting/__tests__/BudgetDetailPage.test.tsx

### Implementation for User Story 1

- [x] T011 [P] [US1] Create budgets folder structure: src/Web/ClientApp/src/features/budgeting/budgets/{pages,index.ts}
- [x] T012 [US1] Implement BudgetsListPage.tsx — use useBudgetsList hook, table with status badge, filters (status, fiscal year, fund), "New Budget" button, empty state, skeleton loader in src/Web/ClientApp/src/features/budgeting/budgets/pages/BudgetsListPage.tsx
- [x] T013 [US1] Implement BudgetCreatePage.tsx — controlled form (BudgetName, BudgetType dropdown, FiscalYear dropdown, Fund dropdown, EffectiveFrom date, EffectiveTo date, Description, TotalAmount), submit via useCreateBudget, navigate to detail on success in src/Web/ClientApp/src/features/budgeting/budgets/pages/BudgetCreatePage.tsx
- [x] T014 [US1] Implement BudgetDetailPage.tsx — use useBudgetDetail, display metadata grid (BudgetNumber, BudgetName, BudgetType, Fund, Status badge, EffectiveFrom/To), ApprovalHistoryPanel, LifecycleActions with status-conditional actions, reuse BudgetItemTree component for item panel in src/Web/ClientApp/src/features/budgeting/budgets/pages/BudgetDetailPage.tsx
- [x] T015 [US1] Wire lifecycle actions in BudgetDetailPage — define action arrays per BudgetStatus, use useSubmitBudget/useApproveBudget/useActivateBudget/useSuspendBudget/useCloseBudget/useCancelBudget hooks, pass can(permission) from usePermission in src/Web/ClientApp/src/features/budgeting/budgets/pages/BudgetDetailPage.tsx
- [x] T016 [US1] Add item CRUD to BudgetDetailPage — "Add Item" button calls useCreateBudgetItem, inline edit calls useUpdateBudgetItem, delete calls useDeleteBudgetItem (only when status is Draft), pass canEdit based on status in src/Web/ClientApp/src/features/budgeting/budgets/pages/BudgetDetailPage.tsx

**Checkpoint**: Budget lifecycle fully functional from UI — create, tree, submit, approve, activate.

---

## Phase 4: User Story 2 — Appropriations & Transfers (Priority: P1)

**Goal**: On an Active budget item, officer records appropriations (Original/Supplement/Reduction/Transfer) with availability feedback and transfer target selection.

**Independent Test**: Create appropriations of each type against an active item, verify availability updates, confirm lifecycle transitions. Transfer target selector verified independently.

### Tests for User Story 2

- [x] T017 [P] [US2] Write AppropriationCreatePage.test.tsx — render form, type dropdown shows all types, Transfer type shows target selector, submit calls createAppropriation, availability indicator visible in src/Web/ClientApp/src/features/budgeting/__tests__/AppropriationCreatePage.test.tsx
- [x] T018 [P] [US2] Write AppropriationsListPage.test.tsx — render list by item, status badges, lifecycle buttons, empty state in src/Web/ClientApp/src/features/budgeting/__tests__/AppropriationsListPage.test.tsx

### Implementation for User Story 2

- [x] T019 [P] [US2] Create appropriations folder structure: src/Web/ClientApp/src/features/budgeting/appropriations/{pages,index.ts}
- [x] T020 [US2] Register appropriation routes in src/Web/ClientApp/src/app/routes.tsx — /budgeting/appropriations, /budgeting/appropriations/create?budgetItemId={id}
- [x] T021 [US2] Implement AppropriationsListPage.tsx — use useAppropriationsList, table with AppropriationNumber, Type badge, Amount, Status badge, lifecycle buttons via LifecycleActions in src/Web/ClientApp/src/features/budgeting/appropriations/pages/AppropriationsListPage.tsx
- [x] T022 [US2] Implement AppropriationCreatePage.tsx — controlled form with AppropriationType dropdown, Amount, DocumentType, DocumentId; when type=Transfer, show target item selector (fetch tree, flatten, filter out source + Closed/Cancelled); display AvailabilityIndicator for selected budgetItemId; submit via useCreateAppropriation in src/Web/ClientApp/src/features/budgeting/appropriations/pages/AppropriationCreatePage.tsx
- [x] T023 [US2] Implement transfer target selector in AppropriationCreatePage — fetch budget items via useBudgetItemsTree(budgetId), filter out source item and items with Closed/Cancelled status, render searchable dropdown, show error for Suspended/Closed targets in src/Web/ClientApp/src/features/budgeting/appropriations/pages/AppropriationCreatePage.tsx
- [x] T024 [US2] Wire appropriation lifecycle actions — define action arrays per AppropriationStatus, use useSubmitAppropriation/useApproveAppropriation/useActivateAppropriation/useSuspendAppropriation/useCloseAppropriation/useCancelAppropriation in src/Web/ClientApp/src/features/budgeting/appropriations/pages/AppropriationsListPage.tsx

**Checkpoint**: Appropriation CRUD + lifecycle + transfer selector functional.

---

## Phase 5: User Story 3 — Encumbrances with Live Availability (Priority: P1)

**Goal**: Officer records encumbrances against items with live availability indicator (green/amber/red) computed before saving. Reversal with reason supported.

**Independent Test**: Create encumbrances against known appropriations, verify availability reflects correct net amounts in real time, confirm blocking/warning behavior matches ControlMethod.

### Tests for User Story 3

- [x] T025 [P] [US3] Write EncumbranceCreatePage.test.tsx — render form, availability indicator fetches on amount change, appropriation selector changes trigger re-fetch, reversal form requires reason in src/Web/ClientApp/src/features/budgeting/__tests__/EncumbranceCreatePage.test.tsx
- [x] T026 [P] [US3] Write EncumbrancesListPage.test.tsx — render list by appropriation, status badges, lifecycle buttons, reversed indicator in src/Web/ClientApp/src/features/budgeting/__tests__/EncumbrancesListPage.test.tsx

### Implementation for User Story 3

- [x] T027 [P] [US3] Create encumbrances folder structure: src/Web/ClientApp/src/features/budgeting/encumbrances/{pages,index.ts}
- [x] T028 [US3] Register encumbrance routes in src/Web/ClientApp/src/app/routes.tsx — /budgeting/encumbrances, /budgeting/encumbrances/create?appropriationId={id}
- [x] T029 [US3] Implement EncumbrancesListPage.tsx — use useEncumbrancesList, table with EncumbranceNumber, Type badge, Amount, Status badge, isReversed indicator, lifecycle buttons via LifecycleActions in src/Web/ClientApp/src/features/budgeting/encumbrances/pages/EncumbrancesListPage.tsx
- [x] T030 [US3] Implement EncumbranceCreatePage.tsx — controlled form with EncumbranceType dropdown, AppropriationId (pre-filled from query param), Amount (live), EncumbranceDate, VendorId, Description; display AvailabilityIndicator (fetch mode via appropriationId); amount changes trigger availability re-fetch via React Query key change in src/Web/ClientApp/src/features/budgeting/encumbrances/pages/EncumbranceCreatePage.tsx
- [x] T031 [US3] Implement reversal form in EncumbranceCreatePage — when user clicks "Reverse", show ReversalReason textarea (required), submit via useReverseEncumbrance which creates new row with ReversalOfId in src/Web/ClientApp/src/features/budgeting/encumbrances/pages/EncumbranceCreatePage.tsx
- [x] T032 [US3] Wire encumbrance lifecycle actions — define action arrays per EncumbranceStatus (Submit, Approve, Activate, Release, Close, Cancel, Reverse), use corresponding hooks in src/Web/ClientApp/src/features/budgeting/encumbrances/pages/EncumbrancesListPage.tsx

**Checkpoint**: Encumbrance CRUD + live availability + reversal functional.

---

## Phase 6: User Story 4 — Monthly Plan Editor (Priority: P2)

**Goal**: Informational 12-month plan editor per budget item, persisted in localStorage, with variance indicator and export.

**Independent Test**: Enter monthly amounts, verify persistence across reloads, confirm variance indicator, export as TSV.

### Tests for User Story 4

- [x] T033 [P] [US4] Write MonthlyPlanEditor.test.tsx — render 12 columns, save persists to localStorage, restore on remount, variance badge shows when totals mismatch, copy produces TSV in src/Web/ClientApp/src/features/budgeting/__tests__/MonthlyPlanEditor.test.tsx

### Implementation for User Story 4

- [x] T034 [P] [US4] Create monthly-plan folder structure: src/Web/ClientApp/src/features/budgeting/monthly-plan/{components,index.ts}
- [x] T035 [US4] Implement MonthlyPlanEditor.tsx — 12-column grid (Jan–Dec), each column has amount input, use useMonthlyPlan hook for state, display total alongside item's appropriated total, variance info badge when mismatch, "Copy" button that copies TSV to clipboard, "Save" button that calls hook's save function in src/Web/ClientApp/src/features/budgeting/monthly-plan/components/MonthlyPlanEditor.tsx
- [x] T036 [US4] Integrate MonthlyPlanEditor into BudgetDetailPage — add "الخطة الشهرية" button when an item is selected, open MonthlyPlanEditor in a panel/modal below the tree in src/Web/ClientApp/src/features/budgeting/budgets/pages/BudgetDetailPage.tsx

**Checkpoint**: Monthly plan editor functional — persists, shows variance, exports.

---

## Phase 7: User Story 5 — Budget Execution Drill-Down (Priority: P2)

**Goal**: Drill-down from budget summary → items → appropriations → encumbrances with totals at each level.

**Independent Test**: Create budget with items/appropriations/encumbrances, verify drill-down shows correct totals, click-to-expand works.

### Tests for User Story 5

- [x] T037 [P] [US5] Write ExecutionDrillDown.test.tsx — render summary totals, click item expands to show appropriations, click appropriation expands to show encumbrances, totals match in src/Web/ClientApp/src/features/budgeting/__tests__/ExecutionDrillDown.test.tsx

### Implementation for User Story 5

- [x] T038 [P] [US5] Create execution folder structure: src/Web/ClientApp/src/features/budgeting/execution/{components,index.ts}
- [x] T039 [US5] Implement ExecutionDrillDown.tsx — summary panel with Total Appropriated / Total Encumbered / Total Available; expandable item rows (click → show appropriations); expandable appropriation rows (click → show encumbrances); totals computed per level; use useAppropriationsList and useEncumbrancesList with filters in src/Web/ClientApp/src/features/budgeting/execution/components/ExecutionDrillDown.tsx
- [x] T040 [US5] Integrate ExecutionDrillDown into BudgetDetailPage — add execution tab/section below item tree for Active budgets, render ExecutionDrillDown with budgetId in src/Web/ClientApp/src/features/budgeting/budgets/pages/BudgetDetailPage.tsx

**Checkpoint**: Execution drill-down functional with correct totals.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: RTL, accessibility, error handling, loading states across all stories.

- [x] T041 [P] Audit all new pages for RTL correctness — verify logical properties (start/end not left/right), Arabic text rendering, no layout breakage in src/Web/ClientApp/src/features/budgeting/
- [x] T042 [P] Add skeleton loaders to all list pages (BudgetsListPage, AppropriationsListPage, EncumbrancesListPage) following FundDetailPage loading pattern in src/Web/ClientApp/src/features/budgeting/
- [x] T043 [P] Add empty state messages with call-to-action to all list pages following BudgetItemTree empty state pattern in src/Web/ClientApp/src/features/budgeting/
- [x] T044 [P] Verify all forms display ProblemDetails errors inline and show concurrency conflict messages (409) per FR-015 in src/Web/ClientApp/src/features/budgeting/
- [x] T045 [P] Verify all interactive elements have ARIA labels and are keyboard-navigable per FR-013 in src/Web/ClientApp/src/features/budgeting/
- [ ] T046 Run quickstart.md validation scenarios VS1–VS7 end-to-end
- [ ] T047 Run full frontend test suite: cd src/Web/ClientApp && npm run test -- --run && npm run lint && npm run build

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — start immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 — BLOCKS all user stories
- **Phase 3 (US1)**: Depends on Phase 2
- **Phase 4 (US2)**: Depends on Phase 2; can run parallel with US1 (needs Active budget from US1 for manual testing, but code is independent)
- **Phase 5 (US3)**: Depends on Phase 2; can run parallel with US1/US2 (needs Active appropriation for manual testing)
- **Phase 6 (US4)**: Depends on Phase 2; independent of US2/US3
- **Phase 7 (US5)**: Depends on Phase 2 + Phase 3 (needs BudgetDetailPage from US1)
- **Phase 8 (Polish)**: Depends on all desired stories being complete

### User Story Dependencies

- **US1 (P1)**: After Phase 2 — no dependencies on other stories
- **US2 (P1)**: After Phase 2 — code independent, manual test needs US1 Active budget
- **US3 (P1)**: After Phase 2 — code independent, manual test needs US2 Active appropriation
- **US4 (P2)**: After Phase 2 — fully independent
- **US5 (P2)**: After Phase 2 + US1 (BudgetDetailPage must exist)

### Parallel Opportunities

- T003, T004, T007 in Phase 1/2 can run in parallel
- T008, T009, T010 (US1 tests) can run in parallel
- T011, T012, T013 can run in parallel (different files)
- T017, T018 (US2 tests) can run in parallel
- T019, T020 can run in parallel
- T025, T026 (US3 tests) can run in parallel
- T027, T028 can run in parallel
- T033 (US4 test) is standalone
- T034, T035 can run in parallel
- T037 (US5 test) is standalone
- T038, T039 can run in parallel
- T041–T045 (Polish) all run in parallel

---

## Parallel Example: User Story 1

```bash
# Launch all US1 tests together:
Task: "Write BudgetsListPage.test.tsx in __tests__/"
Task: "Write BudgetCreatePage.test.tsx in __tests__/"
Task: "Write BudgetDetailPage.test.tsx in __tests__/"

# Launch US1 page creation in parallel:
Task: "Create budgets folder structure"
Task: "Implement BudgetsListPage.tsx"
Task: "Implement BudgetCreatePage.tsx"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (routes + navigation)
2. Complete Phase 2: Foundational (hooks)
3. Complete Phase 3: US1 (budget CRUD + item tree + lifecycle)
4. **STOP and VALIDATE**: Test budget lifecycle end-to-end
5. Demo if ready

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. US1 → Budget lifecycle works → **MVP!**
3. US2 → Appropriations work → Deploy/Demo
4. US3 → Encumbrances + availability → Deploy/Demo
5. US4 → Monthly plan → Deploy/Demo
6. US5 → Execution drill-down → Deploy/Demo
7. Polish → Final quality pass

### Parallel Team Strategy

With multiple developers:
1. Team completes Setup + Foundational together
2. Once done:
   - Developer A: US1 (Budget CRUD)
   - Developer B: US4 (Monthly Plan) — fully independent
   - Developer C: US2 (Appropriations) — needs US1 Active budget for testing only
3. After US1 merges: Developer A + C continue with US3, US5

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Verify tests FAIL before implementing (TDD)
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- All paths relative to `src/Web/ClientApp/src/`
