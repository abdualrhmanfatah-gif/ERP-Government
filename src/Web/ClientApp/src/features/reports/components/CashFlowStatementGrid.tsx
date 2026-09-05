import { ArrowDownRight, ArrowUpRight, Wallet, AlertTriangle } from 'lucide-react';
import type { CashFlowStatementDto } from '../types';
import { ReportSummaryCard } from './ReportSummaryCard';
import { ReportSectionCard } from './ReportSectionCard';
import { ReportDataTable } from './ReportDataTable';

interface CashFlowStatementGridProps {
  data: CashFlowStatementDto;
  loading?: boolean;
}

function CashFlowSection({ title, titleAr, items, total, icon }: {
  title: string;
  titleAr: string;
  items: { description: string; amount: number }[];
  total: number;
  icon: React.ReactNode;
}) {
  const columns = [
    { key: 'description', header: 'الوصف', align: 'start' as const, render: (item: Record<string, unknown>) => String(item.description) },
    { key: 'amount', header: 'المبلغ', align: 'end' as const, render: (item: Record<string, unknown>) => Number(item.amount).toLocaleString(), className: 'tabular-nums font-semibold' },
  ];

  return (
    <ReportSectionCard title={titleAr} titleEn={title} total={total}>
      {items.length === 0 ? (
        <p className="text-center py-6 text-sm text-[var(--color-on-surface-variant)]">لا توجد بنود</p>
      ) : (
        <ReportDataTable
          columns={columns}
          data={items.map(i => i as unknown as Record<string, unknown>)}
          compact
        />
      )}
    </ReportSectionCard>
  );
}

export function CashFlowStatementGrid({ data, loading }: CashFlowStatementGridProps) {
  if (loading) {
    return (
      <div className="space-y-4">
        {[1, 2, 3].map(i => (
          <div key={i} className="h-24 bg-[var(--color-surface-container)] rounded-lg animate-pulse" />
        ))}
      </div>
    );
  }

  if (data.warning) {
    return (
      <div className="flex items-center gap-3 p-4 rounded-lg border border-[var(--color-warning)] bg-[var(--color-warning-bg)]">
        <AlertTriangle className="w-5 h-5 text-[var(--color-warning)] flex-shrink-0" />
        <p className="text-sm text-[var(--color-on-surface)]">{data.warning}</p>
      </div>
    );
  }

  const dateRange = `${new Date(data.startDate).toLocaleDateString('ar-YE')} — ${new Date(data.endDate).toLocaleDateString('ar-YE')}`;

  return (
    <div className="space-y-6">
      {/* Summary Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <ReportSummaryCard
          label="التدفق التشغيلي"
          value={data.operating.total}
          icon={<ArrowDownRight className="w-5 h-5" />}
          trend={data.operating.total >= 0 ? 'positive' : 'negative'}
        />
        <ReportSummaryCard
          label="التدفق الاستثماري"
          value={data.investing.total}
          icon={<ArrowUpRight className="w-5 h-5" />}
          trend={data.investing.total >= 0 ? 'positive' : 'negative'}
        />
        <ReportSummaryCard
          label="التدفق التمويلي"
          value={data.financing.total}
          icon={<Wallet className="w-5 h-5" />}
          trend={data.financing.total >= 0 ? 'positive' : 'negative'}
        />
        <ReportSummaryCard
          label="التدفق النقدي الصافي"
          value={data.netChange}
          icon={<ArrowDownRight className="w-5 h-5" />}
          trend={data.netChange >= 0 ? 'positive' : 'negative'}
          subtitle={dateRange}
        />
      </div>

      {/* Cash Flow Sections */}
      <CashFlowSection {...data.operating} icon={<ArrowDownRight className="w-4 h-4" />} />
      <CashFlowSection {...data.investing} icon={<ArrowUpRight className="w-4 h-4" />} />
      <CashFlowSection {...data.financing} icon={<Wallet className="w-4 h-4" />} />

      {/* Final Summary */}
      <div className={`rounded-lg border-2 p-4 ${data.reconciled ? 'border-[var(--color-success)] bg-[var(--color-success-bg)]' : 'border-[var(--color-warning)] bg-[var(--color-warning-bg)]'}`}>
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <div className="text-center">
            <p className="text-xs text-[var(--color-on-surface-variant)] mb-1">الروابط النقدية في البداية</p>
            <p className="text-lg font-bold tabular-nums text-[var(--color-on-surface)]">{data.openingCash.toLocaleString()}</p>
          </div>
          <div className="text-center border-x border-[var(--color-border-container)]">
            <p className="text-xs text-[var(--color-on-surface-variant)] mb-1">التدفق النقدي الصافي</p>
            <p className={`text-lg font-bold tabular-nums ${data.netChange >= 0 ? 'text-[var(--color-success)]' : 'text-[var(--color-error)]'}`}>
              {data.netChange.toLocaleString()}
            </p>
          </div>
          <div className="text-center">
            <p className="text-xs text-[var(--color-on-surface-variant)] mb-1">الروابط النقدية في النهاية</p>
            <p className="text-lg font-bold tabular-nums text-[var(--color-on-surface)]">{data.closingCash.toLocaleString()}</p>
          </div>
        </div>
        <div className="mt-3 pt-3 border-t border-[var(--color-border-container)] text-center">
          <p className="text-xs text-[var(--color-on-surface-variant)]">
            التوافق مع دفتر الأستاذ: <span className={`font-bold ${data.reconciled ? 'text-[var(--color-success)]' : 'text-[var(--color-warning)]'}`}>{data.reconciled ? 'متوافق' : 'غير متوافق'}</span>
          </p>
        </div>
      </div>
    </div>
  );
}
