# Tasks: Budgeting Frontend Foundation #1 of 5

**Input**: Design documents from `/specs/011-budgeting-frontend-foundation/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/frontend-api-contract.md

**Tests**: Not explicitly requested in the feature specification. Test tasks omitted.

**Organization**: Tasks grouped by user story for independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create directory structure and barrel exports for the budgeting feature module

- [x] T001 Create directory structure: `src/Web/ClientApp/src/features/budgeting/shared/`, `features/budgeting/shared/components/`, `features/budgeting/budget-types/pages/`, `features/budgeting/budget-types/hooks/`, `features/budgeting/funds/pages/`, `features/budgeting/funds/hooks/`
- [x] T002 [P] Create barrel file `src/Web/ClientApp/src/features/budgeting/index.ts` re-exporting from shared, budget-types, and funds sub-modules

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Shared types and API client that ALL user stories depend on

**CRITICAL**: No user story work can begin until this phase is complete

### User Story 1 - Shared Types (Priority: P1)

**Goal**: Provide a single source of truth for all budgeting DTOs, enums, and Arabic label maps

**Independent Test**: Import every type and enum, verify compilation, verify each enum has an Arabic label entry

- [x] T003 [US1] Create `src/Web/ClientApp/src/features/budgeting/shared/types.ts` with all 8 enums (BudgetControlMethod, FundType, FundCategory, BudgetStatus, AppropriationType, AppropriationStatus, EncumbranceType, EncumbranceStatus) and their Arabic label maps per data-model.md
- [x] T004 [US1] Add all 8 DTO interfaces to `src/Web/ClientApp/src/features/budgeting/shared/types.ts` (BudgetTypeDto, FundDto, BudgetClassificationDto, BudgetClassificationTreeDto, BudgetDto, BudgetItemDto, AppropriationDto, EncumbranceDto) per data-model.md
- [x] T005 [US1] Add AvailabilityDto interfaces (ItemAvailabilityDto, EncumbranceAvailabilityDto) and ApprovalDecisionDto to `src/Web/ClientApp/src/features/budgeting/shared/types.ts`
- [x] T006 [US1] Add Create/Update command types (CreateBudgetTypeCommand, UpdateBudgetTypeCommand, ToggleBudgetTypeActiveCommand, CreateFundCommand, UpdateFundCommand) to `src/Web/ClientApp/src/features/budgeting/shared/types.ts`
- [x] T007 [US1] Create `src/Web/ClientApp/src/features/budgeting/shared/index.ts` barrel re-exporting all types, enums, label maps, and command types from types.ts

**Checkpoint**: All budgeting types compile with zero errors. Every enum has an Arabic label entry.

---

### Shared Client (Priority: P1)

**Goal**: Typed API client with cache key factory for all budgeting endpoints

**Independent Test**: Call each client function against the live backend, verify typed responses, verify cache keys are stable and deterministic

- [x] T008 [US1] Create `src/Web/ClientApp/src/features/budgeting/shared/client.ts` with ProblemDetails error type and `handleResponse<T>` helper that surfaces structured errors (400 ValidationProblemDetails, 401, 403, 404, 409)
- [x] T009 [US1] Implement `budgetingKeys` cache key factory in `src/Web/ClientApp/src/features/budgeting/shared/client.ts` with scope prefix `['budgeting']` and per-entity generators (budgetTypes, funds, budgetClassifications, budgets, appropriations, encumbrances) per contracts/frontend-api-contract.md
- [x] T010 [US1] Implement BudgetTypesClient functions in `src/Web/ClientApp/src/features/budgeting/shared/client.ts`: getBudgetTypesList, getBudgetTypeById, createBudgetType, updateBudgetType, toggleBudgetTypeActive
- [x] T011 [US1] Implement FundsClient functions in `src/Web/ClientApp/src/features/budgeting/shared/client.ts`: getFundsList, getFundById, createFund, updateFund, activateFund, deactivateFund
- [x] T012 [US1] Implement BudgetClassificationsClient functions in `src/Web/ClientApp/src/features/budgeting/shared/client.ts`: getBudgetClassificationsTree, getBudgetClassificationById, createBudgetClassification, updateBudgetClassification, toggleBudgetClassificationActive
- [x] T013 [US1] Implement BudgetsClient functions in `src/Web/ClientApp/src/features/budgeting/shared/client.ts`: getBudgetsList, getBudgetById, createBudget, updateBudget, submitBudget, approveBudget, activateBudget, suspendBudget, closeBudget, cancelBudget, getBudgetItemsTree, createBudgetItem, getBudgetItemById, updateBudgetItem, deleteBudgetItem, moveBudgetItem
- [x] T014 [US1] Implement AppropriationsClient functions in `src/Web/ClientApp/src/features/budgeting/shared/client.ts`: getAppropriationsList, getAppropriationById, getAvailabilityForItem, createAppropriation, updateAppropriation, deleteAppropriation, submitAppropriation, approveAppropriation, activateAppropriation, suspendAppropriation, closeAppropriation, cancelAppropriation
- [x] T015 [US1] Implement EncumbrancesClient functions in `src/Web/ClientApp/src/features/budgeting/shared/client.ts`: getEncumbrancesList, getEncumbranceById, getAvailabilityForEncumbrance, createEncumbrance, submitEncumbrance, approveEncumbrance, activateEncumbrance, releaseEncumbrance, liquidateEncumbrancePartial, liquidateEncumbranceFull, closeEncumbrance, cancelEncumbrance, reverseEncumbrance
- [x] T016 [US1] Update `src/Web/ClientApp/src/features/budgeting/shared/index.ts` to re-export client functions and cache key factory

**Checkpoint**: All client functions compile. Cache keys return stable arrays. Error surfacing works for all status codes.

---

## Phase 3: User Story 2 - BudgetType Reference Data Management (Priority: P1)

**Goal**: Budget administrator can view, create, edit, and toggle active state of budget types

**Independent Test**: Navigate to /budgeting/budget-types, perform full CRUD, verify list reflects changes

- [x] T017 [P] [US2] Create `src/Web/ClientApp/src/features/budgeting/budget-types/hooks/useBudgetTypes.ts` with useBudgetTypesList query hook (uses budgetingKeys.budgetTypes.list), useBudgetTypeDetail query hook, useCreateBudgetType mutation (invalidates budgetTypes.all), useUpdateBudgetType mutation (invalidates budgetTypes.all), useToggleBudgetTypeActive mutation (invalidates budgetTypes.all)
- [x] T018 [US2] Create `src/Web/ClientApp/src/features/budgeting/budget-types/pages/BudgetTypesListPage.tsx` with PageHeader (title: "أنواع الموازنة"), ButtonBar with create action (permission-gated BudgetTypes.Create), loading skeleton, error state with retry, empty state
- [x] T019 [US2] Add DataGrid columns to BudgetTypesListPage: Code, Name, ControlMethod (render via budgetControlMethodLabels map), AllowOverrun (render as StatusBadge), IsActive (render as Switch component, permission-gated BudgetTypes.Update)
- [x] T020 [US2] Add FilterBar to BudgetTypesListPage with FilterSearch (search by Code/Name), FilterSelect for ControlMethod (options from budgetControlMethodLabels), FilterSelect for IsActive — all client-side on loaded dataset
- [x] T021 [US2] Create BudgetTypeFormDialog in `src/Web/ClientApp/src/features/budgeting/budget-types/pages/BudgetTypesListPage.tsx` (or separate component file) with Dialog, fields: Code (Input, required), Name (Input, required), Description (Input, optional), ControlMethod (Select with Arabic labels), AllowOverrun (Switch), mode toggle between create/edit
- [x] T022 [US2] Wire create flow: open dialog in create mode → fill form → submit → close dialog → invalidate list → show success toast
- [x] T023 [US2] Wire edit flow: click edit on row → open dialog in edit mode with pre-filled values → submit → close dialog → invalidate list → show success toast
- [x] T024 [US2] Wire toggle active flow: click IsActive Switch → open ConfirmDialog → confirm → call toggle mutation → invalidate list → show success toast. Handle RowVersion conflict with warning toast and refetch.

**Checkpoint**: BudgetTypes page fully functional. CRUD operations work against live backend. Filters are instant. Permission gating hides actions.

---

## Phase 4: User Story 3 - Fund Reference Data Management (Priority: P1)

**Goal**: Budget administrator can view, create, edit, and manage funds with detail view

**Independent Test**: Navigate to /budgeting/funds, perform CRUD, navigate to detail view, verify all data

- [x] T025 [P] [US3] Create `src/Web/ClientApp/src/features/budgeting/funds/hooks/useFunds.ts` with useFundsList query hook (uses budgetingKeys.funds.list), useFundDetail query hook, useCreateFund mutation (invalidates funds.all), useUpdateFund mutation (invalidates funds.all), useActivateFund mutation (invalidates funds.all), useDeactivateFund mutation (invalidates funds.all)
- [x] T026 [US3] Create `src/Web/ClientApp/src/features/budgeting/funds/pages/FundsListPage.tsx` with PageHeader (title: "الأموال"), ButtonBar with create action (permission-gated Funds.Create), loading skeleton, error state with retry, empty state
- [x] T027 [US3] Add DataGrid columns to FundsListPage: FundNumber, FundName, FundType (render via fundTypeLabels), FundCategory (render via fundCategoryLabels), FiscalYear, DefaultRevenueDebitAccount, IsActive
- [x] T028 [US3] Add FilterBar to FundsListPage with FilterSearch (search by FundNumber/FundName), FilterSelect for FundType (options from fundTypeLabels), FilterSelect for FundCategory (options from fundCategoryLabels), FilterSelect for IsActive — all client-side
- [x] T029 [US3] Create FundFormDialog with Dialog, fields: FundNumber (Input, required), FundName (Input, required), FundType (Select), FundCategory (Select), FiscalYear (Combobox, disabled + hint when empty), LegalAuthority (Input), Description (Input, optional), DefaultRevenueDebitAccount (Combobox, disabled + hint when empty), mode toggle between create/edit
- [x] T030 [US3] Wire create flow for Funds: open dialog → fill form → submit → close → invalidate list → success toast. Handle empty Combobox states (disabled + hint).
- [x] T031 [US3] Wire edit flow for Funds: click edit → open dialog pre-filled → submit → close → invalidate list → success toast
- [x] T032 [US3] Create `src/Web/ClientApp/src/features/budgeting/funds/pages/FundDetailPage.tsx` with useParams for id, useFundDetail query, PageHeader with fund name, display all fund fields (FundNumber, FundName, FundType, FundCategory, FiscalYear, LegalAuthority, DefaultRevenueDebitAccount, Description, IsActive), loading/error states

**Checkpoint**: Funds page fully functional. List with filters, create/edit dialogs, detail view at /budgeting/funds/:id. Comboboxes handle empty states.

---

## Phase 5: User Story 4 - Availability Indicator Component (Priority: P2)

**Goal**: Shared component that shows budget availability status (green/amber/red/zero)

**Independent Test**: Render with mock data for each state, verify correct color and amount display

- [x] T033 [P] [US4] Create `src/Web/ClientApp/src/features/budgeting/shared/components/AvailabilityIndicator.tsx` with props: appropriationId (number), query hook that fetches from getAvailabilityForEncumbrance, state machine: available >= 0 → green, available < 0 + ControlMethod None → green, available < 0 + Warning → amber, available < 0 + Blocking → red, empty response → zero-state
- [x] T034 [US4] Add MoneyDisplay integration for amounts, loading skeleton during fetch, RTL layout with logical positioning, dark mode support using design tokens
- [x] T035 [US4] Update `src/Web/ClientApp/src/features/budgeting/shared/index.ts` to re-export AvailabilityIndicator

**Checkpoint**: AvailabilityIndicator renders all 4 states + loading skeleton. Works in dark mode and RTL.

---

## Phase 6: User Story 5 - Routing and Sidebar Navigation (Priority: P1)

**Goal**: Users can reach budgeting pages via sidebar with correct routes and permission gating

**Independent Test**: Load app, verify sidebar group appears, click links navigate to correct pages

- [x] T036 [US5] Register routes in `src/Web/ClientApp/src/app/routes.tsx`: /budgeting/budget-types → BudgetTypesListPage (label: "أنواع الموازنة", permission: BudgetTypes.View), /budgeting/funds → FundsListPage (label: "الأموال", permission: Funds.View), /budgeting/funds/:id → FundDetailPage (label: "تفاصيل الصندوق", permission: Funds.View)
- [x] T037 [US5] Update sidebar "الموازنة" group in `src/Web/ClientApp/src/layouts/navigation.ts`: remove items for classifications, budgets, appropriations, encumbrances (not yet implemented); keep only budget-types and funds items with correct Arabic labels and permission identifiers. Ensure progressive activation pattern is clear for specs #2-#5.
- [x] T038 [US5] Verify sidebar renders "الموازنة" group with only Budget Types and Funds links, active state detection works, RTL layout correct

**Checkpoint**: Sidebar shows "الموازنة" with 2 items. Routes navigate correctly. Permission identifiers present.

---

## Phase 7: User Story 6 - Dark Mode and Design Token Compliance (Priority: P2)

**Goal**: All budgeting pages render correctly in light/dark mode with zero hardcoded values

**Independent Test**: Toggle dark mode on every page, grep for hardcoded colors/fonts

- [x] T039 [US6] Audit all budgeting feature files for hardcoded color values (hex, rgb, hsl) or font-family declarations; replace any found with design token CSS variables or Tailwind token classes
- [x] T040 [US6] Verify DataGrid, Dialog, FilterBar, ConfirmDialog, and all form controls render correctly in dark mode on budgeting pages
- [x] T041 [US6] Verify RTL layout uses logical properties (start/end, not left/right) throughout all budgeting pages and components

**Checkpoint**: Zero hardcoded values. All pages work in light and dark mode. RTL correct throughout.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Final quality pass across all user stories

- [ ] T042 [P] Verify SC-001: run build, confirm zero errors and zero type-checking failures
- [ ] T043 [P] Verify SC-002: perform full CRUD on BudgetTypes and Funds against live backend, confirm list reflects changes immediately
- [ ] T044 [P] Verify SC-003: compare shared/types.ts interfaces field-by-field against backend DTO shapes from contracts/budgeting-dtos.md, confirm zero mismatches
- [ ] T045 [P] Verify SC-004: render AvailabilityIndicator with mock data for all states, confirm visual correctness
- [ ] T046 [P] Verify SC-005: check sidebar "الموازنة" group shows only Budget Types and Funds, links navigate correctly, active state highlights
- [ ] T047 [P] Verify SC-006: test 401 for unauthenticated request, 403 for wrong permission, confirm action buttons hidden without permission
- [ ] T048 [P] Verify SC-007: toggle dark mode on all pages, confirm zero hardcoded color/font values, RTL uses logical properties
- [ ] T049 Run quickstart.md validation scenarios V1-V15 against live backend

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — start immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 — BLOCKS all user stories
- **Phase 3 (US2)**: Depends on Phase 2 complete
- **Phase 4 (US3)**: Depends on Phase 2 complete — can run parallel with Phase 3
- **Phase 5 (US4)**: Depends on Phase 2 complete — can run parallel with Phase 3/4
- **Phase 6 (US5)**: Depends on Phase 3 + Phase 4 complete (routes need pages)
- **Phase 7 (US6)**: Depends on Phase 3 + Phase 4 + Phase 5 complete (audit all pages)
- **Phase 8 (Polish)**: Depends on all phases complete

### User Story Dependencies

- **US1 (Shared Types/Client)**: Phase 2 — no dependencies on other stories
- **US2 (BudgetTypes)**: Phase 3 — depends on US1 only
- **US3 (Funds)**: Phase 4 — depends on US1 only; parallel with US2
- **US4 (AvailabilityIndicator)**: Phase 5 — depends on US1 only; parallel with US2/US3
- **US5 (Routes/Sidebar)**: Phase 6 — depends on US2 + US3 (needs page components)
- **US6 (Dark Mode/Tokens)**: Phase 7 — depends on US2 + US3 + US4 (needs all pages built)

### Within Each User Story

- Hooks before pages (pages consume hooks)
- Pages before routes (routes reference pages)
- Forms before wiring (wiring connects forms to mutations)
- Core implementation before polish

### Parallel Opportunities

- Phase 1: T001 and T002 can run in parallel
- Phase 2: T008-T015 (client functions) can all run in parallel after T003-T007 (types)
- Phase 3 and Phase 4 can run in parallel (different feature modules)
- Phase 5 can run in parallel with Phase 3/4 (shared component, independent)
- Phase 8 verification tasks can all run in parallel

---

## Parallel Example: Phase 2 Client Functions

```bash
# After types are complete (T003-T007), launch all client implementations in parallel:
Task: "Implement BudgetTypesClient in shared/client.ts"
Task: "Implement FundsClient in shared/client.ts"
Task: "Implement BudgetClassificationsClient in shared/client.ts"
Task: "Implement BudgetsClient in shared/client.ts"
Task: "Implement AppropriationsClient in shared/client.ts"
Task: "Implement EncumbrancesClient in shared/client.ts"
```

## Parallel Example: User Stories 2-4

```bash
# After Phase 2 complete, launch US2, US3, US4 in parallel:
Task: "Create useBudgetTypes hooks + BudgetTypesListPage"
Task: "Create useFunds hooks + FundsListPage + FundDetailPage"
Task: "Create AvailabilityIndicator component"
```

---

## Implementation Strategy

### MVP First (US1 + US2)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (shared types + client)
3. Complete Phase 3: User Story 2 (BudgetTypes page)
4. **STOP and VALIDATE**: Test BudgetTypes CRUD against live backend
5. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add BudgetTypes (US2) → Test independently → Deploy/Demo (MVP!)
3. Add Funds (US3) → Test independently → Deploy/Demo
4. Add AvailabilityIndicator (US4) → Test independently → Deploy/Demo
5. Add Routes/Sidebar (US5) → Navigation complete
6. Add Dark Mode audit (US6) → Quality complete

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: BudgetTypes (US2)
   - Developer B: Funds (US3)
   - Developer C: AvailabilityIndicator (US4)
3. All three complete → Routes/Sidebar (US5) + Dark Mode audit (US6)

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Shared types (US1) include ALL 7 entity DTOs even though only BudgetTypes and Funds have pages — this is intentional for the shared layer consumed by specs #2-#5
