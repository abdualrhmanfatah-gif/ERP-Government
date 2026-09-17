---

description: "Task list for Unified UI and Visual Language implementation"
---

# Tasks: Unified UI and Visual Language

**Input**: Design documents from `/specs/052-unified-ui-language/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Tests**: No automated test tasks. This is a frontend-only feature; AGENTS.md governance override prohibits frontend test files (Vitest/RTL/Playwright). Backend behavior is unchanged, so existing backend suites run only as the convergence/pre-merge regression gate. Verification is the structured manual review log required by FR-024.

**Organization**: Tasks are grouped by user story to enable independent implementation and validation of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1–US6)
- All paths are relative to the repository root

## Path Conventions

- Frontend source: `src/Web/ClientApp/src/`
- Feature specs and evidence: `specs/052-unified-ui-language/`
- Commands run from `src/Web/ClientApp` unless stated otherwise

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Baseline capture and the evidence/scope artifacts every story records into.

- [X] T001 [P] Create `specs/052-unified-ui-language/review-evidence.md` with the `ReviewEvidenceRecord` schema from data-model.md §6 (header plus empty rows for the Parties list, party create/edit form, party detail, and gallery × light/dark/RTL/320-css-px/400%-zoom/keyboard/grayscale)
- [X] T002 [P] Create `specs/052-unified-ui-language/scope-inventory.md` per data-model.md §5 (core components list, validated pages, resolved/deferred sections) and record the starting baseline
- [X] T003 [P] Add report-only token-reference scan `src/Web/ClientApp/scripts/check-token-usage.mjs` and the `"design:usage"` script to `src/Web/ClientApp/package.json`; run it and record the baseline offenders (54 references, 21 inside `components/ui/`) in scope-inventory.md
- [X] T004 [P] Baseline gates: run `npm run lint` and `npm run build` in `src/Web/ClientApp`; record results in scope-inventory.md

**Checkpoint**: Baseline and recording artifacts exist.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Token foundation, naming reconciliation, status vocabulary, and appearance mode — every story renders through these.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [X] T005 Extend `src/Web/ClientApp/src/design-system/tokens.ts`: add missing semantic pairs (per design-token-contract §5: `successHover`, `infoHover`, `onSuccess`, `onInfo`, `infoContainer`/`onInfoContainer`, `successContainer`/`onSuccessContainer`, `warningContainer`/`onWarningContainer`), seed the two density roles (design-token-contract §3), and resolve the divider role to the subtle border role (no near-white primitive), for both light and dark exports
- [X] T006 Extend `src/Web/ClientApp/src/design-system/generate-tokens.ts` to emit the new roles for both scopes and their Tailwind mappings (`tokens.tailwind.json`), and fail loudly when a role group generates an empty value
- [X] T007 Regenerate `src/Web/ClientApp/src/design-system/tokens.css.scss` and `tokens.tailwind.json` via `npm run generate-tokens`; verify each new role has a canonical variable in both color scopes
- [X] T008 [P] Fix `src/Web/ClientApp/src/components/ui/StatusBadge.tsx`: remap `posted`, `reversed`, `locked`, `overBudget`, `unbalanced` onto base status pairs plus the non-color cues in contracts/status-semantics.md §2; aliases must render identically to their base role
- [X] T009 [P] Fix `src/Web/ClientApp/src/components/ThemeContext.tsx`: make `auto` follow `prefers-color-scheme` with a media-query listener while explicit `light`/`dark` override; keep the `erpTheme` storage key
- [X] T010 [P] Fix all core-component references to `--color-border-input` → `--color-input-border` and `--color-border-container` → `--color-container-border` in the 17 files listed in design-token-contract §5 (`Input.tsx`, `Textarea.tsx`, `Select.tsx`, `Combobox.tsx`, `FilterBar.tsx`, `FilterSearch.tsx`, `FilterSelect.tsx`, `FilterDate.tsx`, `Dialog.tsx`, `Sheet.tsx`, `Tabs.tsx`, `Accordion.tsx`, `Pagination.tsx`, `Loading.tsx`, `MobileCard.tsx`)
- [X] T011 [P] Remove inline fallback values: `Button.tsx` success/info variants use the new `onSuccess`/`onInfo` action roles, and `Toast.tsx` uses the generated `infoContainer`/`successContainer`/`warningContainer` roles
- [X] T012 Run `npm run generate-tokens`, `npm run design:usage`, `npm run lint`, `npm run build`; confirm zero unknown token references remain in `components/ui/`; record the result and any remaining out-of-scope offenders in scope-inventory.md

**Checkpoint**: Foundation ready — all stories can now proceed.

---

## Phase 3: User Story 1 - Consistent list pages across modules (Priority: P1) 🎯 MVP

**Goal**: Every comparable list page follows one predictable structure (heading, actions, filters, data, pagination, loading, empty, error) via standardized core components and the validated Parties list page.

**Independent Test**: Open `/parties` in the running app and confirm it matches contracts/page-recipes.md §1: one primary create action, consistent filter controls, grid-level loading, distinct empty vs no-results messages, persistent error+retry, and StatusBadge for lifecycle state.

### Implementation for User Story 1

- [X] T013 [P] [US1] Standardize `src/Web/ClientApp/src/components/ui/Page.tsx` header/toolbar/loading/error scope per contracts/component-contract.md (single actions slot, title/description order, `onBack`, no prop changes)
- [X] T014 [P] [US1] Align `FilterBar.tsx`, `FilterSearch.tsx`, `FilterSelect.tsx`, `FilterDate.tsx` to one compact control height/typography/state per design-token-contract §3 and component-contract (fixes the h-8 vs h-11 divergence)
- [X] T015 [P] [US1] Standardize `src/Web/ClientApp/src/components/ui/DataGrid.tsx` per component-contract: grid-level loading, row hover/focus, typed-column usage (no `any` in validated pages), and alignment with `MobileCard.tsx` compact representation
- [X] T016 [P] [US1] Standardize `EmptyState.tsx` (dataset-empty vs no-results variants), `ErrorState.tsx` (persistent + retry), and `Loading.tsx`/`Skeleton` scope usage per component-contract
- [X] T017 [P] [US1] Standardize `src/Web/ClientApp/src/components/ui/Pagination.tsx` states (current, disabled, focus) per component-contract
- [X] T018 [US1] Update `src/Web/ClientApp/src/features/parties/pages/PartiesListPage.tsx` to the list recipe: typed columns, separate no-results message from empty message, primary create + ghost row actions, error retry; no endpoint or hook behavior change
- [X] T019 [US1] Validate the list story against contracts/page-recipes.md §1 with live data and record evidence rows (light, RTL, 320 CSS px) plus the empty/no-results/error checks in `specs/052-unified-ui-language/review-evidence.md`

**Checkpoint**: Parties list is a complete, independently validatable list-page reference.

---

## Phase 4: User Story 2 - Consistent create/edit forms (Priority: P1) 🎯 MVP

**Goal**: One predictable form composition (sections, canonical fields, required/validation feedback, save/cancel/destructive hierarchy) implemented on the validated party create/edit form.

**Independent Test**: Complete `/parties/create` and the detail-page edit flow end to end: invalid submit binds field errors with values preserved; a server rejection binds through the shared error contract; one primary save + secondary cancel; the same form composition in create and edit.

### Implementation for User Story 2

- [X] T020 [P] [US2] Align `Input.tsx`, `Textarea.tsx`, `Select.tsx`, `Combobox.tsx`, `Switch.tsx`, `DatePicker.tsx` to the canonical field pattern (control owns label/required/error/ARIA) and comfortable density roles per component-contract, without prop changes
- [X] T021 [P] [US2] Align `FormField.tsx` to the documented wrapper role (custom/compound controls only, no double-labeling) and `Label.tsx` usage per component-contract
- [X] T022 [P] [US2] Align `ButtonBar.tsx` and `Button.tsx` form-footer hierarchy (primary save, secondary cancel, separated destructive) per contracts/page-recipes.md §2; keep all variant names and the deprecated `default` alias
- [X] T023 [US2] Create `src/Web/ClientApp/src/features/parties/shared/schemas.ts` with the Zod schema for the party form (reuse `src/Web/ClientApp/src/shared/utils/validators.ts` where applicable); schema drives both `defaultValues` and validation
- [X] T024 [US2] Rebuild `src/Web/ClientApp/src/features/parties/components/PartyForm.tsx` to the form recipe: labeled sections, canonical fields, React Hook Form + Zod, field errors with `role="alert"`, preserved values, action bar, and the read-only rendering mode used by the detail page
- [X] T025 [US2] Update `src/Web/ClientApp/src/features/parties/pages/PartyCreatePage.tsx` to use the shared form and `handleApiError` from `src/Web/ClientApp/src/shared/api/result-to-ui.ts` (field binding + root fallback, one notification owner); remove ad-hoc toast/console handling
- [X] T026 [US2] Update `src/Web/ClientApp/src/features/parties/pages/PartyDetailPage.tsx` edit mode to render the same `PartyForm` and use `handleApiError`; submission payloads stay unchanged
- [X] T027 [US2] Validate the form story against contracts/page-recipes.md §2 with live data: invalid-field submit, server rejection binding, preserved values, single notification; record evidence rows in `specs/052-unified-ui-language/review-evidence.md`

**Checkpoint**: Party create/edit is a complete, independently validatable form reference.

---

## Phase 5: User Story 3 - Document and transaction detail structure (Priority: P2)

**Goal**: A predictable detail structure separating identity, status, primary/secondary information, actions, and related sections; applied to the party detail page.

**Independent Test**: Open a party detail page and confirm identity and StatusBadge are dominant, exactly one primary lifecycle action exists, secondary actions are subordinate, and sections follow the detail recipe.

### Implementation for User Story 3

- [X] T028 [P] [US3] Standardize `Card.tsx` section pattern and section-header typography (`label-md`) plus `Alert.tsx` semantic states/icons per component-contract
- [X] T029 [P] [US3] Standardize `Dialog.tsx`, `ConfirmDialog.tsx`, `Sheet.tsx`: focus trap, escape/backdrop close, focus return, destructive confirmation labeled by outcome per component-contract
- [X] T030 [P] [US3] Standardize `Breadcrumb.tsx` and `Tabs.tsx` hierarchy, selected state, and keyboard focus behavior per component-contract
- [X] T031 [US3] Update `src/Web/ClientApp/src/features/parties/pages/PartyDetailPage.tsx` visual structure to contracts/page-recipes.md §3: `StatusBadge` instead of generic `Badge` for active state, identity/status prominence, single primary lifecycle action, edit as secondary, canonical `Page.onBack`, consistent summary sections
- [X] T032 [US3] Validate the detail story against contracts/page-recipes.md §3 and record evidence rows in `specs/052-unified-ui-language/review-evidence.md`

**Checkpoint**: Detail recipe is defined and applied to a real page.

---

## Phase 6: User Story 4 - RTL and small-screen usability (Priority: P2)

**Goal**: Validated interfaces preserve hierarchy, order, and access to essential content/actions in RTL and at the SC-006 reflow condition.

**Independent Test**: View the Parties list, party form, party detail, and gallery at 320 CSS px (and 400% zoom from 1280 CSS px) in RTL with no lost content/actions and no page-level two-dimensional scrolling (inherent 2-D content excepted).

### Implementation for User Story 4

- [X] T033 [US4] Audit touched components/pages for physical CSS properties and fix to logical equivalents (`ms/me/ps/pe`, `start/end`) in `src/Web/ClientApp/src/components/ui/` and `src/Web/ClientApp/src/features/parties/`
- [X] T034 [US4] Fix list responsive behavior: filters wrap/reflow, grid compact representation at 320 CSS px, primary action reachable (`Page.tsx`, `FilterBar.tsx`, `DataGrid.tsx`, `MobileCard.tsx`, `PartiesListPage.tsx`)
- [X] T035 [US4] Fix form responsive behavior: fields stack to one column, labels/required/errors never truncated, action bar reachable (`Input.tsx`, `Select.tsx`, `Textarea.tsx`, `PartyForm.tsx`)
- [X] T036 [US4] Fix detail responsive behavior: identity/status stay visible, sections stack, all actions reachable (`Page.tsx`, `PartyDetailPage.tsx`)
- [X] T037 [US4] Run the 320 CSS px and 400% zoom checks on the validated pages and gallery; record evidence rows and resolve findings in `specs/052-unified-ui-language/review-evidence.md`

**Checkpoint**: Responsive/reflow acceptance condition (SC-006) is evidenced.

---

## Phase 7: User Story 5 - Light and dark mode consistency (Priority: P2)

**Goal**: Semantic meaning, hierarchy, boundaries, states, and affordances hold in both appearance modes.

**Independent Test**: Switch modes on the validated pages and gallery; every status, boundary, state, and action keeps its meaning and readability, and `auto` follows the OS preference.

### Implementation for User Story 5

- [X] T038 [US5] Verify and adjust dark-mode values for all new roles and the six status base pairs in `tokens.ts` to hold ≥ 4.5:1 text contrast and ≥ 3:1 non-text indicator contrast; regenerate tokens and note any accepted exceptions
- [X] T039 [US5] Fix dark-mode defects found in core components (e.g., success/info button fills, toast containers, focus ring visibility, disabled/loading distinction) in `src/Web/ClientApp/src/components/ui/`
- [X] T040 [US5] Run the dark-mode pass over the validated pages and gallery (including mode-switch semantics and `auto` behavior) and record evidence rows in `specs/052-unified-ui-language/review-evidence.md`

**Checkpoint**: SC-008 (and its contrast implications for SC-005) is evidenced.

---

## Phase 8: User Story 6 - Clear guidance for developers and agents (Priority: P1)

**Goal**: Recipes, roles, status mapping, and the dev-only visual reference let a developer/agent build routine interfaces without inventing conventions.

**Independent Test**: Open the gallery and confirm every core component exposes its approved variants/states/roles; follow `docs/ui-patterns.md` alone to build a standard list page and form with no unresolved routine choices.

### Implementation for User Story 6

- [X] T041 [P] [US6] Expand `src/Web/ClientApp/src/components/ui/__gallery__/ComponentGallery.tsx` into the single visual reference: all components from contracts/component-contract.md with approved variants and important states (hover, focus, disabled, loading, error, success, warning, selected, empty), the six status roles plus documented aliases with cues, both densities, and the role groups; keep it gated to `import.meta.env.DEV` in `src/Web/ClientApp/src/app/routes.tsx`
- [X] T042 [P] [US6] Complete `docs/ui-patterns.md` recipes to contracts/page-recipes.md coverage (hierarchy, sections, action priority, spacing, density, responsive, states), the status alias mapping, the canonical field pattern, and the density rules; remove or correct stale guidance
- [X] T043 [US6] Document the reuse-before-invention / new-pattern process in `docs/ui-patterns.md` (how to propose, record, and add a new pattern or alias)
- [X] T044 [US6] Sync `DESIGN.md` with the final `tokens.ts` role set and run `npm run design:lint` + `npm run design:check`
- [X] T045 [US6] Perform the SC-009 walkthrough: follow the guidance to assemble a sample list page and form; record that no routine visual choices were needed in `specs/052-unified-ui-language/review-evidence.md`

**Checkpoint**: Guidance + reference are complete and demonstrated.

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Close the scope inventory, update binding conventions, and run the gates.

- [X] T046 [P] Finalize `specs/052-unified-ui-language/scope-inventory.md`: resolved inconsistencies (with references) vs deferred ones (out-of-scope token references, feature `components/` folder deviation, remaining `docs/ui-patterns.md` backlog)
- [X] T047 [P] Update `AGENTS.md` UI bullets if conventions changed (canonical field pattern, density defaults, status vocabulary pointer) and verify the maintenance rule references stay accurate
- [X] T048 Run full frontend gates from `src/Web/ClientApp`: `npm run generate-tokens`, `npm run design:usage`, `npm run lint`, `npm run build`, `npm run design:lint`, `npm run design:check`; record results
- [X] T049 Complete `specs/052-unified-ui-language/quickstart.md` validation pass; confirm every `review-evidence.md` row is filled or carries an accepted, documented deviation
- [X] T050 Run the backend regression gate (convergence/pre-merge): `dotnet test tests/Domain.UnitTests`, `tests/Application.UnitTests`, `tests/Application.FunctionalTests`, `tests.Infrastructure.IntegrationTests`, `tests.Web.AcceptanceTests`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Setup — BLOCKS all user stories
- **US1 (Phase 3)** and **US2 (Phase 4)**: Start after Foundational; either order (both are MVP P1)
- **US3 (Phase 5)**: After Foundational; integrates with US1/US2 components
- **US4 (Phase 6)**: After US1 + US2 (validates their outputs)
- **US5 (Phase 7)**: After US1 + US2; may overlap US4
- **US6 (Phase 8)**: After Foundational; gallery/guidance describe components finalized by US1–US3 — best after US3, revisit after US4/US5 findings
- **Polish (Phase 9)**: After all desired stories

### User Story Dependencies

- US1 (P1): independent after Foundational
- US2 (P1): independent after Foundational; shares `Page`/`Button` with US1 but touches different files
- US3 (P2): depends on foundation status fix; otherwise independent
- US4 (P2): verifies US1–US3 outputs at reflow conditions
- US5 (P2): verifies foundation + US1–US3 outputs in dark mode
- US6 (P1): guidance/reference; independent, but content depends on final component behavior

### Within Each Story

- Component standardization before the validated page update
- Page update before validation/evidence task
- All evidence recorded before that story's checkpoint

### Parallel Opportunities

- T001–T004 (setup) run in parallel
- T008–T011 (foundational fixes) run in parallel after T005–T007
- T013–T017 (US1 components) run in parallel; T018 after
- T020–T022 (US2 components) run in parallel; T023 before T024; T025/T026 after T024
- T028–T030 (US3 components) run in parallel; T031 after
- T041/T042 (US6) run in parallel; T043–T045 after
- T046/T047 (polish) run in parallel

---

## Parallel Example: User Story 1

```text
# Standardize list components together (different files):
Task: "Standardize Page.tsx header/toolbar/loading/error scope"
Task: "Align FilterBar/FilterSearch/FilterSelect/FilterDate controls"
Task: "Standardize DataGrid states + MobileCard alignment"
Task: "Standardize EmptyState/ErrorState/Loading variants"
Task: "Standardize Pagination states"
# Then:
Task: "Update PartiesListPage to the list recipe"
Task: "Validate list story and record evidence"
```

## Parallel Example: User Story 2

```text
# Align form primitives together:
Task: "Align Input/Textarea/Select/Combobox/Switch/DatePicker to canonical field pattern"
Task: "Align FormField + Label usage"
Task: "Align ButtonBar + Button footer hierarchy"
# Then sequentially:
Task: "Create parties/shared/schemas.ts"
Task: "Rebuild PartyForm on RHF + Zod"
Task: "Update PartyCreatePage error handling"
Task: "Update PartyDetailPage edit mode"
Task: "Validate form story and record evidence"
```

---

## Implementation Strategy

### MVP First (US1 + US2)

1. Complete Phase 1 (Setup) and Phase 2 (Foundational) — the token/status/mode foundation blocks everything.
2. Complete Phase 3 (list) and Phase 4 (form) — the two spec-mandated validation targets.
3. **STOP and VALIDATE**: run the quickstart walkthrough on both pages; record all evidence rows.
4. Then proceed to US3–US6 (detail recipe, responsive, dark mode, guidance/reference) and Polish.

### Incremental Delivery

1. Foundation → components render with correct roles.
2. List story → validated list reference.
3. Form story → validated form reference.
4. Detail story → recipe applied.
5. Responsive + dark mode → acceptance conditions evidenced.
6. Guidance + gallery → system stays unified for future work.
7. Polish → inventory, conventions, gates.

### Notes

- Every story ends with an evidence task; without recorded review rows the story is not done (FR-024).
- Do not modify endpoints, generated API clients, business rules, or pages outside the Phase 1 scope.
- Public component props/variants stay unchanged; the intentional behavior changes are the `auto` theme fix (T009) and party form error surfacing (T025/T026), both justified in research.md.
- [P] tasks touch different files; verify no overlaps before parallelizing.
