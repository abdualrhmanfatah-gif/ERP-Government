/**
 * RTL direction detection utilities.
 * For edge cases where JavaScript needs current direction.
 */

export function getDirection(): 'rtl' | 'ltr' {
  if (typeof document === 'undefined') return 'rtl';
  return document.documentElement.dir === 'rtl' ? 'rtl' : 'ltr';
}

export function isRTL(): boolean {
  return getDirection() === 'rtl';
}

/**
 * Returns the logical start/end for the current direction.
 */
export function logicalStart(): 'left' | 'right' {
  return isRTL() ? 'right' : 'left';
}

export function logicalEnd(): 'left' | 'right' {
  return isRTL() ? 'left' : 'right';
}
