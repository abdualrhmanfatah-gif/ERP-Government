/**
 * Design System — Public API
 *
 * Import from here:
 *   import { tokens, semanticColors, spacing } from '@/design-system';
 */

export {
  primitives,
  semanticColors,
  semanticColorsDark,
  statusColors,
  statusColorsDark,
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
  tokens,
} from './tokens';

export type {
  PrimitiveColorGroup,
  PrimitiveColorScale,
  SemanticColorToken,
  SemanticColorDarkToken,
  StatusToken,
  FontFamilyToken,
  FontSizeToken,
  FontWeightToken,
  LineHeightToken,
  LetterSpacingToken,
  SpacingToken,
  BorderRadiusToken,
  ShadowToken,
  ZIndexToken,
  BreakpointToken,
  TransitionDurationToken,
  TransitionEasingToken,
  TransitionPropertyToken,
  BorderWidthToken,
  OpacityToken,
  LayoutToken,
  TokenCategory,
  Tokens,
  TokenValue,
} from './types';
