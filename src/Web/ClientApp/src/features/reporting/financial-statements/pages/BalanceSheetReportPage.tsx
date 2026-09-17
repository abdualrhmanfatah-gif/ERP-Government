import { useEffect, useMemo, useState } from 'react';
import { Download, FileText, Scale } from 'lucide-react';

import { notify } from '@/features/notifications/notify';
import { buildExportUrl, downloadBlobExport } from '@/shared/utils/download';

import {
  useBalanceSheet,
  useFinancialStatementFiscalYears,
  selectDefaultFiscalYear,
} from '../hooks/useFinancialStatements';

import type { FinancialStatementFilters } from '../shared/types';
import { balanceSheetFilterSchema } from '../shared/schemas';

import {
  Alert,
  Button,
  EmptyState,
  ErrorState,
  FilterDate,
  FilterSelect,
  MoneyDisplay,
  Page,
} from '@/components/ui';

type BalanceSheetLine = {
  accountCode?: string;
  accountName?: string;
  balance?: number;
};

type BalanceSheetSection = {
  title?: string;
  lines?: BalanceSheetLine[];
  total?: number;
};

type BalanceSheetGroup = {
  sections?: BalanceSheetSection[];
  total?: number;
};

type DetailedBalanceSheet = {
  asOfDate?: string | Date;
  assets?: BalanceSheetGroup;
  currentAssets?: BalanceSheetGroup;
  nonCurrentAssets?: BalanceSheetGroup;
  liabilities?: BalanceSheetGroup;
  currentLiabilities?: BalanceSheetGroup;
  nonCurrentLiabilities?: BalanceSheetGroup;
  equity?: BalanceSheetGroup;
  liabilitiesAndEquity?: number;
  balanced?: boolean;
};

type FiscalYearWithDates = {
  id?: number;
  name?: string;
  yearNumber?: number;
  status?: string;
  startDate?: string | Date;
  endDate?: string | Date;
};

function toDateInputValue(value: string | Date | undefined) {
  if (!value) {
    return undefined;
  }

  if (value instanceof Date) {
    return value.toISOString().slice(0, 10);
  }

  return value.slice(0, 10);
}

function minDateInputValue(first: string, second: string) {
  return first <= second ? first : second;
}

function fiscalYearDefaultAsOfDate(year: FiscalYearWithDates | undefined) {
  const today = new Date().toISOString().slice(0, 10);
  const endDate = toDateInputValue(year?.endDate);

  return endDate ? minDateInputValue(endDate, today) : today;
}

function hasLines(group: BalanceSheetGroup | undefined) {
  return group?.sections?.some((section) => section.lines?.length) ?? false;
}

function groupTotal(group: BalanceSheetGroup | undefined) {
  return group?.total ?? 0;
}

function displayText(value: string | undefined) {
  const text = value?.trim();

  return text && text.length > 0 ? text : '-';
}

function StatementGroup({
  title,
  group,
}: {
  title: string;
  group?: BalanceSheetGroup;
}) {
  if (!group) {
    return null;
  }

  return (
    <section className="overflow-hidden rounded-lg border bg-[var(--color-surface)]">
      <div className="flex items-center justify-between gap-4 border-b bg-muted/30 px-4 py-3">
        <h2 className="text-sm font-bold sm:text-base">{title}</h2>

        <div className="text-sm font-bold tabular-nums">
          <MoneyDisplay value={group.total ?? 0} />
        </div>
      </div>

      <div>
        {group.sections?.map((section, sectionIndex) => (
          <div key={`${section.title ?? 'section'}-${sectionIndex}`} className="border-b last:border-b-0">
            <div className="flex items-center justify-between gap-4 bg-muted/10 px-4 py-2.5">
              <h3 className="text-sm font-semibold">{section.title ?? 'غير مصنف'}</h3>

              <div className="text-sm font-semibold tabular-nums">
                <MoneyDisplay value={section.total ?? 0} />
              </div>
            </div>

            {section.lines?.length ? (
              <div className="divide-y">
                {section.lines.map((line, lineIndex) => (
                  <div
                    key={`${line.accountCode ?? line.accountName ?? 'line'}-${lineIndex}`}
                    className="grid grid-cols-[80px_minmax(0,1fr)_minmax(110px,auto)] items-center gap-3 px-4 py-2.5 transition-colors hover:bg-muted/20"
                  >
                    <span dir="ltr" className="font-mono text-sm">
                      {displayText(line.accountCode)}
                    </span>

                    <span className="min-w-0 text-sm">{displayText(line.accountName)}</span>

                    <span className="text-end text-sm tabular-nums">
                      <MoneyDisplay value={line.balance ?? 0} />
                    </span>
                  </div>
                ))}
              </div>
            ) : (
              <div className="px-4 py-5 text-center text-sm text-muted-foreground">
                لا توجد تفاصيل
              </div>
            )}
          </div>
        ))}
      </div>

      <div className="flex items-center justify-between gap-4 border-t bg-muted/20 px-4 py-3">
        <span className="text-sm font-bold">إجمالي {title}</span>

        <span className="text-sm font-bold tabular-nums">
          <MoneyDisplay value={group.total ?? 0} />
        </span>
      </div>
    </section>
  );
}

function StatementTotal({
  title,
  hint,
  value,
}: {
  title: string;
  hint?: string;
  value: number;
}) {
  return (
    <div className="rounded-lg border-2 bg-muted/20 px-4 py-4">
      <div className="flex items-center justify-between gap-4">
        <div>
          <p className="font-bold">{title}</p>
          {hint ? <p className="mt-1 text-xs text-muted-foreground">{hint}</p> : null}
        </div>

        <div className="text-lg font-bold tabular-nums">
          <MoneyDisplay value={value} />
        </div>
      </div>
    </div>
  );
}

export default function BalanceSheetReportPage() {
  const { data: fiscalYears, isLoading: yearsLoading } = useFinancialStatementFiscalYears();

  const [filters, setFilters] = useState<Partial<FinancialStatementFilters>>({});
  const [exporting, setExporting] = useState(false);

  const defaultYear = useMemo(() => selectDefaultFiscalYear(fiscalYears), [fiscalYears]);

  useEffect(() => {
    if (!defaultYear?.id || filters.fiscalYearId !== undefined) {
      return;
    }

    setFilters((current) => ({
      ...current,
      fiscalYearId: defaultYear.id,
      asOfDate: current.asOfDate ?? fiscalYearDefaultAsOfDate(defaultYear as FiscalYearWithDates),
    }));
  }, [defaultYear, filters.fiscalYearId]);

  const parsedFilters = balanceSheetFilterSchema.safeParse(filters);
  const validFilters = parsedFilters.success ? (parsedFilters.data as FinancialStatementFilters) : null;

  const {
    data: reportData,
    isLoading,
    isError,
    refetch,
  } = useBalanceSheet(validFilters);

  const report = reportData as DetailedBalanceSheet | undefined;

  const selectedFiscalYear = useMemo(() => {
    if (!validFilters?.fiscalYearId) {
      return undefined;
    }

    return fiscalYears?.find((year) => year.id === validFilters.fiscalYearId);
  }, [fiscalYears, validFilters?.fiscalYearId]);

  const isPartialData = selectedFiscalYear?.status === 'Open' && validFilters !== null;

  const assetsTotal =
    report?.assets?.total ?? groupTotal(report?.currentAssets) + groupTotal(report?.nonCurrentAssets);

  const liabilitiesAndEquity =
    report?.liabilitiesAndEquity ??
    groupTotal(report?.currentLiabilities) + groupTotal(report?.nonCurrentLiabilities) + groupTotal(report?.equity);

  const hasReportDetails =
    hasLines(report?.currentAssets) ||
    hasLines(report?.nonCurrentAssets) ||
    hasLines(report?.currentLiabilities) ||
    hasLines(report?.nonCurrentLiabilities) ||
    hasLines(report?.equity);

  async function handleExport(format: 'xlsx' | 'pdf') {
    if (!validFilters) {
      return;
    }

    setExporting(true);

    try {
      const url = buildExportUrl('/api/Reports/balance-sheet/export', {
        format,
        fiscalYearId: validFilters.fiscalYearId,
        fiscalPeriodId: validFilters.fiscalPeriodId,
        asOfDate: validFilters.asOfDate,
      });

      await downloadBlobExport(url, `BalanceSheet-${validFilters.asOfDate}.${format}`);

      notify({
        type: 'success',
        title: 'تم تصدير الميزانية العمومية بنجاح',
      });
    } catch {
      notify({
        type: 'error',
        title: 'فشل تصدير الميزانية العمومية',
      });
    } finally {
      setExporting(false);
    }
  }

  return (
    <Page
      title="الميزانية العمومية"
      loading={yearsLoading || (isLoading && !report)}
      actions={
        <div className="flex flex-wrap items-center gap-2 print:hidden">
          <Button
            variant="outline"
            size="sm"
            disabled={!report || exporting}
            onClick={() => handleExport('xlsx')}
          >
            <Download size={16} className="ms-1" />
            Excel
          </Button>

          <Button
            variant="outline"
            size="sm"
            disabled={!report || exporting}
            onClick={() => handleExport('pdf')}
          >
            <FileText size={16} className="ms-1" />
            PDF
          </Button>
        </div>
      }
      toolbar={
        <div className="flex flex-wrap items-end gap-3 print:hidden">
          <div className="min-w-[200px]">
            <FilterSelect
              label="السنة المالية"
              value={filters.fiscalYearId ? String(filters.fiscalYearId) : ''}
              onChange={(value) => {
                const fiscalYearId = value ? Number(value) : undefined;
                const fiscalYear = fiscalYears?.find((year) => year.id === fiscalYearId);

                setFilters((current) => ({
                  ...current,
                  fiscalYearId,
                  fiscalPeriodId: undefined,
                  asOfDate: fiscalYearId
                    ? fiscalYearDefaultAsOfDate(fiscalYear as FiscalYearWithDates | undefined)
                    : undefined,
                }));
              }}
              options={(fiscalYears ?? []).map((year) => ({
                value: String(year.id),
                label: year.name ?? String(year.yearNumber),
              }))}
              placeholder="اختر السنة المالية"
            />
          </div>

          <div className="min-w-[200px]">
            <FilterDate
              label="تاريخ الرصيد"
              value={filters.asOfDate ?? ''}
              onChange={(value) =>
                setFilters((current) => ({
                  ...current,
                  asOfDate: value || undefined,
                }))
              }
            />
          </div>
        </div>
      }
    >
      <div className="space-y-5">
        {isPartialData && (
          <Alert variant="warning">
            بيانات السنة المالية الحالية ما زالت جارية، وقد تتغير أرصدة التقرير عند ترحيل قيود جديدة.
          </Alert>
        )}

        {isError ? (
          <ErrorState onRetry={() => refetch()} />
        ) : !validFilters || (isLoading && !report) ? null : !report || !hasReportDetails ? (
          <EmptyState message="لا توجد بيانات للفترة المحددة" />
        ) : (
          <>
            {report.balanced === false && (
              <Alert variant="error">
                الميزانية غير متوازنة. إجمالي الأصول لا يساوي إجمالي الخصوم وحقوق الملكية.
              </Alert>
            )}

            <div className="grid items-start gap-5 xl:grid-cols-2">
              <div className="space-y-5">
                <StatementGroup title="الأصول المتداولة" group={report.currentAssets} />
                <StatementGroup title="الأصول غير المتداولة" group={report.nonCurrentAssets} />
                <StatementTotal
                  title="إجمالي الأصول"
                  hint="الأصول المتداولة وغير المتداولة"
                  value={assetsTotal}
                />
              </div>

              <div className="space-y-5">
                <StatementGroup title="الخصوم المتداولة" group={report.currentLiabilities} />
                <StatementGroup title="الخصوم غير المتداولة" group={report.nonCurrentLiabilities} />
                <StatementGroup title="حقوق الملكية / صافي الأصول" group={report.equity} />
                <StatementTotal
                  title="إجمالي الخصوم وحقوق الملكية"
                  hint="يجب أن يساوي إجمالي الأصول"
                  value={liabilitiesAndEquity}
                />
              </div>
            </div>

            <div className="flex justify-center pt-1">
              <div
                className={[
                  'inline-flex items-center gap-2 rounded-full border px-4 py-2 text-sm font-semibold',
                  report.balanced
                    ? 'bg-muted/30'
                    : 'border-destructive/30 bg-destructive/5 text-destructive',
                ].join(' ')}
              >
                <Scale size={16} />
                {report.balanced ? 'الميزانية متوازنة' : 'الميزانية غير متوازنة'}
              </div>
            </div>
          </>
        )}
      </div>
    </Page>
  );
}
