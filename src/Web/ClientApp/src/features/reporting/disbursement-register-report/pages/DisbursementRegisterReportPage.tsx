import { useEffect, useMemo, useState } from 'react';
import { Download, FileText } from 'lucide-react';
import { notify } from '@/features/notifications/notify';
import {
  useDisbursementRegisterReport,
  useDisbursementRegisterFiscalYears,
  selectDefaultFiscalYear,
} from '../hooks/useDisbursementRegisterReport';
import { downloadDisbursementRegisterExport } from '../shared/client';
import {
  type DisbursementRegisterFilters,
  type DisbursementRegisterLineDto,
} from '../shared/types';
import { disbursementRegisterFilterSchema } from '../shared/schemas';
import { ReportingDisbursementRegisterFilters } from '@/components/ReportingDisbursementRegisterFilters';
import { ReportingDisbursementRegisterDetail } from '@/components/ReportingDisbursementRegisterDetail';
import { Page } from '@/components/ui/Page';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import { MoneyDisplay } from '@/components/ui/MoneyDisplay';
import { Badge } from '@/components/ui/Badge';
import { EmptyState } from '@/components/ui/EmptyState';
import { ErrorState } from '@/components/ui/ErrorState';
import { Button } from '@/components/ui/Button';

const statusLabels: Record<string, string> = {
  Draft: 'مسودة',
  Submitted: 'مُقدّم',
  Approved: 'معتمد',
  Paid: 'مدفوع',
  Rejected: 'مرفوض',
};

export default function DisbursementRegisterReportPage() {
  const { data: fiscalYears, isLoading: yearsLoading } = useDisbursementRegisterFiscalYears();

  const [filters, setFilters] = useState<Partial<DisbursementRegisterFilters>>({});
  const defaultYear = useMemo(
    () => selectDefaultFiscalYear(fiscalYears),
    [fiscalYears],
  );

  useEffect(() => {
    if (defaultYear?.id && filters.fiscalYearId === undefined) {
      setFilters((f) => ({ ...f, fiscalYearId: defaultYear.id! }));
    }
  }, [defaultYear?.id, filters.fiscalYearId]);

  const parsed = disbursementRegisterFilterSchema.safeParse(filters);
  const validFilters = parsed.success ? (parsed.data as DisbursementRegisterFilters) : null;

  const { data: report, isLoading, isError, refetch } = useDisbursementRegisterReport(validFilters);

  const [detailOrderId, setDetailOrderId] = useState<number | null>(null);

  const lines = useMemo(() => report?.lines ?? [], [report]);
  const totals = report?.totals;

  const columns: DataGridColumn<DisbursementRegisterLineDto>[] = [
    { header: 'رقم الأمر', cell: (row) => <span dir="ltr" className="font-mono text-sm">{row.orderNumber}</span> },
    { header: 'التاريخ', cell: (row) => <span className="text-sm">{row.orderDate ? new Date(row.orderDate).toLocaleDateString('ar-EG') : '—'}</span> },
    { header: 'المستفيد', cell: (row) => <span className="text-sm">{row.payeeName}</span> },
    { header: 'المبلغ', align: 'right', cell: (row) => <MoneyDisplay value={row.amount ?? 0} /> },
    { header: 'الحالة', cell: (row) => <Badge variant="outline">{statusLabels[row.status ?? ''] ?? row.status}</Badge> },
    { header: 'الصندوق', cell: (row) => <span dir="ltr" className="text-sm">{row.fundCode ?? '—'}</span> },
    { header: 'المعتمد', cell: (row) => <span className="text-sm">{row.approverName ?? '—'}</span> },
    { header: 'تاريخ الدفع', cell: (row) => <span className="text-sm">{row.paidAt ? new Date(row.paidAt).toLocaleDateString('ar-EG') : '—'}</span> },
  ];

  const isPartialData = defaultYear?.status === 'Open' && validFilters !== null;

  const [exporting, setExporting] = useState(false);

  async function handleExport(format: 'xlsx' | 'pdf') {
    if (!validFilters) return;
    setExporting(true);
    try {
      await downloadDisbursementRegisterExport(validFilters, format);
      notify({ type: 'success', title: 'تم تصدير السجل بنجاح' });
    } catch {
      notify({ type: 'error', title: 'فشل تصدير السجل' });
    } finally {
      setExporting(false);
    }
  }

  return (
    <Page
      title="سجل الصرف"
      loading={yearsLoading || (isLoading && !report)}
      actions={
        <div className="flex gap-2">
          <Button variant="outline" size="sm" disabled={!validFilters || exporting} onClick={() => handleExport('xlsx')}>
            <Download size={16} className="ms-1" />
            تصدير Excel
          </Button>
          <Button variant="outline" size="sm" disabled={!validFilters || exporting} onClick={() => handleExport('pdf')}>
            <FileText size={16} className="ms-1" />
            تصدير PDF
          </Button>
        </div>
      }
      toolbar={
        <ReportingDisbursementRegisterFilters
          fiscalYears={fiscalYears}
          value={filters}
          onChange={setFilters}
        />
      }
    >
      {isPartialData && (
        <div
          role="status"
          className="mb-3 rounded-md border border-amber-300 bg-amber-50 px-3 py-2 text-sm text-amber-900 dark:border-amber-700 dark:bg-amber-950 dark:text-amber-200"
        >
          بيانات جزئية — الفترة الحالية جارية وقد تتغير الأرقام
        </div>
      )}
      {totals && (
        <div className="mb-4 grid grid-cols-3 gap-3 sm:grid-cols-6">
          {[
            { label: 'الإجمالي', value: totals.totalRequests },
            { label: 'مسودة', value: totals.draftCount },
            { label: 'مُقدّم', value: totals.submittedCount },
            { label: 'معتمد', value: totals.approvedCount },
            { label: 'مدفوع', value: totals.paidCount },
            { label: 'مرفوض', value: totals.rejectedCount },
          ].map((card) => (
            <div key={card.label} className="rounded-md border p-3 text-center">
              <div className="text-2xl font-bold">{card.value}</div>
              <div className="text-muted-foreground text-xs">{card.label}</div>
            </div>
          ))}
        </div>
      )}
      {isError ? (
        <ErrorState onRetry={() => refetch()} />
      ) : !validFilters || (isLoading && !report) ? null : lines.length === 0 ? (
        <EmptyState message="لا توجد بيانات للفترة المحددة" />
      ) : (
        <>
          <DataGrid
            columns={columns}
            data={lines}
            loading={isLoading}
            emptyMessage="لا توجد بيانات للفترة المحددة"
            rowKey={(row) => row.paymentOrderId ?? 0}
            onRowClick={(row) => setDetailOrderId(row.paymentOrderId ?? null)}
          />
          <ReportingDisbursementRegisterDetail
            paymentOrderId={detailOrderId}
            onClose={() => setDetailOrderId(null)}
          />
          {totals && (
            <div className="mt-3 border-t pt-2" data-testid="disbursement-register-totals">
              <div className="flex items-center justify-between text-sm font-bold">
                <span>إجمالي المبالغ</span>
                <div className="flex items-center gap-6">
                  <span className="flex items-center gap-1">
                    <span className="text-muted-foreground font-normal">الإجمالي:</span>
                    <MoneyDisplay value={totals.totalAmount ?? 0} />
                  </span>
                  <span className="flex items-center gap-1">
                    <span className="text-muted-foreground font-normal">المدفوع:</span>
                    <MoneyDisplay value={totals.paidAmount ?? 0} />
                  </span>
                </div>
              </div>
            </div>
          )}
        </>
      )}
    </Page>
  );
}
