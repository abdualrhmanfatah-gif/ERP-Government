import { useEffect, useMemo, useState } from 'react';
import { Download, FileText } from 'lucide-react';
import { notify } from '@/features/notifications/notify';
import {
  useGeneralLedger,
  useFinancialStatementFiscalYears,
  useFinancialStatementPeriods,
  selectDefaultFiscalYear,
} from '../hooks/useFinancialStatements';
import {
  type GeneralLedgerFilters,
  type GeneralLedgerLine,
} from '../shared/types';
import { generalLedgerFilterSchema } from '../shared/schemas';
import { Page } from '@/components/ui/Page';
import { DataGrid, type DataGridColumn } from '@/components/ui/DataGrid';
import { MoneyDisplay } from '@/components/ui/MoneyDisplay';
import { EmptyState } from '@/components/ui/EmptyState';
import { ErrorState } from '@/components/ui/ErrorState';
import { Button } from '@/components/ui/Button';
import { FilterSelect } from '@/components/ui/FilterSelect';

export default function GeneralLedgerReportPage() {
  const { data: fiscalYears, isLoading: yearsLoading } = useFinancialStatementFiscalYears();

  const [filters, setFilters] = useState<Partial<GeneralLedgerFilters>>({});
  const defaultYear = useMemo(
    () => selectDefaultFiscalYear(fiscalYears),
    [fiscalYears],
  );

  useEffect(() => {
    if (defaultYear?.id && filters.fiscalYearId === undefined) {
      setFilters((f) => ({ ...f, fiscalYearId: defaultYear.id! }));
    }
  }, [defaultYear?.id, filters.fiscalYearId]);

  // For GL we need to derive fiscalYearId for the period filter
  const fiscalYearForPeriods = (filters as any).fiscalYearId;
  const { data: periods } = useFinancialStatementPeriods(fiscalYearForPeriods);

  const parsed = generalLedgerFilterSchema.safeParse(filters);
  const validFilters = parsed.success ? (parsed.data as GeneralLedgerFilters) : null;

  const { data: report, isLoading, isError, refetch } = useGeneralLedger(validFilters);

  const lines = useMemo(() => report?.lines ?? [], [report]);
  const totals = report?.totals;
  const currentPage = report?.page ?? 0;
  const pageSize = report?.pageSize ?? 25;
  const totalLines = report?.totalLines ?? 0;
  const totalPages = Math.ceil(totalLines / pageSize);

  const isPartialData = defaultYear?.status === 'Open' && validFilters !== null;

  const [exporting, setExporting] = useState(false);

  async function handleExport(format: 'xlsx' | 'pdf') {
    setExporting(true);
    try {
      const qs = new URLSearchParams();
      qs.set('format', format);
      if (validFilters?.accountId) qs.set('AccountId', String(validFilters.accountId));
      if (validFilters?.fiscalPeriodId) qs.set('FiscalPeriodId', String(validFilters.fiscalPeriodId));
      const response = await fetch(`/api/Reports/general-ledger/export?${qs.toString()}`);
      if (!response.ok) throw new Error('Export failed');
      const blob = await response.blob();
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `GeneralLedger-${new Date().toISOString().slice(0, 10)}.${format === 'pdf' ? 'pdf' : 'xlsx'}`;
      document.body.appendChild(a);
      a.click();
      a.remove();
      URL.revokeObjectURL(url);
      notify({ type: 'success', title: 'تم تصدير دفتر الأستاذ العام بنجاح' });
    } catch {
      notify({ type: 'error', title: 'فشل تصدير دفتر الأستاذ العام' });
    } finally {
      setExporting(false);
    }
  }

  function goToPage(page: number) {
    setFilters((f) => ({ ...f, page }));
  }

  const columns: DataGridColumn<GeneralLedgerLine>[] = [
    { header: 'التاريخ', cell: (row) => <span className="text-sm">{row.date ? new Date(row.date).toLocaleDateString('ar-EG') : '—'}</span> },
    { header: 'رقم القيد', cell: (row) => <span dir="ltr" className="font-mono text-sm">{row.journalEntryNumber ?? '—'}</span> },
    { header: 'الحساب', cell: (row) => <span className="text-sm">{row.accountName ?? '—'}</span> },
    { header: 'البيان', cell: (row) => <span className="text-sm">{row.narration ?? '—'}</span> },
    { header: 'المرجع', cell: (row) => <span className="text-sm">{row.reference ?? '—'}</span> },
    { header: 'مدين', align: 'right', cell: (row) => <MoneyDisplay value={row.debit ?? 0} /> },
    { header: 'دائن', align: 'right', cell: (row) => <MoneyDisplay value={row.credit ?? 0} /> },
  ];

  const periodOptions = (periods ?? []).map((p: any) => ({
    value: String(p.id),
    label: p.name ?? `فترة ${p.periodNumber}`,
  }));

  return (
    <Page
      title="دفتر الأستاذ العام"
      loading={yearsLoading || (isLoading && !report)}
      actions={
        <div className="flex gap-2">
          <Button variant="outline" size="sm" disabled={!report || exporting} onClick={() => handleExport('xlsx')}>
            <Download size={16} className="ms-1" />
            تصدير Excel
          </Button>
          <Button variant="outline" size="sm" disabled={!report || exporting} onClick={() => handleExport('pdf')}>
            <FileText size={16} className="ms-1" />
            تصدير PDF
          </Button>
        </div>
      }
      toolbar={
        <div className="flex gap-2 items-center flex-wrap">
          <FilterSelect
            value={filters.fiscalYearId ? String(filters.fiscalYearId) : ''}
            onChange={(v) => setFilters((f) => ({ ...f, fiscalYearId: v ? Number(v) : undefined }))}
            options={(fiscalYears ?? []).map((fy) => ({ value: String(fy.id), label: fy.name ?? String(fy.yearNumber) }))}
            placeholder="السنة المالية"
            label="السنة المالية"
          />
          <FilterSelect
            value={filters.fiscalPeriodId ? String(filters.fiscalPeriodId) : ''}
            onChange={(v) => setFilters((f) => ({ ...f, fiscalPeriodId: v ? Number(v) : undefined }))}
            options={periodOptions}
            placeholder="الفترة"
            label="الفترة"
          />
        </div>
      }
    >
      {isPartialData && (
        <div role="status" className="mb-3 rounded-md border border-amber-300 bg-amber-50 px-3 py-2 text-sm text-amber-900 dark:border-amber-700 dark:bg-amber-950 dark:text-amber-200">
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
            emptyMessage="لا توجد بيانات"
            rowKey={(row) => row.entryNumber ?? row.documentDate?.toString() ?? ''}
          />
          {totals && (
            <div className="mt-3 border-t pt-2" data-testid="general-ledger-totals">
              <div className="flex items-center justify-between text-sm font-bold">
                <span>الإجماليات</span>
                <div className="flex items-center gap-6">
                  <span className="flex items-center gap-1">
                    <span className="text-muted-foreground font-normal">مدين:</span>
                    <MoneyDisplay value={totals.totalDebit ?? 0} />
                  </span>
                  <span className="flex items-center gap-1">
                    <span className="text-muted-foreground font-normal">دائن:</span>
                    <MoneyDisplay value={totals.totalCredit ?? 0} />
                  </span>
                </div>
              </div>
            </div>
          )}
          {totalPages > 1 && (
            <div className="mt-4 flex items-center justify-center gap-2">
              <Button
                variant="outline"
                size="sm"
                disabled={currentPage <= 0}
                onClick={() => goToPage(currentPage - 1)}
              >
                السابق
              </Button>
              <span className="text-muted-foreground text-sm">
                صفحة {currentPage + 1} من {totalPages}
              </span>
              <Button
                variant="outline"
                size="sm"
                disabled={currentPage >= totalPages - 1}
                onClick={() => goToPage(currentPage + 1)}
              >
                التالي
              </Button>
            </div>
          )}
        </>
      )}
    </Page>
  );
}
