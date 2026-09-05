/**
 * Design Tokens — Single Source of Truth
 * Authority: docs/analysis/00-governance/DESIGN-SYSTEM-INSTITUTIONAL-PROTOCOL.md
 *
 * All design values MUST originate from this file.
 * Do NOT invent colors, fonts, spacing, radii, or shadows.
 *
 * Architecture:
 *   tokens.ts → generate-tokens.ts → tokens.css.scss + tokens.tailwind.json
 */

// ═══════════════════════════════════════════════════════════════════════════════
// 1. PRIMITIVE TOKENS — Raw values, no semantic meaning
// ═══════════════════════════════════════════════════════════════════════════════

export const primitives = {
  /** Navy / Primary palette — Protocol Section 3 */
  navy: {
    50: '#e6eeff',
    100: '#86a0cd',
    200: '#4a6fa5',
    300: '#2d476f',
    400: '#1a365d',
    500: '#002045',
    600: '#001b3c',
    700: '#001430',
    800: '#000e24',
    900: '#000818',
    950: '#00040c',
  } as const,

  /** Gold / Secondary palette — Protocol Section 3 */
  gold: {
    50: '#fef9c3',
    100: '#fed255',
    200: '#ecc246',
    300: '#d4a82a',
    400: '#b8911a',
    500: '#755b00',
    600: '#735a00',
    700: '#584400',
    800: '#3d2e00',
    900: '#241a00',
    950: '#120d00',
  } as const,

  /** Error / Red palette — Protocol Section 3 */
  red: {
    50: '#ffdad6',
    100: '#ffb4ab',
    200: '#ff8a7d',
    300: '#ff5c4d',
    400: '#e53935',
    500: '#ba1a1a',
    600: '#93000a',
    700: '#690005',
    800: '#4a0003',
    900: '#2d0002',
    950: '#170001',
  } as const,

  /** Neutral / Slate palette — Protocol Section 3 */
  slate: {
    50: '#f8f9ff',
    100: '#e2e8f0',
    200: '#dce4f0',
    300: '#c8d4e8',
    400: '#b0c0d8',
    500: '#ccdbf4',
    600: '#a8adb8',
    700: '#8e9099',
    800: '#636870',
    900: '#475569',
    950: '#43474e',
  } as const,

  /** Blue / Info palette */
  blue: {
    50: '#dbeafe',
    100: '#93c5fd',
    200: '#60a5fa',
    300: '#3b82f6',
    400: '#2563eb',
    500: '#1d4ed8',
    600: '#1e40af',
    700: '#1e3a8a',
    800: '#1e3078',
    900: '#1a2866',
    950: '#15204f',
  } as const,

  /** Green / Success palette */
  green: {
    50: '#dcfce7',
    100: '#86efac',
    200: '#4ade80',
    300: '#22c55e',
    400: '#16a34a',
    500: '#166534',
    600: '#15803d',
    700: '#14532d',
    800: '#0f3d21',
    900: '#0a2715',
    950: '#05140b',
  } as const,

  /** Yellow / Warning palette */
  yellow: {
    50: '#fef9c3',
    100: '#fde047',
    200: '#facc15',
    300: '#eab308',
    400: '#ca8a04',
    500: '#854d0e',
    600: '#713f12',
    700: '#422006',
    800: '#2d1604',
    900: '#1e0f03',
    950: '#0f0701',
  } as const,

  /** White */
  white: '#ffffff',

  /** Black */
  black: '#000000',
} as const;

// ═══════════════════════════════════════════════════════════════════════════════
// 2. SEMANTIC TOKENS — Meaning-based aliases over primitives
// ═══════════════════════════════════════════════════════════════════════════════

export const semanticColors = {
  // ── Primary ────────────────────────────────────────────────────────────────
  primary: primitives.navy[500],
  onPrimary: primitives.white,
  primaryContainer: primitives.navy[400],
  onPrimaryContainer: primitives.navy[100],

  // ── Secondary ──────────────────────────────────────────────────────────────
  secondary: primitives.gold[500],
  onSecondary: primitives.white,
  secondaryContainer: primitives.gold[100],
  onSecondaryContainer: primitives.gold[600],

  // ── Error ──────────────────────────────────────────────────────────────────
  error: primitives.red[500],
  onError: primitives.white,
  errorContainer: primitives.red[50],
  onErrorContainer: primitives.red[600],

  // ── Surface ────────────────────────────────────────────────────────────────
  surface: primitives.white,
  surfaceContainerLowest: primitives.white,
  surfaceContainerLow: '#d1d9e6',
  surfaceContainer: primitives.slate[200],
  surfaceContainerHigh: primitives.slate[300],
  surfaceContainerHighest: primitives.slate[400],
  onSurface: '#0d1c2f',
  onSurfaceVariant: primitives.slate[950],

  // ── Tertiary — Protocol Section 3 ────────────────────────────────────────
  tertiary: '#1d2123',
  onTertiary: primitives.white,
  tertiaryContainer: '#333638',
  onTertiaryContainer: '#9c9fa1',

  // ── Outline ────────────────────────────────────────────────────────────────
  outline: primitives.slate[800],
  outlineVariant: primitives.slate[600],

  // ── Border (functional) ───────────────────────────────────────────────────
  inputBorder: primitives.slate[800],      // #636870 — 3:1+ against white
  containerBorder: '#c8d0dc',               // #c8d0dc — section borders

  // ── Surface Tint — Protocol Section 3 ────────────────────────────────────
  surfaceTint: '#455f88',

  // ── Semantic ───────────────────────────────────────────────────────────────
  success: primitives.green[500],
  successBg: primitives.green[50],
  warning: primitives.yellow[500],
  warningBg: primitives.yellow[50],
  info: primitives.blue[500],
  infoBg: primitives.blue[50],
  link: primitives.blue[500],

  // ── Focus ──────────────────────────────────────────────────────────────────
  focusRing: primitives.gold[500],
  focusHalo: primitives.gold[100],

  // ── Disabled ───────────────────────────────────────────────────────────────
  disabledBg: '#f1f5f9',
  disabledFg: '#94a3b8',
} as const;

export const semanticColorsDark = {
  // ── Primary ────────────────────────────────────────────────────────────────
  primary: primitives.navy[100],
  onPrimary: primitives.navy[600],
  primaryContainer: primitives.navy[300],
  onPrimaryContainer: primitives.navy[50],

  // ── Secondary ──────────────────────────────────────────────────────────────
  secondary: primitives.gold[200],
  onSecondary: primitives.gold[900],
  secondaryContainer: primitives.gold[700],
  onSecondaryContainer: primitives.gold[50],

  // ── Error ──────────────────────────────────────────────────────────────────
  error: primitives.red[100],
  onError: primitives.red[700],
  errorContainer: primitives.red[600],
  onErrorContainer: primitives.red[50],

  // ── Surface ────────────────────────────────────────────────────────────────
  surface: '#0d1c2f',
  surfaceContainerLowest: '#07101c',
  surfaceContainerLow: '#0d1c2f',
  surfaceContainer: '#122238',
  surfaceContainerHigh: primitives.navy[400],
  surfaceContainerHighest: '#233144',
  onSurface: '#ebf1ff',
  onSurfaceVariant: '#c4c6cf',

  // ── Tertiary — Protocol Section 3 ────────────────────────────────────────
  tertiary: '#c4c7c9',
  onTertiary: '#191c1e',
  tertiaryContainer: '#444749',
  onTertiaryContainer: '#e0e3e5',

  // ── Outline ────────────────────────────────────────────────────────────────
  outline: primitives.slate[700],
  outlineVariant: primitives.slate[950],

  // ── Surface Tint — Protocol Section 3 ────────────────────────────────────
  surfaceTint: '#adc7f7',

  // ── Semantic ───────────────────────────────────────────────────────────────
  success: primitives.green[100],
  successBg: primitives.green[700],
  warning: primitives.yellow[100],
  warningBg: primitives.yellow[700],
  info: primitives.blue[100],
  infoBg: primitives.blue[700],
  link: primitives.navy[100],

  // ── Focus ──────────────────────────────────────────────────────────────────
  focusRing: primitives.gold[200],
  focusHalo: primitives.gold[700],

  // ── Disabled ───────────────────────────────────────────────────────────────
  disabledBg: primitives.navy[400],
  disabledFg: primitives.slate[700],

  // ── Inverse ────────────────────────────────────────────────────────────────
  inversePrimary: '#455f88',
} as const;

/** Status pair tokens — Protocol Section 5 */
export const statusColors = {
  draft: { bg: '#f1f5f9', fg: '#475569' },
  pending: { bg: primitives.yellow[50], fg: primitives.yellow[500] },
  approved: { bg: primitives.green[50], fg: primitives.green[500] },
  active: { bg: primitives.blue[50], fg: primitives.blue[500] },
  closed: { bg: '#e2e8f0', fg: '#1e293b' },
} as const;

export const statusColorsDark = {
  draft: { bg: '#334155', fg: '#cbd5e1' },
  pending: { bg: primitives.yellow[700], fg: primitives.yellow[100] },
  approved: { bg: primitives.green[700], fg: primitives.green[100] },
  active: { bg: primitives.blue[700], fg: primitives.blue[100] },
  closed: { bg: '#1e293b', fg: '#cbd5e1' },
} as const;

// ═══════════════════════════════════════════════════════════════════════════════
// 2B. CHART PALETTE — Protocol Section 34 — categorical sequence
// ═══════════════════════════════════════════════════════════════════════════════

export const chartPalette = {
  light: [
    primitives.navy[500],    // 1. #002045 primary
    primitives.gold[500],    // 2. #755b00 secondary
    primitives.blue[400],    // 3. #2563eb status Active fg
    primitives.green[500],   // 4. #166534 success
    '#455f88',               // 5. surfaceTint
    primitives.red[500],     // 6. #ba1a1a error
  ] as const,
  dark: [
    primitives.navy[100],    // 1. #adc7f7 primary
    primitives.gold[200],    // 2. #ecc246 secondary
    primitives.blue[100],    // 3. #93c5fd status Active fg
    primitives.green[100],   // 4. #86efac success
    '#c4c7c9',               // 5. tertiary
    primitives.red[100],     // 6. #ffb4ab error
  ] as const,
} as const;

// ═══════════════════════════════════════════════════════════════════════════════
// 2C. GRADIENT COMPOSITES — same-hue navy blends, no invented colors
// ═══════════════════════════════════════════════════════════════════════════════

export const gradients = {
  /** Sidebar: navy-500 → navy-400 vertical — Protocol §34 colors */
  sidebar: `linear-gradient(180deg, ${primitives.navy[500]} 0%, ${primitives.navy[400]} 100%)`,
  /** Sidebar dark: navy-600 → navy-700 vertical */
  sidebarDark: `linear-gradient(180deg, ${primitives.navy[600]} 0%, ${primitives.navy[700]} 100%)`,
} as const;

// ═══════════════════════════════════════════════════════════════════════════════
// 3. TYPOGRAPHY — Protocol Section 6
// ═══════════════════════════════════════════════════════════════════════════════

export const fontFamily = {
  sans: '"IBM Plex Sans Arabic", "IBM Plex Sans", "Segoe UI", Tahoma, sans-serif',
  heading: '"IBM Plex Sans Arabic", "IBM Plex Sans", "Segoe UI", Tahoma, sans-serif',
  mono: '"IBM Plex Mono", ui-monospace, monospace',
} as const;

export const fontSize = {
  /** 13px — label-sm (Protocol: 13px, 700w) */
  xs: '0.8125rem',
  /** 14px — body-sm / label-md (Protocol: +2px scale) */
  sm: '0.875rem',
  /** 16px — body-md (Protocol: +2px scale) */
  base: '1rem',
  /** 18px — body-lg (Protocol: +2px scale) */
  lg: '1.125rem',
  /** 20px — headline-sm (Protocol: +2px scale) */
  xl: '1.25rem',
  /** 22px — headline-md (Protocol: +2px scale) */
  '2xl': '1.375rem',
  /** 26px — headline-lg (Protocol: +2px scale) */
  '3xl': '1.625rem',
  /** 32px — display */
  '4xl': '2rem',
  /** 36px — display-lg */
  '5xl': '2.25rem',
  /** 42px — display-xl */
  '6xl': '2.625rem',
} as const;

export const fontWeight = {
  thin: '100',
  extralight: '200',
  light: '300',
  normal: '400',
  medium: '500',
  semibold: '600',
  bold: '700',
  extrabold: '800',
  black: '900',
} as const;

export const lineHeight = {
  none: '1',
  tight: '1.25',
  snug: '1.375',
  normal: '1.5',
  relaxed: '1.75',
  loose: '2',
} as const;

// ── Semantic Typography Tokens ──────────────────────────────────────────────
export interface TypographyToken {
  fontSize: keyof typeof fontSize;
  fontWeight: keyof typeof fontWeight;
  lineHeight: string; // Absolute value for line-height
}

export const typography = {
  'headline-lg': { fontSize: '4xl', fontWeight: 'bold', lineHeight: '2.5rem' } as TypographyToken,
  'headline-md': { fontSize: '3xl', fontWeight: 'bold', lineHeight: '2.125rem' } as TypographyToken,
  'headline-sm': { fontSize: '2xl', fontWeight: 'bold', lineHeight: '1.875rem' } as TypographyToken,
  'body-lg': { fontSize: 'lg', fontWeight: 'medium', lineHeight: '1.75rem' } as TypographyToken,
  'body-md': { fontSize: 'base', fontWeight: 'medium', lineHeight: '1.5rem' } as TypographyToken,
  'body-sm': { fontSize: 'sm', fontWeight: 'medium', lineHeight: '1.25rem' } as TypographyToken,
  'label-md': { fontSize: 'sm', fontWeight: 'semibold', lineHeight: '1.25rem' } as TypographyToken,
  'label-sm': { fontSize: 'xs', fontWeight: 'bold', lineHeight: '1.125rem' } as TypographyToken,
} as const;

export const letterSpacing = {
  tighter: '-0.05em',
  tight: '-0.025em',
  normal: '0em',
  wide: '0.025em',
  wider: '0.05em',
  widest: '0.1em',
} as const;

// ═══════════════════════════════════════════════════════════════════════════════
// 5. SPACING — 8px base grid — Protocol Section 8
// ═══════════════════════════════════════════════════════════════════════════════

export const spacing = {
  0: '0',
  px: '1px',
  0.5: '0.125rem',
  1: '0.25rem',
  1.5: '0.375rem',
  2: '0.5rem',
  2.5: '0.625rem',
  3: '0.75rem',
  3.5: '0.875rem',
  4: '1rem',
  5: '1.25rem',
  6: '1.5rem',
  7: '1.75rem',
  8: '2rem',
  9: '2.25rem',
  10: '2.5rem',
  11: '2.75rem',
  12: '3rem',
  14: '3.5rem',
  16: '4rem',
  20: '5rem',
  24: '6rem',
  28: '7rem',
  32: '8rem',
  36: '9rem',
  40: '10rem',
  44: '11rem',
  48: '12rem',
  52: '13rem',
  56: '14rem',
  60: '15rem',
  64: '16rem',
  72: '18rem',
  80: '20rem',
  96: '24rem',
} as const;

// ═══════════════════════════════════════════════════════════════════════════════
// 6. BORDER RADIUS — Protocol Section 9
// ═══════════════════════════════════════════════════════════════════════════════

export const borderRadius = {
  none: '0',
  xs: '0.0625rem',
  sm: '0.125rem',
  base: '0.25rem',
  md: '0.375rem',
  lg: '0.5rem',
  xl: '0.75rem',
  '2xl': '1rem',
  '3xl': '1.5rem',
  full: '9999px',
} as const;

// ═══════════════════════════════════════════════════════════════════════════════
// 7. SHADOWS — Protocol Section 19
// ═══════════════════════════════════════════════════════════════════════════════

export const shadows = {
  none: 'none',
  xs: '0 1px 2px 0 rgb(0 0 0 / 0.05)',
  sm: '0 1px 3px 0 rgb(0 0 0 / 0.1), 0 1px 2px -1px rgb(0 0 0 / 0.1)',
  md: '0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1)',
  lg: '0 10px 15px -3px rgb(0 0 0 / 0.1), 0 4px 6px -4px rgb(0 0 0 / 0.1)',
  xl: '0 20px 25px -5px rgb(0 0 0 / 0.1), 0 8px 10px -6px rgb(0 0 0 / 0.1)',
  '2xl': '0 25px 50px -12px rgb(0 0 0 / 0.25)',
  inner: 'inset 0 2px 4px 0 rgb(0 0 0 / 0.05)',
  transient: '0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06)',
} as const;

// ═══════════════════════════════════════════════════════════════════════════════
// 8. Z-INDEX — Protocol Section 12
// ═══════════════════════════════════════════════════════════════════════════════

export const zIndex = {
  hide: -1,
  base: 0,
  dropdown: 300,
  sticky: 100,
  fixed: 1020,
  modalBackdrop: 1030,
  modal: 1040,
  popover: 1050,
  toast: 500,
  tooltip: 1070,
  // Project-specific (AppLayout) — Protocol Section 10
  header: 100,
  sidebar: 200,
  dialog: 400,
} as const;

// ═══════════════════════════════════════════════════════════════════════════════
// 9. BREAKPOINTS
// ═══════════════════════════════════════════════════════════════════════════════

export const breakpoints = {
  xs: '0px',
  sm: '576px',
  md: '768px',
  lg: '1024px',
  xl: '1280px',
  '2xl': '1536px',
  desktopMin: '960px',
  tabletMax: '959px',
} as const;

// ═══════════════════════════════════════════════════════════════════════════════
// 10. TRANSITIONS & MOTION — Protocol Section 20, 33
// ═══════════════════════════════════════════════════════════════════════════════

export const transitions = {
  duration: {
    instant: '0ms',
    fast: '100ms',
    base: '150ms',
    normal: '200ms',
    slow: '300ms',
    slower: '500ms',
  },
  easing: {
    linear: 'linear',
    easeIn: 'cubic-bezier(0.4, 0, 1, 1)',
    easeOut: 'cubic-bezier(0, 0, 0.2, 1)',
    easeInOut: 'cubic-bezier(0.4, 0, 0.2, 1)',
    /** Default from Protocol Section 20 */
    default: 'cubic-bezier(0.2, 0, 0, 1)',
  },
  property: {
    all: 'all',
    colors: 'color, background-color, border-color, text-decoration-color, fill, stroke',
    opacity: 'opacity',
    transform: 'transform',
    shadow: 'box-shadow',
  },
} as const;

// ═══════════════════════════════════════════════════════════════════════════════
// 11. BORDER WIDTH
// ═══════════════════════════════════════════════════════════════════════════════

export const borderWidth = {
  0: '0',
  1: '1px',
  2: '2px',
  4: '4px',
  8: '8px',
} as const;

// ═══════════════════════════════════════════════════════════════════════════════
// 12. OPACITY
// ═══════════════════════════════════════════════════════════════════════════════

export const opacity = {
  0: '0',
  5: '0.05',
  10: '0.1',
  20: '0.2',
  25: '0.25',
  30: '0.3',
  40: '0.4',
  50: '0.5',
  60: '0.6',
  70: '0.7',
  75: '0.75',
  80: '0.8',
  90: '0.9',
  95: '0.95',
  100: '1',
} as const;

// ═══════════════════════════════════════════════════════════════════════════════
// 13. LAYOUT — Protocol Section 12
// ═══════════════════════════════════════════════════════════════════════════════

export const layout = {
  headerHeight: '64px',
  sidebarWidth: '280px',
  pageGutter: '1.5rem',
  containerPadding: '2rem',
} as const;

// ═══════════════════════════════════════════════════════════════════════════════
// 14. SIZING
// ═══════════════════════════════════════════════════════════════════════════════

export const sizing = {
  icon: {
    sm: '16px',
    md: '18px',
    lg: '20px',
    xl: '24px',
  },
  control: {
    height: {
      sm: '32px',
      md: '36px',
      lg: '40px',
    },
    minWidth: {
      sm: '64px',
      md: '80px',
      lg: '120px',
    },
  },
  maxWidth: {
    dialog: '32rem',
  },
} as const;

// ═══════════════════════════════════════════════════════════════════════════════
// 15. BORDERS (composite)
// ═══════════════════════════════════════════════════════════════════════════════

export const borders = {
  container: `1px solid ${semanticColors.containerBorder}`,
  containerDark: `1px solid #233144`,
  divider: `1px solid ${primitives.slate[50]}`,
  control: `1px solid ${semanticColors.outlineVariant}`,
  input: `1px solid ${semanticColors.inputBorder}`,
} as const;

// ═══════════════════════════════════════════════════════════════════════════════
// 16. AGGREGATE — All tokens in one object (for programmatic access)
// ═══════════════════════════════════════════════════════════════════════════════

export const tokens = {
  primitives,
  semanticColors,
  semanticColorsDark,
  statusColors,
  statusColorsDark,
  chartPalette,
  gradients,
  fontFamily,
  fontSize,
  fontWeight,
  lineHeight,
  letterSpacing,
  spacing,
  borderRadius,
  shadows,
  zIndex,
  breakpoints,
  transitions,
  borderWidth,
  opacity,
  layout,
  sizing,
  borders,
} as const;

export type Tokens = typeof tokens;
