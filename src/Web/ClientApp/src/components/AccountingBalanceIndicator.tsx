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
      className={`flex flex-wrap items-center justify-between gap-3 text-sm p-3 rounded-lg ${className}`}
      style={{
        backgroundColor: isBalanced ? 'var(--color-successBg)' : 'var(--color-errorContainer)',
        color: isBalanced ? 'var(--color-success)' : 'var(--color-error)',
      }}
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