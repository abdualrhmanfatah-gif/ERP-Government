/**
 * Convert a date value to an HTML date input string (YYYY-MM-DD).
 */
export function toDateInput(value: unknown): string {
  if (!value) return '';
  if (typeof value === 'string') {
    const d = new Date(value);
    return isNaN(d.getTime()) ? '' : d.toISOString().split('T')[0];
  }
  if (value instanceof Date) {
    return isNaN(value.getTime()) ? '' : value.toISOString().split('T')[0];
  }
  return '';
}

/**
 * Date formatting — Arabic locale.
 * Format options vary by context.
 */
export function formatDate(date: Date | string | null | undefined, options?: Intl.DateTimeFormatOptions): string {
  if (!date) return '—';
  const d = typeof date === 'string' ? new Date(date) : date;
  if (isNaN(d.getTime())) return '—';
  return d.toLocaleDateString('ar-YE', options ?? { year: 'numeric', month: '2-digit', day: '2-digit' });
}

export function formatDateTime(date: Date | string): string {
  return formatDate(date, {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  });
}

/**
 * Compact date with short month name — Arabic (Saudi) locale.
 * Used in dashboards and approval lists.
 */
export function formatDateCompact(date: Date | string): string {
  const d = typeof date === 'string' ? new Date(date) : date;
  return new Intl.DateTimeFormat('ar-SA', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  }).format(d);
}

/**
 * Medium date+time — Arabic (EG) locale.
 * Used in approval history panels.
 */
export function formatDateTimeMedium(value?: Date | string): string {
  if (!value) return '—';
  const d = value instanceof Date ? value : new Date(value);
  if (Number.isNaN(d.getTime())) return '—';
  return new Intl.DateTimeFormat('ar-YE', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(d);
}

/**
 * Number formatting — Arabic locale.
 */
export function formatNumber(value: number, decimals = 0): string {
  return new Intl.NumberFormat('ar-YE', {
    minimumFractionDigits: decimals,
    maximumFractionDigits: decimals,
  }).format(value);
}

/**
 * Currency display — uses Intl.NumberFormat.
 * Backend is authoritative for currency symbol/precision.
 */
export function formatCurrency(value: number, currency = 'IQD', decimals = 0): string {
  try {
    return new Intl.NumberFormat('ar-YE', {
      style: 'currency',
      currency,
      minimumFractionDigits: decimals,
      maximumFractionDigits: decimals,
    }).format(value);
  } catch {
    return `${formatNumber(value, decimals)} ${currency}`;
  }
}

/**
 * Exchange rate formatting — 6 decimals per Protocol.
 */
export function formatExchangeRate(rate: number): string {
  return formatNumber(rate, 6);
}

/**
 * Negative value display — parenthesized for financial context.
 */
export function formatNegative(value: number, decimals = 2): string {
  const abs = formatNumber(Math.abs(value), decimals);
  return value < 0 ? `(${abs})` : abs;
}

/**
 * Percentage formatting from a ratio — financial tables use Western digits and the ASCII percent sign.
 * 0.04 → "4%" · 0.366667 → "36.6667%"
 */
export function formatPercent(value: number, maxDecimals = 4): string {
  const percent = new Intl.NumberFormat('ar-YE', {
    minimumFractionDigits: 0,
    maximumFractionDigits: maxDecimals,
  }).format(value * 100);
  return `${percent}%`;
}

/**
 * Normal balance label — maps English/Russian values to Arabic.
 */
export function normalBalanceLabel(val?: string): string {
  if (!val) return '—';
  const v = val.toLowerCase();
  if (v === 'debit' || v === 'дебет') return 'مدين';
  if (v === 'credit' || v === 'кредит') return 'دائن';
  return val;
}
