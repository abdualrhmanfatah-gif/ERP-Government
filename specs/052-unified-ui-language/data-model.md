# Phase 1 Data Model: Unified UI and Visual Language

**Date**: 2026-09-14 | **Plan**: [plan.md](./plan.md) | **Spec**: [spec.md](./spec.md)

This feature has no database or API data model. The entities below are the conceptual structures that the design foundation, contracts, and validation artifacts must satisfy. Their concrete value tables live in `contracts/`.

## 1. RoleDefinition

A named functional style definition selected by purpose. Roles are declared in `tokens.ts` and flow only through generated artifacts.

| Field | Description | Validation |
|-------|-------------|------------|
| `group` | `surface` \| `text` \| `border` \| `action` \| `state` \| `density` | Exactly one of the six groups |
| `name` | Stable role name (e.g., `onSurfaceVariant`, `containerBorder`) | Unique within the foundation; kebab-cased into the canonical CSS variable name |
| `lightValue` / `darkValue` | Token value per appearance mode | Both modes defined for every role (FR-011) |
| `usageRule` | When the role is selected | Documented in `docs/ui-patterns.md` / contracts |
| `generatedName` | Canonical CSS custom property `--color-<kebab(semanticKey)>` (or sizing/spacing equivalent) | Must exist in generated `tokens.css.scss` before any component references it |

Rules:
- Every role has exactly one canonical CSS variable name; components reference generated names only (FR-001, FR-027).
- A role used by a component but missing from the foundation is a defect — the fix is adding the role, never an inline fallback.

## 2. StatusRoleVocabulary

The approved mapping from lifecycle meaning to appearance.

| Field | Description | Validation |
|-------|-------------|------------|
| `baseRole` | One of `draft`, `pending`, `approved`, `active`, `closed`, `inactive` | Exactly six base roles |
| `alias` | Existing variant name (e.g., `posted`, `paid`, `rejected`) or none for the base name | Each alias maps to exactly one base role |
| `meaning` | Canonical Arabic label + business meaning | One meaning per alias (FR-002, SC-003) |
| `lightPair` / `darkPair` | Background/foreground token pair | Contrast ≥ 4.5:1 with its own text in both modes (SC-005) |
| `cue` | Non-color cue (text label always; icon/shape where two states could be confused) | Present for every state (FR-003) |

Rules:
- Lifecycle states render through `StatusBadge`; generic `Badge` is for non-status metadata only (FR-022).
- New aliases are added to the contract before use (FR-018, FR-025).

## 3. ComponentContract

A shared UI component and its approved surface.

| Field | Description | Validation |
|-------|-------------|------------|
| `name` | Component (e.g., `Button`, `DataGrid`) | Must be in the Phase 1 core inventory |
| `category` | `structure`, `form`, `data`, `filter`, `status`, `feedback`, `overlay` | Matches `contracts/component-contract.md` |
| `variants` | Approved visual/semantic variants with their roles | No variant outside the contract |
| `states` | Required interaction states: default, hover, focus, active/selected, disabled, loading, error, success, warning | States applicable to the component are all covered in the visual reference (FR-006) |
| `density` | `compact` \| `comfortable` \| `both` | Matches the owning recipes |
| `contractStatus` | `preserved` \| `changed` | Any `changed` entry carries a written justification (FR-010) |

## 4. PageRecipe

Composition rules for a recurring page type.

| Field | Description | Validation |
|-------|-------------|------------|
| `pageType` | `list` \| `form` \| `document-detail` | All three required (FR-016) |
| `slots` | Ordered structural slots (e.g., breadcrumbs, title, actions, toolbar, content, sections, history) | Order and hierarchy defined (FR-004, FR-007–FR-009) |
| `actionPriority` | Primary / secondary / tertiary / destructive assignment per context | At most one primary per context or a documented justification (FR-005) |
| `density` | Default density for the page type | `compact` for list and detail, `comfortable` for forms (FR-020) |
| `states` | Loading, empty (dataset vs no-results), error behaviors and their scope (page, grid, form, boundary) | All applicable states defined (FR-007, FR-017) |
| `responsive` | Behavior at the SC-006 reflow condition | Essential content/actions remain reachable (FR-012) |

## 5. Phase1ScopeInventory

The recorded boundary of this phase.

| Field | Description | Validation |
|-------|-------------|------------|
| `coreComponents` | The 32 components in the spec inventory with their standardization status | Every listed component reviewed |
| `validatedPages` | Parties list page; party create/edit form (real data flow) | Both validated with recorded evidence (SC-011) |
| `resolvedInconsistencies` | Inconsistencies fixed within scope (e.g., undefined token references in core components, filter control divergence, duplicate party form stacks, status badge misuse, theme auto mode) | Each has a resolution reference |
| `deferredInconsistencies` | Out-of-scope findings (e.g., undefined token references outside core components, feature `components/` folder deviation, remaining items in `docs/ui-patterns.md` backlog) | Recorded, not silently fixed |
| `guidanceArtifacts` | `docs/ui-patterns.md`, `DESIGN.md`, the gallery, the contracts in this feature | Updated in the same change (FR-025) |

## 6. ReviewEvidenceRecord

One structured row per interface × condition, committed as `specs/052-unified-ui-language/review-evidence.md` during implementation.

| Field | Description | Validation |
|-------|-------------|------------|
| `interface` | Validated page, reference section, or component group | One of the Phase 1 validated interfaces or the visual reference |
| `condition` | `light`, `dark`, `RTL`, `320-css-px`, `400%-zoom`, `keyboard`, `grayscale/forced-colors` | All seven conditions per validated interface (FR-024) |
| `result` | `pass` \| `fail` \| `n/a` (with reason) | A `fail` requires an issue and resolution or an accepted, documented deviation |
| `issue` / `resolution` | What was found and how it was resolved | Empty only when result is `pass` |
| `reviewer` / `date` | Who reviewed and when | Required for every row |
| `screenshot` | Optional path to before/after capture | Optional |

## Relationships

- A **PageRecipe** selects **RoleDefinitions** and **ComponentContracts** for its composition; a **ComponentContract** exposes only approved **RoleDefinitions** and, when status-bearing, only **StatusRoleVocabulary** entries.
- The **Phase1ScopeInventory** references the validated pages and the contracts; the **ReviewEvidenceRecord** proves each validated interface against SC-002, SC-004–SC-008, SC-011.

## Explicit non-entities (out of scope)

- No persisted user preference beyond the existing `erpTheme` key.
- No new runtime configuration, density context, or theming state.
- No backend, database, or API entity is introduced or modified.
