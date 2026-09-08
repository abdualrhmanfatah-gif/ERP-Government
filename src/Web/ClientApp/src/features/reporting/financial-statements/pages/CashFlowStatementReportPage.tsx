import { useEffect, useMemo, useState } from 'react';
import { Download, FileText, AlertTriangle } from 'lucide-react';
import { notify } from '@/features/notifications/notify';
import {
  useCashFlowStatement,
  useFinancialStatementFiscalYears,
  selectDefaultFiscalYear,
} from '../hooks/useFinancialStatements';
import {
  type FinancialStatementFilters,
} from '../shared/types';
import { cashFlowStatementFilterSchema } from '../shared/schemas';
import { FilterSelect } from '@/components/ui/FilterSelect';
import { FilterDate } from '@/components/ui/FilterDate';
import { Page } from '@/components/ui/Page';
import { MoneyDisplay } from '@/components/ui/MoneyDisplay';
import { EmptyState } from '@/components/ui/EmptyState';
import { ErrorState } from '@/components/ui/ErrorState';
import { Button } from '@/components/ui/Button';

export default function CashFlowStatementReportPage() {
  const { data: fiscalYears, isLoading: yearsLoading } = useFinancialStatementFiscalYears();

  const [filters, setFilters] = useState<Partial<FinancialStatementFilters>>({});
  const defaultYear = useMemo(
    () => selectDefaultFiscalYear(fiscalYears),
    [fiscalYears],
  );

  useEffect(() => {
    if (defaultYear?.id && filters.fiscalYearId === undefined) {
      const today = new Date().toISOString().slice(0, 10);
      setFilters((f) => ({
        ...f,
        fiscalYearId: defaultYear.id!,
        startDate: f.startDate ?? today,
        endDate: f.endDate ?? today,
      }));
    }
  }, [defaultYear?.id, filters.fiscalYearId, filters.startDate, filters.endDate]);

  const parsed = cashFlowStatementFilterSchema.safeParse(filters);
  const validFilters = parsed.success ? (parsed.data as FinancialStatementFilters) : null;

  const { data: report, isLoading, isError, refetch } = useCashFlowStatement(validFilters);

  const isPartialData = defaultYear?.status === 'Open' && validFilters !== null;

  const [exporting, setExporting] = useState(false);

  async function handleExport(format: 'xlsx' | 'pdf') {
    setExporting(true);
    try {
      const qs = new URLSearchParams();
      qs.set('format', format);
      if (report?.startDate) qs.set('startDate', new Date(report.startDate).toISOString().slice(0, 10));
      if (report?.endDate) qs.set('endDate', new Date(report.endDate).toISOString().slice(0, 10));
      const response = await fetch(`/api/Reports/cash-flow/export?${qs.toString()}`);
      if (!response.ok) throw new Error('Export failed');
      const blob = await response.blob();
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `CashFlowStatement-${new Date().toISOString().slice(0, 10)}.${format === 'pdf' ? 'pdf' : 'xlsx'}`;
      document.body.appendChild(a);
      a.click();
      a.remove();
      URL.revokeObjectURL(url);
      notify({ type: 'success', title: 'تم تصدير قائمة التدفقات النقدية بنجاح' });
    } catch {
      notify({ type: 'error', title: 'فشل تصدير قائمة التدفقات النقدية' });
    } finally {
      setExporting(false);
    }
  }

  function renderSection(label: string, section?: { items?: Array<{ label?: string; amount?: number }>; total?: number }) {
    if (!section) return null;
    return (
      <div className="mb-6">
        <h3 className="mb-2 text-lg font-bold">{label}</h3>
        <table className="w-full text-sm">
          <tbody>
            {section.items?.map((item, i) => (
              <tr key={i} className="border-b">
                <td className="py-1 text-sm">{item.label ?? '—'}</td>
                <td className="py-1 text-end"><MoneyDisplay value={item.amount ?? 0} /></td>
              </tr>
            ))}
          </tbody>
        </table>
        <div className="text-muted-foreground mt-1 text-end text-sm font-bold">
          الإجمالي: <MoneyDisplay value={section.total ?? 0} />
        </div>
      </div>
    );
  }

  return (
    <Page
      title="قائمة التدفقات النقدية"
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
          <FilterDate
            label="من تاريخ"
            value={filters.startDate ?? ''}
            onChange={(v) => setFilters((f) => ({ ...f, startDate: v || undefined }))}
          />
          <FilterDate
            label="إلى تاريخ"
            value={filters.endDate ?? ''}
            onChange={(v) => setFilters((f) => ({ ...f, endDate: v || undefined }))}
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
      ) : !validFilters || (isLoading && !report) ? null : !report ? (
        <EmptyState message="لا توجد بيانات للفترة المحددة" />
      ) : (
        <>
          {report.reconciled === false && (
            <div className="mb-3 flex items-center gap-1 rounded-md border border-amber-300 bg-amber-50 px-3 py-2 text-sm text-amber-800 dark:border-amber-700 dark:bg-amber-950 dark:text-amber-200">
              <AlertTriangle size={14} />
              تنبيه: عدم تطابق في التدفقات النقدية
            </div>
          )}
          {renderSection('التدفقات التشغيلية', report.operating)}
          {renderSection('التدفقات الاستثمارية', report.investing)}
          {renderSection('التدفقات التمويلية', report.financing)}
          <div className="mt-4 border-t pt-3 space-y-2 text-end text-sm font-bold">
            <div>صافي التغيير: <MoneyDisplay value={report.netChange ?? 0} /></div>
            <div>التدفقات النقدية الأولية: <MoneyDisplay value={report.openingCash ?? 0} /></div>
            <div>التدفقات النقدية النهائية: <MoneyDisplay value={report.closingCash ?? 0} /></div>
          </div>
          {report.warning && (
            <div className="mt-3 rounded-md border border-amber-300 bg-amber-50 px-3 py-2 text-sm text-amber-800 dark:border-amber-700 dark:bg-amber-950 dark:text-amber-200">
              {report.warning}
            </div>
          )}
        </>
      )}
    </Page>
  );
}
