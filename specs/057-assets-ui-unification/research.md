# Research: Assets Management UI Unification (Phase 3)

**Feature**: 057-assets-ui-unification | **Date**: 2026-09-15 | **Plan**: [plan.md](./plan.md)

## R1. Asset lifecycle status → base-role mapping

- **Decision**: Draft→`draft`, Active→`active`, UnderMaintenance→`inactive`, Disposed→`closed`, WrittenOff→`closed`; unknown values fall back to `draft` while rendering the raw Arabic label. Group `isActive` → `active`/`inactive`.
- **Rationale**: `status-semantics.md` defines six base roles and requires the same meaning → the same base role. A temporarily out-of-service asset "exists but is not participating" (`inactive`); disposal and write-off end the lifecycle (`closed`). No new aliases are needed, so the alias-registration rule does not apply. The fallback honors the "unknown status never renders unstyled" edge case.
- **Alternatives considered**: New aliases `underMaintenance`/`disposed`/`writtenOff` (rejected: contract requires registration plus gallery growth; base roles already carry the meaning); Disposed→`inactive` (rejected: disposal is terminal, not temporary).

## R2. List surfaces: shared grid vs justified tree

- **Decision**: The assets register list migrates to `DataGrid` (typed columns, row click, compact/mobile presentation) with server-side paging preserved through the grid's pagination props and the shared `Pagination`. The asset-groups list keeps its custom hierarchical tree as a documented variation (logical indentation, keyboard focus) and migrates its flat grid view to `DataGrid`.
- **Rationale**: `page-recipes.md` requires `DataGrid` with typed columns for list content; the tree is a justified structural variation (053 precedent for the accounts tree). Replacing the current next-page heuristic with API totals removes the hidden-last-page defect.
- **Alternatives considered**: Keep both raw tables (rejected: violates the component contract and leaves states ad hoc); force the tree into `DataGrid` (rejected: collapse/expand and depth semantics are not grid features).

## R3. Form architecture

- **Decision**: One shared asset form for create/edit using the canonical field pattern with shared `Input`/`Select`/`Textarea` controls, `Card` sections, comfortable density, and a footer save (`primary`) + cancel (`ghost`/`outline`). Create validates with `createAssetSchema`; edit validates with `updateAssetSchema`, with the concurrency token seeded from the loaded record. The detail page renders the same form in read-only mode with **no action bar and no submit path**; edit lives in the page header as the single primary action. Asset-group create/edit keep their dedicated routes and per-mode schemas, with the same field-pattern treatment.
- **Rationale**: `page-recipes.md` form recipe; FR-003/FR-004; removes the current no-op save defect and uses the currently-unused update schema.
- **Alternatives considered**: A separate read-only detail component (rejected: duplicates 20+ field definitions and risks drift); keeping the no-op submit path (rejected: defect).

## R4. State handling

- **Decision**: Page-level loading/error via `Page` (`loading`, `error` + `onRetry` using `getQueryErrorMessage` and query `refetch`); list regions render loading at grid scope; empty states use `EmptyState` with dataset-empty vs no-results copy driven by active filters; not-found renders `EmptyState` inside `Page` only after a resolved miss (never while loading or after a failure). The groups list's unreachable local skeleton path is removed.
- **Rationale**: FR-005 and the recipe state rules; resolves the conflated/absent states found in the inventory.
- **Alternatives considered**: Keep the ad-hoc pulse blocks (rejected: inconsistent, no error path).

## R5. Error contract and notification ownership

- **Decision**: Forms call `handleApiError(err, setError)` and render `errors.root` in a form-level `Alert`; lifecycle toggles use `handleLifecycleError`; query failures surface through `Page`/`ErrorState` with normalized messages. Pages never also toast the same failure (single notification owner).
- **Rationale**: `docs/error-handling.md` + Principle XIII; matches the 053 precedent.
- **Alternatives considered**: Local `serverError` divs with manual `err.message` (current state; rejected: bypasses the classification/safe-message contract).

## R6. Palette → design-token migration

- **Decision**: Replace all Tailwind palette color classes in scope (`bg-blue-*`, `bg-gray-*`, `bg-red-*`, `text-gray-*`, `border-gray-*`, `ring-blue-*`) with shared component variants and semantic token classes. Static scan gate: `rg "bg-(blue|gray|red|slate|green)-|text-(blue|gray|red|slate|green)-|border-gray-"` returns zero matches in scope.
- **Rationale**: Principle X (all visual values originate from the token source); FR-009/SC-010.
- **Alternatives considered**: Keep palette on a per-screen basis (rejected: forbidden pattern).

## R7. Navigation permission identifier

- **Decision**: Correct the `/assets` navigation entry from `Assets.Read` to the defined, route-matching `Assets.View`. Record as a behavior change (navigation visibility aligns with the actual guard).
- **Rationale**: FR-020; Principle X requires matching permission naming; the routes use `Assets.View`.
- **Alternatives considered**: Leave and defer (rejected: the batch's screens are unreachable through navigation once permission checks stop being placeholders; one-line correction).

## R8. Authorization scope

- **Decision**: Preserve route guards unchanged; do not add new in-page permission gates in this batch.
- **Rationale**: FR-010/FR-017 bound the change to visual/interaction unification; the frontend permission layer is a registered exception placeholder (constitution exception #4), and changing gating logic is out of scope. Listed in the deferred inventory.
- **Alternatives considered**: Add `usePermission` gating to asset actions (rejected: scope; deferred).

## R9. Text integrity

- **Decision**: Correct corrupted strings in scope: the mixed-script heading `البيانات الت识别ية` (asset form) and `نسبة القيمة التخليضية` → `نسبة القيمة التخريدية` (asset-group form/detail), and remove the duplicate `Active` option in the assets status filter.
- **Rationale**: FR-019; label integrity is user-facing correctness.
- **Alternatives considered**: Defer (rejected: the files are already in scope and clean labels are part of the evidence).

## R10. Concurrency token and payload preservation

- **Decision**: Keep all endpoint calls and payload shapes unchanged. `rowVersion` continues to travel on asset update/deactivate and group update/activate/deactivate; the edit form carries it from the loaded record into the validated submit. Create payloads remain without `rowVersion`.
- **Rationale**: FR-010, Principle IX; the server contracts (specs 054/055) are unchanged.
- **Alternatives considered**: Rebuild payload construction around the form schemas (rejected: risk without benefit).
