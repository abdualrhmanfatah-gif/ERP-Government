# Visual & Interaction Review Evidence — 052 Unified UI and Visual Language

**Purpose**: structured review log required by FR-024. One row per interface × condition, per `data-model.md` §6.

**Conditions**: light, dark, RTL, 320-css-px, 400%-zoom, keyboard, grayscale/forced-colors.

**Result values**: `pass` | `fail` | `pending-visual` (static checks done, human visual confirmation required) | `n/a` (reason required).

**Reviewer/date**: each row must identify who confirmed the result and when. `static:*` entries mean verified by code inspection/automated gate, not by eye.

## Parties list page — `/parties`

**Static verification (2026-09-14, implementation pass)**: single primary create action; FilterBar search/select share the compact control height token; typed `DataGridColumn<PartyResponse>`; distinct empty vs no-results messages; grid-level loading; persistent `ErrorState` + retry via `Page error/onRetry`; StatusBadge for lifecycle state; LTR isolation on tax number. Human visual confirmation below is still required.

| Condition | Result | Issue | Resolution | Reviewer / date |
|-----------|--------|-------|------------|-----------------|
| light | pending-visual | — | — | — |
| dark | pending-visual | — | — | — |
| RTL | pending-visual | — | — | — |
| 320-css-px | pending-visual | — | — | — |
| 400%-zoom | pending-visual | — | — | — |
| keyboard | pending-visual | — | — | — |
| grayscale/forced-colors | pending-visual | — | — | — |

## Party create form — `/parties/create`

**Static verification (2026-09-14, implementation pass)**: one shared `PartyForm` (React Hook Form + Zod schema in `features/parties/shared/schemas.ts`) with three labeled sections, control-owned labels/required markers/errors with ARIA, server errors bound through `handleApiError` (field paths + form-level `Alert` fallback, single notification owner), values preserved on failure, primary save + secondary cancel, comfortable density controls. Human visual confirmation below is still required.

| Condition | Result | Issue | Resolution | Reviewer / date |
|-----------|--------|-------|------------|-----------------|
| light | pending-visual | — | — | — |
| dark | pending-visual | — | — | — |
| RTL | pending-visual | — | — | — |
| 320-css-px | pending-visual | — | — | — |
| 400%-zoom | pending-visual | — | — | — |
| keyboard | pending-visual | — | — | — |
| grayscale/forced-colors | pending-visual | — | — | — |

## Party edit form — `/parties/:id` (edit mode)

**Static verification (2026-09-14, implementation pass)**: edit mode renders the same `PartyForm` as create with `initialData`, same schema/validation/error binding; submission payload unchanged (`UpdatePartyCommand` shape); success exits edit mode with one success notification. Human visual confirmation below is still required.

| Condition | Result | Issue | Resolution | Reviewer / date |
|-----------|--------|-------|------------|-----------------|
| light | pending-visual | — | — | — |
| dark | pending-visual | — | — | — |
| RTL | pending-visual | — | — | — |
| 320-css-px | pending-visual | — | — | — |
| 400%-zoom | pending-visual | — | — | — |
| keyboard | pending-visual | — | — | — |
| grayscale/forced-colors | pending-visual | — | — | — |

## Party detail page — `/parties/:id`

**Static verification (2026-09-14, implementation pass)**: identity (nameAr) is the page title and status uses `StatusBadge` (active/inactive) in the toolbar, not a generic `Badge`; single primary action ("تعديل") in view mode with the destructive/outline lifecycle toggle as the only other action; canonical `Page.onBack` replaces the custom back button; summary card uses `label-md` section heading; edit mode uses the shared `PartyForm`. Human visual confirmation below is still required.

| Condition | Result | Issue | Resolution | Reviewer / date |
|-----------|--------|-------|------------|-----------------|
| light | pending-visual | — | — | — |
| dark | pending-visual | — | — | — |
| RTL | pending-visual | — | — | — |
| 320-css-px | pending-visual | — | — | — |
| 400%-zoom | pending-visual | — | — | — |
| keyboard | pending-visual | — | — | — |
| grayscale/forced-colors | pending-visual | — | — | — |

## Component gallery — `/__gallery__` (development only)

| Condition | Result | Issue | Resolution | Reviewer / date |
|-----------|--------|-------|------------|-----------------|
| light | pending-visual | — | — | — |
| dark | pending-visual | — | — | — |
| RTL | pending-visual | — | — | — |
| 320-css-px | pending-visual | — | — | — |
| 400%-zoom | pending-visual | — | — | — |
| keyboard | pending-visual | — | — | — |
| grayscale/forced-colors | pending-visual | — | — | — |

## Cross-cutting static verification (US4/US5)

**RTL (2026-09-14)**: no physical direction utilities (`ml-*`, `mr-*`, `pl-*`, `pr-*`, `left-*`, `right-*`, `border-l/r`, `text-left/right`) remain in `components/ui/` or `features/parties/`; the pattern scan returns zero matches. Toast centering was corrected from `start-1/2 -translate-x-1/2` (broken in RTL) to `inset-x-0 mx-auto`; the slide-in keyframes no longer use a horizontal translate. Sheet panel anchors to logical `end/start`.

**Responsive (2026-09-14)**: Page header stacks below `sm`; FilterBar wraps; DataGrid renders the `MobileCard` list below `md` and the table above; form grids are `grid-cols-1 md:grid-cols-2`; Toast has `px-4` gutters at 320 CSS px; Pagination uses the compact control height. Human 320 px / 400% zoom confirmation still required (rows above).

**Dark mode / contrast (2026-09-14)**: computed WCAG contrast from the generated token values —
all six status pairs pass AA in both modes (light 5.49–11.87:1, dark 5.74–11.06:1);
semantic state pairs pass AA in both modes (success/info/warning/error text-on-fill 6.46–15.15:1; container pairs 7.24–13.57:1);
normal text roles pass AA (surface/on-surface 15.16–17.15:1; surface-container-lowest/on-surface-variant 9.33–11.21:1).
Accepted exception: `--color-disabled-fg` on `--color-disabled-bg` is 2.34:1 (light) — WCAG 1.4.3 exempts inactive controls; disabled states also carry `cursor-not-allowed` and are programmatically disabled.
Hard-coded light-only colors in core components were removed (Button `header` variant now uses `--color-on-primary` with `color-mix`).

## Static checks (not a substitute for visual review)

| Check | Command | Result | Notes |
|-------|---------|--------|-------|
| Token generator | `npm run generate-tokens` | pass | regenerated from `tokens.ts` with new roles + densities; re-verified 2026-09-16, output idempotent (stable hashes across runs) |
| Token-reference scan | `npm run design:usage` | pass in scope | 476 files, 271 defined variables; 0 unknown refs in `components/ui/`; 33 remain outside scope (`--color-border-container` 31, `--color-border-input` 2). Re-verified 2026-09-16: 521 files scanned, 30 unknown (28 `--color-border-container`, 2 `--color-border-input`), still 0 in `components/ui/` |
| Lint | `npm run lint` | 141 problems (118 errors, 23 warnings) | baseline was 142; no new problems added. Re-verified 2026-09-16: 133 problems (112 errors, 21 warnings), zero in `components/ui/`, `features/parties/`, `__gallery__/`, `design-system/` |
| Build | `npm run build` | pass | Vite production build; re-verified 2026-09-16 (chunk-size warning only, pre-existing) |
| DESIGN sync | `npm run design:lint` | 0 errors, 23 warnings | warnings pre-existing (orphaned-token/dangling-reference style); re-verified 2026-09-16: 0 errors, 23 warnings |
| DESIGN diff | `npm run design:check` | pass | `"regression": false`; re-verified 2026-09-16: pass |
| Backend regression (convergence) | `dotnet test tests/Domain.UnitTests` (and remaining 4 projects) | **blocked — pre-existing** | The working tree does not compile: `tests/Domain.UnitTests/Revenue/ReceiptVoucherTests.cs` references removed `DepositSlip`/`FormType`/`DepositSlipStatus` types from unrelated in-flight backend work. This feature changes no backend code, so backend regression is not attributable to it; re-run the 5-project suite once the working tree compiles. Re-verified 2026-09-16: Domain.UnitTests 10/10 pass; Application.UnitTests 327/361 (34 failures, all in asset/procurement suites 052 never touches — pre-existing mock/API drift from other specs); Application.FunctionalTests does not compile (`Result<T>` contract drift from other specs); Infrastructure.IntegrationTests does not compile (broken project references); full-solution build additionally blocked by a running dev server locking Web output (`ERP_Government.Web` PID 10252, left running). Web.AcceptanceTests compiles but was not executed (needs Aspire host + browsers; visual acceptance for 052 is manual per FR-024). |

## SC-009 reuse-before-invention walkthrough

Walkthrough performed against the implemented guidance and contract set (2026-09-14):

| Step | Guidance used | Routine choices that required invention | Result |
|------|---------------|------------------------------------------|--------|
| Build a standard list page from guidance only | `docs/ui-patterns.md` list recipe + `contracts/page-recipes.md` §1 + `contracts/component-contract.md` + gallery | None — page skeleton (Page slots, FilterBar controls, DataGrid states, empty/no-results copy, pagination, action priority, compact density, responsive behavior) all resolve to approved components/variants | pass (static) |
| Build a standard form from guidance only | `docs/ui-patterns.md` form recipe + canonical field pattern + density table + `contracts/page-recipes.md` §2 + gallery | None — section grouping, field pattern, required/error presentation, Zod+RHF placement, error binding, save/cancel hierarchy, comfortable density all documented | pass (static) |
| Add a new pattern or status alias | `docs/ui-patterns.md` → Reuse Before Invention process + `contracts/status-semantics.md` | None — the recording/approval path, token regeneration, contrast verification, and gallery demonstration are defined | pass (static) |

**Not yet replaced by human review**: building an actual new page under observation is the remaining SC-009 assurance step for the team.
