import { useEffect, useMemo, useState } from 'react';
import { Download, FileText, AlertTriangle } from 'lucide-react';
import { notify } from '@/features/notifications/notify';
import {
  useTrialBalanceReport,
  useTrialBalanceFiscalYears,
  useTrialBalancePeriods,
  selectDefaultFiscalYear,
} from '../hooks/useTrialBalanceReport';
import { downloadTrialBalanceExport } from '../shared/client';
import {
  type TrialBalanceFilters,
  type TrialBalanceLineDto,
} from '../shared/types';
import { trialBalanceFilterSchema } from '../shared/schemas';
import { ReportingTrialBalanceFilters } from '@/components/ReportingTrialBalanceFilters';
import { ReportingTrialBalanceDetail } from '@/components/ReportingTrialBalanceDetail';
import { Page, DataGrid, MoneyDisplay, EmptyState, ErrorState, Button, Alert, Badge } from '@/components/ui';
import type { DataGridColumn } from '@/components/ui/DataGrid';

const accountTypeLabels: Record<string, string> = {
  Asset: 'أصول',
  Liability: 'خصوم',
  Equity: 'حقوق ملكية',
  Revenue: 'إيرادات',
  Expense: 'مصروفات',
};

export default function TrialBalanceReportPage() {
  const { data: fiscalYears, isLoading: yearsLoading } = useTrialBalanceFiscalYears();

  const [filters, setFilters] = useState<Partial<TrialBalanceFilters>>({});
  const defaultYear = useMemo(
    () => selectDefaultFiscalYear(fiscalYears),
    [fiscalYears],
  );

  useEffect(() => {
    if (defaultYear?.id && filters.fiscalYearId === undefined) {
      setFilters((f) => ({ ...f, fiscalYearId: defaultYear.id! }));
    }
  }, [defaultYear?.id, filters.fiscalYearId]);

  const { data: periods } = useTrialBalancePeriods(filters.fiscalYearId);

  const parsed = trialBalanceFilterSchema.safeParse(filters);
  const validFilters = parsed.success ? (parsed.data as TrialBalanceFilters) : null;

  const { data: report, isLoading, isError, refetch } = useTrialBalanceReport(validFilters);

  const [detailAccountId, setDetailAccountId] = useState<number | null>(null);

  const lines = useMemo(() => report?.lines ?? [], [report]);
  const totals = report?.totals;

  const isBalanced = totals && totals.totalDebits === totals.totalCredits;

  const columns: DataGridColumn<TrialBalanceLineDto>[] = [
    { header: 'كود الحساب', cell: (row) => <span dir="ltr" className="font-mono text-sm">{row.accountCode}</span> },
    { header: 'اسم الحساب', cell: (row) => <span className="text-sm">{row.accountName}</span> },
    { header: 'النوع', cell: (row) => <span className="text-sm">{accountTypeLabels[row.accountType ?? ''] ?? row.accountType ?? '—'}</span> },
    { header: 'مدين افتتاحي', align: 'right', cell: (row) => <MoneyDisplay value={row.openingDebit ?? 0} /> },
    { header: 'دائن افتتاحي', align: 'right', cell: (row) => <MoneyDisplay value={row.openingCredit ?? 0} /> },
   
    { header: 'حركة المدين', align: 'right', cell: (row) => <MoneyDisplay value={row.debitTotal ?? 0} /> },
    { header: 'حركة الدائن', align: 'right', cell: (row) => <MoneyDisplay value={row.creditTotal ?? 0} /> },
    { header: 'الرصيد الختامي', align: 'right', cell: (row) => <MoneyDisplay value={row.closingBalance ?? 0} /> },
  ];

  const isPartialData = defaultYear?.status === 'Open' && validFilters !== null;

  const [exporting, setExporting] = useState(false);

  async function handleExport(format: 'xlsx' | 'pdf') {
    if (!validFilters) return;
    setExporting(true);
    try {
      await downloadTrialBalanceExport(validFilters, format);
      notify({ type: 'success', title: 'تم تصدير الميزان بنجاح' });
    } catch {
      notify({ type: 'error', title: 'فشل تصدير الميزان' });
    } finally {
      setExporting(false);
    }
  }

  return (
    <Page
      title="ميزان المراجعة"
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
        <ReportingTrialBalanceFilters
          fiscalYears={fiscalYears}
          periods={periods}
          value={filters}
          onChange={setFilters}
        />
      }
    >
      {isPartialData && (
        <Alert variant="warning">
          أرقام قابلة للتغير — الفترة غير مغلقة
        </Alert>
      )}
      {totals && (
        <div className="mb-3 flex items-center gap-2">
          {isBalanced ? (
            <Badge variant="success">ميزان — متماثل (عرض)</Badge>
          ) : (
            <Badge variant="danger">
              <AlertTriangle size={14} className="me-1 inline" />
              تنبيه: عدم توازن في الدفاتر
            </Badge>
          )}
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
            rowKey={(row) => row.accountId ?? 0}
            onRowClick={(row) => setDetailAccountId(row.accountId ?? null)}
          />
          <ReportingTrialBalanceDetail
            accountId={detailAccountId}
            fiscalYearId={validFilters?.fiscalYearId ?? 0}
            onClose={() => setDetailAccountId(null)}
          />
          {totals && (
            <div className="mt-3 border-t pt-2" data-testid="trial-balance-totals">
              <div className="flex items-center justify-between text-sm font-bold">
                <span>الإجماليات</span>
                <div className="flex items-center gap-6">
                  <span className="flex items-center gap-1">
                    <span className="text-muted-foreground font-normal">الرصيد الافتتاحي:</span>
                    <MoneyDisplay value={totals.totalOpeningBalance ?? 0} />
                  </span>
                  <span className="flex items-center gap-1">
                    <span className="text-muted-foreground font-normal">مدين:</span>
                    <MoneyDisplay value={totals.totalDebits ?? 0} />
                  </span>
                  <span className="flex items-center gap-1">
                    <span className="text-muted-foreground font-normal">دائن:</span>
                    <MoneyDisplay value={totals.totalCredits ?? 0} />
                  </span>
                  <span className="flex items-center gap-1">
                    <span className="text-muted-foreground font-normal">الرصيد الختامي:</span>
                    <MoneyDisplay value={totals.totalClosingBalance ?? 0} />
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
