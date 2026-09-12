import { cn } from '@/lib/utils';

interface BalanceIndicatorProps {
  totalDebit: number;
  totalCredit: number;
  className?: string;
}

export function BalanceIndicator({ totalDebit = 0, totalCredit = 0, className = '' }: BalanceIndicatorProps) {
  const difference = totalDebit - totalCredit;
  const isBalanced = Math.abs(difference) < 0.001;

  return (
    <div
      dir="rtl"
      className={cn(
        'flex flex-wrap items-center justify-between gap-3 text-sm p-3 rounded-lg',
        isBalanced
          ? 'bg-[var(--color-success-bg)] text-[var(--color-success)]'
          : 'bg-[var(--color-error-container)] text-[var(--color-on-error-container)]',
        className
      )}
    >
      <div className="flex items-center gap-2">
        <span className="opacity-70">مدين:</span>
        <span className="font-bold tabular-nums">{totalDebit.toLocaleString('ar-YE')}</span>
      </div>
      <div className="flex items-center gap-2">
        <span className="opacity-70">دائن:</span>
        <span className="font-bold tabular-nums">{totalCredit.toLocaleString('ar-YE')}</span>
      </div>
      <div className="flex items-center gap-2 font-bold">
        {isBalanced ? (
          <span>متوازن</span>
        ) : (
          <span>غير متوازن بـ {Math.abs(difference).toLocaleString('ar-YE')}</span>
        )}
      </div>
    </div>
  );
}