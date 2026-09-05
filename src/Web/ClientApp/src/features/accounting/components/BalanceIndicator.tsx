interface BalanceIndicatorProps {
  lines: { debit: number; credit: number }[];
}

export function BalanceIndicator({ lines }: BalanceIndicatorProps) {
  const totalDebit = lines.reduce((sum, l) => sum + (l.debit || 0), 0);
  const totalCredit = lines.reduce((sum, l) => sum + (l.credit || 0), 0);
  const difference = totalDebit - totalCredit;
  const isBalanced = Math.abs(difference) < 0.001;

  return (
    <div className="flex items-center gap-4 text-xs px-3 py-2 rounded-lg bg-[var(--color-surface-container-low)]">
      <span>
        المدين: <strong className="tabular-nums">{totalDebit.toFixed(2)}</strong>
      </span>
      <span>
        الدائن: <strong className="tabular-nums">{totalCredit.toFixed(2)}</strong>
      </span>
      <span className={isBalanced ? 'text-[var(--color-success)]' : 'text-[var(--color-error)] font-semibold'}>
        الفرق: <strong className="tabular-nums">{difference.toFixed(2)}</strong>
      </span>
      {!isBalanced && (
        <span className="text-[var(--color-error)] font-bold">✗ غير متطابق</span>
      )}
    </div>
  );
}
