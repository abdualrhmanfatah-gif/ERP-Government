interface ClosingEntryLine {
  accountCode: string;
  accountName: string;
  debit: number;
  credit: number;
  description?: string;
}

interface ClosingEntryLinesProps {
  lines: ClosingEntryLine[];
}

function formatAmount(value: number): string {
  return value.toLocaleString('ar-EG', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

export function ClosingEntryLines({ lines }: ClosingEntryLinesProps) {
  const totalDebit = lines.reduce((sum, l) => sum + l.debit, 0);
  const totalCredit = lines.reduce((sum, l) => sum + l.credit, 0);
  const isBalanced = Math.abs(totalDebit - totalCredit) < 0.01;

  return (
    <div className="w-full overflow-x-auto rounded-xl border border-[var(--color-border-container)] bg-[var(--color-surface-container-lowest)] shadow-sm">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b border-[var(--color-border-container)] bg-[var(--color-surface-container)]">
            <th className="px-4 py-3 text-right font-semibold text-[var(--color-on-surface)]">رقم الحساب</th>
            <th className="px-4 py-3 text-right font-semibold text-[var(--color-on-surface)]">اسم الحساب</th>
            <th className="px-4 py-3 text-end font-semibold text-[var(--color-on-surface)]">مدين</th>
            <th className="px-4 py-3 text-end font-semibold text-[var(--color-on-surface)]">دائن</th>
            <th className="px-4 py-3 text-right font-semibold text-[var(--color-on-surface)]">الوصف</th>
          </tr>
        </thead>
        <tbody>
          {lines.map((line, idx) => (
            <tr key={idx} className="border-b border-[var(--color-border-container)] last:border-0">
              <td className="px-4 py-3 font-mono text-[var(--color-on-surface)]">{line.accountCode}</td>
              <td className="px-4 py-3 text-[var(--color-on-surface)]">{line.accountName}</td>
              <td className="px-4 py-3 text-end font-mono text-[var(--color-on-surface)]">
                {line.debit > 0 ? formatAmount(line.debit) : '—'}
              </td>
              <td className="px-4 py-3 text-end font-mono text-[var(--color-on-surface)]">
                {line.credit > 0 ? formatAmount(line.credit) : '—'}
              </td>
              <td className="px-4 py-3 text-[var(--color-on-surface-variant)]">{line.description ?? '—'}</td>
            </tr>
          ))}
        </tbody>
        <tfoot>
          <tr className="border-t-2 border-[var(--color-border-container)] bg-[var(--color-surface-container)] font-semibold">
            <td colSpan={2} className="px-4 py-3 text-[var(--color-on-surface)]">الإجمالي</td>
            <td className="px-4 py-3 text-end font-mono text-[var(--color-on-surface)]">{formatAmount(totalDebit)}</td>
            <td className="px-4 py-3 text-end font-mono text-[var(--color-on-surface)]">{formatAmount(totalCredit)}</td>
            <td className="px-4 py-3">
              <span className={isBalanced ? 'text-[var(--color-success)]' : 'text-[var(--color-error)]'}>
                {isBalanced ? 'متوازن' : 'غير متوازن'}
              </span>
            </td>
          </tr>
        </tfoot>
      </table>
    </div>
  );
}
