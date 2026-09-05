# Test List: Budget Classifications Tree #2 of 5

## Outer loop: acceptance behaviors

One per acceptance criterion in `spec.md`. Each stays red until the feature works
end to end through its real entry point.

No acceptance runner is configured. Outer-loop tests run as component tests in
vitest/jsdom (`@testing-library/react`), not against a real browser or backend.
This is weaker than an E2E test and the list notes this.

| id  | behavior                                                                                     | traces | kind    | state   | test |
| --- | -------------------------------------------------------------------------------------------- | ------ | ------- | ------- | ---- |
| A1  | Navigating to /budgeting/budget-classifications shows a tree with Code, Name, Level per node | AC-1   | example | PENDING |      |
| A2  | Nodes with children show an expand/collapse toggle; children are indented in RTL             | AC-2   | example | PENDING |      |
| A3  | Clicking collapse hides all descendants and updates the toggle icon                          | AC-3   | example | PENDING |      |
| A4  | Typing search text auto-expands ancestors of matching nodes; non-matching branches collapsed  | AC-4   | example | PENDING |      |
| A5  | Selecting IsActive filter shows only matching nodes and their ancestors                      | AC-5   | example | PENDING |      |
| A6  | 5+ level tree renders with correct RTL indentation and no overflow                           | AC-6   | example | PENDING |      |
| A7  | Create dialog opens with Code (required), Name (required), ParentId (optional tree-select), IsActive (switch) | AC-7 | example | PENDING | |
| A8  | Selecting a parent in create dialog creates a child classification                           | AC-8   | example | PENDING |      |
| A9  | Creating without a parent creates a root-level classification                                | AC-9   | example | PENDING |      |
| A10 | Edit dialog pre-fills values; ParentId excludes self and descendants (cycle prevention)      | AC-10  | example | PENDING |      |
| A11 | Empty Code or Name shows validation errors and blocks submission                             | AC-11  | example | PENDING |      |
| A12 | Successful save closes dialog, refreshes tree, shows updated values                          | AC-12  | example | PENDING |      |
| A13 | Clicking IsActive switch opens confirmation dialog                                           | AC-13  | example | PENDING |      |
| A14 | Confirming toggle refreshes tree and shows success toast                                     | AC-14  | example | PENDING |      |
| A15 | RowVersion conflict shows error toast and refetches tree                                     | AC-15  | example | PENDING |      |
| A16 | Sidebar "الموازنة" group shows "التصنيفات المالية" link                                      | AC-16  | example | PENDING |      |
| A17 | Clicking sidebar link navigates to /budgeting/budget-classifications                         | AC-17  | example | PENDING |      |
| A18 | On the classifications page, sidebar link is highlighted as active                           | AC-18  | example | PENDING |      |

## Inner loop: unit behaviors

Grouped by the component from `plan.md` that owns them. Each line names one
observable result.

### `src/features/budgeting/classifications/pages/ClassificationsListPage.tsx` — Tree normalization

| id  | behavior                                                                                    | traces       | kind             | state   | test |
| --- | ------------------------------------------------------------------------------------------- | ------------ | ---------------- | ------- | ---- |
| U1  | Node with parentId referencing non-existent id is moved to root level                       | AC-7, FR-5.7 | example          | DONE    | `src/features/budgeting/__tests__/normalizeTree.test.ts::orphaned parentId` |
| U2  | Node forming a cycle (parentId is self or descendant) is moved to root level                | AC-7, FR-5.7 | example          | DONE    | `src/features/budgeting/__tests__/normalizeTree.test.ts::cycle descendant` |
| U3  | Warning toast shown when any nodes are normalized                                           | FR-5.7       | example          | PENDING |      |
| U4  | Valid tree is returned unchanged                                                            | FR-5.7       | example          | DONE    | `src/features/budgeting/__tests__/normalizeTree.test.ts::valid tree unchanged` |

### `src/features/budgeting/classifications/pages/ClassificationsListPage.tsx` — Expand/collapse state

| id  | behavior                                                                                    | traces       | kind     | state   | test |
| --- | ------------------------------------------------------------------------------------------- | ------------ | -------- | ------- | ---- |
| U5  | Initial expand state: root nodes with children are expanded                                | AC-2, FR-1.2 | example  | DONE    | `ClassificationsListPage.test.tsx::root expanded by default` |
| U6  | Initial expand state: deeper nodes are collapsed                                           | AC-2, FR-1.2 | example  | DONE    | `ClassificationsListPage.test.tsx::deeper collapsed by default` |
| U7  | Clicking expand toggle on collapsed node expands it and shows children                      | AC-2, FR-1.2 | PENDING  |         |
| U8  | Clicking collapse on expanded node hides all descendants                                    | AC-3, FR-1.2 | PENDING  |         |
| U9  | Empty tree renders empty state with create action                                           | Edge Case 4  | DONE    | `ClassificationsListPage.test.tsx::empty state` |
| U10 | Single-node tree (root, no children) renders without expand/collapse toggle                 | Edge Case 5  | PENDING  |         |

### `src/features/budgeting/classifications/pages/ClassificationsListPage.tsx` — Search

| id  | behavior                                                                                    | traces       | kind     | state   | test |
| --- | ------------------------------------------------------------------------------------------- | ------------ | -------- | ------- | ---- |
| U11 | Search matches against Code field                                                           | AC-4, FR-1.3 | example  | PENDING |      |
| U12 | Search matches against Name field                                                           | AC-4, FR-1.3 | example  | PENDING |      |
| U13 | Matching node and all its ancestors are auto-expanded                                       | AC-4, FR-1.3 | example  | PENDING |      |
| U14 | Non-matching branches remain collapsed during search                                        | AC-4, FR-1.3 | example  | PENDING |      |
| U15 | Clearing search returns tree to previous expand/collapse state                              | AC-4, FR-1.3 | example  | PENDING |      |
| U16 | No matches found: tree shows empty state or all collapsed                                   | AC-4, FR-1.3 | example  | PENDING |      |

### `src/features/budgeting/classifications/pages/ClassificationsListPage.tsx` — Filter

| id  | behavior                                                                                    | traces       | kind     | state   | test |
| --- | ------------------------------------------------------------------------------------------- | ------------ | -------- | ------- | ---- |
| U17 | Selecting "نشط" filter shows only active nodes and their ancestors                          | AC-5, FR-1.4 | example  | PENDING |      |
| U18 | Selecting "معطل" filter shows only inactive nodes and their ancestors                       | AC-5, FR-1.4 | example  | PENDING |      |
| U19 | No filter selected: all nodes shown                                                         | AC-5, FR-1.4 | example  | PENDING |      |

### `src/features/budgeting/classifications/hooks/useClassifications.ts` — Mutation hooks

| id  | behavior                                                                                    | traces       | kind     | state   | test |
| --- | ------------------------------------------------------------------------------------------- | ------------ | -------- | ------- | ---- |
| U20 | useCreateClassification invalidates budgetClassifications.all on success                    | FR-2.6       | example  | PENDING |      |
| U21 | useUpdateClassification invalidates budgetClassifications.all on success                    | FR-2.6       | example  | PENDING |      |
| U22 | useToggleClassificationActive invalidates budgetClassifications.all on success              | FR-3.3       | example  | PENDING |      |

### `src/features/budgeting/classifications/pages/ClassificationsListPage.tsx` — Cycle prevention

| id  | behavior                                                                                    | traces       | kind     | state   | test |
| --- | ------------------------------------------------------------------------------------------- | ------------ | -------- | ------- | ---- |
| U23 | getExcludedDescendantIds returns empty set for leaf node                                    | AC-10, FR-2.3| example  | PENDING |      |
| U24 | getExcludedDescendantIds returns self + all descendants for node with children              | AC-10, FR-2.3| example  | PENDING |      |
| U25 | getExcludedDescendantIds returns self only for node with no children                        | AC-10, FR-2.3| example  | PENDING |      |
| U26 | Inactive nodes are NOT excluded from parent tree-select                                     | AC-10, FR-2.3| example  | PENDING |      |

### `src/features/budgeting/classifications/pages/ClassificationsListPage.tsx` — Validation

| id  | behavior                                                                                    | traces       | kind     | state   | test |
| --- | ------------------------------------------------------------------------------------------- | ------------ | -------- | ------- | ---- |
| U27 | Empty Code field blocks submission and shows validation error                               | AC-11, FR-2.5| example  | PENDING |      |
| U28 | Empty Name field blocks submission and shows validation error                               | AC-11, FR-2.5| example  | PENDING |      |

### `src/features/budgeting/classifications/pages/ClassificationsListPage.tsx` — Permission gating

| id  | behavior                                                                                    | traces       | kind     | state   | test |
| --- | ------------------------------------------------------------------------------------------- | ------------ | -------- | ------- | ---- |
| U29 | Create button hidden when BudgetClassifications.Create not granted                          | FR-2.6       | example  | PENDING |      |
| U30 | Edit button hidden when BudgetClassifications.Update not granted                            | FR-2.6       | example  | PENDING |      |
| U31 | IsActive switch hidden when BudgetClassifications.Update not granted                        | FR-3.1       | example  | PENDING |      |

### `src/app/routes.tsx` — Route registration

| id  | behavior                                                                                    | traces       | kind     | state   | test |
| --- | ------------------------------------------------------------------------------------------- | ------------ | -------- | ------- | ---- |
| U32 | Route /budgeting/budget-classifications maps to ClassificationsListPage                     | AC-17, FR-4.1| example  | PENDING |      |

### `src/layouts/navigation.ts` — Sidebar link

| id  | behavior                                                                                    | traces       | kind     | state   | test |
| --- | ------------------------------------------------------------------------------------------- | ------------ | -------- | ------- | ---- |
| U33 | Sidebar "الموازنة" group includes "التصنيفات المالية" link                                   | AC-16, FR-4.2| example  | PENDING |      |
| U34 | Sidebar link has BudgetClassifications.View permission                                      | FR-4.2       | example  | PENDING |      |
| U35 | Sidebar link active state detection works on the classifications page                       | AC-18, FR-4.3| example  | PENDING |      |

## Invariants and edge cases still to place

- Keyboard navigation: Right arrow expands, Left arrow collapses, Up/Down move focus (FR-1.6, SC-005). Requires real DOM focus management; place when component is built.
- RTL overflow: 5+ level tree must not cause horizontal scrolling (SC-002, Edge Case 6). Requires viewport measurement; place when component is built.
- Delete with children: backend returns Restrict error; UI surfaces as error toast (Edge Case 2). Server-side behavior; frontend just shows toast on 409/400.

## Out of scope

- Backend implementation of BudgetClassification endpoints: separate feature, spec #10.
- Shared types/client modifications: consumed as-is from spec #11.
- BudgetItem management: spec #13.
- Other budgeting entities (Budgets, Appropriations, Encumbrances): separate specs.
- Browser-level acceptance tests: no acceptance runner configured (Playwright/Cypress absent). Outer-loop tests run as component tests in vitest/jsdom.

## Verification commands

Copied verbatim from `.specify/memory/tdd-profile.md` at planning time:

- Single test: `npx vitest run {file} -t "{name}"`
- Full suite: `npm test`
- Coverage: null (not installed)
- Mutation: null (not installed)
