import { TrendingUp, TrendingDown, DollarSign } from 'lucide-react';
import type { IncomeStatementDto } from '../types';
import { ReportSummaryCard } from './ReportSummaryCard';
import { ReportSectionCard } from './ReportSectionCard';
import { ReportDataTable } from './ReportDataTable';

interface IncomeStatementGridProps {
  data: IncomeStatementDto;
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

export function IncomeStatementGrid({ data, loading }: IncomeStatementGridProps) {
  if (loading) {
    return (
      <div className="space-y-4">
        {[1, 2, 3].map(i => (
          <div key={i} className="h-24 bg-[var(--color-surface-container)] rounded-lg animate-pulse" />
        ))}
      </div>
    );
  }

  const dateRange = `${new Date(data.startDate).toLocaleDateString('ar-YE')} — ${new Date(data.endDate).toLocaleDateString('ar-YE')}`;

  return (
    <div className="space-y-6">
      {/* Summary Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <ReportSummaryCard
          label="إجمالي الإيرادات"
          value={data.revenue.total}
          icon={<TrendingUp className="w-5 h-5" />}
          trend="positive"
        />
        <ReportSummaryCard
          label="إجمالي المصروفات"
          value={data.expenses.total}
          icon={<TrendingDown className="w-5 h-5" />}
          trend="negative"
        />
        <ReportSummaryCard
          label="صافي الدخل"
          value={data.netIncome}
          icon={<DollarSign className="w-5 h-5" />}
          trend={data.netIncome >= 0 ? 'positive' : 'negative'}
          subtitle={dateRange}
        />
      </div>

      {/* Revenue Section */}
      <ReportSectionCard
        title="الإيرادات"
        titleEn="Revenue"
        total={data.revenue.total}
      >
        {data.revenue.sections.length === 0 ? (
          <p className="text-center py-6 text-sm text-[var(--color-on-surface-variant)]">لا توجد إيرادات</p>
        ) : (
          data.revenue.sections.map((section, i) => (
            <div key={i} className="border-b border-[var(--color-border-container)] last:border-b-0">
              <div className="px-4 py-2 bg-[var(--color-surface-container-low)]">
                <p className="text-xs font-semibold text-[var(--color-on-surface-variant)]">{section.title}</p>
              </div>
              <SectionTable lines={section.lines} />
            </div>
          ))
        )}
      </ReportSectionCard>

      {/* Expenses Section */}
      <ReportSectionCard
        title="المصروفات"
        titleEn="Expenses"
        total={data.expenses.total}
      >
        {data.expenses.sections.length === 0 ? (
          <p className="text-center py-6 text-sm text-[var(--color-on-surface-variant)]">لا توجد مصروفات</p>
        ) : (
          data.expenses.sections.map((section, i) => (
            <div key={i} className="border-b border-[var(--color-border-container)] last:border-b-0">
              <div className="px-4 py-2 bg-[var(--color-surface-container-low)]">
                <p className="text-xs font-semibold text-[var(--color-on-surface-variant)]">{section.title}</p>
              </div>
              <SectionTable lines={section.lines} />
            </div>
          ))
        )}
      </ReportSectionCard>

      {/* Net Income Summary */}
      <div className={`rounded-lg border-2 p-4 ${data.netIncome >= 0 ? 'border-[var(--color-success)] bg-[var(--color-success-bg)]' : 'border-[var(--color-error)] bg-[var(--color-error-bg)]'}`}>
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            {data.netIncome >= 0 ? (
              <TrendingUp className="w-5 h-5 text-[var(--color-success)]" />
            ) : (
              <TrendingDown className="w-5 h-5 text-[var(--color-error)]" />
            )}
            <div>
              <p className="text-sm font-semibold text-[var(--color-on-surface)]">صافي الدخل</p>
              <p className="text-xs text-[var(--color-on-surface-variant)]">{dateRange}</p>
            </div>
          </div>
          <div className="text-start">
            <p className="text-xs text-[var(--color-on-surface-variant)]">الإيرادات - المصروفات</p>
            <p className={`text-xl font-bold tabular-nums ${data.netIncome >= 0 ? 'text-[var(--color-success)]' : 'text-[var(--color-error)]'}`}>
              {data.netIncome.toLocaleString()}
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
