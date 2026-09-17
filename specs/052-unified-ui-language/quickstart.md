# Quickstart: Validating the Unified UI Language

**Feature**: 052-unified-ui-language | **Plan**: [plan.md](./plan.md) | **Evidence log**: create `specs/052-unified-ui-language/review-evidence.md` from the schema in [data-model.md](./data-model.md)

## Prerequisites

- .NET 10 SDK and the repository's development database available; Node dependencies installed in `src/Web/ClientApp` (`npm install` if needed).
- Start the stack: `dotnet run --project src/AppHost` (Aspire dashboard opens; it hosts the API and the frontend).
- Sign in with the existing development account used by the team. Seed data includes parties.

## Surfaces under validation

| Surface | Route | Purpose |
|---------|-------|---------|
| Parties list | `/parties` | Validated list page (SC-011) |
| Party create | `/parties/create` | Validated create form (SC-011) |
| Party detail / edit | `/parties/:id` then "تعديل" | Validated edit form + detail recipe check |
| Component gallery | `/__gallery__` (development builds only) | Visual reference for every core component, role, status alias, density, and state (FR-015, SC-010) |

## Validation matrix (record every row)

For each surface, run all conditions and record result, issues, resolution, reviewer, and date in the evidence log:

1. **Light mode** — semantic roles, hierarchy, status meaning, contrast.
2. **Dark mode** — switch via the theme control (`auto` must follow the OS preference, explicit `light`/`dark` override it); same meanings must hold.
3. **RTL** — Arabic layout, logical ordering, no mirrored-wrong icons/actions (default direction).
4. **320 CSS px equivalent** — narrow viewport; filters wrap or collapse, grid uses its compact representation, primary action reachable.
5. **400% zoom from 1280 CSS px** — no loss of content/functionality, no page-level two-dimensional scrolling (inherent 2-D content excepted).
6. **Keyboard only** — Tab to every interactive element; visible focus in both modes; complete create/edit, filters, list navigation, dialogs, and back navigation; no focus traps.
7. **Grayscale / forced-colors** — status, validation, required, disabled, and action hierarchy remain readable without color; look for text/icon/shape cues (SC-004).

Contrast spot-checks (SC-005): sample normal text, large text, and non-text indicators (borders, focus rings, status chips) in both modes; normal text must reach 4.5:1, large text and indicators 3:1.

## Scenario walkthrough (real data flow)

1. **List recipe**: open `/parties`; confirm heading, single primary "طرف جديد", filter bar with matching-height controls, typed grid, row actions; type a filter that matches nothing and confirm the no-results message differs from the genuinely-empty message; force an API failure (e.g., stop the API) and confirm the persistent error state with retry, distinct from empty/loading.
2. **Create form**: click "طرف جديد"; submit with an empty Arabic name and confirm the field error is bound, announced (`role="alert"`/ARIA), and values are preserved; submit a valid party; trigger a server rejection (e.g., duplicate tax number against the API) and confirm field/root binding and exactly one notification; confirm the primary save and secondary cancel hierarchy.
3. **Edit form**: from the party detail, enter edit mode; confirm the same form composition and validation behavior as create; save and verify success feedback.
4. **Detail recipe**: confirm identity and `StatusBadge` prominence, single primary lifecycle action, secondary actions subordinate, action hierarchy in both modes, and canonical `Page.onBack`.
5. **Gallery**: compare every core component's variants and states against `contracts/component-contract.md`, the six status roles and aliases against `contracts/status-semantics.md`, and both densities against `contracts/design-token-contract.md`.

## Automated gates (not acceptance evidence)

Run from `src/Web/ClientApp`:

- `npm run lint`
- `npm run build`
- `npm run design:lint` and `npm run design:check` (DESIGN.md consistency)
- Regenerate tokens after `tokens.ts` changes: `npm run generate-tokens`
- Token-reference scan (report-only): confirm zero unknown `var(--color-*)` references inside `components/ui/`; record remaining out-of-scope offenders in the scope inventory.

Backend regression gate at convergence/pre-merge (frontend-only change, must remain green):
`dotnet test tests/Domain.UnitTests`, `tests/Application.UnitTests`, `tests/Application.FunctionalTests`, `tests.Infrastructure.IntegrationTests`, `tests.Web.AcceptanceTests`.

## Exit criteria

- All valid rows for the two validated pages and the gallery in light, dark, RTL, 320 CSS px, 400% zoom, keyboard, and grayscale/forced-colors.
- Zero unexplained inconsistencies within Phase 1 scope (SC-001); any remaining variation carries a documented justification.
- `docs/ui-patterns.md` and `DESIGN.md` reflect the final roles, recipes, and status mapping (FR-025).
