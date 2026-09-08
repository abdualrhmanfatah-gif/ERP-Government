import { useEffect, useMemo, useState } from 'react';
import { Download, FileText } from 'lucide-react';
import { notify } from '@/features/notifications/notify';
import {
  useAvailabilitySnapshotReport,
  useAvailabilitySnapshotFiscalYears,
  selectDefaultFiscalYear,
} from '../hooks/useAvailabilitySnapshotReport';
import { downloadAvailabilitySnapshotExport } from '../shared/client';
import {
  type AvailabilitySnapshotFilters,
} from '../shared/types';
import { availabilitySnapshotFilterSchema } from '../shared/schemas';
import { ReportingAvailabilitySnapshotFilters } from '@/components/ReportingAvailabilitySnapshotFilters';
import { ReportingAvailabilitySnapshotDetail } from '@/components/ReportingAvailabilitySnapshotDetail';
import { Page } from '@/components/ui/Page';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import { MoneyDisplay } from '@/components/ui/MoneyDisplay';
import { Badge } from '@/components/ui/Badge';
import { EmptyState } from '@/components/ui/EmptyState';
import { ErrorState } from '@/components/ui/ErrorState';
import { Button } from '@/components/ui/Button';

const controlStateColors: Record<string, string> = {
  Normal: 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200',
  Warning: 'bg-amber-100 text-amber-800 dark:bg-amber-900 dark:text-amber-200',
  Critical: 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200',
  Frozen: 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200',
};

const controlStateLabels: Record<string, string> = {
  Normal: 'طبيعي',
  Warning: 'تحذير',
  Critical: 'حرج',
  Frozen: 'مجمّد',
};

export default function AvailabilitySnapshotReportPage() {
  const { data: fiscalYears, isLoading: yearsLoading } = useAvailabilitySnapshotFiscalYears();

  const [filters, setFilters] = useState<Partial<AvailabilitySnapshotFilters>>({});
  const defaultYear = useMemo(
    () => selectDefaultFiscalYear(fiscalYears),
    [fiscalYears],
  );

  useEffect(() => {
    if (defaultYear?.id && filters.fiscalYearId === undefined) {
      setFilters((f) => ({ ...f, fiscalYearId: defaultYear.id! }));
    }
  }, [defaultYear?.id, filters.fiscalYearId]);

  const parsed = availabilitySnapshotFilterSchema.safeParse(filters);
  const validFilters = parsed.success ? (parsed.data as AvailabilitySnapshotFilters) : null;

  const { data: report, isLoading, isError, refetch } = useAvailabilitySnapshotReport(validFilters);

  const [detailItemId, setDetailItemId] = useState<number | null>(null);

  const lines = useMemo(() => {
    if (!report) return [];
    // AvailabilitySnapshotDto has breakdown as the lines array
    return (report as any).breakdown ?? report.lines ?? [];
  }, [report]);

  const columns: DataGridColumn<any>[] = [
    { header: 'كود البند', cell: (row) => <span dir="ltr" className="font-mono text-sm">{row.itemCode}</span> },
    { header: 'اسم البند', cell: (row) => <span className="text-sm">{row.itemName}</span> },
    {
      header: 'الحالة الرقابية',
      cell: (row) => {
        const state = row.controlState ?? 'Normal';
        return (
          <Badge className={controlStateColors[state] ?? ''}>
            {controlStateLabels[state] ?? state}
          </Badge>
        );
      },
    },
    { header: 'المخصص', align: 'right', cell: (row) => <MoneyDisplay value={row.appropriatedAmount ?? row.totalAppropriated ?? 0} /> },
    { header: 'الالتزامات', align: 'right', cell: (row) => <MoneyDisplay value={row.encumberedAmount ?? row.totalEncumbered ?? 0} /> },
    { header: 'المدفوعات', align: 'right', cell: (row) => <MoneyDisplay value={row.paidAmount ?? row.totalPaid ?? 0} /> },
    { header: 'المتاح', align: 'right', cell: (row) => <MoneyDisplay value={row.availableAmount ?? row.totalAvailable ?? 0} /> },
  ];

  const isPartialData = defaultYear?.status === 'Open' && validFilters !== null;

  const [exporting, setExporting] = useState(false);

  async function handleExport(format: 'xlsx' | 'pdf') {
    if (!validFilters) return;
    setExporting(true);
    try {
      await downloadAvailabilitySnapshotExport(validFilters, format);
      notify({ type: 'success', title: 'تم تصدير اللقطة بنجاح' });
    } catch {
      notify({ type: 'error', title: 'فشل تصدير اللقطة' });
    } finally {
      setExporting(false);
    }
  }

  return (
    <Page
      title="لقطة التوفر"
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
        <ReportingAvailabilitySnapshotFilters
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
            rowKey={(row) => row.budgetItemId ?? 0}
            onRowClick={(row) => setDetailItemId(row.budgetItemId ?? null)}
          />
          <ReportingAvailabilitySnapshotDetail
            budgetItemId={detailItemId}
            fiscalYearId={validFilters?.fiscalYearId ?? 0}
            onClose={() => setDetailItemId(null)}
          />
        </>
      )}
    </Page>
  );
}
