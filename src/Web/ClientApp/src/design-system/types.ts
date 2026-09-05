/**
 * Design Token Types — Auto-derived from tokens.ts
 * Do NOT manually define union types. Use `keyof typeof` from tokens.
 */

import {
  primitives,
  semanticColors,
  semanticColorsDark,
  statusColors,
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
  tokens,
} from './tokens';

// ── Primitive Color Keys ──────────────────────────────────────────────────────
export type PrimitiveColorGroup = keyof typeof primitives;
export type PrimitiveColorScale<T extends PrimitiveColorGroup> =
  keyof (typeof primitives)[T];

// ── Semantic Color Keys ───────────────────────────────────────────────────────
export type SemanticColorToken = keyof typeof semanticColors;
export type SemanticColorDarkToken = keyof typeof semanticColorsDark;

// ── Status Keys ───────────────────────────────────────────────────────────────
export type StatusToken = keyof typeof statusColors;

// ── Typography Keys ───────────────────────────────────────────────────────────
export type FontFamilyToken = keyof typeof fontFamily;
export type FontSizeToken = keyof typeof fontSize;
export type FontWeightToken = keyof typeof fontWeight;
export type LineHeightToken = keyof typeof lineHeight;
export type LetterSpacingToken = keyof typeof letterSpacing;

// ── Spacing Keys ──────────────────────────────────────────────────────────────
export type SpacingToken = keyof typeof spacing;

// ── Border Radius Keys ────────────────────────────────────────────────────────
export type BorderRadiusToken = keyof typeof borderRadius;

// ── Shadow Keys ───────────────────────────────────────────────────────────────
export type ShadowToken = keyof typeof shadows;

// ── Z-Index Keys ──────────────────────────────────────────────────────────────
export type ZIndexToken = keyof typeof zIndex;

// ── Breakpoint Keys ───────────────────────────────────────────────────────────
export type BreakpointToken = keyof typeof breakpoints;

// ── Transition Keys ───────────────────────────────────────────────────────────
export type TransitionDurationToken = keyof typeof transitions.duration;
export type TransitionEasingToken = keyof typeof transitions.easing;
export type TransitionPropertyToken = keyof typeof transitions.property;

// ── Border Width Keys ─────────────────────────────────────────────────────────
export type BorderWidthToken = keyof typeof borderWidth;

// ── Opacity Keys ──────────────────────────────────────────────────────────────
export type OpacityToken = keyof typeof opacity;

// ── Layout Keys ───────────────────────────────────────────────────────────────
export type LayoutToken = keyof typeof layout;

// ── Aggregate ─────────────────────────────────────────────────────────────────
export type TokenCategory = keyof Tokens;
export type Tokens = typeof tokens;

// ── Helper: Extract token value type ──────────────────────────────────────────
export type TokenValue<
  T extends Record<string, unknown>,
  K extends keyof T,
> = T[K];
