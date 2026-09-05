import { Scale, TrendingUp, TrendingDown } from 'lucide-react';
import type { TrialBalanceDto } from '../types';
import { ReportSummaryCard } from './ReportSummaryCard';
import { ReportSectionCard } from './ReportSectionCard';
import { ReportDataTable } from './ReportDataTable';

interface TrialBalanceGridProps {
  data: TrialBalanceDto;
  loading?: boolean;
}

function SectionTable({ lines }: { lines: { accountCode: string; accountName: string; debit: number; credit: number; balance: number }[] }) {
  const columns = [
    { key: 'account', header: 'الحساب', align: 'start' as const, render: (item: typeof lines[0]) => `${item.accountCode} - ${item.accountName}` },
    { key: 'debit', header: 'المدين', align: 'end' as const, render: (item: typeof lines[0]) => item.debit.toLocaleString() },
    { key: 'credit', header: 'الدائن', align: 'end' as const, render: (item: typeof lines[0]) => item.credit.toLocaleString() },
    { key: 'balance', header: 'الرصيد', align: 'end' as const, render: (item: typeof lines[0]) => item.balance.toLocaleString(), className: 'font-semibold' },
  ];

  return (
    <ReportDataTable
      columns={columns}
      data={lines.map(l => l as unknown as Record<string, unknown>)}
      compact
    />
  );
}

export function TrialBalanceGrid({ data, loading }: TrialBalanceGridProps) {
  if (loading) {
    return (
      <div className="space-y-4">
        {[1, 2, 3].map(i => (
          <div key={i} className="h-24 bg-[var(--color-surface-container)] rounded-lg animate-pulse" />
        ))}
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Summary Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <ReportSummaryCard
          label="إجمالي المدين"
          value={data.totalDebit}
          icon={<TrendingUp className="w-5 h-5" />}
          trend="neutral"
        />
        <ReportSummaryCard
          label="إجمالي الدائن"
          value={data.totalCredit}
          icon={<TrendingDown className="w-5 h-5" />}
          trend="neutral"
        />
        <ReportSummaryCard
          label="الفرق"
          value={Math.abs(data.totalDebit - data.totalCredit)}
          icon={<Scale className="w-5 h-5" />}
          trend={data.isBalanced ? 'positive' : 'negative'}
        />
        <ReportSummaryCard
          label="حالة التوازن"
          value={data.isBalanced ? 'متوازن' : 'غير متوازن'}
          icon={<Scale className="w-5 h-5" />}
          trend={data.isBalanced ? 'positive' : 'negative'}
          subtitle={`${data.fiscalYearName} — ${data.periodName}`}
        />
      </div>

      {/* Sections */}
      {data.sections.map((section, i) => (
        <ReportSectionCard
          key={i}
          title={section.title}
          titleEn={section.titleEn}
          total={section.total}
        >
          <SectionTable lines={section.lines} />
        </ReportSectionCard>
      ))}

      {/* Final Summary */}
      <div className={`rounded-lg border-2 p-4 ${data.isBalanced ? 'border-[var(--color-success)] bg-[var(--color-success-bg)]' : 'border-[var(--color-error)] bg-[var(--color-error-bg)]'}`}>
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <Scale className={`w-5 h-5 ${data.isBalanced ? 'text-[var(--color-success)]' : 'text-[var(--color-error)]'}`} />
            <div>
              <p className="text-sm font-semibold text-[var(--color-on-surface)]">ميزان المراجعة</p>
              <p className="text-xs text-[var(--color-on-surface-variant)]">التحقق من التوازن</p>
            </div>
          </div>
          <div className="text-start">
            <p className="text-xs text-[var(--color-on-surface-variant)]">المدين - الدائن</p>
            <p className={`text-xl font-bold tabular-nums ${data.isBalanced ? 'text-[var(--color-success)]' : 'text-[var(--color-error)]'}`}>
              {(data.totalDebit - data.totalCredit).toLocaleString()}
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
