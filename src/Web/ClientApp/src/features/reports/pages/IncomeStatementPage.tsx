import { useState } from 'react';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { RefreshCw } from 'lucide-react';
import { ReportFilters } from '../components/ReportFilters';
import { IncomeStatementGrid } from '../components/IncomeStatementGrid';
import { ReportExportDropdown } from '../components/ReportExportDropdown';
import { ReportEmptyState } from '../components/ReportEmptyState';
import { useReport } from '../hooks/useReport';
import type { IncomeStatementDto } from '../types';

const incomeStatementFields = [
  { type: 'date' as const, key: 'startDate', label: 'من' },
  { type: 'date' as const, key: 'endDate', label: 'إلى' },
];

export function IncomeStatementPage() {
  const [filters, setFilters] = useState<{ startDate?: string; endDate?: string }>({});

  const { data, isLoading, error, refetch } = useReport<IncomeStatementDto>({
    endpoint: '/income-statement',
    params: filters,
    enabled: !!(filters.startDate && filters.endDate),
  });

  return (
    <div className="space-y-6">
      <PageHeader
        title="قائمة الدخل"
        description="عرض الإيرادات والمصروفات خلال فترة محددة"
        actions={
          <div className="flex gap-2">
            {data && <ReportExportDropdown reportType="income-statement" params={filters} />}
            <Button variant="outline" size="sm" onClick={() => refetch()} disabled={isLoading}>
              <RefreshCw className={`h-4 w-4 ms-1 ${isLoading ? 'animate-spin' : ''}`} />
              تحديث
            </Button>
          </div>
        }
      />

      <ReportFilters fields={incomeStatementFields} onApply={setFilters} />

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

      {data && <IncomeStatementGrid data={data} />}

      {!data && !isLoading && !error && (
        <ReportEmptyState
          title="قائمة الدخل"
          description="اختر تاريخ البداية والنهاية لعرض قائمة الدخل. ستظهر الإيرادات والمصروفات وصافي الدخل خلال الفترة المحددة."
        />
      )}
    </div>
  );
}
