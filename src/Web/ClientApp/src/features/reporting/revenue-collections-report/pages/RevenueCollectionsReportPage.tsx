import { useEffect, useMemo, useState } from 'react';
import { Download, FileText } from 'lucide-react';
import { notify } from '@/features/notifications/notify';
import {
  useRevenueCollectionsReport,
  useRevenueCollectionsFiscalYears,
  selectDefaultFiscalYear,
} from '../hooks/useRevenueCollectionsReport';
import { downloadRevenueCollectionsExport } from '../shared/client';
import {
  type RevenueCollectionsFilters,
  type RevenueCollectionsLineDto,
  paymentMethodLabels,
  depositStatusLabels,
} from '../shared/types';
import { revenueCollectionsFilterSchema } from '../shared/schemas';
import { ReportingRevenueCollectionsFilters } from '@/components/ReportingRevenueCollectionsFilters';
import { ReportingRevenueCollectionsDetail } from '@/components/ReportingRevenueCollectionsDetail';
import { Page, DataGrid, MoneyDisplay, Badge, EmptyState, ErrorState, Button, Alert } from '@/components/ui';
import type { DataGridColumn } from '@/components/ui/DataGrid';

export default function RevenueCollectionsReportPage() {
  const { data: fiscalYears, isLoading: yearsLoading } = useRevenueCollectionsFiscalYears();

  const [filters, setFilters] = useState<Partial<RevenueCollectionsFilters>>({});
  const defaultYear = useMemo(
    () => selectDefaultFiscalYear(fiscalYears),
    [fiscalYears],
  );

  useEffect(() => {
    if (defaultYear?.id && filters.fiscalYearId === undefined) {
      setFilters((f) => ({ ...f, fiscalYearId: defaultYear.id! }));
    }
  }, [defaultYear?.id, filters.fiscalYearId]);

  const parsed = revenueCollectionsFilterSchema.safeParse(filters);
  const validFilters = parsed.success ? (parsed.data as RevenueCollectionsFilters) : null;

  const { data: report, isLoading, isError, refetch } = useRevenueCollectionsReport(validFilters);

  const [detailVoucherId, setDetailVoucherId] = useState<number | null>(null);

  const lines = useMemo(() => report?.lines ?? [], [report]);

  const columns: DataGridColumn<RevenueCollectionsLineDto>[] = [
    { header: 'رقم السند', cell: (row) => <span dir="ltr" className="font-mono text-sm">{row.voucherNumber}</span> },
    { header: 'التاريخ', cell: (row) => <span className="text-sm">{row.voucherDate ? new Date(row.voucherDate).toLocaleDateString('ar-YE') : '—'}</span> },
    { header: 'المورد', cell: (row) => <span className="text-sm">{row.partyName ?? '—'}</span> },
    { header: 'المبلغ', align: 'right', cell: (row) => <MoneyDisplay value={row.amount ?? 0} /> },
    { header: 'طريقة الدفع', cell: (row) => <span className="text-sm">{paymentMethodLabels[row.paymentMethod ?? ''] ?? row.paymentMethod ?? '—'}</span> },
    {
      header: 'بطاقة الإيداع',
      cell: (row) => {
        if (!row.depositSlipNumber) return <span className="text-muted-foreground text-sm">—</span>;
        return (
          <div className="flex items-center gap-1">
            <span dir="ltr" className="font-mono text-sm">{row.depositSlipNumber}</span>
            {row.depositSlipStatus && (
              <Badge variant="outline">{depositStatusLabels[row.depositSlipStatus] ?? row.depositSlipStatus}</Badge>
            )}
          </div>
        );
      },
    },
  ];

  const totals = report?.totals;

  const isPartialData = defaultYear?.status === 'Open' && validFilters !== null;

  const [exporting, setExporting] = useState(false);

  async function handleExport(format: 'xlsx' | 'pdf') {
    if (!validFilters) return;
    setExporting(true);
    try {
      await downloadRevenueCollectionsExport(validFilters, format);
      notify({ type: 'success', title: 'تم تصدير التقرير بنجاح' });
    } catch {
      notify({ type: 'error', title: 'فشل تصدير التقرير' });
    } finally {
      setExporting(false);
    }
  }

  return (
    <Page
      title="تقرير تحصيل الإيرادات"
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
        <ReportingRevenueCollectionsFilters
          fiscalYears={fiscalYears}
          value={filters}
          onChange={setFilters}
        />
      }
    >
      {isPartialData && (
        <Alert variant="warning">
          بيانات جزئية — الفترة الحالية جارية وقد تتغير الأرقام
        </Alert>
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
            rowKey={(row) => row.receiptVoucherId ?? 0}
            onRowClick={(row) => setDetailVoucherId(row.receiptVoucherId ?? null)}
          />
          <ReportingRevenueCollectionsDetail
            receiptVoucherId={detailVoucherId}
            onClose={() => setDetailVoucherId(null)}
          />
          {totals && (
            <div className="mt-3 border-t pt-2" data-testid="revenue-collections-totals">
              <div className="flex items-center justify-between text-sm font-bold">
                <span>الإجماليات</span>
                <div className="flex items-center gap-6">
                  <span className="flex items-center gap-1">
                    <span className="text-muted-foreground font-normal">الإجمالي:</span>
                    <MoneyDisplay value={totals.totalAmount ?? 0} />
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
