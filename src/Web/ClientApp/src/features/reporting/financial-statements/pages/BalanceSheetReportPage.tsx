import { useEffect, useMemo, useState } from 'react';
import { Download, FileText } from 'lucide-react';
import { notify } from '@/features/notifications/notify';
import { downloadBlobExport, buildExportUrl } from '@/shared/utils/download';
import {
  useBalanceSheet,
  useFinancialStatementFiscalYears,
  selectDefaultFiscalYear,
} from '../hooks/useFinancialStatements';
import {
  type FinancialStatementFilters,
} from '../shared/types';
import { balanceSheetFilterSchema } from '../shared/schemas';
import { FilterSelect, FilterDate, Page, MoneyDisplay, EmptyState, ErrorState, Button, Alert } from '@/components/ui';

export default function BalanceSheetReportPage() {
  const { data: fiscalYears, isLoading: yearsLoading } = useFinancialStatementFiscalYears();

  const [filters, setFilters] = useState<Partial<FinancialStatementFilters>>({});
  const defaultYear = useMemo(
    () => selectDefaultFiscalYear(fiscalYears),
    [fiscalYears],
  );

  useEffect(() => {
    if (defaultYear?.id && filters.fiscalYearId === undefined) {
      const today = new Date().toISOString().slice(0, 10);
      setFilters((f) => ({ ...f, fiscalYearId: defaultYear.id!, asOfDate: f.asOfDate ?? today }));
    }
  }, [defaultYear?.id, filters.fiscalYearId, filters.asOfDate]);

  const parsed = balanceSheetFilterSchema.safeParse(filters);
  const validFilters = parsed.success ? (parsed.data as FinancialStatementFilters) : null;

  const { data: report, isLoading, isError, refetch } = useBalanceSheet(validFilters);

  const isPartialData = defaultYear?.status === 'Open' && validFilters !== null;

  const [exporting, setExporting] = useState(false);

  async function handleExport(format: 'xlsx' | 'pdf') {
    setExporting(true);
    try {
      const url = buildExportUrl('/api/Reports/balance-sheet/export', {
        format,
        asOfDate: report?.asOfDate ? new Date(report.asOfDate).toISOString().slice(0, 10) : undefined,
      });
      await downloadBlobExport(url, `BalanceSheet-${new Date().toISOString().slice(0, 10)}.${format}`);
      notify({ type: 'success', title: 'تم تصدير الميزانية العمومية بنجاح' });
    } catch {
      notify({ type: 'error', title: 'فشل تصدير الميزانية العمومية' });
    } finally {
      setExporting(false);
    }
  }

  function renderGroup(label: string, group?: { sections?: Array<{ title?: string; lines?: Array<{ label?: string; amount?: number }>; total?: number }>; total?: number }) {
    if (!group) return null;
    return (
      <div className="mb-6">
        <h3 className="mb-2 text-lg font-bold">{label}</h3>
        {group.sections?.map((section, si) => (
          <div key={si} className="mb-3">
            <h4 className="text-muted-foreground mb-1 text-sm font-semibold">{section.title}</h4>
            <table className="w-full text-sm">
              <tbody>
                {section.lines?.map((line, li) => (
                  <tr key={li} className="border-b">
                    <td className="py-1 text-sm">{line.label ?? '—'}</td>
                    <td className="py-1 text-end"><MoneyDisplay value={line.amount ?? 0} /></td>
                  </tr>
                ))}
              </tbody>
            </table>
            <div className="text-muted-foreground mt-1 text-end text-sm font-bold">
              الإجمالي: <MoneyDisplay value={section.total ?? 0} />
            </div>
          </div>
        ))}
        <div className="border-t pt-2 text-end text-sm font-bold">
          {label}: <MoneyDisplay value={group.total ?? 0} />
        </div>
      </div>
    );
  }

  return (
    <Page
      title="الميزانية العمومية"
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
            label="تاريخ الرصيد"
            value={filters.asOfDate ?? ''}
            onChange={(v) => setFilters((f) => ({ ...f, asOfDate: v || undefined }))}
          />
        </div>
      }
    >
      {isPartialData && (
        <Alert variant="warning">
          بيانات جزئية — الفترة الحالية جارية وقد تتغير الأرقام
        </Alert>
      )}
      {isError ? (
        <ErrorState onRetry={() => refetch()} />
      ) : !validFilters || (isLoading && !report) ? null : !report ? (
        <EmptyState message="لا توجد بيانات للفترة المحددة" />
      ) : (
        <>
          {report.balanced === false && (
            <Alert variant="error">
              تنبيه: الميزانية غير متوازنة (الأصول ≠ الخصوم + حقوق الملكية)
            </Alert>
          )}
          {renderGroup('الأصول', report.assets as any)}
          {renderGroup('الخصوم', report.liabilities as any)}
          {renderGroup('حقوق الملكية', report.equity as any)}
          <div className="mt-4 border-t pt-3 text-end text-sm font-bold">
            الخصوم + حقوق الملكية: <MoneyDisplay value={report.liabilitiesAndEquity ?? 0} />
          </div>
        </>
      )}
    </Page>
  );
}
