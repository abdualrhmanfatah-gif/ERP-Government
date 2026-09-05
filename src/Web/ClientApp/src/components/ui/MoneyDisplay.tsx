import { cn } from '@/lib/utils';

interface MoneyDisplayProps {
  value: number | string;
  currency?: string;
  className?: string;
}

function formatMoney(value: number | string): string {
  const num = typeof value === 'string' ? parseFloat(value) : value;

  if (isNaN(num)) {
    return '0.00';
  }

  const formatted = new Intl.NumberFormat('ar-EG', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(Math.abs(num));

  if (num < 0) {
    return `(${formatted})`;
  }

  return formatted;
}

export function MoneyDisplay({
  value,
  currency,
  className,
}: MoneyDisplayProps) {
  return (
    <span
      className={cn(
        'tabular-nums text-end font-mono',
        className
      )}
    >
      {formatMoney(value)}
      {currency && (
        <span className="ms-1 text-[var(--color-on-surface-variant)]">{currency}</span>
      )}
    </span>
  );
}
