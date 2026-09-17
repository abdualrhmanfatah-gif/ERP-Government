# Contract: Status Semantics

**Feature**: 052-unified-ui-language | **Component**: `StatusBadge` | **Vocabulary owner**: this contract + `docs/ui-patterns.md`

## 1. Base roles (the approved vocabulary)

| Role | Meaning | Light pair | Dark pair | Non-color cue |
|------|---------|-----------|-----------|---------------|
| `draft` | Not yet submitted or started | `--status-draft-bg/-fg` | same names, dark values | Text label |
| `pending` | Awaiting review or approval | `--status-pending-bg/-fg` | same names, dark values | Text label |
| `approved` | Affirmed or accepted | `--status-approved-bg/-fg` | same names, dark values | Text label + check icon in dense contexts |
| `active` | Currently participating in workflows | `--status-active-bg/-fg` | same names, dark values | Text label |
| `closed` | Lifecycle ended | `--status-closed-bg/-fg` | same names, dark values | Text label |
| `inactive` | Exists but not participating | `--status-inactive-bg/-fg` | same names, dark values | Text label |

Rules:
- Same meaning → same base role everywhere (FR-002, SC-003).
- Every rendered state carries its Arabic text label; color is never the only cue (FR-003, SC-004).
- Base pairs must keep ≥ 4.5:1 contrast between `-fg` and `-bg` in both modes (SC-005).

## 2. Alias contract

Every `BadgeVariant` other than the six base roles is an alias rendering **exactly like its base role** (no alias-specific token pairs). Variant names remain for contract compatibility (FR-010).

| Alias variant | Base role | Canonical meaning (Arabic label) |
|---------------|-----------|----------------------------------|
| `posted` | `closed` | مرحل — posted and final |
| `reversed` | `closed` | معكوس — reversed by a correcting entry |
| `cancelled` | `closed` | ملغى — cancelled |
| `voided` | `closed` | ملغاة نهائياً — voided |
| `locked` | `closed` | مقفل — locked period/document |
| `submitted` | `pending` | مرسلة — submitted for review |
| `sentToTreasury` | `pending` | مرسلة للخزينة — sent to treasury |
| `paid` | `approved` | مدفوعة — paid |
| `disbursed` | `approved` | صرفت — disbursed |
| `partiallyPaid` | `active` | مدفوعة جزئياً — partially paid |
| `passed` | `active` | ناجح — passed |
| `rejected` | `closed` | مرفوضة — rejected |
| `failed` | `closed` | فاشلة — failed |
| `overBudget` | `pending` + icon | يتجاوز الميزانية — over budget (validation state, warning icon required) |
| `warning` | `pending` + icon | تحذير — warning (validation state, warning icon required) |
| `overridden` | `pending` + icon | تم التجاوز — overridden (validation state, approval icon required) |
| `unbalanced` | `pending` + icon | غير متوازن — does not balance (validation state, error icon + border cue required) |

Remapping note: `posted`, `reversed`, `locked`, `overBudget`, `unbalanced` previously referenced token variables that are never generated, so they rendered unstyled. Phase 1 remaps them to base pairs with the cues above.

## 3. Usage rules

1. Lifecycle states render through `StatusBadge`. Generic `Badge` is only for non-status metadata (counts, categories, system flags) — never for lifecycle state (FR-022).
2. Domain statuses map to base roles at the feature boundary via a local mapping object (existing pattern in `docs/ui-patterns.md`), with a defined fallback for unknown future values (edge case: unknown status degrades to `draft` or nearest role with the raw Arabic label, never unstyled).
3. Validation-type states (`unbalanced`, `overBudget`, `warning`, `overridden`) are shown as `StatusBadge` only in status contexts; if they appear as feedback on an action, use `Alert`/form-level feedback instead.
4. New aliases MUST be added to this contract (and the gallery) before first use; unregistered variants are a defect (FR-018, FR-025).
