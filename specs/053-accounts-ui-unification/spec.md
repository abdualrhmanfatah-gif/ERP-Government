# Feature Specification: Accounts Management UI Unification (Phase 2)

**Feature Branch**: `053-accounts-ui-unification`

**Created**: 2026-09-14

**Status**: Draft

**Input**: User description: "Start the Phase 2 specification for the accounts management unit (إدارة الحسابات): migrate its screens — accounts list, account create/edit form, account detail, and account groups list/detail — to the approved unified visual language from spec 052 (recipes, contracts, gallery, densities, status vocabulary) without changing business behavior, and validate with real data flows."

## Clarifications

### Session 2026-09-14

- Q: On the account and account-group detail pages, which action should be the single primary action? → A: Edit is the single primary action (mirroring the Phase 1 Parties detail decision); activate/deactivate stays secondary (`outline`) or destructive with confirmation.
- Q: Should account-group creation and editing stay as dialogs on the groups list page, or become dedicated pages? → A: Keep the existing dialogs; align their contents to the canonical field pattern, Zod validation, field-bound server errors, and the action hierarchy.
- Q: Should the accounts list keep its current fetch-all data pattern with client-side search and no pagination, or gain server-side paging/search? → A: Keep the current data pattern (fetch-all + client-side search, no pagination; presentation-only alignment, no API change). The list recipe's pagination slot applies only to screens that already page server-side.
- Q: How should the account detail's sub-accounts tab be presented while that feature is not built? → A: Build the sub-accounts view. It derives direct child accounts from the existing accounts data (`AccountDto.ParentId`/`Level`, already fetched by the accounts list) with no backend/API changes, and follows the list presentation rules with navigation to each child; deeper multi-level tree exploration stays out of scope.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Consistent accounts list page (Priority: P1)

A user working in the chart of accounts should encounter the same list-page structure used elsewhere in the system: page heading, one primary create action, filters (search, group, active state, postable), a data grid with loading/empty/no-results/error states — and should see lifecycle status through the shared semantic status display. The accounts list keeps its current fetch-all data pattern and does not paginate.

**Why this priority**: The accounts list is the entry point of the module and the mandatory list-page validation target for Phase 2.

**Independent Test**: Open `/accounting/accounts` with the real backend and confirm the page matches the approved list recipe: single primary create action, compact filter controls, typed grid columns, distinct empty vs no-results messages, persistent error state with retry, `StatusBadge` for active/inactive, and no undefined design tokens in the page or its grid component.

**Acceptance Scenarios**:

1. **Given** a user opens the accounts list, **When** the page loads, **Then** title, primary create action, filters, and grid follow the approved list recipe order and weights (spec 052 `contracts/page-recipes.md` §1); the pagination slot is absent because the screen does not page server-side.
2. **Given** the list query is loading, **When** the user views the page, **Then** loading is scoped to the grid (not a full-page spinner).
3. **Given** no accounts exist, **When** the list renders, **Then** an empty-dataset message is shown; **Given** filters match nothing including the search box, **Then** a distinct no-results message is shown.
4. **Given** the list query fails, **When** the page renders, **Then** a persistent error state with retry is shown, distinct from empty and loading.
5. **Given** an account's active state renders, **When** reviewed, **Then** it uses `StatusBadge` (`active`/`inactive`) and never a generic `Badge` color variant.
6. **Given** a user selects a row, **When** navigating to the account, **Then** row actions follow the tertiary (`ghost`) action role.

---

### User Story 2 - Consistent account create/edit form (Priority: P1)

A user creating or editing an account should see the approved form composition: labeled sections, canonical fields with required indicators and inline validation, server errors bound to the correct fields with a visible form-level fallback, and one primary save plus a secondary cancel — regardless of whether the account is new or existing.

**Why this priority**: The form is where financial master data is entered; Phase 1 found toast-only error surfacing and ad-hoc submit typing here. This is the mandatory form validation target for Phase 2.

**Independent Test**: Create an account with an invalid/duplicate code and edit an existing account with a rejected payload; confirm field-level errors appear on the correct fields (or a visible form-level alert when untargeted), entered values are preserved, and the save/cancel hierarchy matches the recipe.

**Acceptance Scenarios**:

1. **Given** a user opens create or edit, **When** the form renders, **Then** fields are grouped into labeled sections in task order and use the canonical field pattern (control owns label/required/error/ARIA) with comfortable density.
2. **Given** the user submits invalid input, **When** validation runs, **Then** errors appear on the affected fields without losing entered values, and a form-level fallback exists for untargeted failures.
3. **Given** the server rejects the submission (duplicate code, concurrency conflict, forbidden), **When** the response is handled, **Then** the error is bound through the shared error contract (`handleApiError`) exactly once, with no duplicate toasts.
4. **Given** the form is submitting, **When** the user views the actions, **Then** save is the single primary action, disabled with a loading indication, and cancel is secondary/tertiary.
5. **Given** edit mode loads, **When** the account is still loading, **Then** a loading state is shown; **Given** the account does not exist, **Then** a not-found state with a return action is shown; **Given** loading failed, **Then** an error state with retry is shown — these three are never conflated.
6. **Given** an account code or other identifier is displayed, **When** read in RTL, **Then** it is direction-isolated (LTR) and not visually reordered.

---

### User Story 3 - Consistent account detail structure (Priority: P2)

A user opening an account should immediately identify the account identity (name + code), its status, its key accounting attributes, the primary action, and secondary/related information (sub-accounts, activity) without competing elements.

**Why this priority**: Detail screens carry accounting decisions; Phase 1 defined the detail recipe and Phase 2 applies it to the accounts module.

**Independent Test**: Open an account detail page and confirm identity/status prominence, a single primary action, secondary information subordinate, section structure matching the approved detail recipe, and the sub-accounts tab listing direct children (or the explicit no-children state) with working navigation.

**Acceptance Scenarios**:

1. **Given** a user opens an account detail, **When** the page renders, **Then** the account name is the page identity, the code is shown consistently with the list, and the active state uses `StatusBadge`.
2. **Given** the detail offers navigation to edit, **When** actions render, **Then** edit is the single primary action and any activate/deactivate toggle is secondary (`outline`) or destructive with confirmation.
3. **Given** the page contains tabs/sections (details, sub-accounts), **When** the user views them, **Then** the section pattern and headers match the detail recipe and no placeholder is passed off as a complete feature.
4. **Given** the sub-accounts tab, **When** the account has direct child accounts, **Then** a compact table of children (code, name, group, state) renders from the existing accounts data with navigation to each child's detail; **Given** there are no children, **Then** an explicit no-children empty state is shown, distinct from loading and error states.
5. **Given** the sub-accounts tab is loading or fails, **When** the user views it, **Then** the state follows the list rules (compact loading, persistent error with retry) and never shows the no-children message for a failure.
4. **Given** the account loads or fails, **When** the page renders, **Then** loading and error states follow the recipe (no empty title or wrong-state flashes).

---

### User Story 4 - Consistent account groups list and detail (Priority: P2)

A user managing the chart-of-accounts hierarchy should find the same list and detail patterns as accounts: filter bar with the shared search control, grid or tree with the correct loading/empty/error states, correct semantic status, typed columns, and a detail page with the approved structure.

**Why this priority**: The groups screens share the same recipe expectations but currently diverge (raw input in the filter bar, `closed` used for inactive, plain-text errors).

**Independent Test**: Open `/accounting/account-groups` and a group detail; confirm filter controls, status semantics, error/empty handling, typed rows, and detail structure match the approved recipes.

**Acceptance Scenarios**:

1. **Given** the groups list renders, **When** the filter bar is reviewed, **Then** it uses `FilterSearch`/`FilterSelect` (compact density) and not raw form inputs.
2. **Given** a group's active state renders, **When** reviewed, **Then** inactive uses `StatusBadge variant="inactive"` and never `closed`/generic `Badge`.
3. **Given** the groups query fails, **When** the list renders, **Then** a persistent error state with retry is shown instead of a plain error sentence.
4. **Given** the groups list is paginated, **When** the user changes pages, **Then** pagination and totals follow the list recipe.
5. **Given** a group detail opens, **When** reviewed, **Then** identity, status, primary action, and related sections follow the detail recipe, with no custom back button where `Page.onBack` applies.
6. **Given** the group create/edit dialogs remain the editing surface, **When** validation fails, **Then** feedback follows the canonical field pattern with field-bound server errors, and the confirmation for deactivation is labeled by outcome, not color alone.

---

### User Story 5 - RTL, light/dark, responsive, and keyboard acceptance for the accounts screens (Priority: P2)

A user working in RTL, in light or dark mode, on a small screen, or with a keyboard must be able to use all six accounts-management screens with the same semantics and hierarchy as elsewhere in the system.

**Why this priority**: Phase 1 established these acceptance conditions; each batch of migrated pages must prove them for its own scope.

**Independent Test**: Run the six screens through the Phase 1 review matrix (light, dark, RTL, 320 CSS px equivalent, 400% zoom, keyboard, grayscale/forced-colors) and record results in the evidence log.

**Acceptance Scenarios**:

1. **Given** narrow viewport or 400% zoom, **When** the accounts screens render, **Then** essential information and actions remain reachable with no page-level two-dimensional scrolling (inherently 2-D grids excepted).
2. **Given** RTL, **When** any accounts screen renders, **Then** order, alignment, and action placement are correct with no physical-direction styling.
3. **Given** keyboard-only use, **When** completing the primary workflows (filter list, create account, edit account, toggle group), **Then** every control is reachable with a visible focus indicator.
4. **Given** grayscale/forced-colors, **When** statuses, validation, and action hierarchy render, **Then** meaning survives without color.

---

### Edge Cases

- The accounts list has more than one page of results only after filtering: pagination and no-results must not conflict or double-count.
- A newly created account's code is generated by the server: the form must not require the user to invent a code, and the success path must navigate consistently.
- Editing an account that another user changed: the optimistic-concurrency token round-trips and a conflict surfaces as a bound error, not a silent failure.
- A group with active children cannot be deactivated: the rejection is explained with a safe message and does not close the confirmation silently.
- Sub-accounts or group trees contain deep levels in RTL: indentation must be logical and not clip long Arabic names.
- Unknown future account types or group types must not render unstyled text or crash the grid.
- Loading, then error, then retry recovering data in the same page must not leave stale or overlapping states.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The six accounts-management screens (accounts list, create, edit, detail; groups list, detail) MUST conform to the Phase 1 page recipes, component contracts, status vocabulary, and density rules from spec 052. (Principle X)
- **FR-002**: The accounts list and account groups MUST render lifecycle state through `StatusBadge` with the correct base role (`active`/`inactive`); generic `Badge` MUST NOT be used for lifecycle state on any migrated screen. (Principles X)
- **FR-003**: The account create/edit form and the account-group create/edit dialogs MUST use the canonical field pattern and provide field-bound validation feedback plus a visible form-level fallback via the shared error contract for server rejections; exactly one notification owner per failed action. Account-group creation/editing MUST remain dialog-based on the groups list page (no new routes). (Principles X, XIII)
- **FR-004**: Account and group forms MUST validate with the shared Zod schema at the feature boundary and preserve entered values on failure. (Principle X)
- **FR-005**: Loading, empty (dataset vs no-results), not-found, and error states MUST be distinguishable and follow the recipe scope rules on every migrated screen; the edit page MUST NOT display a not-found state while data is still loading or has failed to load. (Principle X)
- **FR-006**: Action hierarchy MUST follow the recipes: one primary action per context, secondary actions subordinate, destructive/lifecycle toggles labeled by outcome and confirmed. On account and account-group detail pages, edit MUST be the single primary action; any activate/deactivate toggle MUST be secondary (`outline`) or destructive with confirmation. (Principle X)
- **FR-007**: Lifecycle toggles (group activate/deactivate and any account state action) MUST round-trip the optimistic-concurrency token and surface failures with safe messages. (Principles X, IX)
- **FR-008**: Grid columns in the migrated screens MUST be typed (no `DataGridColumn<any>`), and identifiers (codes, paths) MUST be direction-isolated for RTL. (Principle X)
- **FR-009**: The migrated pages and their feature components MUST contain zero references to undefined design tokens (verified with the Phase 1 `design:usage` scan) and zero physical-direction CSS utilities. (Principle X)
- **FR-010**: All migrated screens MUST preserve existing endpoints, payload shapes, permission checks, and business rules; any necessary behavioral change MUST be explicitly justified and recorded. (Principles III, IX, XII)
- **FR-011**: The migrated screens MUST render correctly in RTL and in both light and dark modes with identical semantic meaning. (Principle X)
- **FR-012**: The migrated screens MUST meet the SC-006 reflow condition of spec 052 (320 CSS px equivalent, 400% zoom from 1280 CSS px) without loss of essential content or actions. (Principle X)
- **FR-013**: Primary workflows on the migrated screens MUST be keyboard operable with visible focus indication in both modes. (Principle X)
- **FR-014**: Contrast in the migrated screens MUST meet WCAG 2.2 AA (normal text ≥ 4.5:1; large text and non-text indicators ≥ 3:1) in both modes. (Principle X)
- **FR-015**: Visual/interaction evidence for the six screens MUST be recorded in a structured review log in this feature folder using the Phase 1 evidence schema. (Principle XI, subject to the frontend governance override)
- **FR-016**: The deferred inconsistency list (`docs/ui-patterns.md` backlog and spec 052 `scope-inventory.md`) MUST be updated in this change: items resolved for accounts screens are marked fixed; the rest stay deferred. (Principle X)
- **FR-017**: No screens outside the six in scope may be migrated or restyled by this specification; shared-special-case components touched only to serve these screens MUST preserve their other consumers' behavior. (Principle XII)
- **FR-018**: The accounts list MUST keep its current fetch-all + client-side search data pattern without introducing pagination or API changes; pagination is applied only on screens that already page server-side (the account groups list). (Principles IX, XII)
- **FR-019**: The account detail's sub-accounts tab MUST display the account's direct child accounts derived client-side from the existing accounts data (`ParentId`, `Level`) — no new endpoints, queries, or business rules — and MUST follow the list presentation rules (compact density, typed rows, `StatusBadge`, distinct empty/loading/error states) with navigation to each child's detail. Deeper multi-level tree exploration is out of scope. (Principles IX, X)

### Key Entities

- **Migrated screen**: one of the six accounts-management pages with its feature/shared components; carries a conformance status (list/form/detail recipe) and a validation-evidence reference.
- **Accounts scope inventory**: the resolved-vs-deferred inconsistency list for this batch, extending the Phase 1 inventory.
- **Review evidence record**: same schema as spec 052 (`data-model.md` §6): interface × condition rows with result, issues, resolution, reviewer, date.
- **Form schema**: the account/group validation schema at the feature boundary (shared with the form defaults and server-error mapping).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001 — Consistency**: Evaluation of the six migrated screens against the approved recipes finds no unexplained visual or behavioral inconsistencies within scope.
- **SC-002 — Primary-action clarity**: An independent reviewer correctly identifies the primary action on each migrated screen and is not confused by secondary actions.
- **SC-003 — Semantic consistency**: Every lifecycle state on the migrated screens uses the approved status vocabulary with one meaning across list, detail, and any badges.
- **SC-004 — Color independence**: No critical status, validation, or interaction meaning on the migrated screens depends on color alone.
- **SC-005 — Text contrast**: Normal text ≥ 4.5:1 and large text/non-text indicators ≥ 3:1 in both modes across the six screens.
- **SC-006 — Responsive usability**: Essential information and functionality remain reachable at the spec 052 reflow condition (320 CSS px equivalent; 400% zoom).
- **SC-007 — Keyboard usability**: Primary workflows can be completed with a keyboard alone with visible focus.
- **SC-008 — Light and dark mode consistency**: Semantic meaning, hierarchy, readability, and interaction clarity hold in both modes.
- **SC-009 — Form error quality**: Every server rejection path exercised on the forms surfaces exactly one notification and either a bound field error or a visible form-level message, with values preserved.
- **SC-010 — Token integrity**: `npm run design:usage` reports zero unknown token references in the migrated pages and their feature components.
- **SC-011 — Real-interface validation**: The approach is validated on the six screens using real backend data flows with recorded evidence.
- **SC-012 — Business behavior preservation**: Endpoints, payloads, permissions, and business rules are unchanged; existing regression suites remain the gate.
- **SC-013 — Sub-accounts view**: For an account with direct children, the sub-accounts tab lists every direct child with working navigation to its detail; for a leaf account it shows the explicit no-children state, and loading/error states are never confused with either.

## Assumptions

- Spec 052 (Phase 1) is implemented: contracts, recipes, densities, status vocabulary, and the dev-only gallery are the baseline this batch consumes.
- The accounts and account-groups backend contracts are unchanged; no API, permission, or schema work is part of this spec.
- The account code is entered by the user and validated for uniqueness server-side; it is read-only when editing an existing account.
- The sub-accounts view is built from the existing accounts data (`ParentId`/`Level` already returned by the accounts query); no new endpoints or business rules are introduced, and deeper multi-level tree exploration remains out of scope.
- Frontend governance: no automated frontend tests are added; acceptance evidence is manual and recorded.
- Items in the deferred backlog that do not affect these six screens remain deferred.

## Dependencies

- **Spec 052** contracts + gallery + `docs/ui-patterns.md` (spec Dependencies section of 052).
- **AGENTS.md** frontend conventions (RHF + Zod schemas at the feature boundary; domain-prefixed shared components).
- **docs/error-handling.md** (spec 051 contract) for form error binding and notification ownership.
- **Existing accounting code**: `features/accounting/pages/*`, `features/accounting/account-groups/*`, `src/components/AccountingAccountGrid.tsx`, `AccountingAccountForm.tsx`, `AccountingAccountDetail.tsx`, `AccountingAccountGroupForm.tsx`, `AccountingGroupTree.tsx`, and their hooks.

## Scope Boundaries

### In Scope

- The six accounts-management screens and the shared accounting components they directly use.
- Recipe conformance (list/form/detail), status semantics, density, action hierarchy, state handling, typed columns, RTL/dark/responsive/keyboard review.
- Form validation and server-error binding through the shared error contract.
- The account detail sub-accounts view (direct children from existing data, list presentation rules, navigation).
- Evidence log and backlog/inventory updates for this batch.

### Out of Scope

- Journal entries, journals, templates, and recurring entries screens (later batches).
- Backend endpoints, DTOs, permissions, or business rules.
- Backend/queries for the chart of accounts, schema changes, or application-wide restyling. (The sub-accounts view is in scope but derives from existing data.)
- Fixing deferred backlog items that do not touch the six screens.

## Risk and Mitigation

| Risk | Impact | Mitigation |
|------|--------|------------|
| Shared accounting components are used outside the six screens | Medium | Preserve public props/behavior; verify other consumers after edits |
| Form migration changes payload shapes | High | Keep command mapping unchanged; regression-check create/edit flows against the real API |
| Grid/tree presentation changes hide data at narrow widths | Medium | Follow the list recipe's compact representation; verify reflow condition |
| Scope drift into journal/other accounting screens | Medium | Only the six screens; other findings go to the deferred inventory |

## Verification Governance

- Frontend gates: `npm run generate-tokens`, `npm run design:usage`, `npm run lint`, `npm run build`, `npm run design:lint`/`design:check`.
- Manual visual/interaction evidence recorded per screen and condition (FR-015), per the Phase 1 evidence schema.
- Backend behavior unchanged; the five backend test projects remain the convergence gate once the working tree compiles.
- Constitution check at the plan gate and after design; contract-affecting changes escalate to a decision record.
