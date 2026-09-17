# Feature Specification: Unified UI and Visual Language

**Feature Branch**: `052-unified-ui-language`

**Created**: 2026-09-14

**Status**: Draft

**Input**: User description: "Unify the user experience and visual language across ERP-Government so users can move between modules and complete administrative and financial tasks without unnecessary inconsistencies in component appearance, color meaning, action hierarchy, page structure, or interaction patterns. Phase 1: identify and resolve foundational inconsistencies in the existing design system; establish clear semantic and visual roles; standardize the core shared components; define composition rules for recurring page types; provide a visual reference for approved component states; validate the result against one real list page and one real create/edit form using real ERP-Government behavior and data flows."

## Clarifications

### Session 2026-09-14

- Q: Which screens should be the Phase 1 validation targets and receive the unification changes? → A: The Parties list page and the party create/edit form (the documented frontend exemplar: simple CRUD, lowest migration risk, demonstrates both required page types).
- Q: How should the internal visual reference required by FR-015/SC-010 be made available? → A: Development-only route (expand the existing dev-only component gallery; no production exposure).
- Q: Should the unified visual language define different default content densities for different page types? → A: Yes — exactly two approved densities (compact / comfortable); list and document/transaction detail pages default compact, create/edit forms default comfortable, and each recipe states its default.
- Q: Which viewport width and zoom/reflow level define the responsive-usability acceptance check? → A: A viewport equivalent to 320 CSS px width, including 400% browser zoom from a 1280 CSS px viewport, without loss of content or functionality and without page-level two-dimensional scrolling — except content that inherently requires two-dimensional layout such as complex data tables or diagrams.
- Q: Should Phase 1 preserve every existing status-badge variant as an approved alias of the six base status roles, or consolidate overlapping variants? → A: Preserve the six base roles as the approved vocabulary and document every existing variant as an alias of exactly one base role with one canonical meaning; Phase 1 remaps or removes only conflicting aliases found within the validated scope; full consolidation follows later module migration.
- Q: What form should the recorded visual/interaction review evidence take? → A: A structured review log committed in the feature folder: per validated interface and the visual reference, one row per condition (light, dark, RTL, 320 CSS px, 400% zoom, keyboard, grayscale/forced-colors) with result, issues found, resolution, and reviewer/date; before/after screenshots are encouraged but optional.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Consistent list pages across modules (Priority: P1)

A user moving between list pages in different modules (parties, budgets, documents, inventory) should encounter the same predictable structure: page heading, primary and secondary actions, filters, data presentation, pagination, loading, empty, and error states. The user should be able to begin working on any list page without learning a new interaction pattern per module.

**Why this priority**: List pages are the most frequently visited screens; inconsistency there multiplies across every module. This is also one of the two mandatory Phase 1 validation targets.

**Independent Test**: Can be fully tested by opening the validated Parties list page alongside other list pages and confirming each structural element (heading, actions, filters, grid, pagination, loading/empty/error) follows the approved list recipe and behaves predictably.

**Acceptance Scenarios**:

1. **Given** a user on any list page, **When** the page loads, **Then** the title, primary create/action button, filters, data grid, and pagination appear in the same relative order and visual weight defined by the list recipe.
2. **Given** a list query is loading, **When** the user views the page, **Then** the loading treatment matches the approved scope rule (full-page for initial load, grid-level for refresh/filter/pagination) and never shows a spinner as the default for the whole page.
3. **Given** a list query returns no data, **When** the user views the page, **Then** an empty-dataset message is shown; **Given** filters match nothing, **Then** a distinct no-matching-results message is shown.
4. **Given** a list query fails, **When** the user views the page, **Then** a persistent error state with a retry action is shown, distinct from empty and loading states.
5. **Given** a list page has both a primary and secondary actions, **When** a reviewer looks at the page, **Then** exactly one primary-weight action is identifiable.
6. **Given** a lifecycle status is displayed on a list page, **When** reviewed, **Then** it uses the shared semantic status display with an accessible label, and generic metadata styling is not used for the state.

---

### User Story 2 - Consistent create/edit forms (Priority: P1)

A user creating or editing a record should clearly distinguish form sections, related fields, required information, validation feedback, and primary, secondary, and destructive actions. The form should present information in a logical, predictable hierarchy regardless of module.

**Why this priority**: Forms are where users enter financial and administrative data; inconsistent structure and unclear action priority cause entry errors and rework. This is the second mandatory Phase 1 validation target.

**Independent Test**: Can be fully tested by opening the validated party create/edit form and confirming sections, field layout, required indicators, validation feedback, and the save/cancel/destructive actions follow the approved form recipe.

**Acceptance Scenarios**:

1. **Given** a user opening a create or edit form, **When** the form renders, **Then** fields are grouped into labeled sections in a predictable order and the layout follows the approved field arrangement.
2. **Given** a form field is required, **When** the field renders, **Then** the required status is indicated by more than color alone (label marker and/or explanatory text) and is consistent across forms.
3. **Given** a user submits a form with invalid input, **When** validation feedback appears, **Then** each message is bound to its field with a visible indicator, a form-level fallback exists for failures without a field target, and the entered values are preserved.
4. **Given** a form has save and cancel actions, **When** the actions render, **Then** save is the single primary action, cancel is secondary, and destructive actions (when present) are visually distinct and labeled with their outcome.
5. **Given** a form contains many fields, **When** the user scrolls, **Then** section grouping and spacing follow the approved form recipe and remain readable in both light and dark modes.
6. **Given** a form displays monetary or prominent numeric values, **When** rendered, **Then** they use the shared money-display and numeric formatting convention, not ad-hoc formatting.

---

### User Story 3 - Document and transaction detail structure (Priority: P2)

A user opening a document or transaction detail page should immediately identify the document identity, its status, the important business information, available primary actions, secondary information, and related records or sections — without every element competing for attention.

**Why this priority**: Detail pages carry lifecycle-critical decisions (approve, post, cancel). Phase 1 must define the recipe so future detail pages converge, but only the list page and the form are validated in Phase 1.

**Independent Test**: Can be fully tested by applying the approved detail recipe to an existing document detail page and confirming identity, status, information hierarchy, actions, and related sections are visually separated as specified.

**Acceptance Scenarios**:

1. **Given** a user opens a document or transaction detail page, **When** the page renders, **Then** the document identity and current status are visually dominant over secondary metadata.
2. **Given** a detail page offers lifecycle actions, **When** the actions render, **Then** at most one primary action is shown for the current status and secondary or destructive actions are visually subordinate.
3. **Given** a detail page contains related sections (lines, items, history), **When** the user views the page, **Then** sections are separated by a consistent section pattern and secondary information does not compete with primary business information.

---

### User Story 4 - RTL and small-screen usability (Priority: P2)

A user working in the Arabic RTL interface or on a small screen must retain hierarchy, content order, action clarity, and access to important functionality. Nothing essential may disappear or become unreachable because of layout reflow.

**Why this priority**: The product is Arabic-first and used on varied devices; RTL and responsive behavior are part of the acceptance of every validated interface, not an optional refinement.

**Independent Test**: Can be fully tested by viewing the validated list page and form at narrow viewport widths and under supported zoom/reflow, in RTL, and confirming all essential information and actions remain reachable and logically ordered.

**Acceptance Scenarios**:

1. **Given** a narrow viewport, **When** the validated list page renders, **Then** the data remains present in a usable form (compact representation where required), filters remain reachable, and the primary action remains available.
2. **Given** a narrow viewport, **When** the validated form renders, **Then** fields stack without loss of labels, required indicators, or validation feedback, and the primary action remains reachable.
3. **Given** the interface is RTL, **When** any validated page renders, **Then** reading order, alignment, icons, and action placement follow RTL logic and do not mirror incorrectly.
4. **Given** the user zooms to the supported reflow level, **When** the validated pages render, **Then** no essential content or action is clipped, hidden, or requires two-dimensional scrolling of the page.

---

### User Story 5 - Light and dark mode consistency (Priority: P2)

A user switching between light and dark appearance must find the same semantic meaning, hierarchy, boundaries, states, and interactive affordances. Changing appearance mode must not change what a color means.

**Why this priority**: Both modes are supported today; inconsistent dark-mode treatments undermine trust and accessibility.

**Independent Test**: Can be fully tested by switching appearance mode on the visual reference and the validated pages and confirming every status, state, border, and action retains its meaning and remains readable.

**Acceptance Scenarios**:

1. **Given** a semantic status (success, warning, error, information) displayed in light mode, **When** the mode switches to dark, **Then** the same status keeps the same meaning and remains distinguishable without color alone.
2. **Given** the validated pages rendered in dark mode, **When** reviewed, **Then** surfaces, borders, and text levels remain distinguishable and text contrast still meets the approved threshold.
3. **Given** a disabled or loading state in either mode, **When** the user views it, **Then** it is distinguishable from both the enabled state and the error state.

---

### User Story 6 - Clear guidance for developers and agents (Priority: P1)

A developer or coding agent creating or modifying an interface should be able to choose the appropriate page pattern, select existing shared components, use approved visual roles, compose common structures, handle component states, and decide when a genuinely new pattern is required — without inventing styling conventions for routine work.

**Why this priority**: Uncontrolled invention is the root cause of the inconsistency this feature addresses. Guidance plus a visual reference is the mechanism that keeps the system unified after Phase 1.

**Independent Test**: Can be fully tested by having a developer/agent build a standard list page and form using only the approved guidance and component reference, and confirming no new visual conventions were needed for routine states.

**Acceptance Scenarios**:

1. **Given** a developer needs a routine list page or form, **When** they consult the project UI guidance, **Then** a recipe exists that answers hierarchy, section organization, action priority, spacing/density, responsive behavior, and page states without further invention.
2. **Given** a developer needs a component state (loading, empty, error, disabled, validation), **When** they consult guidance, **Then** the approved component and variant for that state is identified.
3. **Given** a genuine gap with no approved pattern, **When** the developer introduces a new pattern, **Then** the guidance defines how to record and approve it so it becomes part of the system instead of a one-off.
4. **Given** the internal visual reference (development-only route), **When** a developer or reviewer opens it, **Then** every Phase 1 core component is demonstrated with its approved variants and important interaction states.
5. **Given** Phase 1 components and validated pages, **When** inspected, **Then** no hard-coded design values or competing token sources exist, and per-feature duplicates of shared primitives are consolidated or explicitly justified.
6. **Given** an approved pattern, variant, or role changes, **When** the change lands, **Then** the project UI guidance and the visual reference are updated in the same change.
7. **Given** Phase 1 concludes, **When** the scope inventory is inspected, **Then** it records the inconsistencies found and resolved within scope and the inconsistencies deferred to later phases.

---

### Edge Cases

- A page needs a state or variant that has no approved component role: the guidance must direct the developer to reuse the closest approved role or record a new pattern; the reference must not silently invent a one-off style.
- A workflow legitimately requires two equally weighted actions (e.g., dual approval paths): the exception must be explicitly documented with a functional justification in the page or recipe.
- An unknown or future domain status value reaches a status display: it must degrade to a defined fallback appearance and accessible label, never an unstyled or empty badge.
- Mixed-direction content (Latin codes, document numbers, currencies, percentages) inside an RTL page must keep correct order and alignment.
- Long Arabic labels and long numeric/monetary strings must not truncate essential meaning at narrow widths.
- A list page must show loading, then an error, then recovered data on retry without state overlap or stale combinations.
- A form must handle server-side validation that arrives without field targets, with unknown field paths, and with multiple errors on the same field.
- Keyboard-only operation must not trap focus in filters, dialogs, or the mobile representation of a data grid.
- Light/dark switching while a dialog, toast, or validation message is visible must keep those elements readable and semantically unchanged.
- Forced-colors / high-contrast environments and grayscale rendering must not remove essential meaning (color-independence check).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Every visual style used within the Phase 1 scope MUST map to a named functional role (surface, text, border, action, state) defined in the existing design foundation. Case-by-case or arbitrary style selection is prohibited. (Principle X)
- **FR-002**: Semantic states — success, warning, error, information, disabled — MUST preserve the same meaning across every component and page in scope. A given meaning MUST always map to the same approved role. (Principle X)
- **FR-003**: Important meaning, status, validation feedback, and interaction affordance MUST NOT be communicated by color alone; each MUST include a text, icon, shape, or positional cue that survives grayscale and forced-colors rendering. (Principle X)
- **FR-004**: Interfaces MUST visually distinguish page title, primary content, sections/groups, primary information, secondary information, primary actions, secondary actions, and destructive/high-risk actions using hierarchy (typography, spacing, composition), not color alone. (Principle X)
- **FR-005**: Multiple actions in the same context MUST NOT compete at the same primary visual priority unless a documented functional justification exists; when a workflow has a primary action, exactly one MUST be identifiable. (Principle X)
- **FR-006**: Applicable interaction states — default, hover, focus, active, selected, disabled, loading, error, success, warning — MUST be visually distinguishable and consistent across components, deriving from shared role definitions rather than per-component styling. (Principle X)
- **FR-007**: Comparable list pages MUST follow one predictable structure covering page heading, actions, filters, data presentation, pagination, loading, empty (dataset vs no-results), and error states. (Principle X)
- **FR-008**: Comparable forms MUST follow predictable patterns for section grouping, field layout, labels, supporting information, required-field indication, validation feedback placement, and primary/secondary/cancel action placement. (Principle X)
- **FR-009**: Document and transaction detail interfaces MUST follow a predictable structure separating identity, status, primary information, secondary information, actions, and related sections, and a recipe for them MUST exist in Phase 1. (Principle X)
- **FR-010**: Existing business behavior and public component contracts MUST remain unchanged unless a change is necessary to satisfy a requirement of this specification; every necessary behavioral change MUST be explicitly justified and recorded. Contract-breaking changes require a decision record before implementation. (Principles IX, XII)
- **FR-011**: Affected interfaces MUST remain coherent and usable in RTL and in both light and dark appearance modes; semantic meaning and hierarchy MUST be identical across modes. (Principle X)
- **FR-012**: Core functionality and important information MUST remain accessible on smaller screens and under supported zoom/reflow, verified per SC-006 (320 CSS px equivalent width; 400% zoom from a 1280 CSS px viewport); layouts MUST avoid page-level two-dimensional scrolling and MUST NOT hide essential content or actions, except for content that inherently requires two-dimensional layout such as complex data tables or diagrams. (Principle X)
- **FR-013**: Primary interactive elements and workflows MUST be operable by keyboard where applicable, with a clearly visible focus indicator in both light and dark modes. (Principle X)
- **FR-014**: Text and important visual indicators MUST meet WCAG 2.2 AA contrast requirements: at least 4.5:1 for normal text, at least 3:1 for large text and non-text indicators. (Principle X)
- **FR-015**: A single internal visual reference MUST exist as a development-only route, demonstrating the approved variants and important states (default, hover, focus, disabled, loading, error, success, warning, empty) of every Phase 1 core component, so actual component behavior can be compared against the approved rules. (Principle X)
- **FR-016**: Reusable composition guidance (recipes) MUST exist for at least list pages, forms, and document/transaction detail pages, and MUST be discoverable from the project's developer/agent UI guidance. (Principle X)
- **FR-017**: Each recipe MUST define information hierarchy, section organization, action priority, spacing, density, responsive behavior, and the important page states (loading, empty, error). (Principle X)
- **FR-018**: Guidance MUST enable determining the appropriate existing pattern or recipe before introducing a new visual convention; existing shared patterns MUST be preferred whenever they satisfy the requirement, and genuinely new patterns MUST be recorded following the guidance rather than applied as one-offs. (Principle X)
- **FR-019**: The design foundation MUST define named roles for surfaces (base canvas, card, inset, overlay), text levels (primary, secondary, disabled, inverse), borders/dividers, and content density, not only status colors. (Principle X)
- **FR-020**: The design foundation MUST define exactly two approved content densities — compact and comfortable. List pages and document/transaction detail pages MUST default to compact; create/edit forms MUST default to comfortable. Each recipe MUST state its default density, and components MUST NOT receive per-page arbitrary spacing that conflicts with the shared roles. (Principle X)
- **FR-021**: Monetary values and prominent numeric values in scope MUST render through the shared money-display and formatting conventions (locale-consistent, tabular formatting, explicit negative representation); ad-hoc formatting is prohibited. (Principle X)
- **FR-022**: Lifecycle states MUST use the shared semantic status display. The approved status vocabulary MUST be the six base roles (draft, pending, approved, active, closed, inactive); every additional existing variant MUST be documented as an alias of exactly one base role with one canonical meaning. Phase 1 MUST remap or remove only those aliases found within the validated scope that conflict with the documented meaning. Generic metadata badges MUST NOT be used for lifecycle states; domain statuses MUST map to approved semantic roles at the feature boundary. (Principle X)
- **FR-023**: Phase 1 MUST produce an inventory of the visual inconsistencies found within its scope (core components and the validated pages) and resolve the foundational ones; inconsistencies outside Phase 1 scope MUST be recorded for later phases rather than silently expanded into this work.
- **FR-024**: Visual and interaction review evidence MUST be recorded as a structured review log committed in the feature folder: for the validated pages and the core component reference, one row per condition (light, dark, RTL, 320 CSS px, 400% zoom, keyboard, grayscale/forced-colors) with result, issues found, resolution, and reviewer/date; before/after screenshots are encouraged but optional. Automated checks alone MUST NOT be treated as acceptance evidence. (Principle XI, subject to the frontend governance override)
- **FR-025**: When an approved pattern, variant, or role changes, the project UI guidance and internal visual reference MUST be updated in the same change that alters it. (Principle X)
- **FR-026**: Per-feature duplicates of shared UI primitives within the Phase 1 scope MUST be consolidated into the shared component library, or explicitly justified as functionally distinct. (Principle X)
- **FR-027**: No parallel token source, no hard-coded design values in Phase 1 components/pages, and no second design system may be introduced; the existing central design-token source remains the single source of truth. (Principle X)
- **FR-028**: Form validation feedback presentation MUST conform to the existing unified error-handling contract without changing its guarantees (field-bound errors, form-level fallback, preserved input, one notification owner per action). (Principle XIII; dependency, not a redefinition)

### Phase 1 Core Component Inventory

The following existing shared components define the visual language and are in scope for standardization and visual reference. Components not listed remain out of Phase 1 scope.

| Area | Components |
|------|------------|
| Page composition | Page (heading, actions, toolbar, loading/error), Card, Button, ButtonBar, Breadcrumb, Tabs |
| Form controls | FormField, Label, Input, Textarea, Select, Combobox, Switch, DatePicker |
| Data presentation | DataGrid, MobileCard, Pagination, MoneyDisplay |
| Filtering | FilterBar, FilterSearch, FilterSelect, FilterDate |
| Status and feedback | StatusBadge, Badge, Alert, EmptyState, ErrorState, Loading, Toast |
| Overlays | Dialog, ConfirmDialog, Sheet |

### Key Entities

- **Visual role**: A named functional style definition (surface, text, border, action, state, density) derived from the central design foundation, selected by purpose rather than by value.
- **Semantic state role**: The approved meaning-to-appearance mapping for success, warning, error, information, and disabled, shared by all components.
- **Status role vocabulary**: The six approved base lifecycle roles (draft, pending, approved, active, closed, inactive) plus the documented alias list mapping every additional variant to exactly one base role with one canonical meaning.
- **Design foundation / token source**: The central definition of design values from which all roles and generated artifacts derive; the single source of truth.
- **Shared component contract**: The public behavior, variants, and states of a shared component; preserved unless a change is justified by this specification.
- **Page recipe**: The documented composition rules for a recurring page type (list, form, document/transaction detail), covering hierarchy, sections, actions, spacing, density, responsive behavior, and page states.
- **Visual reference**: The development-only route presenting every core component with approved variants and important states for comparison against the rules.
- **Phase 1 scope inventory**: The list of affected components, the two validated pages, and the recorded inconsistencies (resolved vs deferred).
- **Validation evidence record**: The structured review log committed in the feature folder, with one row per interface-condition pair (light, dark, RTL, 320 CSS px, 400% zoom, keyboard, grayscale/forced-colors) capturing result, issues, resolution, and reviewer/date.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001 — Consistency**: Independent review of the validated list page and form against the approved rules finds no unexplained visual or behavioral inconsistencies within Phase 1 scope; every remaining variation carries a documented justification.
- **SC-002 — Primary-action clarity**: In 100% of reviewed interfaces (validated pages and reference components), an independent reviewer correctly identifies the primary action without confusing it with secondary actions.
- **SC-003 — Semantic consistency**: Every semantic state used in the validated scope communicates one meaning across all components and pages where it appears; sampling finds no conflicting usage.
- **SC-004 — Color independence**: No critical status, validation state, meaning, or required interaction in the validated interfaces depends solely on color; verifying in grayscale or forced-colors mode preserves all meaning.
- **SC-005 — Text contrast**: Normal text in the affected scope achieves at least 4.5:1 contrast, and large text and non-text indicators achieve at least 3:1, in both light and dark modes, except where an applicable WCAG exception permits otherwise.
- **SC-006 — Responsive usability**: All validated interfaces MUST remain fully usable at a viewport equivalent to 320 CSS px width, including when tested at 400% browser zoom from a 1280 CSS px viewport, without loss of content or functionality and without page-level two-dimensional scrolling, except for content that inherently requires two-dimensional layout such as complex data tables or diagrams.
- **SC-007 — Keyboard usability**: Primary workflows and interactive controls in the validated interfaces can be completed using only a keyboard, with a clearly visible focus indicator at every step, in both light and dark modes.
- **SC-008 — Light and dark mode consistency**: The validated interfaces preserve semantic meaning, visual hierarchy, readability, and interaction clarity in both light and dark modes; switching modes never changes meaning.
- **SC-009 — Reusable guidance**: A developer or coding agent can create a standard list page and form using only the approved guidance, without inventing new visual conventions for routine states and interactions; a sample walkthrough documents zero unresolved routine choices.
- **SC-010 — Visual reference availability**: A single development-only internal reference exists and demonstrates the approved variants and important states of every Phase 1 core component.
- **SC-011 — Real-interface validation**: The unified approach is validated against the real Parties list page and the real party create/edit form using the application's real behavior and data flow — no isolated mock interfaces — with recorded evidence.
- **SC-012 — Business behavior preservation**: Existing business rules, workflows, and public component behavior are unchanged except for changes explicitly required and justified by this specification; existing regression suites remain green.

## Assumptions

- The current ERP-Government visual identity (typography, brand colors, Arabic-first RTL character) is the baseline and is refined, not replaced.
- The existing central design-token source and shared component library are the single system to improve; the existing internal component gallery is the seed for the required visual reference.
- Consistency means removing unintended variation, not making all interfaces visually identical; deliberate variations with documented functional reasons are acceptable.
- Phase 1 validates and updates exactly two real screens (Parties list; party create/edit form) plus the core shared components and guidance; all other module pages are migrated in later phases.
- The deferred inconsistency backlog already recorded for non-validated pages remains out of Phase 1 scope unless an item directly blocks the validated pages or a core component.
- Automated validation (lint/build) remains a gate but is not acceptance evidence for visual and interaction quality.
- Frontend governance: no automated frontend tests are added; backend behavior is unaffected, so existing backend suites remain the regression gate.
- All user-facing strings remain hardcoded Arabic; no localization layer is introduced by this work.
- Changes to shared component public behavior are limited to what the approved design rules require; any such change is justified and recorded.

## Dependencies

- **Constitution Principles X (UI and Design System Consistency), IX (API/Frontend Contract Integrity), XI (Testing/Evidence), XII (Controlled Change), XIII (Error Handling)**.
- **AGENTS.md**: binding UI/design-system conventions, frontend architecture rules, and the frontend governance override (no frontend automated tests).
- **DESIGN.md**: agent-facing design tokens and rationale (kept in sync with the token source).
- **docs/ui-patterns.md**: existing component recipes and the deferred inconsistency backlog; the page recipes required by FR-016/FR-017 are maintained here.
- **docs/error-handling.md** (spec 051 contract): form validation feedback and error presentation conventions that FR-028 must conform to.
- **Existing frontend artifacts**: the central design-token source and generator, the shared component library, and the development-only component gallery.

## Scope Boundaries

### In Scope

- Semantic and visual role definitions (surfaces, text, borders, actions, states, density), building on the existing foundation.
- Standardization of the Phase 1 core shared components (inventory above) and consolidation of their duplicates where found in scope.
- The three page recipes (list, create/edit form, document/transaction detail) with hierarchy, section, action, spacing, density, responsive, and state rules.
- The development-only visual reference covering core component variants and states.
- Validation and unification of the Parties list page and the party create/edit form using real data flows.
- Recorded review evidence (light/dark, RTL, narrow viewport, keyboard) and an inventory of resolved vs deferred inconsistencies.
- Guidance updates so future pages reuse established patterns (reuse before invention).

### Out of Scope

- Redesigning all ERP-Government pages or performing an application-wide migration.
- Replacing the existing visual identity or creating a parallel design system.
- Rebuilding every shared component with new contracts.
- Changing module business workflows, business rules, or backend behavior.
- Introducing visual variation for aesthetic diversity, or enforcing a fixed number of colors.
- Resolving the entire historical inconsistency backlog outside the validated scope.
- Automated frontend test suites (governance override) or treating automated checks as visual acceptance.

## Risk and Mitigation

| Risk | Impact | Mitigation |
|------|--------|------------|
| Scope creep into a full application redesign | High | Phase 1 validates only the two agreed screens; all other findings are recorded for later phases |
| Standardization silently changes shared component behavior used elsewhere | High | Preserve public contracts; any behavioral change is explicitly justified, recorded, and regression-gated |
| Visual review is subjective and disagreements block acceptance | Medium | Approval anchored to recorded rules, the visual reference, and per-interface evidence for the defined modes |
| Dark mode / RTL regressions in components not directly touched | Medium | Reference covers both modes and RTL; validated pages re-reviewed after component changes |
| Guidance drifts from actual components | Medium | FR-025 requires guidance and reference updates in the same change; reconciliation against the reference is part of review |
| Legacy backlog items leak into Phase 1 through the validated pages | Medium | Apply the rule: fix only what directly blocks the validated pages or core components; record the rest |

## Verification Governance

- Frontend: `npm run lint` and `npm run build` as the automated gate; no automated frontend tests (AGENTS.md governance override).
- Visual/interaction acceptance: recorded manual evidence for the validated pages and the visual reference across light mode, dark mode, RTL, narrow viewport, and keyboard operation (FR-024).
- Business behavior: backend behavior unchanged; existing five backend test projects serve as the regression gate at convergence/pre-merge.
- Constitution compliance check at the plan gate, re-checked after design, verified at convergence; contract-affecting changes escalate to a decision record.
