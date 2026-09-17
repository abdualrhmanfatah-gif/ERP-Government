# Implementation Plan: Unified UI and Visual Language

**Branch**: `052-unified-ui-language` | **Date**: 2026-09-14 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/052-unified-ui-language/spec.md`

## Summary

Unify the existing ERP-Government design language without replacing it. Extend the central token foundation with explicit functional roles (surfaces, text, borders, actions, states, density), reconcile the token-to-component naming drift that currently breaks styling silently, standardize the Phase 1 core shared components and their interaction states, complete the list/form/document-detail page recipes, expand the existing dev-only component gallery into the single visual reference, and validate the result on the real Parties list page and party create/edit form using live data flows.

## Technical Context

**Language/Version**: TypeScript / React 19 (frontend only; no backend changes)

**Primary Dependencies**: Vite, Tailwind v3, shadcn-style primitives in `src/Web/ClientApp/src/components/ui/` (`@base-ui/react` headless primitives), TanStack Query/Table, React Hook Form + Zod, sonner toasts, lucide-react; token pipeline `tokens.ts` → `generate-tokens.ts` → `tokens.css.scss` + `tokens.tailwind.json`

**Storage**: N/A — no schema or data changes. Existing theme preference stays in `localStorage` (`erpTheme`).

**Testing**: Frontend has no automated test suite by governance (AGENTS.md override) — ESLint + Vite build plus the structured manual visual/interaction review log required by FR-024. Backend behavior is untouched; the existing five backend test projects remain the regression gate at convergence/pre-merge.

**Target Platform**: Web SPA — Arabic-first RTL, light + dark modes, responsive down to a 320 CSS px equivalent viewport

**Project Type**: Frontend design-system consolidation inside the existing web application

**Performance Goals**: No regression to existing page load or interaction; no new runtime requests; the visual reference stays dev-only.

**Constraints**: Preserve existing component public contracts (FR-010); tokens remain the single source of truth (FR-027); no parallel design system; no application-wide page migration; reference is dev-only; no frontend automated tests.

**Scale/Scope**: 32 core shared components (spec inventory), 3 page recipes, 2 validated real screens (Parties list, party create/edit form), guidance (`DESIGN.md`, `docs/ui-patterns.md`), and the dev-only gallery.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Layered Architectural Integrity | ✅ PASS | Frontend-only change; no backend project or layer touched |
| II. Bounded Contexts | ✅ PASS | No cross-feature imports introduced; changes live in `components/ui/` and the Parties feature |
| III. Server-Side Business-Rule Integrity | ✅ PASS | No business rule or use case changed |
| IV. Financial Integrity | ⬜ N/A | No posting, ledger, or money logic changed; `MoneyDisplay` contract preserved |
| V. Budget Control | ⬜ N/A | Not touched |
| VI. Data Integrity | ⬜ N/A | No schema, migration, or persisted-data change |
| VII. Authorization | ✅ PASS | Parties permission checks and endpoint policies untouched |
| VIII. Approval Workflows and Audit | ⬜ N/A | Not touched |
| IX. API and Frontend Contract Integrity | ✅ PASS | No HTTP contract change; generated client untouched |
| X. UI and Design System Consistency | ✅ PASS | This spec implements the principle |
| XI. Testing, Verification, and Evidence | ⚠️ CONDITIONAL | Frontend automated tests excluded by AGENTS.md governance override; visual/interaction evidence recorded per FR-024; backend suites unaffected |
| XII. Controlled Architectural Change | ✅ PASS | No architectural change; if a shared component contract change proves necessary it is justified and recorded per FR-010 |
| XIII. Error Handling, Diagnostics, and Recovery | ✅ PASS | Form presentation conforms to the existing `docs/error-handling.md` contract; no error contract change (FR-028) |

**Gate result**: PASS with the documented frontend testing condition (AGENTS.md override, unchanged by this spec). No registered exception is modified; DEP-027 (Spec 045) is untouched.

**Post-Phase 1 re-check**: unchanged — `research.md`, `data-model.md`, and `contracts/` introduce no new dependency, layer, storage, error-contract, or authorization change; the conditional testing item remains the only condition.

## Project Structure

### Documentation (this feature)

```text
specs/052-unified-ui-language/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   ├── design-token-contract.md
│   ├── component-contract.md
│   ├── status-semantics.md
│   └── page-recipes.md
└── tasks.md             # Phase 2 output (NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/Web/ClientApp/src/
├── design-system/
│   ├── tokens.ts                       # EXTEND: role groups (surface/text/border/density), missing semantic pairs
│   ├── generate-tokens.ts              # EXTEND: emit role + density variables under one canonical naming scheme
│   ├── tokens.css.scss                 # REGENERATED artifact
│   └── tokens.tailwind.json            # REGENERATED artifact
├── components/
│   ├── ThemeContext.tsx                # FIX (small): `auto` must follow system preference, not force light
│   └── ui/                             # STANDARDIZE Phase 1 core components
│       ├── Button.tsx                  # action roles; remove undefined-var fallbacks
│       ├── Input.tsx, Textarea.tsx, Select.tsx, Combobox.tsx   # canonical field pattern + border var fix
│       ├── FormField.tsx               # document/align wrapper role with canonical field pattern
│       ├── FilterBar.tsx, FilterSearch.tsx, FilterSelect.tsx, FilterDate.tsx  # height/state consistency
│       ├── DataGrid.tsx, MobileCard.tsx, Pagination.tsx        # list recipe states
│       ├── StatusBadge.tsx, Badge.tsx, Alert.tsx               # status vocabulary + metadata separation
│       ├── EmptyState.tsx, ErrorState.tsx, Loading.tsx, Toast.tsx  # feedback states
│       ├── Page.tsx, Card.tsx, ButtonBar.tsx, Breadcrumb.tsx, Tabs.tsx  # structure + hierarchy
│       ├── Dialog.tsx, ConfirmDialog.tsx, Sheet.tsx            # overlays: focus, states, border var fix
│       ├── MoneyDisplay.tsx            # formatting conformance only
│       └── __gallery__/ComponentGallery.tsx   # EXPAND: full core inventory, states, roles, density, status
├── features/parties/
│   ├── pages/PartiesListPage.tsx       # CONFORM: list recipe (empty vs no-results, action hierarchy, grid states)
│   ├── pages/PartyCreatePage.tsx       # CONFORM: shared form + error handling
│   ├── pages/PartyDetailPage.tsx       # CONFORM: detail structure, StatusBadge, action hierarchy, shared edit form
│   ├── components/PartyForm.tsx        # REBUILD: one form used by create and edit, recipe-conformant
│   └── shared/schemas.ts               # NEW: Zod schema shared by form defaults + validation (AGENTS.md rule)
└── shared/api/result-to-ui.ts          # REUSE: `handleApiError` for field-bound server errors (no change)

docs/
├── ui-patterns.md                      # UPDATE: completed recipes, density, states, action rules, status mapping
└── DESIGN.md                           # UPDATE: sync role definitions with tokens.ts (maintenance rule)

DESIGN.md                               # repo-root agent-facing design reference (synced with tokens.ts)
```

**Structure Decision**: The existing layered frontend structure is preserved. Work is confined to the design-system module, the shared UI component library, the Parties feature (the agreed validation target), and the two guidance documents. No new project, no cross-feature imports, no feature-folder restructuring (the widespread `features/**/components/` deviation is recorded as deferred, not migrated in Phase 1).

## Complexity Tracking

> No Constitution violations requiring justification. All changes stay inside existing architectural and governance boundaries.
