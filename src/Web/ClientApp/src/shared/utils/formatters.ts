/**
 * Date formatting — Arabic locale.
 * Format options vary by context.
 */
export function formatDate(date: Date | string, options?: Intl.DateTimeFormatOptions): string {
  const d = typeof date === 'string' ? new Date(date) : date;
  return d.toLocaleDateString('ar-EG', options ?? { year: 'numeric', month: '2-digit', day: '2-digit' });
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
 * Number formatting — BUSINESS-CONFIRMATION pending.
 * Uses Arabic-Indic numerals by default.
 * Separators pending business confirmation.
 */
export function formatNumber(value: number, decimals = 0): string {
  return new Intl.NumberFormat('ar-EG', {
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
    return new Intl.NumberFormat('ar-EG', {
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
 * Percentage formatting.
 */
export function formatPercent(value: number, decimals = 1): string {
  return new Intl.NumberFormat('ar-EG', {
    style: 'percent',
    minimumFractionDigits: decimals,
    maximumFractionDigits: decimals,
  }).format(value / 100);
}
