import { useEffect, useMemo, useState } from 'react';
import {
  Download,
  FileText,
  Landmark,
  Scale,
  WalletCards,
} from 'lucide-react';

import { notify } from '@/features/notifications/notify';
import {
  buildExportUrl,
  downloadBlobExport,
} from '@/shared/utils/download';

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

/* -------------------------------------------------------------------------- */
/* Types                                                                      */
/* -------------------------------------------------------------------------- */

type BalanceSheetLine = {
  label?: string;
  amount?: number;
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

/* -------------------------------------------------------------------------- */
/* Helpers                                                                    */
/* -------------------------------------------------------------------------- */

function formatArabicDate(value?: string | Date | null) {
  if (!value) return '—';

  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return '—';
  }

  return new Intl.DateTimeFormat('ar-YE', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  }).format(date);
}

/* -------------------------------------------------------------------------- */
/* Summary Card                                                               */
/* -------------------------------------------------------------------------- */

function SummaryCard({
  title,
  value,
  icon,
  description,
}: {
  title: string;
  value: number;
  icon: React.ReactNode;
  description?: string;
}) {
  return (
    <div className="rounded-xl border bg-[var(--color-surface)] p-4 shadow-sm">
      <div className="flex items-start justify-between gap-4">
        <div className="min-w-0">
          <p className="text-sm font-medium text-muted-foreground">
            {title}
          </p>

          <div className="mt-2 text-xl font-bold tabular-nums">
            <MoneyDisplay value={value} />
          </div>

          {description && (
            <p className="mt-1 text-xs text-muted-foreground">
              {description}
            </p>
          )}
        </div>

        <div className="flex size-10 shrink-0 items-center justify-center rounded-lg border bg-muted/40">
          {icon}
        </div>
      </div>
    </div>
  );
}

/* -------------------------------------------------------------------------- */
/* Statement Group                                                            */
/* -------------------------------------------------------------------------- */

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
    <section className="overflow-hidden rounded-xl border bg-[var(--color-surface)]">
      {/* Group Header */}
      <div className="flex items-center justify-between gap-4 border-b bg-muted/30 px-4 py-3">
        <h2 className="font-bold">{title}</h2>

        <div className="text-sm font-bold tabular-nums">
          <MoneyDisplay value={group.total ?? 0} />
        </div>
      </div>

      {/* Sections */}
      <div>
        {group.sections?.map((section, sectionIndex) => (
          <div
            key={`${section.title ?? 'section'}-${sectionIndex}`}
            className="border-b last:border-b-0"
          >
            {/* Section Header */}
            <div className="flex items-center justify-between gap-4 bg-muted/10 px-4 py-2.5">
              <h3 className="text-sm font-semibold">
                {section.title ?? 'غير مصنف'}
              </h3>

              <div className="text-sm font-semibold tabular-nums">
                <MoneyDisplay value={section.total ?? 0} />
              </div>
            </div>

            {/* Lines */}
            {section.lines?.length ? (
              <div className="divide-y">
                {section.lines.map((line, lineIndex) => (
                  <div
                    key={`${line.label ?? 'line'}-${lineIndex}`}
                    className="grid grid-cols-[minmax(0,1fr)_auto] items-center gap-4 px-4 py-2.5 transition-colors hover:bg-muted/20"
                  >
                    <div className="min-w-0 ps-3">
                      <span className="text-sm">
                        {line.label ?? '—'}
                      </span>
                    </div>

                    <div className="min-w-[120px] text-end text-sm tabular-nums">
                      <MoneyDisplay value={line.amount ?? 0} />
                    </div>
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

      {/* Group Total */}
      <div className="flex items-center justify-between gap-4 border-t bg-muted/20 px-4 py-3">
        <span className="text-sm font-bold">
          إجمالي {title}
        </span>

        <span className="text-sm font-bold tabular-nums">
          <MoneyDisplay value={group.total ?? 0} />
        </span>
      </div>
    </section>
  );
}

/* -------------------------------------------------------------------------- */
/* Main Page                                                                  */
/* -------------------------------------------------------------------------- */

export default function BalanceSheetReportPage() {
  const {
    data: fiscalYears,
    isLoading: yearsLoading,
  } = useFinancialStatementFiscalYears();

  const [filters, setFilters] =
    useState<Partial<FinancialStatementFilters>>({});

  const [exporting, setExporting] = useState(false);

  const defaultYear = useMemo(
    () => selectDefaultFiscalYear(fiscalYears),
    [fiscalYears],
  );

  /* ------------------------------------------------------------------------ */
  /* Default filters                                                          */
  /* ------------------------------------------------------------------------ */

  useEffect(() => {
    if (!defaultYear?.id || filters.fiscalYearId !== undefined) {
      return;
    }

    const today = new Date().toISOString().slice(0, 10);

    setFilters((current) => ({
      ...current,
      fiscalYearId: defaultYear.id,
      asOfDate: current.asOfDate ?? today,
    }));
  }, [defaultYear?.id, filters.fiscalYearId]);

  /* ------------------------------------------------------------------------ */
  /* Validation                                                               */
  /* ------------------------------------------------------------------------ */

  const parsedFilters = balanceSheetFilterSchema.safeParse(filters);

  const validFilters = parsedFilters.success
    ? (parsedFilters.data as FinancialStatementFilters)
    : null;

  const {
    data: report,
    isLoading,
    isError,
    refetch,
  } = useBalanceSheet(validFilters);

  /* ------------------------------------------------------------------------ */
  /* Selected Fiscal Year                                                     */
  /* ------------------------------------------------------------------------ */

  const selectedFiscalYear = useMemo(() => {
    if (!validFilters?.fiscalYearId) {
      return undefined;
    }

    return fiscalYears?.find(
      (year) => year.id === validFilters.fiscalYearId,
    );
  }, [fiscalYears, validFilters?.fiscalYearId]);

  const isPartialData =
    selectedFiscalYear?.status === 'Open' &&
    validFilters !== null;

  /* ------------------------------------------------------------------------ */
  /* Totals                                                                   */
  /* ------------------------------------------------------------------------ */

  const assetsTotal =
    (report?.assets as BalanceSheetGroup | undefined)?.total ?? 0;

  const liabilitiesTotal =
    (report?.liabilities as BalanceSheetGroup | undefined)?.total ?? 0;

  const equityTotal =
    (report?.equity as BalanceSheetGroup | undefined)?.total ?? 0;

  const liabilitiesAndEquity =
    report?.liabilitiesAndEquity ??
    liabilitiesTotal + equityTotal;

  /* ------------------------------------------------------------------------ */
  /* Export                                                                   */
  /* ------------------------------------------------------------------------ */

  async function handleExport(format: 'xlsx' | 'pdf') {
    if (!validFilters) {
      return;
    }

    setExporting(true);

    try {
      const url = buildExportUrl(
        '/api/Reports/balance-sheet/export',
        {
          format,
          fiscalYearId: validFilters.fiscalYearId,
          asOfDate: validFilters.asOfDate,
        },
      );

      const date =
        validFilters.asOfDate ??
        new Date().toISOString().slice(0, 10);

      await downloadBlobExport(
        url,
        `BalanceSheet-${date}.${format}`,
      );

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

  /* ------------------------------------------------------------------------ */
  /* Render                                                                   */
  /* ------------------------------------------------------------------------ */

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
              value={
                filters.fiscalYearId
                  ? String(filters.fiscalYearId)
                  : ''
              }
              onChange={(value) =>
                setFilters((current) => ({
                  ...current,
                  fiscalYearId: value
                    ? Number(value)
                    : undefined,
                }))
              }
              options={(fiscalYears ?? []).map((year) => ({
                value: String(year.id),
                label:
                  year.name ??
                  String(year.yearNumber),
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
        {/* Partial data warning */}
        {isPartialData && (
          <Alert variant="warning">
            بيانات السنة المالية الحالية ما زالت جارية، وقد تتغير
            أرصدة التقرير عند ترحيل قيود جديدة.
          </Alert>
        )}

        {/* Request error */}
        {isError ? (
          <ErrorState onRetry={() => refetch()} />
        ) : !validFilters || (isLoading && !report) ? null : !report ? (
          <EmptyState message="لا توجد بيانات للفترة المحددة" />
        ) : (
          <>
            {/* Balance validation */}
            {report.balanced === false && (
              <Alert variant="error">
                الميزانية غير متوازنة. إجمالي الأصول لا يساوي
                إجمالي الخصوم وحقوق الملكية.
              </Alert>
            )}

        


           



            {/* Main statement */}
            <div className="grid items-start gap-5 xl:grid-cols-2">
              {/* Assets */}
              <StatementGroup
                title="الأصول"
                group={report.assets as BalanceSheetGroup}
              />

              {/* Liabilities + Equity */}
              <div className="space-y-5">
                <StatementGroup
                  title="الخصوم"
                  group={
                    report.liabilities as BalanceSheetGroup
                  }
                />

                <StatementGroup
                  title="حقوق الملكية"
                  group={report.equity as BalanceSheetGroup}
                />

                {/* Right side total */}
                <div className="rounded-xl border-2 bg-muted/20 px-4 py-4">
                  <div className="flex items-center justify-between gap-4">
                    <div>
                      <p className="font-bold">
                        إجمالي الخصوم وحقوق الملكية
                      </p>

                      <p className="mt-1 text-xs text-muted-foreground">
                        يجب أن يساوي إجمالي الأصول
                      </p>
                    </div>

                    <div className="text-lg font-bold tabular-nums">
                      <MoneyDisplay
                        value={liabilitiesAndEquity}
                      />
                    </div>
                  </div>
                </div>
              </div>
            </div>

            {/* Footer balance status */}
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

                {report.balanced
                  ? 'الميزانية متوازنة'
                  : 'الميزانية غير متوازنة'}
              </div>
            </div>
          </>
        )}
      </div>
    </Page>
  );
}