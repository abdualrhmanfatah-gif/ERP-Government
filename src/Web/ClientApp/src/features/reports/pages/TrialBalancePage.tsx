import { useState } from 'react';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { RefreshCw } from 'lucide-react';
import { ReportFilters } from '../components/ReportFilters';
import { TrialBalanceGrid } from '../components/TrialBalanceGrid';
import { ReportExportDropdown } from '../components/ReportExportDropdown';
import { ReportEmptyState } from '../components/ReportEmptyState';
import { useReport } from '../hooks/useReport';
import type { TrialBalanceDto } from '../types';

const trialBalanceFields = [
  {
    type: 'select' as const,
    key: 'fiscalYearId',
    label: 'السنة المالية',
    placeholder: 'اختر السنة المالية',
    fetchUrl: '/api/FiscalYears',
  },
  {
    type: 'select' as const,
    key: 'fiscalPeriodId',
    label: 'الفترة المالية',
    placeholder: 'اختر الفترة المالية',
    fetchUrl: '/api/FiscalPeriods',
    dependsOn: 'fiscalYearId',
  },
];

export function TrialBalancePage() {
  const [filters, setFilters] = useState<{ fiscalYearId?: number; fiscalPeriodId?: number }>({});

  const { data, isLoading, error, refetch } = useReport<TrialBalanceDto>({
    endpoint: '/trial-balance',
    params: filters,
    enabled: !!(filters.fiscalYearId && filters.fiscalPeriodId),
  });

  return (
    <div className="space-y-6">
      <PageHeader
        title="ميزان المراجعة"
        description="عرض أرصدة الحسابات في فترة مالية محددة"
        actions={
          <div className="flex gap-2">
            {data && (
              <ReportExportDropdown
                reportType="trial-balance"
                params={{
                  fiscalYearId: filters.fiscalYearId,
                  fiscalPeriodId: filters.fiscalPeriodId,
                }}
              />
            )}
            <Button variant="outline" size="sm" onClick={() => refetch()} disabled={isLoading}>
              <RefreshCw className={`h-4 w-4 ms-1 ${isLoading ? 'animate-spin' : ''}`} />
              تحديث
            </Button>
          </div>
        }
      />

      <ReportFilters fields={trialBalanceFields} onApply={setFilters} />

      {isLoading && (
        <div className="space-y-4">
          {[1, 2, 3].map(i => (
            <div key={i} className="h-24 bg-[var(--color-surface-container)] rounded-lg animate-pulse" />
          ))}
        </div>
      )}

      {error && (
        <div className="p-4 rounded-lg border border-[var(--color-error)] bg-[var(--color-error-bg)] text-[var(--color-on-surface)]">
          <p className="text-sm font-medium">خطأ في تحميل البيانات</p>
          <p className="text-xs text-[var(--color-on-surface-variant)] mt-1">{(error as Error).message}</p>
        </div>
      )}

      {data && <TrialBalanceGrid data={data} />}

      {!data && !isLoading && !error && (
        <ReportEmptyState
          title="ميزان المراجعة"
          description="اختر السنة المالية والفترة المالية لعرض ميزان المراجعة. ستظهر أرصدة الحسابات مع إجمالي المدين والدائن."
        />
      )}
    </div>
  );
}
