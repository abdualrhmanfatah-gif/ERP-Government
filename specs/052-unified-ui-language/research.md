# Phase 0 Research: Unified UI and Visual Language

**Date**: 2026-09-14 | **Plan**: [plan.md](./plan.md) | **Spec**: [spec.md](./spec.md)

All decisions below are grounded in the current repository state; file references are evidence, not proposal.

## 1. Foundation approach — extend the existing token source

- **Decision**: Extend `src/Web/ClientApp/src/design-system/tokens.ts` with explicit functional role groups (surface, text, border, action, state, density) and regenerate `tokens.css.scss` + `tokens.tailwind.json` through the existing `generate-tokens.ts`. No role is defined outside this pipeline.
- **Rationale**: Principle X requires all design values to originate from the central token source and flow only through generated artifacts. The foundation already contains semantic colors, dark variants, status pairs, typography, spacing, radii, and sizing — it lacks explicit role *names* and density roles.
- **Alternatives considered**: (a) CSS-only alias layer over current variables — rejected: creates a second definition surface, violating FR-027; (b) per-component values — rejected: the inconsistency this feature removes.

## 2. Token-to-component naming drift must be fixed (evidence)

- **Evidence**: Components reference `--color-border-input` (11 occurrences across the repo) and `--color-border-container` (43), but the generator emits `--color-input-border` and `--color-container-border` (semantic keys kebab-cased). 21 of the broken references are inside `components/ui/` (Input, Textarea, Select, Combobox, FilterBar, FilterSearch, FilterSelect, FilterDate, Dialog, Sheet, Tabs, Accordion, Pagination, Loading, MobileCard). An undefined custom property makes the declaration invalid at computed-value time, so borders silently fall back instead of matching the approved role.
- **Decision**: One canonical naming rule — `--color-<kebab-case-of-semantic-key>` — and all core components updated to it. Components MUST NOT use inline fallback values (`var(--color-x,#hex)`) to mask a missing token; missing tokens are added to `tokens.ts` instead.
- **Rationale**: This is the single highest-value foundational fix: it makes role selection actually effective and removes silent divergence between token intent and rendered UI (FR-001, FR-019, FR-027).
- **Alternatives considered**: (a) Emit duplicate aliases for the misspelled names — rejected: codifies drift and grows the token surface; (b) fix only components touched by validation — rejected: the 21 core-component references are inside the Phase 1 core inventory.
- **Residual**: Non-core references in `features/` and other locations are recorded in the Phase 1 scope inventory as deferred (they do not block the validated pages). Missing semantic pairs referenced by components (`info/success/warning` container + hover roles) are added explicitly to `tokens.ts`.

## 3. Missing semantic tokens referenced by core components

- **Evidence**: `Button.tsx` uses `var(--color-success-hover,…)` / `var(--color-info-hover,…)` with hard fallbacks; `Toast.tsx` uses `--color-info-container`, `--color-success-container`, `--color-warning-container`, which the generator never emits.
- **Decision**: Add the missing semantic pairs (`infoContainer`, `onInfoContainer`, `successContainer`, `onSuccessContainer`, `warningContainer`, `onWarningContainer`, and the hover roles the components need) to `tokens.ts` for both light and dark modes, generate them, and remove the hard-coded fallbacks.
- **Rationale**: Hard-coded fallbacks are design values outside the token source (Principle X) and hide the gap from `design:check`.
- **Alternatives considered**: Keep fallbacks — rejected: violates the token-only rule and makes dark mode inconsistent.

## 4. Density model — two approved densities, recipe-owned defaults

- **Decision**: Define two densities in the foundation — **compact** and **comfortable** — as named sizing/spacing roles (control heights, field gaps, section spacing, table cell padding). Components expose the size/variant that a density maps to; recipes state the page-type default: lists and detail pages compact, create/edit forms comfortable. No global runtime density context.
- **Rationale**: Matches the clarified spec answer (FR-020) while avoiding a global context that would implicitly restyle untouched pages (application-wide migration is out of scope). A context toggle was considered and rejected because it changes every page at once and adds a new state dimension with no Phase 1 demand.
- **Alternatives considered**: (a) Single density — rejected in clarification; (b) `data-density` attribute on the page root with descendant overrides — rejected: pervasive overrides, hard to verify, and implicitly migrates pages outside scope.

## 5. Status vocabulary — six base roles, aliases documented, in-scope conflicts resolved

- **Evidence**: `StatusBadge.tsx` currently accepts 23 variants; aliases reuse base color pairs (`submitted`/`sentToTreasury` → pending, `paid`/`disbursed` → approved, `rejected`/`failed` → reversed, `cancelled`/`voided` → closed), and one variant (`unbalanced`) already uses a shape cue (border-only) instead of a filled color. Five variants — `posted`, `reversed`, `locked`, `overBudget`, `unbalanced` — reference `--status-*` variables the generator never emits (only the six base pairs are generated), so those badges render with no status styling at all today.
- **Decision**: The approved vocabulary is the six base roles (draft, pending, approved, active, closed, inactive), and aliases render exactly like their base role (no separate alias token pairs). Every existing variant is documented as an alias of exactly one base role with one canonical meaning and one Arabic label in `contracts/status-semantics.md`; the five broken variants are remapped onto base pairs plus a non-color cue (and validation-type states such as `unbalanced`/`overBudget` additionally carry an icon cue). Variant names are preserved (public component contract, FR-010); other aliases found inside the validated scope whose meaning conflicts with the documented mapping are remapped in Phase 1. Status display always pairs color with text and, where ambiguity remains, an icon/shape cue (FR-003).
- **Rationale**: Preserves contracts while giving every state one meaning (FR-002, FR-022) and avoids a breaking rename across pages this phase must not touch.
- **Alternatives considered**: (a) Rename variants to the base roles — rejected: breaking contract change requiring a decision record and application-wide migration; (b) freeze variants undocumented — rejected: SC-003 cannot pass without canonical meanings.

## 6. Field pattern — one canonical form-field composition

- **Evidence**: Two competing patterns exist: controls with built-in `label`/`error`/required wiring (Input 493 usages, Select, Textarea — correctly wire `aria-invalid`/`aria-describedby`) and the `FormField` wrapper (49 usages). `FormField` does not wire error ids into its children and, combined with a labeled control, produces double labels. `PartyForm` hand-rolls labels, required asterisks, and error paragraphs with no ARIA linkage.
- **Decision**: The canonical simple-field pattern is the control-level pattern (`Input`/`Select`/`Textarea`/`Combobox` own label, required indicator, error message, and ARIA wiring). `FormField` is reserved for custom or compound controls that cannot own a label, and MUST NOT wrap an already-labeled control. The form recipe documents this and the validated form uses it. Both patterns remain supported elsewhere (no app-wide migration).
- **Rationale**: Reuses the better accessible implementation, avoids a mass migration, and removes the ambiguity that produced the hand-rolled PartyForm.
- **Alternatives considered**: (a) FormField as the only allowed pattern — rejected: requires rewriting the majority of forms and FormField lacks child ARIA wiring; (b) leave both undocumented — rejected: fails FR-008/FR-018.

## 7. Validated list page conformance (Parties)

- **Evidence**: `PartiesListPage.tsx` already follows most of the list recipe (Page title, primary `sm` create button, FilterBar/search/selects, DataGrid loading/empty, Page-level error + retry). Gaps: one `emptyMessage` is used for both genuinely empty data and filter-no-results; `FilterSearch` (h-8, text-xs) and `FilterSelect` (h-11, text-sm) heights/styles diverge; column typing uses `DataGridColumn<any>`; the row toggle action is inline ghost text (acceptable) but its label does not change emphasis on destructive direction.
- **Decision**: Apply the list recipe: distinct empty vs no-results messages, one filter-bar control size/state from the foundation, typed columns for the validated page, action hierarchy unchanged (single primary create; row actions ghost). Pagination stays as-is (not used by this page).
- **Rationale**: Minimal, targeted conformance; the page becomes the reference implementation of the recipe (SC-011) without inventing new structure.
- **Alternatives considered**: Rebuild the page around a new abstraction — rejected: unnecessary and violates "reuse before invention".

## 8. Validated form conformance (party create/edit)

- **Evidence**: Two separate implementations exist: `PartyForm` (create/read-only) and the inline edit block in `PartyDetailPage` (edit mode). Both hand-roll labels, required markers, validation, and error display; `PartyCreatePage` shows a toast and logs to console instead of binding server field errors; the detail page uses `Badge variant="success"` for lifecycle state and a gold `secondary` button for "edit".
- **Decision**: One recipe-conformant party form used by create and edit, built on the canonical field pattern (decision 6) and a shared Zod schema in `features/parties/shared/schemas.ts` (AGENTS.md forms rule). Server errors bind through `handleApiError` from `shared/api/result-to-ui.ts` (existing spec-051 contract: field binding, root fallback, single notification owner). Action hierarchy: save primary, cancel ghost, edit outline, destructive toggle clearly labeled; detail status uses `StatusBadge`. Submission/persistence behavior and endpoints are unchanged.
- **Rationale**: The two stacks are the form inconsistency the spec targets; consolidating them satisfies FR-008, FR-022, FR-026, FR-028 without touching business behavior (FR-010).
- **Alternatives considered**: (a) Style-only pass on both implementations — rejected: leaves duplicate patterns and inconsistent validation; (b) full RHF migration of every form — rejected: application-wide migration out of scope.
- **Justified behavior change**: form submission still sends the same command payloads; only presentation, error surfacing, and validation placement change. Recorded here per FR-010.

## 9. Appearance modes — fix `auto`, then validate both modes

- **Evidence**: `ThemeContext.tsx` stores `auto | light | dark`, but `applyTheme` removes the `dark` class for `auto`, so the default silently renders light instead of following the OS preference.
- **Decision**: Make `auto` follow `prefers-color-scheme` via a media-query listener and keep explicit `light`/`dark` overrides. This is a small foundational fix required for reliable light/dark validation (FR-011, SC-008). The toggle mechanism and stored key stay unchanged.
- **Rationale**: Mode consistency cannot be verified if the default mode does not correspond to a defined preference.
- **Alternatives considered**: Drop `auto` — rejected: removes existing behavior (FR-010) and is unnecessary.

## 10. Visual reference — expand the existing dev-only gallery

- **Decision**: `components/ui/__gallery__/ComponentGallery.tsx` becomes the single visual reference: every Phase 1 core component with its approved variants and important states (default, hover where demonstrable, focus, disabled, loading, error, success, warning, selected, empty), plus sections for semantic roles, the six status roles with aliases, and the two densities. It stays gated to `import.meta.env.DEV` in `routes.tsx`; no production exposure.
- **Rationale**: The gallery already exists, is route-gated, and needs no mock API data — expanding it satisfies FR-015/SC-010 without new infrastructure.
- **Alternatives considered**: Storybook or a separate static site — rejected: new tooling/parallel surface, violating "improve the existing system" and adding dependencies.

## 11. Guidance and recipes — complete `docs/ui-patterns.md`, sync `DESIGN.md`

- **Decision**: The page recipes in `docs/ui-patterns.md` are completed to cover, per recipe: information hierarchy, section organization, action priority, spacing, density, responsive behavior, and page states (FR-017). Status semantics and density rules are documented there with the contracts in this feature. `DESIGN.md` is synced whenever `tokens.ts` changes (existing maintenance rule, FR-025).
- **Rationale**: The repository already treats `docs/ui-patterns.md` as the agent-facing recipe source; completing it avoids inventing a new documentation location.
- **Alternatives considered**: New recipe documents under `docs/` — rejected: duplicate guidance surfaces.

## 12. Verification approach — manual evidence log plus existing gates

- **Decision**: Acceptance evidence is a structured review log committed in the feature folder (`review-evidence.md`), one row per interface × condition (light, dark, RTL, 320 CSS px, 400% zoom, keyboard, grayscale/forced-colors) with result, issues, resolution, reviewer, and date (FR-024). Automated gates: `npm run lint`, `npm run build`, `npm run design:lint` / `design:check` for `DESIGN.md` consistency. A small report-only token-reference check (scans `var(--color-*)` references against generated names) is run manually and its remaining out-of-scope offenders recorded; it is **not** wired into `prebuild` and is not an acceptance gate.
- **Rationale**: WCAG 2.2 AA checks (contrast 1.4.3/1.4.11, use of color 1.4.1, reflow 1.4.10, focus visible 2.4.7) and interaction quality are review-verified; an automated check cannot prove them (spec constraint). The token-reference scan prevents recurrence of decision 2's defect in the components this phase fixes.
- **Alternatives considered**: (a) Add automated frontend tests — prohibited by AGENTS.md override; (b) wire the token scan into the build — rejected: legacy feature references would break builds outside scope.

## 13. Scope guardrails (explicit non-decisions)

- No application-wide migration: only the validated pages, core components, guidance, and gallery change.
- No new shared components beyond gap-filling inside the core inventory.
- No changes to backend, endpoints, generated API client, business workflows, routes, permissions, or data.
- Public component props/variants are preserved; the only intentional behavior changes are the `auto` theme fix (decision 9) and the party form's error surfacing/validation presentation (decision 8), both recorded.
- The `docs/ui-patterns.md` deferred backlog remains deferred except where an item blocks a validated page or a core component (filter height, status-badge misuse, back navigation on the validated detail page, console/debug cleanup).
