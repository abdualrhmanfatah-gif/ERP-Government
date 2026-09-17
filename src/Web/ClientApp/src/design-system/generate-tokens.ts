/**
 * Token Generator — Build script
 *
 * Reads tokens.ts (Single Source of Truth) and generates:
 *   1. src/design-system/tokens.css.scss  (CSS variables)
 *   2. src/design-system/tokens.tailwind.json (Tailwind consumption)
 *
 * Usage:
 *   npx tsx src/design-system/generate-tokens.ts
 *   — or via npm script: npm run generate-tokens
 */

import { writeFileSync } from 'node:fs';
import { resolve, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

// Import tokens directly (TypeScript execution via tsx)
import {
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
  borders,
  sizing,
  typography,
  density,
} from './tokens';

const __dirname = dirname(fileURLToPath(import.meta.url));

// ─── Helpers ──────────────────────────────────────────────────────────────────

function cssVar(name: string, value: string): string {
  // CSS custom properties can't contain dots — replace with dashes
  const safeName = name.replace(/\./g, '-');
  if (value === undefined || value === null || String(value).trim() === '') {
    throw new Error(`Token generation failed: empty value for --${safeName}`);
  }
  return `  --${safeName}: ${value};`;
}

// ─── Generate CSS Variables SCSS ──────────────────────────────────────────────

function generateCssScss(): string {
  const lines: string[] = [];

  lines.push(
    '// Auto-generated from tokens.ts — DO NOT EDIT MANUALLY',
    '// Run: npx tsx src/design-system/generate-tokens.ts',
    '',
    '// ── :root (Light Theme) ───────────────────────────────────────────────────',
    ':root {',
  );

  // Semantic Colors
  for (const [key, val] of Object.entries(semanticColors)) {
    const cssKey = key.replace(/([A-Z])/g, '-$1').toLowerCase();
    lines.push(cssVar(`color-${cssKey}`, val));
  }
  lines.push('');

  // shadcn-compatible aliases (bridge design tokens → shadcn Tailwind config)
  lines.push(cssVar('background', semanticColors.surface));
  lines.push(cssVar('foreground', semanticColors.onSurface));
  lines.push(cssVar('primary', semanticColors.primary));
  lines.push(cssVar('primary-foreground', semanticColors.onPrimary));
  lines.push(cssVar('secondary', semanticColors.secondary));
  lines.push(cssVar('secondary-foreground', semanticColors.onSecondary));
  lines.push(cssVar('destructive', semanticColors.error));
  lines.push(cssVar('destructive-foreground', semanticColors.onError));
  lines.push(cssVar('muted', semanticColors.surfaceContainerLow));
  lines.push(cssVar('muted-foreground', semanticColors.onSurfaceVariant));
  lines.push(cssVar('accent', semanticColors.surfaceContainer));
  lines.push(cssVar('accent-foreground', semanticColors.onSurface));
  lines.push(cssVar('popover', semanticColors.surface));
  lines.push(cssVar('popover-foreground', semanticColors.onSurface));
  lines.push(cssVar('card', semanticColors.surface));
  lines.push(cssVar('card-foreground', semanticColors.onSurface));
  lines.push(cssVar('input', semanticColors.inputBorder));
  lines.push(cssVar('ring', semanticColors.focusRing));
  lines.push('');

  // Status Colors
  for (const [status, pair] of Object.entries(statusColors)) {
    lines.push(cssVar(`status-${status}-bg`, pair.bg));
    lines.push(cssVar(`status-${status}-fg`, pair.fg));
  }
  lines.push('');

  // Chart Palette (light)
  chartPalette.light.forEach((hex, i) => {
    lines.push(cssVar(`chart-${i + 1}`, hex));
  });
  lines.push('');

  // Gradients (light)
  for (const [key, val] of Object.entries(gradients)) {
    if (!key.endsWith('Dark')) {
      lines.push(cssVar(`gradient-${key}`, val));
    }
  }
  lines.push('');

  // Typography
  for (const [key, val] of Object.entries(fontFamily)) {
    lines.push(cssVar(`font-family-${key}`, val));
  }
  for (const [key, val] of Object.entries(fontSize)) {
    lines.push(cssVar(`font-size-${key}`, val));
  }
  for (const [key, val] of Object.entries(fontWeight)) {
    lines.push(cssVar(`font-weight-${key}`, val));
  }
  for (const [key, val] of Object.entries(lineHeight)) {
    lines.push(cssVar(`line-height-${key}`, val));
  }
  for (const [key, val] of Object.entries(letterSpacing)) {
    lines.push(cssVar(`letter-spacing-${key}`, val));
  }
  lines.push('');

  // Semantic Typography Tokens (تشار للـ primitives)
  for (const [key, token] of Object.entries(typography)) {
    lines.push(cssVar(`font-size-${key}`, `var(--font-size-${token.fontSize})`));
    lines.push(cssVar(`font-weight-${key}`, `var(--font-weight-${token.fontWeight})`));
    lines.push(cssVar(`line-height-${key}`, token.lineHeight));
  }
  lines.push('');

  // Spacing
  for (const [key, val] of Object.entries(spacing)) {
    lines.push(cssVar(`spacing-${key}`, val));
  }
  lines.push('');

  // Border Radius
  for (const [key, val] of Object.entries(borderRadius)) {
    lines.push(cssVar(`radius-${key}`, val));
  }
  lines.push('');

  // Shadows
  for (const [key, val] of Object.entries(shadows)) {
    lines.push(cssVar(`shadow-${key}`, val));
  }
  lines.push('');

  // Z-Index
  for (const [key, val] of Object.entries(zIndex)) {
    lines.push(cssVar(`z-${key}`, String(val)));
  }
  lines.push('');

  // Breakpoints (as CSS custom properties for JS consumption)
  for (const [key, val] of Object.entries(breakpoints)) {
    lines.push(cssVar(`bp-${key}`, val));
  }
  lines.push('');

  // Transitions
  for (const [key, val] of Object.entries(transitions.duration)) {
    lines.push(cssVar(`duration-${key}`, val));
  }
  for (const [key, val] of Object.entries(transitions.easing)) {
    lines.push(cssVar(`easing-${key}`, val));
  }
  lines.push('');

  // Border Width
  for (const [key, val] of Object.entries(borderWidth)) {
    lines.push(cssVar(`border-w-${key}`, val));
  }
  lines.push('');

  // Opacity
  for (const [key, val] of Object.entries(opacity)) {
    lines.push(cssVar(`opacity-${key}`, val));
  }
  lines.push('');

  // Layout
  for (const [key, val] of Object.entries(layout)) {
    const cssKey = key.replace(/([A-Z])/g, '-$1').toLowerCase();
    lines.push(cssVar(cssKey, val));
  }
  lines.push('');

  // Density roles (mode-independent)
  for (const [level, roles] of Object.entries(density)) {
    for (const [role, val] of Object.entries(roles)) {
      const cssKey = role.replace(/([A-Z])/g, '-$1').toLowerCase();
      lines.push(cssVar(`density-${level}-${cssKey}`, val));
    }
  }
  lines.push('');

  // Border composites
  for (const [key, val] of Object.entries(borders)) {
    if (!key.endsWith('Dark')) {
      lines.push(cssVar(`border-${key}`, val));
    }
  }

  lines.push('}');
  lines.push('');

  // ── Dark Theme ────────────────────────────────────────────────────────────
  lines.push('// ── .dark (Dark Theme) ────────────────────────────────────────────────────');
  lines.push('.dark {');

  for (const [key, val] of Object.entries(semanticColorsDark)) {
    const cssKey = key.replace(/([A-Z])/g, '-$1').toLowerCase();
    lines.push(cssVar(`color-${cssKey}`, val));
  }
  lines.push('');

  // shadcn-compatible aliases (dark theme)
  lines.push(cssVar('background', semanticColorsDark.surface));
  lines.push(cssVar('foreground', semanticColorsDark.onSurface));
  lines.push(cssVar('primary', semanticColorsDark.primary));
  lines.push(cssVar('primary-foreground', semanticColorsDark.onPrimary));
  lines.push(cssVar('secondary', semanticColorsDark.secondary));
  lines.push(cssVar('secondary-foreground', semanticColorsDark.onSecondary));
  lines.push(cssVar('destructive', semanticColorsDark.error));
  lines.push(cssVar('destructive-foreground', semanticColorsDark.onError));
  lines.push(cssVar('muted', semanticColorsDark.surfaceContainerLow));
  lines.push(cssVar('muted-foreground', semanticColorsDark.onSurfaceVariant));
  lines.push(cssVar('accent', semanticColorsDark.surfaceContainer));
  lines.push(cssVar('accent-foreground', semanticColorsDark.onSurface));
  lines.push(cssVar('popover', semanticColorsDark.surface));
  lines.push(cssVar('popover-foreground', semanticColorsDark.onSurface));
  lines.push(cssVar('card', semanticColorsDark.surface));
  lines.push(cssVar('card-foreground', semanticColorsDark.onSurface));
  lines.push(cssVar('input', semanticColorsDark.inputBorder));
  lines.push(cssVar('ring', semanticColorsDark.focusRing));
  lines.push('');

  for (const [status, pair] of Object.entries(statusColorsDark)) {
    lines.push(cssVar(`status-${status}-bg`, pair.bg));
    lines.push(cssVar(`status-${status}-fg`, pair.fg));
  }
  lines.push('');

  // Chart Palette (dark)
  chartPalette.dark.forEach((hex, i) => {
    lines.push(cssVar(`chart-${i + 1}`, hex));
  });
  lines.push('');

  // Gradients (dark)
  for (const [key, val] of Object.entries(gradients)) {
    if (key.endsWith('Dark')) {
      const baseKey = key.replace('Dark', '');
      lines.push(cssVar(`gradient-${baseKey}`, val));
    }
  }
  lines.push('');

  // Dark border composites
  for (const [key, val] of Object.entries(borders)) {
    if (key.endsWith('Dark')) {
      const baseKey = key.replace('Dark', '');
      lines.push(cssVar(`border-${baseKey}`, val));
    }
  }
  // Extract base border color from containerDark composite ("1px solid #233144" → "#233144")
  const darkBorderMatch = (borders.containerDark as string).match(/#[0-9a-fA-F]{3,8}/);
  if (darkBorderMatch) {
    lines.push(cssVar('border', darkBorderMatch[0]));
  }

  lines.push('}');

  return lines.join('\n') + '\n';
}

// ─── Generate Tailwind JSON ───────────────────────────────────────────────────

function generateTailwindJson(): string {
  const tailwindConfig = {
    colors: {
      // Mapped directly from semanticColors
      border: {
        DEFAULT: 'var(--border)',
        input: 'var(--color-input-border)',
        container: 'var(--color-container-border)',
        'outline-variant': 'var(--color-outline-variant)',
      },
      input: 'var(--input)',
      ring: 'var(--ring)',
      background: 'var(--background)',
      foreground: 'var(--foreground)',
      primary: {
        DEFAULT: 'var(--primary)',
        foreground: 'var(--primary-foreground)',
        container: 'var(--color-primary-container)',
        'on-container': 'var(--color-on-primary-container)',
        inverse: 'var(--color-inverse-primary)',
      },
      secondary: {
        DEFAULT: 'var(--secondary)',
        foreground: 'var(--secondary-foreground)',
        container: 'var(--color-secondary-container)',
        'on-container': 'var(--color-on-secondary-container)',
      },
      destructive: {
        DEFAULT: 'var(--destructive)',
        foreground: 'var(--destructive-foreground)',
      },
      muted: {
        DEFAULT: 'var(--muted)',
        foreground: 'var(--muted-foreground)',
      },
      accent: {
        DEFAULT: 'var(--accent)',
        foreground: 'var(--accent-foreground)',
      },
      popover: {
        DEFAULT: 'var(--popover)',
        foreground: 'var(--popover-foreground)',
      },
      card: {
        DEFAULT: 'var(--card)',
        foreground: 'var(--card-foreground)',
      },
      error: {
        DEFAULT: 'var(--color-error)',
        container: 'var(--color-error-container)',
        'on-container': 'var(--color-on-error-container)',
      },
      success: {
        DEFAULT: 'var(--color-success)',
        bg: 'var(--color-success-bg)',
      },
      warning: {
        DEFAULT: 'var(--color-warning)',
        bg: 'var(--color-warning-bg)',
      },
      info: {
        DEFAULT: 'var(--color-info)',
        bg: 'var(--color-info-bg)',
      },
      surface: {
        DEFAULT: 'var(--color-surface)',
        'container-lowest': 'var(--color-surface-container-lowest)',
        'container-low': 'var(--color-surface-container-low)',
        container: 'var(--color-surface-container)',
        'container-high': 'var(--color-surface-container-high)',
        'container-highest': 'var(--color-surface-container-highest)',
      },
      'on-primary': 'var(--color-on-primary)',
      'on-secondary': 'var(--color-on-secondary)',
      'on-error': 'var(--color-on-error)',
      'on-surface': 'var(--color-on-surface)',
      'on-surface-variant': 'var(--color-on-surface-variant)',
      'input-border': 'var(--color-input-border)',
      'container-border': 'var(--color-container-border)',
      status: {
        'draft-bg': 'var(--status-draft-bg)',
        'draft-fg': 'var(--status-draft-fg)',
        'pending-bg': 'var(--status-pending-bg)',
        'pending-fg': 'var(--status-pending-fg)',
        'approved-bg': 'var(--status-approved-bg)',
        'approved-fg': 'var(--status-approved-fg)',
        'active-bg': 'var(--status-active-bg)',
        'active-fg': 'var(--status-active-fg)',
        'closed-bg': 'var(--status-closed-bg)',
        'closed-fg': 'var(--status-closed-fg)',
        // Aliases resolve to their base role pair (contracts/status-semantics.md §2)
        'posted-bg': 'var(--status-closed-bg)',
        'posted-fg': 'var(--status-closed-fg)',
        'reversed-bg': 'var(--status-closed-bg)',
        'reversed-fg': 'var(--status-closed-fg)',
        'cancelled-bg': 'var(--status-closed-bg)',
        'cancelled-fg': 'var(--status-closed-fg)',
        'locked-bg': 'var(--status-closed-bg)',
        'locked-fg': 'var(--status-closed-fg)',
        'overBudget-bg': 'var(--status-pending-bg)',
        'overBudget-fg': 'var(--status-pending-fg)',
        'unbalanced-border': 'var(--status-pending-fg)',
        'unbalanced-fg': 'var(--status-pending-fg)',
        'inactive-bg': 'var(--status-inactive-bg)',
        'inactive-fg': 'var(--status-inactive-fg)',
      },
      link: 'var(--color-link)',
      tertiary: {
        DEFAULT: 'var(--color-tertiary)',
        container: 'var(--color-tertiary-container)',
        'on-container': 'var(--color-on-tertiary-container)',
      },
      chart: {
        1: 'var(--chart-1)',
        2: 'var(--chart-2)',
        3: 'var(--chart-3)',
        4: 'var(--chart-4)',
        5: 'var(--chart-5)',
        6: 'var(--chart-6)',
      },
      'surface-tint': 'var(--color-surface-tint)',
    },
    fontFamily: Object.fromEntries(
      Object.entries(fontFamily).map(([k, v]) => [k, v.split(',').map(s => s.trim().replace(/"/g, ''))])
    ),
    fontSize: {
      // Primitive: text-xs, text-sm, text-lg
      ...Object.fromEntries(
        Object.entries(fontSize).map(([key]) => [
          key,
          [`var(--font-size-${key})`, {}]
        ])
      ),

      // Semantic: text-headline-lg, text-body-lg, text-label-md
      ...Object.fromEntries(
        Object.entries(typography).map(([key, token]) => [
          key,
          [
            `var(--font-size-${key})`,
            {
              lineHeight: token.lineHeight,
              fontWeight: `var(--font-weight-${token.fontWeight})`,
            }
          ]
        ])
      ),
    },
    fontWeight: Object.fromEntries(
      Object.entries(fontWeight).map(([key]) => [
        key,
        `var(--font-weight-${key})`
      ])
    ),
    lineHeight: Object.fromEntries(
      Object.entries(lineHeight).map(([key]) => [
        key,
        `var(--line-height-${key})`
      ])
    ),
    spacing: Object.fromEntries(
      Object.entries(spacing).map(([k, v]) => [k, v])
    ),
    borderRadius: {
      lg: 'var(--radius-lg)',
      md: 'var(--radius-md)',
      sm: 'var(--radius-sm)',
      xl: 'var(--radius-xl)',
      full: 'var(--radius-full)',
    },
    width: {
      sidebar: 'var(--sidebar-width)',
    },
    height: {
      header: 'var(--header-height)',
    },
    maxWidth: {
      sidebar: 'var(--sidebar-width)',
      dialog: sizing.maxWidth.dialog,
    },
    minHeight: {
      header: 'var(--header-height)',
    },
    boxShadow: {
      transient: 'var(--shadow-transient)',
    },
    backgroundImage: {
      'gradient-sidebar': 'var(--gradient-sidebar)',
    },
    transitionDuration: {
      fast: 'var(--duration-fast)',
      DEFAULT: 'var(--duration-normal)',
    },
    transitionTimingFunction: {
      DEFAULT: 'var(--easing-default)',
    },
    zIndex: {
      header: String(zIndex.header),
      sidebar: String(zIndex.sidebar),
      dropdown: String(zIndex.dropdown),
      dialog: String(zIndex.dialog),
      toast: String(zIndex.toast),
    },
  };

  return JSON.stringify(tailwindConfig, null, 2);
}

// ─── Main ─────────────────────────────────────────────────────────────────────

function main() {
  const outDir = resolve(__dirname);

  // Generate CSS variables SCSS
  const cssContent = generateCssScss();
  const cssPath = resolve(outDir, 'tokens.css.scss');
  writeFileSync(cssPath, cssContent, 'utf-8');
  console.log(`✓ Generated ${cssPath}`);

  // Generate Tailwind JSON
  const tailwindContent = generateTailwindJson();
  const tailwindPath = resolve(outDir, 'tokens.tailwind.json');
  writeFileSync(tailwindPath, tailwindContent, 'utf-8');
  console.log(`✓ Generated ${tailwindPath}`);
}

main();
