# Quickstart Validation: Assets Management UI Unification (Phase 3)

**Feature**: 057-assets-ui-unification | **Purpose**: runnable validation guide for the eight migrated screens.

## Prerequisites

- Working tree at the `057-assets-ui-unification` branch with the implementation applied.
- Backend running with real data covering: every asset lifecycle status, a group tree with ≥3 levels (long Arabic names), a group with active children (deactivation rejection), and enough assets for multiple server pages.
- Frontend dev server: from `src/Web/ClientApp`, `npm run dev`.

## Automated gates

Run from `src/Web/ClientApp` (record results in `review-evidence.md`):

```bash
npm run generate-tokens
npm run design:usage
npm run lint
npm run design:lint
npm run design:check
npm run build
rg "bg-(blue|gray|red|slate|green)-|text-(blue|gray|red|slate|green)-|border-gray-" src/features/assets
rg "text-right|text-left|ml-|mr-|pl-|pr-" src/features/assets
```

Expected: token scan reports zero unknown references in scope; the two `rg` scans report no matches; lint has no new problems; build passes.

## Scenario validation

### S1 — Assets list (FR-005, FR-008, FR-018, SC-013)

1. Open `/assets`; confirm the page recipe (title/description, one primary create) and compact density.
2. Filter by search/status; confirm results, total, and filtering agree; clear filters restores the full list.
3. Navigate to the last page; confirm the full last page is reachable and totals are correct.
4. Filter to a no-match combination; confirm the no-results message differs from the dataset-empty message.
5. Force an API failure (stop backend); confirm the persistent error state and that retry refetches.
6. Keyboard: tab to the grid, activate a row with Enter/Space; confirm navigation and visible focus.

### S2 — Asset create/edit (FR-003, FR-004, FR-009, SC-009)

1. Open `/assets/create`; submit empty; confirm bound field errors per the canonical pattern.
2. Submit with a server rejection (e.g., invalid group); confirm exactly one notification (field or root `Alert`) and preserved values.
3. Open an existing asset's edit route; confirm `Page loading` → form; confirm server-managed fields follow the read-only rules; save a change and confirm the detail reflects it.
4. Open `/assets/:id/edit` with an unknown id; confirm the not-found state appears only after loading resolves, with a way back.
5. Confirm every control is a shared control (no native input/select styling) and the submit button shows loading while saving.

### S3 — Asset detail (FR-002, FR-005, FR-006, FR-007)

1. Open an asset in each status; confirm the toolbar `StatusBadge` matches the base-role map and the label is always visible.
2. Confirm تعديل is the single primary action and there is no operative save/submit in the read-only presentation.
3. Trigger تعطيل; confirm the shared confirmation dialog, cancellation, and success feedback.
4. Force a stale `rowVersion` conflict; confirm the safe failure message and that the dialog does not close silently.

### S4 — Asset groups (FR-002, FR-005, FR-006, FR-019)

1. Open `/assets/asset-groups`; confirm `FilterBar` (search, type, active) and that the active filter affects results in tree and grid modes.
2. Confirm lifecycle rendering uses `StatusBadge` base roles and category metadata uses `Badge` only.
3. Create and edit a group; reject a save from the server; confirm bound errors and preserved values.
4. Open a group detail; confirm `Page.onBack`, primary تعديل, confirmed toggles (`outline`/`destructive`), and empty states for children/accounts sections.
5. Attempt deactivation of a group with active children; confirm the safe rejection message.

### S5 — Cross-cutting matrix (FR-011..FR-014)

For each of the eight screens, exercise: light, dark, RTL, 320 CSS px, 400% zoom, keyboard-only, and grayscale/forced-colors; record one row per screen × condition in `review-evidence.md`.

## Evidence recording

Use the Phase 1 evidence schema (`data-model.md` §5): one row per interface × condition with result, issue, resolution, reviewer, and date. Rows may start as `pending-visual` during implementation and must be finalized by the human reviewer before merge.
