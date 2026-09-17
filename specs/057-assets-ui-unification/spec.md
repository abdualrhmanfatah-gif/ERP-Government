# Feature Specification: Assets Management UI Unification (Phase 3)

**Feature Branch**: `057-assets-ui-unification`

**Created**: 2026-09-15

**Status**: Draft

**Input**: User description: "اريد لميزه الاصول" — unify the eight assets-management screens with the approved Phase 1 design language (spec 052), following the spec 053 accounts batch as the precedent. Scope confirmed with the requester: all eight screens (assets list/create/edit/detail and asset groups list/create/edit/detail).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Consistent assets list page (Priority: P1)

An accountant opens الأصول to browse the asset register. They filter by search and status, read lifecycle state at a glance, page through server-side results, and open an asset by activating a row.

**Why this priority**: The register list is the module's most-used screen and this batch's MVP reference for list-recipe conformance.

**Independent Test**: Open `/assets` with real data; confirm the shared page recipe, exactly one primary create action, functional filters (including removal of the duplicate status option), lifecycle status through the approved status component, server pagination with correct totals, distinct empty vs no-results messages, and a persistent retry state on failure.

**Acceptance Scenarios**:

1. **Given** the list has data, **When** the user applies filters, **Then** server-paged results and the total count reflect the filters, and clearing filters restores the full list.
2. **Given** a filter combination yields no rows, **When** the grid renders, **Then** a no-results message distinct from the dataset-empty message appears.
3. **Given** the backend fails, **When** the page loads, **Then** an error state with a retry action replaces the grid and retry refetches.
4. **Given** an asset row, **When** the user activates it, **Then** its detail page opens; identifiers render direction-isolated.

---

### User Story 2 - Consistent asset create/edit form (Priority: P1)

A user creates or edits an asset through one form built from the shared controls, with labeled sections, bound validation, preserved input on failure, and a clear save/cancel hierarchy; the edit page separates loading, not-found, and error states.

**Why this priority**: Create/edit are the primary write journeys and this batch's form-recipe reference.

**Independent Test**: Create with invalid/duplicate data and edit with a rejected payload; confirm field/root error binding (single notification), preserved values, cancel behavior, and the edit-page state machine (loading ≠ not-found ≠ error).

**Acceptance Scenarios**:

1. **Given** an empty submit, **When** validation runs, **Then** field errors bind per the canonical field pattern.
2. **Given** a server rejection, **When** the save fails, **Then** exactly one notification appears (field-bound or form-level Alert) and entered values remain.
3. **Given** the edit route with an unknown id, **When** loading resolves empty, **Then** a not-found state with a way back appears — never while still loading or after a load failure.
4. **Given** the edit page, **When** it renders, **Then** read-only rules for server-managed fields hold and the concurrency token round-trips unchanged.

---

### User Story 3 - Consistent asset detail structure (Priority: P2)

An asset's detail page applies the detail recipe: identity in the page header, lifecycle status in the toolbar, one primary edit action, a read-only presentation with working navigation to edit, and a confirmed deactivate action.

**Why this priority**: Detail conformance completes the register journey and removes the current bespoke modal and no-op save defects.

**Independent Test**: Open an asset in each lifecycle state; confirm header/toolbar, single primary edit, read-only presentation without an operative save, and a confirmed deactivate flow that surfaces failures safely.

**Acceptance Scenarios**:

1. **Given** the detail page, **When** it renders, **Then** edit is the single primary action and deactivate is subordinate and confirmed.
2. **Given** a deactivate conflict (stale concurrency token), **When** the action fails, **Then** a safe message appears and the confirmation does not close silently.
3. **Given** the read-only presentation, **When** the user views it, **Then** no operative save/submit control is offered.

---

### User Story 4 - Consistent asset groups screens (Priority: P2)

Asset group list/create/edit/detail follow the same recipes: functional filters, lifecycle status through the approved status vocabulary, shared forms with bound errors, primary edit, and confirmed lifecycle toggles.

**Why this priority**: Groups are the classification backbone and share every recipe; their current screens carry the most status-vocabulary misuse.

**Independent Test**: Open `/assets/asset-groups`, its create/edit routes, and a group detail; confirm the FilterBar (with the currently dead active filter functional), approved status rendering, page recipes, bound form errors, primary edit with confirmed toggles, and empty states for child groups/assets.

**Acceptance Scenarios**:

1. **Given** the groups list, **When** filters change, **Then** every visible filter control affects the results (no dead controls) in both tree and grid views.
2. **Given** a group detail, **When** it renders, **Then** edit is primary and activate/deactivate is subordinate with confirmation, and empty sections show explicit empty states.
3. **Given** a group form rejected by the server, **When** saving fails, **Then** the failure binds to the form (field or root Alert) with values preserved.

---

### User Story 5 - RTL, light/dark, responsive, keyboard (Priority: P2)

All eight screens meet the Phase 1 acceptance conditions: correct RTL, both modes, reflow at 320 CSS px equivalent / 400% zoom, keyboard-operable workflows with visible focus, and meaning that survives grayscale/forced-colors.

**Why this priority**: Phase 1 established these acceptance conditions; each batch must prove them for its own scope.

**Independent Test**: Run the eight screens through the Phase 1 review matrix and record results in the evidence log.

**Acceptance Scenarios**:

1. **Given** narrow viewport or 400% zoom, **When** the assets screens render, **Then** essential information and actions remain reachable with no page-level two-dimensional scrolling (inherently 2-D tables excepted).
2. **Given** RTL, **When** any screen renders, **Then** order, alignment, and action placement are correct with no physical-direction styling.
3. **Given** keyboard-only use, **When** completing the primary workflows (filter list, create, edit, deactivate group), **Then** every control is reachable with a visible focus indicator.
4. **Given** grayscale/forced-colors, **When** statuses, validation, and action hierarchy render, **Then** meaning survives without color.

---

### Edge Cases

- The asset register pages server-side: totals, filters, and the no-results state must agree at all times, and the current next-page heuristic must not hide a full last page or double-count.
- The status filter must not present duplicate options; every filter control (including the groups list's currently unwired active filter) must affect results or be removed.
- A newly created asset has server-managed identity/rules: the form must not require users to invent server-managed values.
- Editing with a stale concurrency token surfaces state/concurrency conflicts as bound errors, not silent failures.
- Deactivating a group with active children/accounts is rejected with a safe message and does not close the confirmation silently (server rule preserved).
- Asset lifecycle includes maintenance/disposal/write-off states: every value — including unknown future values — renders a defined status presentation, never an unstyled or color-only signal.
- Deep group trees in RTL must keep logical indentation and not clip long Arabic names.
- Loading → error → retry recovering in the same page must not leave stale or overlapping states (including the groups list's currently unreachable skeleton path).
- Read-only detail presentations must not offer operative save controls; a no-op submit path must not exist.
- Labels and headings must contain valid Arabic text; the current corrupted mixed-script strings must be corrected.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The eight assets-management screens (assets list, create, edit, detail; asset groups list, create, edit, detail) MUST conform to the Phase 1 page recipes, component contracts, status vocabulary, and density rules from spec 052. (Principle X)
- **FR-002**: Assets and asset groups MUST render lifecycle state through `StatusBadge` with a feature-boundary mapping to the approved base roles, including a defined fallback for unknown values; generic `Badge` MUST NOT be used for lifecycle state on any migrated screen. (Principle X)
- **FR-003**: The asset create/edit form and the asset-group create/edit forms MUST use the canonical field pattern with shared controls, field-bound validation feedback, and a visible form-level fallback via the shared error contract for server rejections; exactly one notification owner per failed action; create/edit MUST remain dedicated routes (no new dialogs or routes). (Principles X, XIII)
- **FR-004**: Asset and asset-group forms MUST validate with shared schemas at the feature boundary (the correct create vs update schema per mode) and MUST preserve entered values on failure. (Principle X)
- **FR-005**: Loading, empty (dataset vs no-results), not-found, and error states MUST be distinguishable and follow the recipe scope rules on every migrated screen; edit/detail screens MUST NOT display not-found while data is still loading or has failed to load. (Principle X)
- **FR-006**: Action hierarchy MUST follow the recipes: one primary action per context; on asset and group detail pages edit MUST be the single primary action; lifecycle toggles MUST be secondary (`outline`) when activating and destructive with confirmation when deactivating. (Principle X)
- **FR-007**: Lifecycle toggles (asset deactivate, group activate/deactivate) MUST round-trip the optimistic-concurrency token and surface failures with safe messages. (Principles X, IX)
- **FR-008**: List surfaces MUST render typed columns/grids (no `any` column typing), and identifiers (codes, paths) MUST be direction-isolated for RTL. (Principle X)
- **FR-009**: The migrated pages and their feature components MUST contain zero references to undefined design tokens, zero physical-direction CSS utilities, and zero hard-coded color palette values (verified with the Phase 1 `design:usage` scan and static checks). (Principle X)
- **FR-010**: All migrated screens MUST preserve existing endpoints, payload shapes (including concurrency tokens), permission checks, and business rules; any necessary behavioral change MUST be explicitly justified and recorded. (Principles III, IX, XII)
- **FR-011**: The migrated screens MUST render correctly in RTL and in both light and dark modes with identical semantic meaning. (Principle X)
- **FR-012**: The migrated screens MUST meet the SC-006 reflow condition of spec 052 (320 CSS px equivalent, 400% zoom from 1280 CSS px) without loss of essential content or actions. (Principle X)
- **FR-013**: Primary workflows on the migrated screens MUST be keyboard operable with visible focus indication in both modes. (Principle X)
- **FR-014**: Contrast in the migrated screens MUST meet WCAG 2.2 AA (normal text ≥ 4.5:1; large text and non-text indicators ≥ 3:1) in both modes. (Principle X)
- **FR-015**: Visual/interaction evidence for the eight screens MUST be recorded in a structured review log in this feature folder using the Phase 1 evidence schema. (Principle XI, subject to the frontend governance override)
- **FR-016**: The deferred inconsistency list (`docs/ui-patterns.md` backlog and spec 052 `scope-inventory.md`) MUST be updated in this change: items resolved for assets screens are marked fixed; the rest stay deferred. (Principle X)
- **FR-017**: No screens outside the eight in scope may be migrated or restyled by this specification; shared-special-case components touched only to serve these screens MUST preserve their other consumers' behavior. (Principle XII)
- **FR-018**: The assets list MUST keep its server-side pagination data pattern (no fetch-all rewrite, no API changes); the groups list MUST keep its tree/grid view modes. Form option lists (categories, locations, custodians, cost centers, funds) MUST NOT be newly populated by this batch. (Principles IX, XII)
- **FR-019**: Visible controls MUST be functional: no dead filter state or duplicate/conflicting options; the groups list's active filter MUST either work or be removed; labels and headings MUST contain valid Arabic text with no corrupted characters. (Principle X)
- **FR-020**: The navigation entry for the assets screens MUST reference the permission identifier actually defined and used by their routes, so the batch's screens are consistently reachable; no other authorization behavior changes. (Principles X, VII)

### Key Entities

- **Migrated screen**: one of the eight assets-management pages with its feature components; carries a conformance status (list/form/detail recipe) and a validation-evidence reference.
- **Assets scope inventory**: the resolved-vs-deferred inconsistency list for this batch, extending the Phase 1 inventory.
- **Review evidence record**: same schema as spec 052 (`data-model.md` §6): interface × condition rows with result, issues, resolution, reviewer, date.
- **Form schema**: the asset/asset-group create and update validation schemas at the feature boundary (shared with form defaults and server-error mapping).
- **Status base-role map**: the feature-boundary mapping from asset/group lifecycle values to approved status base roles, with a fallback for unknown values.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001 — Consistency**: Evaluation of the eight migrated screens against the approved recipes finds no unexplained visual or behavioral inconsistencies within scope.
- **SC-002 — Primary-action clarity**: An independent reviewer correctly identifies the primary action on each migrated screen and is not confused by secondary actions.
- **SC-003 — Semantic consistency**: Every lifecycle state on the migrated screens uses the approved status vocabulary with one meaning across list, detail, and badges, including maintenance/disposal/write-off states.
- **SC-004 — Color independence**: No critical status, validation, or interaction meaning on the migrated screens depends on color alone.
- **SC-005 — Text contrast**: Normal text ≥ 4.5:1 and large text/non-text indicators ≥ 3:1 in both modes across the eight screens.
- **SC-006 — Responsive usability**: Essential information and functionality remain reachable at the spec 052 reflow condition (320 CSS px equivalent; 400% zoom).
- **SC-007 — Keyboard usability**: Primary workflows can be completed with a keyboard alone with visible focus.
- **SC-008 — Light and dark mode consistency**: Semantic meaning, hierarchy, readability, and interaction clarity hold in both modes.
- **SC-009 — Form error quality**: Every server rejection path exercised on the forms surfaces exactly one notification and either a bound field error or a visible form-level message, with values preserved.
- **SC-010 — Token and style integrity**: `npm run design:usage` reports zero unknown token references in the migrated pages and their feature components, and static checks find zero physical-direction utilities and zero hard-coded palette values in scope.
- **SC-011 — Real-interface validation**: The approach is validated on the eight screens using real backend data flows with recorded evidence.
- **SC-012 — Business behavior preservation**: Endpoints, payloads, permissions, and business rules are unchanged (the navigation identifier correction aside, recorded); existing regression suites remain the gate.
- **SC-013 — List state integrity**: The assets list's server pagination, filters, totals, empty and no-results states agree at all times; the groups list's tree/grid modes and filters agree with the same empty/no-results rules.

## Assumptions

- Spec 052 (Phase 1) is implemented: contracts, recipes, densities, status vocabulary, and the dev-only gallery are the baseline this batch consumes.
- The assets and asset-groups backend contracts (specs 054/055) are unchanged; no API, permission, or schema work is part of this spec.
- Create/edit remain dedicated routes for both assets and asset groups (no dialogs), matching the existing route structure.
- The batch stays frontend-only; form option lists already exist as dormant props and are not populated in this batch.
- Frontend governance: no automated frontend tests are added; acceptance evidence is manual and recorded.
- Items in the deferred backlog that do not affect these eight screens remain deferred.

## Dependencies

- **Spec 052** contracts + gallery + `docs/ui-patterns.md` (spec Dependencies section of 052).
- **Specs 054/055** (assets groups + register) — the screen definitions and API contracts this batch must preserve.
- **AGENTS.md** frontend conventions (RHF + Zod schemas at the feature boundary; domain-prefixed shared components).
- **docs/error-handling.md** (spec 051 contract) for form error binding and notification ownership.
- **Existing assets code**: `features/assets/assets/**`, `features/assets/asset-groups/**` and their hooks, plus `routes.tsx` asset entries and `navigation.ts`.

## Scope Boundaries

### In Scope

- The eight assets-management screens and the shared components they directly use.
- Recipe conformance (list/form/detail), status semantics and base-role mapping, density, action hierarchy, state handling, typed columns, RTL/dark/responsive/keyboard review.
- Form validation and server-error binding through the shared error contract; correct per-mode schemas.
- Replacement of hard-coded palette styling with tokens/shared components in scope.
- The navigation permission identifier correction for the assets entry.
- Evidence log and backlog/inventory updates for this batch.

### Out of Scope

- Backend endpoints, DTOs, permissions, or business rules.
- Asset acquisition/activation (spec 056) screens or flows, depreciation, disposal, movements, physical counts, revaluations, and other asset sub-features.
- Other modules' screens (journal entries, journals, templates, recurring entries, etc.).
- Populating form option lists from new queries/endpoints.
- Fixing deferred backlog items that do not touch the eight screens.

## Risk and Mitigation

| Risk | Impact | Mitigation |
|------|--------|------------|
| Palette-to-token migration changes visual hierarchy unintentionally | Medium | Use shared components' variants; verify light/dark evidence per screen |
| Migrating raw tables to the shared grid changes DOM/interaction | Medium | Preserve row navigation and server paging; verify keyboard and mobile-card behavior |
| Server pagination heuristics or duplicate filter options regress | High | Keep data patterns; correct options; verify totals/empty/no-results against the real API |
| Shared form behavior affects the read-only detail use | Medium | Define a read-only presentation with no operative submit; verify the no-op path is removed |
| Scope drift into other asset sub-features | Medium | Only the eight screens; other findings go to the deferred inventory |

## Verification Governance

- Frontend gates: `npm run generate-tokens`, `npm run design:usage`, `npm run lint`, `npm run build`, `npm run design:lint`/`design:check`.
- Manual visual/interaction evidence recorded per screen and condition (FR-015), per the Phase 1 evidence schema.
- Backend behavior unchanged; the five backend test projects remain the convergence gate once the working tree compiles.
- Constitution check at the plan gate and after design; contract-affecting changes escalate to a decision record.
