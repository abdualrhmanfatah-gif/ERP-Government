# Data Model: Assets Management UI Unification (Phase 3)

Design-time entities only; no persistence, API, or DTO changes.

## 1. Migrated screen registry

| # | Screen | Route | Recipe | Route permission | Primary file |
|---|--------|-------|--------|------------------|--------------|
| 1 | Assets list | `/assets` | list | `Assets.View` | `features/assets/assets/pages/AssetsListPage.tsx` |
| 2 | Asset create | `/assets/create` | form | `Assets.Create` | `features/assets/assets/pages/AssetCreatePage.tsx` |
| 3 | Asset edit | `/assets/:id/edit` | form | `Assets.Update` | `features/assets/assets/pages/AssetEditPage.tsx` |
| 4 | Asset detail | `/assets/:id` | detail | `Assets.View` | `features/assets/assets/pages/AssetDetailPage.tsx` |
| 5 | Asset groups list | `/assets/asset-groups` | list (tree/grid variation) | `AssetGroups.View` | `features/assets/asset-groups/pages/AssetGroupsListPage.tsx` |
| 6 | Asset group create | `/assets/asset-groups/create` | form | `AssetGroups.Create` | `features/assets/asset-groups/pages/AssetGroupCreatePage.tsx` |
| 7 | Asset group edit | `/assets/asset-groups/:id/edit` | form | `AssetGroups.Update` | `features/assets/asset-groups/pages/AssetGroupEditPage.tsx` |
| 8 | Asset group detail | `/assets/asset-groups/:id` | detail | `AssetGroups.View` | `features/assets/asset-groups/pages/AssetGroupDetailPage.tsx` |

Each screen carries: conformance status (list/form/detail), evidence reference (review-evidence row set), and a behavior-change note when applicable.

## 2. Status base-role map (feature boundary)

| Domain value | Base role | Arabic label | Non-color cue |
|--------------|-----------|--------------|---------------|
| `Draft` | `draft` | مسودة | Text label |
| `Active` | `active` | نشط | Text label |
| `UnderMaintenance` | `inactive` | تحت الصيانة | Text label |
| `Disposed` | `closed` | متخلص | Text label |
| `WrittenOff` | `closed` | مُشطوب | Text label |
| *(unknown)* | `draft` fallback | raw Arabic label if provided, else the raw value | Text label |
| Group `isActive: true` | `active` | نشط | Text label |
| Group `isActive: false` | `inactive` | غير نشط | Text label |

Rules: no new `StatusBadge` variants are introduced; the map lives at the feature boundary (assets + asset-groups); every rendered state includes its Arabic text label (never color-only).

## 3. Form schema registry

| Mode | Schema | File | Key rules |
|------|--------|------|-----------|
| Asset create | `createAssetSchema` | `features/assets/assets/shared/schemas.ts` | name 1..200; description ≤500 optional; assetGroupId ≥1; originalValue ≥0; purchaseDate required; depreciationStartDate required; acquisitionType required; usefulLifeYears int ≥1 optional; notes ≤2000; optional ids/codes |
| Asset edit | `updateAssetSchema` | same | create rules + `status` optional + `rowVersion` required (seeded from the loaded record) |
| Asset group create | `createAssetGroupSchema` | `features/assets/asset-groups/shared/schemas.ts` | code 1..50; name 1..200; assetCategory required; depreciationMethod required; residualValuePercentage 0..100 optional; account ids optional |
| Asset group edit | `updateAssetGroupSchema` | same | create rules minus `code` + `rowVersion` required (seeded from the loaded record) |

Payloads keep their current shapes: create without `rowVersion`; update/deactivate with `rowVersion`.

## 4. State contract per recipe

| Recipe | loading | empty | not-found | error |
|--------|---------|-------|-----------|-------|
| List | grid scope on filter/page change; page-level on first load | `EmptyState` dataset-empty vs no-results (filters active) | n/a | persistent `ErrorState`/`Page error` + retry (`refetch`) |
| Form (create) | n/a (submit loading on primary button) | n/a | n/a | field-bound errors + root `Alert`; values preserved |
| Form (edit) | `Page loading` | n/a | `EmptyState` only after a resolved miss | `Page error` + retry (`refetch`) |
| Detail | `Page loading` | section-level `EmptyState` for empty related lists | `EmptyState` inside `Page` after a resolved miss | `Page error` + retry (`refetch`) |

## 5. Review evidence record

Same schema as spec 052 (`data-model.md` §6): one row per screen × condition.

- **Conditions**: light, dark, RTL, 320-css-px, 400%-zoom, keyboard, grayscale/forced-colors.
- **Fields**: interface, condition, result (`pass` | `fail` | `pending-visual` | `n/a` + reason), issue, resolution, reviewer, date.
- **Interfaces**: the eight screens in §1.

## 6. Assets scope inventory

Extends the Phase 1/Phase 2 inventories with three tables:

- **Resolved inconsistencies**: each with file(s) and resolution.
- **Deferred inconsistencies**: with evidence and reason (in-page permission gating, dormant form option lists, other asset sub-features).
- **Behavior changes**: the register from `contracts/screen-contract.md` §6 with justification and risk.

## 7. Edge-case fixtures for evidence

- Asset with each lifecycle status (Draft/Active/UnderMaintenance/Disposed/WrittenOff) + an unknown-status record if producible.
- Asset list: first page, middle page, last full page, filtered no-results, and dataset-empty states.
- Asset groups: tree with ≥3 levels (RTL long Arabic names), flat grid, empty children/accounts sections, group with active children (deactivation rejection).
- Concurrency: stale `rowVersion` on asset deactivate and group toggle.
