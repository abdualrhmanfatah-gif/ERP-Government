---
name: ERP-Government
description: Yemeni government ERP — Financial Law 8/1990. Arabic-first RTL interface, light + dark mode.
version: "alpha"
colors:
  primary: "#002045"
  onPrimary: "#ffffff"
  primaryContainer: "#1a365d"
  onPrimaryContainer: "#86a0cd"
  secondary: "#755b00"
  onSecondary: "#ffffff"
  secondaryContainer: "#fed255"
  onSecondaryContainer: "#735a00"
  error: "#ba1a1a"
  onError: "#ffffff"
  errorContainer: "#ffdad6"
  onErrorContainer: "#93000a"
  success: "#166534"
  successBg: "#dcfce7"
  warning: "#854d0e"
  warningBg: "#fef9c3"
  info: "#1d4ed8"
  infoBg: "#dbeafe"
  surface: "#ffffff"
  surfaceContainerLowest: "#ffffff"
  surfaceContainerLow: "#d1d9e6"
  surfaceContainer: "#dce4f0"
  surfaceContainerHigh: "#c8d4e8"
  surfaceContainerHighest: "#b0c0d8"
  onSurface: "#0d1c2f"
  onSurfaceVariant: "#43474e"
  tertiary: "#1d2123"
  onTertiary: "#ffffff"
  tertiaryContainer: "#333638"
  onTertiaryContainer: "#9c9fa1"
  outline: "#636870"
  outlineVariant: "#a8adb8"
  link: "#1d4ed8"
  focusRing: "#755b00"
  focusHalo: "#fed255"
  disabledBg: "#f1f5f9"
  disabledFg: "#94a3b8"
  statusDraft: "#f1f5f9"
  statusDraftFg: "#475569"
  statusPending: "#fef9c3"
  statusPendingFg: "#854d0e"
  statusApproved: "#dcfce7"
  statusApprovedFg: "#166534"
  statusActive: "#dbeafe"
  statusActiveFg: "#1d4ed8"
  statusClosed: "#e2e8f0"
  statusClosedFg: "#1e293b"
  statusInactive: "#f5f5f4"
  statusInactiveFg: "#57534e"
typography:
  body-sm:
    fontFamily: IBM Plex Sans Arabic
    fontSize: 0.875rem
    fontWeight: 500
    lineHeight: 1.25rem
  body-md:
    fontFamily: IBM Plex Sans Arabic
    fontSize: 1rem
    fontWeight: 500
    lineHeight: 1.5rem
  body-lg:
    fontFamily: IBM Plex Sans Arabic
    fontSize: 1.125rem
    fontWeight: 500
    lineHeight: 1.75rem
  label-sm:
    fontFamily: IBM Plex Sans Arabic
    fontSize: 0.8125rem
    fontWeight: 700
    lineHeight: 1.125rem
  label-md:
    fontFamily: IBM Plex Sans Arabic
    fontSize: 0.875rem
    fontWeight: 600
    lineHeight: 1.25rem
  headline-sm:
    fontFamily: IBM Plex Sans Arabic
    fontSize: 1.375rem
    fontWeight: 700
    lineHeight: 1.875rem
  headline-md:
    fontFamily: IBM Plex Sans Arabic
    fontSize: 1.625rem
    fontWeight: 700
    lineHeight: 2.125rem
  headline-lg:
    fontFamily: IBM Plex Sans Arabic
    fontSize: 2rem
    fontWeight: 700
    lineHeight: 2.5rem
  mono:
    fontFamily: IBM Plex Mono
    fontSize: 0.875rem
    fontWeight: 400
    lineHeight: 1.5rem
rounded:
  none: 0
  xs: 0.0625rem
  sm: 0.125rem
  base: 0.25rem
  md: 0.375rem
  lg: 0.5rem
  xl: 0.75rem
  2xl: 1rem
  3xl: 1.5rem
  full: 9999px
spacing:
  0: 0
  1: 0.25rem
  2: 0.5rem
  3: 0.75rem
  4: 1rem
  5: 1.25rem
  6: 1.5rem
  8: 2rem
  10: 2.5rem
  12: 3rem
  16: 4rem
  20: 5rem
  24: 6rem
components:
  button-primary:
    backgroundColor: "{colors.primary}"
    textColor: "{colors.onPrimary}"
    rounded: "{rounded.lg}"
  button-primary-hover:
    backgroundColor: "{colors.primaryContainer}"
    textColor: "{colors.onPrimary}"
  button-secondary:
    backgroundColor: "{colors.secondaryContainer}"
    textColor: "{colors.onSecondaryContainer}"
    rounded: "{rounded.lg}"
  button-secondary-hover:
    backgroundColor: "{colors.secondary}"
    textColor: "{colors.onSecondary}"
  button-destructive:
    backgroundColor: "{colors.error}"
    textColor: "{colors.onError}"
    rounded: "{rounded.lg}"
  button-destructive-hover:
    backgroundColor: "{colors.errorContainer}"
    textColor: "{colors.onErrorContainer}"
  card-default:
    backgroundColor: "{colors.surface}"
    rounded: "{rounded.xl}"
    padding: 24px
  badge-status-draft:
    backgroundColor: "{colors.statusDraft}"
    textColor: "{colors.statusDraftFg}"
    rounded: "{rounded.full}"
  badge-status-pending:
    backgroundColor: "{colors.statusPending}"
    textColor: "{colors.statusPendingFg}"
    rounded: "{rounded.full}"
  badge-status-approved:
    backgroundColor: "{colors.statusApproved}"
    textColor: "{colors.statusApprovedFg}"
    rounded: "{rounded.full}"
  badge-status-active:
    backgroundColor: "{colors.statusActive}"
    textColor: "{colors.statusActiveFg}"
    rounded: "{rounded.full}"
  badge-status-closed:
    backgroundColor: "{colors.statusClosed}"
    textColor: "{colors.statusClosedFg}"
    rounded: "{rounded.full}"
  badge-status-inactive:
    backgroundColor: "{colors.statusInactive}"
    textColor: "{colors.statusInactiveFg}"
    rounded: "{rounded.full}"
---

## Overview

Arabic RTL government ERP for sustained work with financial records. The intended character is calm, precise, readable, and recognizably navy/gold. Visual quality comes from hierarchy, alignment, spacing, and meaningful states; using more colors is not a success criterion.

**Design foundations implemented in Phase 1 of spec 052 (2026-09-14).** [Constitution X](.specify/memory/constitution.md#x-ui-and-design-system-consistency) owns the guarantees. [docs/ui-patterns.md](docs/ui-patterns.md) owns component recipes, page composition, accessibility criteria, implementation gaps, and evidence requirements; the approved contracts live in [specs/052-unified-ui-language/contracts](specs/052-unified-ui-language/contracts/) and the dev-only gallery at `/__gallery__` is the visual reference. (Phase 1 note: the DEP-030 decision-record reference inherited from earlier documentation was removed because the record was never created; the foundation work is governed by Constitution X and spec 052.)

`src/Web/ClientApp/src/design-system/tokens.ts` is the numeric/color source of truth. The YAML above is a selected **light-theme snapshot** of resolved source values and selected current component properties, not a complete component specification or accessibility certification. Its eight named typography roles match the exported semantic mappings. The additional `mono` example combines the existing monospace family, small size, normal weight, and normal line-height; it is not an exported semantic typography key.

Use the existing generation path: tokens.ts → generate-tokens.ts → tokens.css.scss / tokens.tailwind.json. Components consume theme-aware semantic roles; pages consume component variants. Add a token only for a demonstrated reusable role or accessibility gap, with light/dark values and migration evidence.

The method is a component catalog plus page recipes. Existing business pages are wiring examples; none becomes an approved visual baseline through this document. The catalog still needs implementation and visual verification. Current `design:check` compares this file with itself; neither that command nor `design:lint` proves source synchronization or rendered quality.

## Colors

There are **seven** primitive families: navy, gold, red, green, yellow, blue, and slate, plus white/black. Semantic roles, state pairs, and chart colors serve different purposes. Not every existing color has a complete four-token family.

| Role | Intended use |
|---|---|
| Page/content surfaces | Quiet neutral or gently tinted layers that separate content areas |
| Primary action | Highest-priority available action in its task context |
| Navy brand | Stable application identity and selected emphasis |
| Gold accent | Limited brand emphasis and existing focus treatment |
| Main/supporting text | onSurface / onSurfaceVariant, subject to actual contrast |
| Borders | Subtle grouping vs visible input/control boundaries |
| Status/feedback | Paired state colors with text/icon support |
| Charts | Theme-aware categorical palette with labels |

No minimum color count or fixed 60/30/10 quota applies. Avoid equally strong competing actions. Navy need not fill every section/table heading. The current Button `secondary` variant denotes gold; it is not a mandate to color all lower-priority actions gold. Neutral/outline/ghost treatments are the default candidates for those actions.

These are project choices informed by [Carbon color roles/layers](https://carbondesignsystem.com/elements/color/overview/) and [GOV.UK action hierarchy](https://design-system.service.gov.uk/components/button/). Neither source prescribes this project's exact palette or proportions.

Current `surface` and `surfaceContainerLowest` share the same light value. The five surfaceContainer roles do not require five nested layers or guarantee perceptually ordered values. Use only needed layers; do not automatically give dialogs the darkest/highest token.

Dark themes use separately mapped values, not inverted colors or contrast ratios. Verify paired colors on the actual background in every state. Do not introduce page-local tints to compensate for an absent semantic role. Color alone must not convey selection, errors, or document state. [W3C use of color](https://www.w3.org/WAI/WCAG22/Understanding/use-of-color.html).

## Typography

The declared text/heading family is **IBM Plex Sans Arabic**, with fallbacks in tokens.ts; the declared monospace family is **IBM Plex Mono**. Verify the browser's computed font; a family declaration alone does not prove the font asset loaded.

Use exported semantic typography mappings rather than contradictory comments on primitive sizes. At a 16px root, current headline-sm/md/lg are 22/26/32px; body-sm/md/lg are 14/16/18px. These are existing project values, not WCAG minimum-font-size rules.

Build hierarchy with heading level, size, weight, and spacing before adding color. Prefer body-md for long text and body-sm for compact data where readable. Allow Arabic glyphs, diacritics, and long labels to wrap without clipping. Avoid letter-spacing/uppercase treatments on Arabic labels. Verify zoom and user text-spacing overrides.

Financial tables retain the project's Western digits (0123456789), shared formatters, currency/precision, and tabular-number alignment. This is a project convention, not a legal or universal Arabic rule. Tabular digits do not require a monospace font. Isolate mixed-script amounts/IDs/dates so signs and punctuation remain attached correctly.

## Layout

Use token spacing with a **4px base rhythm and 8px grouping rhythm**; the source also offers 1px/2px detail adjustments. Separate unrelated sections more than related fields. Select density by task rather than shrinking every screen to fit: the foundation defines two approved densities (`compact` 36px controls for lists/detail, `comfortable` 44px for forms) emitted as `--density-*` roles and selected by the page recipe.

Current layout tokens declare a 64px header, 280px sidebar, 1.5rem page gutter, and 2rem container padding. Actual components have additional padding choices. Establish one owner for outer spacing to avoid doubled padding and unnecessary card nesting.

**Breakpoint gap:** tokens.ts declares sm=576px and desktopMin=960px, but Tailwind config does not map `screens` from those tokens. Current sm:/lg: utilities use Tailwind v3 defaults (640/1024px) unless separately overridden. Do not claim the entire app switches at 960px. Reconcile the mapping during implementation and verify affected layouts.

Keep root lang="ar"/dir="rtl", logical layout properties, and DOM order matching reading/focus order. Isolate mixed content and mirror directional navigation icons when appropriate, not every icon. [W3C RTL guidance](https://www.w3.org/International/questions/qa-html-dir).

Reflow ordinary content at 320 CSS px without page-wide horizontal scrolling. Two-dimensional tables may have an accessible, labeled horizontal-scroll region while surrounding controls reflow. Preserve financial content rather than hiding columns solely to fit. [W3C reflow](https://www.w3.org/WAI/WCAG22/Understanding/reflow.html).

## Elevation & Depth

Use spacing, surfaces, and suitable borders for normal grouping. Reserve elevation primarily for transient menus/dialogs. Functional icons and subtle separators are compatible with a formal ERP. Approved shell gradients remain in the shell role, not repeated as form decoration.

Use shared layering tokens and verify stacking contexts/portals. Current values mix dialog=400 with modal=1040, dropdown=300, and tooltip=1070; these do not define an ordered hierarchy to copy blindly. Reconciliation is tracked in the guide. Focus and controls must remain reachable around sticky bars/overlays. [W3C focus not obscured](https://www.w3.org/WAI/WCAG22/Understanding/focus-not-obscured-minimum.html).

## Shapes

Use the radius assigned by the shared component recipe. Current Card uses rounded-xl; Button's base uses rounded-lg with size-specific overrides. Full rounding can suit badges, avatars, and round icon controls; it is not restricted to pills.

Use functional containerBorder/inputBorder roles where mapped. Decorative separators need not meet the same contrast requirements as boundaries necessary to identify a control. Verify each control on its actual background.

## Components

Reuse `src/Web/ClientApp/src/components/ui/` and the central domain-prefixed components folder. YAML references are documentation metadata; application code uses supported component APIs and theme-aware variables.

Current Button variants: default, primary, outline, secondary, ghost, destructive, success, info, link, header. Their existence is not approval for every context. The guide assigns action roles and tracks incomplete/unsafe state pairs.

Current Card variants: default, flat, outlined; **elevated does not exist**. Page, FilterBar, DataGrid, and input implementations are migration starting points. Improve a shared variant instead of repeating local background/text/radius overrides.

Document each component's context, variants, paired colors, density, and applicable default/hover/focus/active/selected/disabled/loading/error states. A local catalog is a visual review aid, not a frontend automated test suite or mock API. Business-page evidence uses the live backend.

Status labels follow domain lifecycles. Do not equate active with approved or recolor states for variety. StatusBadge exposes six approved base roles; every additional variant renders exactly like its base role and validation-type aliases carry an icon cue (see `specs/052-unified-ui-language/contracts/status-semantics.md`). The previously broken aliases (`posted`, `reversed`, `locked`, `overBudget`, `unbalanced`) now resolve to base pairs.

The `inactive` variant uses a warm-neutral stone palette (#f5f5f4 / #57534e light, #292524 / #d6d3d1 dark) chosen to be semantically neutral (not error-like), visually distinguishable from `draft` (cool slate) and `closed` (cooler slate), and to satisfy WCAG AA contrast (~4.9:1 light, ~4.6:1 dark). It is intended for entities that exist but are not currently participating in workflows (e.g., deactivated parties, disabled items). Do not conflate `inactive` with `closed` (lifecycle ended) or `draft` (not yet started).

## Do's and Don'ts

- Start from the task's page recipe and choose supported component roles.
- Use formal, clear Arabic and functional Lucide icons with accessible names where needed.
- Make task priority, grouping, and data legibility evident before adding accent color.
- Verify light/dark states, keyboard use, zoom/reflow, and long Arabic/mixed-script content.
- Preserve feature behavior, error handling, permissions, formatting, and user-entered data during visual work.
- Capture manual/visual evidence and report gaps; lint success alone is insufficient (see `specs/052-unified-ui-language/review-evidence.md`).
- Check the gallery (`/__gallery__`) and `specs/052-unified-ui-language/contracts/` before introducing a new visual convention.
- Keep docs-only work separate from token regeneration and runtime changes.

Follow [docs/ui-patterns.md](docs/ui-patterns.md) for component recipes, page composition, semantic status roles, action hierarchy, accessibility criteria, and implementation gaps. Current code gaps remain open until verified fixed.
