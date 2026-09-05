import { BookOpen, FileText, Hash } from 'lucide-react';
import type { GeneralLedgerDto } from '../types';
import { ReportSummaryCard } from './ReportSummaryCard';
import { ReportDataTable } from './ReportDataTable';

interface GeneralLedgerGridProps {
  data: GeneralLedgerDto;
  loading?: boolean;
}

export function GeneralLedgerGrid({ data, loading }: GeneralLedgerGridProps) {
  if (loading) {
    return (
      <div className="space-y-4">
        {[1, 2, 3].map(i => (
          <div key={i} className="h-24 bg-[var(--color-surface-container)] rounded-lg animate-pulse" />
        ))}
      </div>
    );
  }

  const totalPages = Math.ceil(data.totalLines / data.pageSize);
  const netBalance = data.totals.debit - data.totals.credit;

  const columns = [
    {
      key: 'documentDate',
      header: 'التاريخ',
      align: 'start' as const,
      width: '100px',
      render: (item: Record<string, unknown>) => (
        <span className="whitespace-nowrap">{String(item.documentDate)}</span>
      ),
    },
    {
      key: 'entryNumber',
      header: 'رقم القيد',
      align: 'start' as const,
      width: '80px',
      render: (item: Record<string, unknown>) => (
        <span className="inline-flex items-center gap-1">
          <Hash className="w-3 h-3 text-[var(--color-on-surface-variant)]" />
          {String(item.entryNumber)}
        </span>
      ),
    },
    {
      key: 'reference',
      header: 'المرجع',
      align: 'start' as const,
      width: '100px',
      render: (item: Record<string, unknown>) => String(item.reference),
    },
    {
      key: 'narration',
      header: 'البيان',
      align: 'start' as const,
      render: (item: Record<string, unknown>) => (
        <span className="block max-w-[200px] truncate text-[var(--color-on-surface-variant)]" title={String(item.narration)}>
          {String(item.narration)}
        </span>
      ),
    },
    {
      key: 'account',
      header: 'الحساب',
      align: 'start' as const,
      render: (item: Record<string, unknown>) => (
        <span className="font-medium">{String(item.accountCode)} - {String(item.accountName)}</span>
      ),
    },
    {
      key: 'debit',
      header: 'المدين',
      align: 'end' as const,
      width: '120px',
      render: (item: Record<string, unknown>) => (
        <span className="tabular-nums">{Number(item.debit).toLocaleString()}</span>
      ),
    },
    {
      key: 'credit',
      header: 'الدائن',
      align: 'end' as const,
      width: '120px',
      render: (item: Record<string, unknown>) => (
        <span className="tabular-nums">{Number(item.credit).toLocaleString()}</span>
      ),
    },
    {
      key: 'runningBalance',
      header: 'الرصيد الجاري',
      align: 'end' as const,
      width: '140px',
      className: 'font-bold',
      render: (item: Record<string, unknown>) => (
        <span className="tabular-nums font-bold">{Number(item.runningBalance).toLocaleString()}</span>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      {/* Summary Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <ReportSummaryCard
          label="إجمالي السجلات"
          value={data.totalLines}
          icon={<BookOpen className="w-5 h-5" />}
          trend="neutral"
          subtitle={`صفحة ${data.page} من ${totalPages}`}
        />
        <ReportSummaryCard
          label="إجمالي المدين"
          value={data.totals.debit}
          icon={<FileText className="w-5 h-5" />}
          trend="neutral"
        />
        <ReportSummaryCard
          label="إجمالي الدائن"
          value={data.totals.credit}
          icon={<FileText className="w-5 h-5" />}
          trend="neutral"
        />
        <ReportSummaryCard
          label="الرصيد الصافي"
          value={Math.abs(netBalance)}
          icon={<Hash className="w-5 h-5" />}
          trend={netBalance >= 0 ? 'positive' : 'negative'}
          subtitle={netBalance >= 0 ? 'مدين' : 'دائن'}
        />
      </div>

      {/* Ledger Table */}
      <div className="rounded-lg border border-[var(--color-border-container)] overflow-hidden bg-[var(--color-surface-container-lowest)]">
        <ReportDataTable
          columns={columns}
          data={data.lines as unknown as Record<string, unknown>[]}
          totalRow={
            <>
              <td colSpan={5} className="px-4 py-3 text-sm font-bold">الإجمالي</td>
              <td className="px-4 py-3 text-end tabular-nums font-bold">{data.totals.debit.toLocaleString()}</td>
              <td className="px-4 py-3 text-end tabular-nums font-bold">{data.totals.credit.toLocaleString()}</td>
              <td className="px-4 py-3 text-end tabular-nums font-bold">
                {netBalance >= 0 ? 'مدين' : 'دائن'} {Math.abs(netBalance).toLocaleString()}
              </td>
            </>
          }
        />
      </div>

      {/* Pagination Info */}
      {totalPages > 1 && (
        <div className="text-center text-sm text-[var(--color-on-surface-variant)]">
          صفحة {data.page} من {totalPages} — عرض {data.lines.length} من {data.totalLines} سجل
        </div>
      )}
    </div>
  );
}
