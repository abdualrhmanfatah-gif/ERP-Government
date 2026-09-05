/**
 * Resolve CSS variable to computed color value.
 * Nivo does not resolve CSS variables in color props.
 */
export function resolveCssVar(varName: string): string {
  if (typeof window === 'undefined') return varName;
  const resolved = getComputedStyle(document.documentElement).getPropertyValue(varName).trim();
  return resolved || varName;
}

/**
 * Resolve array of CSS variables to computed color values.
 */
export function resolveCssVars(vars: string[]): string[] {
  return vars.map(resolveCssVar);
}
