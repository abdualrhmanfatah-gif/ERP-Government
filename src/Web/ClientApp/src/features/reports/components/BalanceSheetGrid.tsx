import { Scale, TrendingUp, TrendingDown } from 'lucide-react';
import type { BalanceSheetDto } from '../types';
import { ReportSummaryCard } from './ReportSummaryCard';
import { ReportSectionCard } from './ReportSectionCard';
import { ReportDataTable } from './ReportDataTable';

interface BalanceSheetGridProps {
  data: BalanceSheetDto;
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

export function BalanceSheetGrid({ data, loading }: BalanceSheetGridProps) {
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
          label="إجمالي الأصول"
          value={data.assets.total}
          icon={<TrendingUp className="w-5 h-5" />}
          trend="neutral"
        />
        <ReportSummaryCard
          label="إجمالي الخصوم"
          value={data.liabilities.total}
          icon={<TrendingDown className="w-5 h-5" />}
          trend="neutral"
        />
        <ReportSummaryCard
          label="إجمالي حقوق الملكية"
          value={data.equity.total}
          icon={<TrendingUp className="w-5 h-5" />}
          trend="neutral"
        />
        <ReportSummaryCard
          label="حالة التوازن"
          value={data.balanced ? 'متوازنة' : 'غير متوازنة'}
          icon={<Scale className="w-5 h-5" />}
          trend={data.balanced ? 'positive' : 'negative'}
          subtitle={`${data.currency} — ${new Date(data.asOfDate).toLocaleDateString('ar-YE')}`}
        />
      </div>

      {/* Assets Section */}
      <ReportSectionCard
        title="الأصول"
        titleEn="Assets"
        total={data.assets.total}
      >
        {data.assets.sections.map((section, i) => (
          <div key={i} className="border-b border-[var(--color-border-container)] last:border-b-0">
            <div className="px-4 py-2 bg-[var(--color-surface-container-low)]">
              <p className="text-xs font-semibold text-[var(--color-on-surface-variant)]">{section.title}</p>
            </div>
            <SectionTable lines={section.lines} />
          </div>
        ))}
      </ReportSectionCard>

      {/* Liabilities Section */}
      <ReportSectionCard
        title="الخصوم"
        titleEn="Liabilities"
        total={data.liabilities.total}
      >
        {data.liabilities.sections.length === 0 ? (
          <p className="text-center py-6 text-sm text-[var(--color-on-surface-variant)]">لا توجد خصوم</p>
        ) : (
          data.liabilities.sections.map((section, i) => (
            <div key={i} className="border-b border-[var(--color-border-container)] last:border-b-0">
              <div className="px-4 py-2 bg-[var(--color-surface-container-low)]">
                <p className="text-xs font-semibold text-[var(--color-on-surface-variant)]">{section.title}</p>
              </div>
              <SectionTable lines={section.lines} />
            </div>
          ))
        )}
      </ReportSectionCard>

      {/* Equity Section */}
      <ReportSectionCard
        title="حقوق الملكية"
        titleEn="Equity"
        total={data.equity.total}
      >
        {data.equity.sections.length === 0 ? (
          <p className="text-center py-6 text-sm text-[var(--color-on-surface-variant)]">لا توجد حقوق ملكية</p>
        ) : (
          data.equity.sections.map((section, i) => (
            <div key={i} className="border-b border-[var(--color-border-container)] last:border-b-0">
              <div className="px-4 py-2 bg-[var(--color-surface-container-low)]">
                <p className="text-xs font-semibold text-[var(--color-on-surface-variant)]">{section.title}</p>
              </div>
              <SectionTable lines={section.lines} />
            </div>
          ))
        )}
      </ReportSectionCard>

      {/* Final Summary */}
      <div className={`rounded-lg border-2 p-4 ${data.balanced ? 'border-[var(--color-success)] bg-[var(--color-success-bg)]' : 'border-[var(--color-error)] bg-[var(--color-error-bg)]'}`}>
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <Scale className={`w-5 h-5 ${data.balanced ? 'text-[var(--color-success)]' : 'text-[var(--color-error)]'}`} />
            <div>
              <p className="text-sm font-semibold text-[var(--color-on-surface)]">الميزانية العمومية</p>
              <p className="text-xs text-[var(--color-on-surface-variant)]">التحقق من التوازن</p>
            </div>
          </div>
          <div className="text-start">
            <p className="text-xs text-[var(--color-on-surface-variant)]">الخصوم + حقوق الملكية</p>
            <p className="text-xl font-bold tabular-nums text-[var(--color-on-surface)]">{data.liabilitiesAndEquity.toLocaleString()}</p>
          </div>
        </div>
      </div>
    </div>
  );
}
