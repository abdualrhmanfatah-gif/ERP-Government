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
  secondaryContainer: "#fef9c3"
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
  focusHalo: "#fef9c3"
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
    rounded: "{rounded.md}"
    padding: 8px 16px
    typography: "{typography.body-md}"
  button-primary-hover:
    backgroundColor: "{colors.primaryContainer}"
    textColor: "{colors.onPrimary}"
    rounded: "{rounded.md}"
    padding: 8px 16px
  button-secondary:
    backgroundColor: "{colors.secondaryContainer}"
    textColor: "{colors.onSecondaryContainer}"
    rounded: "{rounded.md}"
    padding: 8px 16px
    typography: "{typography.body-md}"
  button-secondary-hover:
    backgroundColor: "{colors.secondary}"
    textColor: "{colors.onSecondary}"
    rounded: "{rounded.md}"
    padding: 8px 16px
  button-destructive:
    backgroundColor: "{colors.error}"
    textColor: "{colors.onError}"
    rounded: "{rounded.md}"
    padding: 8px 16px
    typography: "{typography.body-md}"
  button-destructive-hover:
    backgroundColor: "{colors.onErrorContainer}"
    textColor: "{colors.onError}"
    rounded: "{rounded.md}"
    padding: 8px 16px
  input:
    backgroundColor: "{colors.surface}"
    textColor: "{colors.onSurface}"
    rounded: "{rounded.md}"
    padding: 8px 12px
    typography: "{typography.body-md}"
  input-focus:
    backgroundColor: "{colors.surface}"
    textColor: "{colors.onSurface}"
    rounded: "{rounded.md}"
    padding: 8px 12px
    typography: "{typography.body-md}"
  card:
    backgroundColor: "{colors.surface}"
    textColor: "{colors.onSurface}"
    rounded: "{rounded.lg}"
    padding: 24px
  card-elevated:
    backgroundColor: "{colors.surfaceContainerLow}"
    textColor: "{colors.onSurface}"
    rounded: "{rounded.lg}"
    padding: 24px
  badge-status-draft:
    backgroundColor: "{colors.statusDraft}"
    textColor: "{colors.statusDraftFg}"
    rounded: "{rounded.full}"
    padding: 2px 8px
    typography: "{typography.label-sm}"
  badge-status-pending:
    backgroundColor: "{colors.statusPending}"
    textColor: "{colors.statusPendingFg}"
    rounded: "{rounded.full}"
    padding: 2px 8px
    typography: "{typography.label-sm}"
  badge-status-approved:
    backgroundColor: "{colors.statusApproved}"
    textColor: "{colors.statusApprovedFg}"
    rounded: "{rounded.full}"
    padding: 2px 8px
    typography: "{typography.label-sm}"
  badge-status-active:
    backgroundColor: "{colors.statusActive}"
    textColor: "{colors.statusActiveFg}"
    rounded: "{rounded.full}"
    padding: 2px 8px
    typography: "{typography.label-sm}"
  badge-status-closed:
    backgroundColor: "{colors.statusClosed}"
    textColor: "{colors.statusClosedFg}"
    rounded: "{rounded.full}"
    padding: 2px 8px
    typography: "{typography.label-sm}"
---

## Overview

Yemeni government ERP design system for Financial Law 8/1990 compliance. The UI is Arabic-first, right-to-left, with a formal government tone. Navy and gold evoke authority; semantic status colors map to document lifecycle stages. Light mode is primary; dark mode inverts contrast ratios while preserving hue families.

Design tokens live in `src/Web/ClientApp/src/design-system/tokens.ts` — that file is the single source of truth. This DESIGN.md documents those tokens for agents; never invent values here that don't exist in tokens.ts.

## Colors

The palette derives from five primitive families: navy (primary), gold (secondary), red (error), green (success), yellow (warning), blue (info), and slate (neutral/surface).

**Primary — Navy (#002045):** Headlines, primary buttons, active sidebar gradient. Dark navy conveys government authority. On-primary is white for WCAG AA contrast (15.42:1).

**Secondary — Gold (#755b00):** Secondary actions, focus rings, accent indicators. On-secondary is white. Gold references Yemeni national symbolism.

**Semantic pairs:** Every color has a `color`/`on-color`/`colorContainer`/`on-colorContainer` quadruple following Material Design 3 conventions. Container colors are lighter tints for surfaces; `on-container` colors are dark enough for readability on those tints.

**Status colors:** Draft (slate gray), Pending (yellow), Approved (green), Active (blue), Closed (dark slate). Each has a light and dark pair defined in tokens.ts as `statusColors` and `statusColorsDark`.

**Dark mode:** All semantic colors swap to their dark-mode counterparts (e.g., primary becomes navy[100] instead of navy[500], surfaces invert to deep navy). The dark palette is defined separately in `semanticColorsDark`; CSS custom properties switch at runtime.

**Surface hierarchy:** `surfaceContainerLowest` → `surfaceContainerHighest` creates four tiers of depth. Use the lowest for page background, highest for overlays.

## Typography

Font family: **IBM Plex Sans Arabic** for all UI text (Latin + Arabic glyphs). Fallbacks: IBM Plex Sans → Segoe UI → Tahoma → system sans-serif. Monospace: **IBM Plex Mono** for data/code.

Arabic text inherits the same IBM Plex Sans Arabic family — no separate font needed. The typeface supports Arabic ligatures, kashida elongation, and diacritical marks (tashkeel) natively.

**Scale:** 8-step semantic scale from label-sm (13px) to headline-lg (32px). All sizes use rem for relative scaling. Weights range from medium (500) for body text to bold (700) for labels and headlines.

**Arabic considerations:** Line height is generous (1.25–2.0) to accommodate ascenders/descenders in Arabic script. Font weight 500 (medium) for body text ensures legibility at small sizes on Arabic glyphs.

**Roman numerals:** Government forms use Arabic-Indic numerals (٠١٢٣٤٥٦٧٨٩) for Arabic text contexts and Western Arabic numerals (0123456789) for data tables. Both are supported by IBM Plex Sans Arabic.

## Layout

**Grid:** 8px base grid. Spacing scale maps 1:1 to the grid (spacing-1 = 0.25rem = 4px, spacing-2 = 0.5rem = 8px, etc.).

**Page structure:** Header height 64px, sidebar width 280px, page gutter 1.5rem, container padding 2rem.

**Breakpoints:** xs(0), sm(576), md(768), lg(1024), xl(1280), 2xl(1536). Desktop layout activates at 960px+ (sidebar + content). Tablet/mobile at 959px and below collapses to single-column.

**RTL:** All layout uses logical properties (`margin-inline-start`, `padding-inline-end`, `border-inline-start`) instead of physical `left`/`right`. In RTL, the sidebar moves to the right edge. Text alignment defaults to `start` (right in RTL, left in LTR). Do not hardcode `margin-left` or `padding-right` — always use logical properties.

## Elevation & Depth

**Shadow scale:** 7 levels from xs (0 1px 2px) to 2xl (0 25px 50px). Inner shadow for inset elements. Transient shadow for popovers/toasts.

**Z-index scale:** Base(0) → dropdown(300) → sidebar(200) → dialog(400) → modalBackdrop(1030) → modal(1040) → popover(1050) → tooltip(1070). Header and sidebar use project-specific values (100, 200).

**Dark mode elevation:** Use `surfaceTint` color (#455f88 light, #adc7f7 dark) with opacity for dark-mode elevation overlays instead of shadows, since shadows are less visible on dark backgrounds.

## Shapes

**Border radius:** 10-step scale from none(0) to full(9999px). Most components use md(0.375rem) or lg(0.5rem). Cards use lg, badges use full for pills.

**Borders:** Container borders use `#c8d0dc` (light) / `#233144` (dark). Input borders use `#636870`. Divider borders use `#f8f9ff`. Border widths: 0/1/2/4/8px.

## Components

Components reference design tokens via `{path}` syntax. Agents must use these exact values — not approximate colors or sizes.

**Buttons:** Three variants — primary (navy bg, white text), secondary (gold container bg, gold text), destructive (red bg, white text). All use rounded.md, padding 8px 16px. Hover states shift to darker/lighter container variants.

**Input:** White background, onSurface text, rounded.md, 8px/12px padding. Focus state adds focusRing outline (gold).

**Card:** Two tiers — default (white bg, 24px padding, rounded.lg) and elevated (surfaceContainerLow bg). Cards contain form sections, document summaries, or data tables.

**Badge-status:** Five document lifecycle states. Each badge is a rounded pill (full radius) with status-specific bg/fg pair. Labels-sm typography (13px bold).

## Do's and Don'ts

**Do:**
- Always read `tokens.ts` before adding or changing any UI value
- Use logical CSS properties (`margin-inline-start`, not `margin-left`)
- Match Arabic text to IBM Plex Sans Arabic; never substitute a Latin-only font
- Use status badge tokens for document lifecycle indicators
- Respect the 8px grid for all spacing
- Prefer `start`/`end` text alignment over `left`/`right`

**Don't:**
- Invent new color values, font sizes, or spacing outside tokens.ts
- Hardcode `margin-left`/`padding-right` — use logical properties
- Skip dark mode — every component must work in both themes
- Use decorative icons, gradients, or shadows on government forms
- Display Arabic-Indic numerals in data tables (use Western Arabic numerals)
- Mix font families — IBM Plex Sans Arabic covers Latin and Arabic
- Place critical actions at z-index above tooltip (1070)
- Use border-radius full on non-pill elements

**Government formality:** Text is formal Arabic (Modern Standard Arabic). No colloquialisms, no emojis, no decorative elements. Legal terminology per Financial Law 8/1990. Number formatting follows Yemeni conventions (comma thousands separator, period decimal).
