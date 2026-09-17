# Contract: Design Token Foundation

**Feature**: 052-unified-ui-language | **Source of truth**: `src/Web/ClientApp/src/design-system/tokens.ts`

## 1. Canonical naming rule

Every role in `tokens.ts` produces exactly one CSS custom property:

- Colors: `--color-<kebab-case-of-semantic-key>` (e.g., `inputBorder` → `--color-input-border`, `surfaceContainerLow` → `--color-surface-container-low`).
- Status pairs: `--status-<baseRole>-bg` / `--status-<baseRole>-fg` for the six base roles only.
- Other groups follow the existing generator conventions (`--spacing-*`, `--radius-*`, `--font-*`, `--shadow-*`, `--size-*`).

Components MUST reference generated names only. Referencing a variable the generator does not emit is a defect; the fix is adding/adjusting a role, never an inline fallback (`var(--color-x, #hex)` is prohibited for token-backed styling).

## 2. Required role groups

| Group | Roles (existing unless marked) | Meaning |
|-------|-------------------------------|---------|
| Surface | `surface`, `surfaceContainerLowest`, `surfaceContainerLow`, `surfaceContainer`, `surfaceContainerHigh`, `surfaceContainerHighest`, `tertiary`, `tertiaryContainer`, `surfaceTint` | Base canvas, cards, insets, overlays, subtle hierarchy |
| Text | `onSurface` (primary), `onSurfaceVariant` (secondary), `disabledFg` (disabled), `link`, and on-color pairs (`onPrimary`, `onSecondary`, `onError`, `onTertiary`, `on*Container`) | Text levels and inverse/on-color text |
| Border | `outline` (strong), `outlineVariant` (subtle), `inputBorder` (form controls), `containerBorder` (containers/sections), divider role (must resolve to `outlineVariant`, not a near-white primitive) | Boundaries and dividers |
| Action | `primary`/`onPrimary`, `secondary`/`onSecondary`/`secondaryContainer`/`onSecondaryContainer`, `error`/`onError` (destructive), `success`/`onSuccess` (new), `info`/`onInfo` (new), `link`, focus (`focusRing`, `focusHalo`), disabled (`disabledBg`, `disabledFg`) | Action hierarchy and interactive states |
| State | `success`, `warning`, `error`, `info`, their `*Bg`/`*Container` and `on*` pairs, plus `successHover`/`infoHover` (new) | Semantic feedback; identical meaning in light and dark |
| Status | `draft`, `pending`, `approved`, `active`, `closed`, `inactive` pairs | Lifecycle roles per `status-semantics.md` |
| Density | `compact` and `comfortable` sizing roles: control height, field gap, section spacing, table cell padding | Per-recipe content density |

## 3. Density roles

| Role | Compact (lists, document/detail) | Comfortable (create/edit forms) |
|------|----------------------------------|----------------------------------|
| Control height | 36 px (`--size-control-height-compact`) | 44 px (`--size-control-height-comfortable`) |
| Field gap | 8 px | 16 px |
| Section spacing | 16 px | 24 px |
| Table cell padding (block) | 8 px | 12 px |

Exact values are fixed through the token scale; recipes select the density role, components never receive per-page arbitrary spacing.

## 4. Generator responsibilities (`generate-tokens.ts`)

1. Emit every role in section 2 for both light and dark scopes with identical names.
2. Emit Tailwind mappings (`tokens.tailwind.json`) for colors, sizing, and spacing used by components.
3. Fail loudly (non-zero exit) when a role is missing from a generated group instead of emitting an empty value.
4. Keep the pipeline one-directional: `tokens.ts` → generator → `tokens.css.scss` + `tokens.tailwind.json`. Generated files are never hand-edited.

## 5. Known gaps to close in Phase 1

| Gap | Evidence | Resolution |
|-----|----------|------------|
| `--color-border-input` referenced in 17 core files | Input, Textarea, Select, Combobox, FilterBar, FilterSearch, FilterSelect, FilterDate, Dialog, Sheet, Tabs, Accordion, Pagination, Loading, MobileCard | Rename to generated `--color-input-border` |
| `--color-border-container` referenced in core files | FilterBar, Dialog, Sheet, Tabs, Accordion, MobileCard, Loading | Rename to generated `--color-container-border` |
| `--status-posted/-reversed/-locked/-overBudget/-unbalanced` referenced but never generated | `StatusBadge.tsx` | Remap aliases onto the six base pairs + non-color cues per `status-semantics.md`; no alias token pairs |
| `--color-info-hover`, `--color-success-hover`, `--color-info-container`, `--color-success-container`, `--color-warning-container` | `Button.tsx`, `Toast.tsx` | Add explicit `success/info/warning` container + `on*` + hover roles; remove hard fallbacks |
| White text on `success`/`info` fills | `Button.tsx` success/info variants | Use `onSuccess`/`onInfo`; verify ≥ 4.5:1 in both modes |
| `borders.divider` uses a near-white primitive | `tokens.ts` `borders` composite | Resolve divider to the subtle border role |

## 6. Verification

- `npm run design:lint` and `npm run design:check` continue to validate `DESIGN.md` consistency.
- A report-only token-reference scan checks `var(--color-*)` uses against generated names; Phase 1 must leave zero unknown references inside `components/ui/`. Remaining out-of-scope references are recorded in the scope inventory, not silently fixed.
- No automated visual proof: contrast and color-independence are verified manually per `quickstart.md`.
