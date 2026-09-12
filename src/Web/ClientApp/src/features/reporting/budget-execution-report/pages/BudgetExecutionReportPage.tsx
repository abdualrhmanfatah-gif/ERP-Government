import { useEffect, useMemo, useState } from 'react';
import { Download, FileText } from 'lucide-react';
import { notify } from '@/features/notifications/notify';
import {
  useBudgetExecutionReport,
  useBudgetExecutionFiscalYears,
  useBudgetExecutionFilterOptions,
  selectDefaultFiscalYear,
} from '../hooks/useBudgetExecutionReport';
import { downloadBudgetExecutionExport } from '../shared/client';
import {
  PAGINATION_THRESHOLD,
  computeUsageRatio,
  type BudgetExecutionFilters,
  type BudgetExecutionLineDto,
} from '../shared/types';
import { budgetExecutionFilterSchema } from '../shared/schemas';
import { ReportingBudgetExecutionFilters } from '@/components/ReportingBudgetExecutionFilters';
import { ReportingBudgetExecutionDetail } from '@/components/ReportingBudgetExecutionDetail';
import { Page, DataGrid, MoneyDisplay, Pagination, EmptyState, ErrorState, Button, Alert } from '@/components/ui';
import type { DataGridColumn } from '@/components/ui/DataGrid';

const PAGE_SIZE = 25;

export default function BudgetExecutionReportPage() {
  const { data: fiscalYears, isLoading: yearsLoading } = useBudgetExecutionFiscalYears();
  const { funds, classifications } = useBudgetExecutionFilterOptions();

  const [filters, setFilters] = useState<Partial<BudgetExecutionFilters>>({});
  const defaultYear = useMemo(
    () => selectDefaultFiscalYear(fiscalYears),
    [fiscalYears],
  );

  useEffect(() => {
    if (defaultYear?.id && filters.fiscalYearId === undefined) {
      setFilters((f) => ({ ...f, fiscalYearId: defaultYear.id! }));
    }
  }, [defaultYear?.id, filters.fiscalYearId]);

  const parsed = budgetExecutionFilterSchema.safeParse(filters);
  const validFilters = parsed.success ? (parsed.data as BudgetExecutionFilters) : null;

  const { data: report, isLoading, isError, refetch } = useBudgetExecutionReport(validFilters);

  const [page, setPage] = useState(1);
  const [detailItemId, setDetailItemId] = useState<number | null>(null);
  useEffect(() => {
    setPage(1);
  }, [filters]);

  const lines = useMemo(() => report?.lines ?? [], [report]);
  const paged = useMemo(() => {
    if (lines.length <= PAGINATION_THRESHOLD) return lines;
    return lines.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);
  }, [lines, page]);
  const paginated = lines.length > PAGINATION_THRESHOLD;

  const columns: DataGridColumn<BudgetExecutionLineDto>[] = [
    { header: 'كود البند', cell: (row) => <span dir="ltr" className="font-mono text-sm">{row.itemCode}</span> },
    { header: 'اسم البند', cell: (row) => <span className="text-sm">{row.itemName}</span> },
    { header: 'الصندوق', cell: (row) => <span className="text-sm">{row.fundNumber} — {row.fundName}</span> },
    { header: 'البرنامج', cell: (row) => <span dir="ltr" className="text-sm">{row.programCode ?? '—'}</span> },
    { header: 'المشروع', cell: (row) => <span dir="ltr" className="text-sm">{row.projectCode ?? '—'}</span> },
    { header: 'المخصص', align: 'end', cell: (row) => <MoneyDisplay value={row.appropriatedAmount} /> },
    { header: 'الالتزامات', align: 'end', cell: (row) => <MoneyDisplay value={row.encumberedAmount} /> },
    { header: 'المدفوعات', align: 'end', cell: (row) => <MoneyDisplay value={row.paidAmount} /> },
    { header: 'المتاح', align: 'end', cell: (row) => <MoneyDisplay value={row.availableAmount} /> },
    {
      header: 'عرض',
      align: 'end',
      cell: (row) => {
        const ratio = computeUsageRatio(row.paidAmount, row.appropriatedAmount);
        return (
          <span className="tabular-nums text-end font-mono text-sm">
            {ratio === null ? '—' : `${(ratio * 100).toFixed(1)}%`}
          </span>
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
      await downloadBudgetExecutionExport(validFilters, format);
      notify({ type: 'success', title: 'تم تصدير التقرير بنجاح' });
    } catch {
      notify({ type: 'error', title: 'فشل تصدير التقرير' });
    } finally {
      setExporting(false);
    }
  }

  return (
    <Page
      title="تقرير تنفيذ الموازنة"
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
        <ReportingBudgetExecutionFilters
          fiscalYears={fiscalYears}
          funds={funds.data}
          classifications={classifications.data}
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
            data={paged}
            loading={isLoading}
            emptyMessage="لا توجد بيانات للفترة المحددة"
            rowKey={(row) => row.budgetItemId}
            onRowClick={(row) => setDetailItemId(row.budgetItemId)}
          />
          <ReportingBudgetExecutionDetail
            budgetItemId={detailItemId}
            onClose={() => setDetailItemId(null)}
          />
          {paginated && (
            <div className="mt-2 flex justify-center">
              <Pagination page={page} total={lines.length} pageSize={PAGE_SIZE} onChange={setPage} />
            </div>
          )}
          {totals && (
            <div className="mt-3 border-t pt-2" data-testid="budget-execution-totals">
              <div className="flex items-center justify-between text-sm font-bold">
                <span>الإجماليات</span>
                <div className="flex items-center gap-6">
                  <span className="flex items-center gap-1">
                    <span className="text-muted-foreground font-normal">المخصص:</span>
                    <MoneyDisplay value={totals.appropriatedAmount} />
                  </span>
                  <span className="flex items-center gap-1">
                    <span className="text-muted-foreground font-normal">الالتزامات:</span>
                    <MoneyDisplay value={totals.encumberedAmount} />
                  </span>
                  <span className="flex items-center gap-1">
                    <span className="text-muted-foreground font-normal">المدفوعات:</span>
                    <MoneyDisplay value={totals.paidAmount} />
                  </span>
                  <span className="flex items-center gap-1">
                    <span className="text-muted-foreground font-normal">المتاح:</span>
                    <MoneyDisplay value={totals.availableAmount} />
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
