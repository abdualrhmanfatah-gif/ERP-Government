/**
 * Shared Arabic labels for common status values.
 * Use these instead of hardcoding Arabic strings in pages.
 */

/** Active/inactive status labels — use for isActive boolean fields */
export const activeStatusLabels = {
  active: 'نشط',
  inactive: 'غير نشط',
  disabled: 'معطل',
} as const;

/** Boolean yes/no labels */
export const booleanLabels = {
  true: 'نعم',
  false: 'لا',
} as const;

/** Get active status label from boolean */
export function getActiveStatusLabel(isActive: boolean): string {
  return isActive ? activeStatusLabels.active : activeStatusLabels.inactive;
}

/** Get disabled status label from boolean */
export function getDisabledStatusLabel(isActive: boolean): string {
  return isActive ? activeStatusLabels.active : activeStatusLabels.disabled;
}

/** Get boolean label */
export function getBooleanLabel(value: boolean): string {
  return booleanLabels[String(value) as keyof typeof booleanLabels];
}
