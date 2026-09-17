# Phase 1 Scope Inventory — 052 Unified UI and Visual Language

Per `data-model.md` §5. Finalized 2026-09-14.

## Core components (32)

All reviewed against `contracts/component-contract.md`; statuses below are the outcome of the Phase 1 pass.

| Component | Status | Notes |
|-----------|--------|-------|
| Page | standardized | Back button tokenized; single actions slot; loading/error scope unchanged |
| Card | reviewed | Surface roles token-based; section heading rule documented in recipes |
| Button | standardized | Inline fallbacks removed; `on-success/on-info` roles; `header` variant tokenized; `default` alias kept |
| ButtonBar | standardized | Unspecified actions default to `outline` (no accidental primaries) |
| Breadcrumb | reviewed | Logical ordering and focus compliant; no change needed |
| Tabs | reviewed | Roving focus + arrow/Home/End keyboard support already compliant |
| FormField | standardized | Error/description text unified to `text-xs`; wrapper role documented (custom controls only) |
| Label | reviewed | Required marker compliant |
| Input | standardized | Comfortable density height role |
| Textarea | standardized | Border token fixed; same error/ARIA contract as Input |
| Select | standardized | Required marker added; comfortable density height; border token fixed |
| Combobox | standardized | Stable `useId` ids; keyboard handler on trigger; comfortable density height |
| Switch | standardized | RTL thumb translation fixed; physical flex reversal removed |
| DatePicker | reviewed | Wraps the canonical Input pattern; compliant |
| DataGrid | standardized | Tokenized header/striping; grid-level states; typed columns in validated usage; compact representation |
| MobileCard | reviewed | Logical border; compact representation of rows |
| Pagination | standardized | Compact control height; current/disabled/focus states |
| MoneyDisplay | reviewed | Formatting convention unchanged and compliant |
| FilterBar | standardized | Border token fixed; clear action hierarchy |
| FilterSearch | standardized | Compact height + typography unified with siblings |
| FilterSelect | standardized | Compact height (was h-11), border-1, token border fixed |
| FilterDate | standardized | Compact height, border token fixed |
| StatusBadge | standardized | Alias map resolves to base pairs; default icon cues for validation aliases |
| Badge | standardized | `danger` mapped to error container roles; metadata-only rule documented |
| Alert | standardized | Variant icons added (non-color cue) |
| EmptyState | reviewed | Dataset-empty vs no-results variants documented and used |
| ErrorState | standardized | Retry uses `outline` (secondary); persistent state distinct from empty/loading |
| Loading / Skeleton | reviewed | Scope rules documented (page vs grid) |
| Toast | standardized | Container roles tokenized; RTL-safe centering; fallbacks removed |
| Dialog | standardized | Backdrop tokenized; native focus trap/restore |
| ConfirmDialog | reviewed | Cancel ghost + primary/destructive hierarchy compliant |
| Sheet | standardized | Focus trap + focus return; `useId`; overlay token; logical side anchoring |

## Validated pages

| Page | Route | Status |
|------|-------|--------|
| Parties list | `/parties` | updated to list recipe — static checks pass, human visual review pending |
| Party create/edit form | `/parties/create`, `/parties/:id` edit | one shared Zod+RHF form — static checks pass, human visual review pending |
| Party detail (recipe application) | `/parties/:id` | detail structure applied — static checks pass, human visual review pending |

## Baseline gates (T004)

| Gate | Result | Notes |
|------|--------|-------|
| `npm run lint` | pre-existing failures | 142 problems (119 errors, 23 warnings) on the untouched baseline |
| `npm run build` | pass | Vite build succeeds (Sass `@import` deprecation warning only) |
| `npm run design:usage` | baseline recorded | 475 files, 251 defined variables, 73 unknown references |

## Resolved inconsistencies

| # | Inconsistency | Evidence | Resolution |
|---|---------------|----------|------------|
| R1 | Core components referenced undefined `--color-border-input` / `--color-border-container` | token scan baseline: 21 refs in `components/ui/` across 15 files | Renamed to generated `--color-input-border` / `--color-container-border`; scan now 0 in `components/ui/` |
| R2 | Five status aliases + `Badge danger` referenced undefined `--status-*` vars | `StatusBadge.tsx`, `Badge.tsx`, tailwind status map | Aliases resolve to base pairs; validation aliases carry icon cues; `Badge danger` uses error container roles |
| R3 | `Toast`/`Button` used hard-coded fallbacks for missing semantic containers/hovers | scan + code inspection | Added `success/info/warning` container + `on*` + hover roles; regenerated; fallbacks removed |
| R4 | `auto` theme selection silently forced light mode | `ThemeContext.tsx` | `auto` follows `prefers-color-scheme`; explicit overrides unchanged |
| R5 | Divider border role resolved to a near-white primitive | `tokens.ts` `borders.divider` | Resolves to `outlineVariant` |
| R6 | Token generator could emit empty role values silently | generator review | `cssVar` throws on empty/undefined values |
| R7 | No approved density roles | spec FR-020 | `compact`/`comfortable` roles emitted as `--density-*`; recipes select defaults |
| R8 | Overlay/backdrop color hard-coded in components and global styles | Dialog/Sheet/styles.scss | Added `overlay` role (light/dark) and consumed it |
| R9 | Form error/description text sizes diverged (12px vs 14px) | Input/Select vs FormField | Unified to `text-xs` across form feedback |
| R10 | Filter controls diverged (h-8 vs h-11, border vs border-2, text-xs vs text-sm) | FilterSearch/FilterSelect/FilterDate | Unified compact control height + border + typography |
| R11 | Two party form implementations with ad-hoc validation and toast-only server errors | PartyForm + PartyDetailPage inline edit | One `PartyForm` (RHF+Zod) used by create and edit; field-bound server errors via `handleApiError` |
| R12 | Toast container centered with physical transform (broken in RTL) | `Toast.tsx`, `styles.scss` | `inset-x-0 mx-auto` + direction-agnostic keyframes |
| R13 | Switch thumb translated physically (RTL wrong direction) | `Switch.tsx` | `rtl:` negative translate; physical flex reversal removed |
| R14 | `StatusBadge`/`Badge` misuse on the validated detail page | `Badge variant="success"` | `StatusBadge variant="active"/"inactive"` |
| R15 | Parties list row toggle called the mutation with the wrong target (type error; hook id `0` ignored the row id) | `PartiesListPage.tsx` + `useTogglePartyActive` | Hook now accepts an optional target id per call (detail page behavior unchanged); list row toggles the correct party |

## Deferred inconsistencies (out of Phase 1 scope)

| # | Inconsistency | Evidence | Reason deferred |
|---|---------------|----------|-----------------|
| 1 | Non-core `var(--color-border-*)` references in `features/` and other locations | token scan: 33 refs outside `components/ui/` | Pages outside validated scope; no blocking impact — **3 accounting refs fixed in spec 053 batch 1; 30 remain** |
| 2 | Feature `components/` subfolders in 14 features (AGENTS.md structure rule) | directory scan | Application-wide restructuring out of scope |
| 3 | Remaining `docs/ui-patterns.md` deferred backlog items (1–15 except fixed rows) | backlog table | Only items blocking validated pages are in scope |
| 4 | Pre-existing ESLint errors (118) across the app | `npm run lint` baseline | Not caused by this feature; do not expand scope to fix |
| 5 | Dangling `DEP-030` reference in DESIGN.md | DESIGN.md (pre-existing) | No such decision record exists; reference removed in this phase; creating a decision record is outside Phase 1 scope |
| 6 | `docs/ui-patterns.md` breakpoint gap (Tailwind `screens` not mapped from tokens) | DESIGN.md body | Layout mapping change outside the validated scope; tracked for a later phase |
| 7 | AGENTS.md is memory-offloaded (cavemem block 656faaa70d4de653) | AGENTS.md pointer file | Not edited in place; a supplementary memory (`mem_ef134e35eaf90591`) records the Phase 1 UI conventions; fold into the block at the next recompression |

## Phase 3 batch — assets (spec 057)

- **Resolved for the eight assets screens**: lifecycle status misuse (`Badge`/raw span → `StatusBadge` with a feature base-role map + fallback), missing loading/empty/error states, `handleApiError` + root `Alert` form binding, palette styling replaced by tokens/shared components, physical `text-right` removed, dead active filter wired, duplicate status-filter option removed, typed grid columns, server pagination via the shared grid (heuristic removed), read-only detail no longer exposes a no-op save, bespoke deactivate modal → shared `ConfirmDialog`, navigation identifier corrected to `Assets.View`.
- **Still deferred**: 30 non-core `--color-border-*` references (unchanged); in-page permission gating for assets; dormant form option lists (categories/locations/custodians/cost centers/funds); other asset sub-features (acquisition/activation, depreciation, disposal, movements, counts, revaluations).
- **Backlog impact**: `docs/ui-patterns.md` rows 1, 5, 6, 7, 9, 13, 14 updated; new rows 18–20 record the resolved palette/dead-filter/duplicate-option findings.
