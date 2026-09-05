import { useState } from 'react';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { RefreshCw } from 'lucide-react';
import { ReportFilters } from '../components/ReportFilters';
import { BalanceSheetGrid } from '../components/BalanceSheetGrid';
import { ReportExportDropdown } from '../components/ReportExportDropdown';
import { ReportEmptyState } from '../components/ReportEmptyState';
import { useReport } from '../hooks/useReport';
import type { BalanceSheetDto } from '../types';

const balanceSheetFields = [
  { type: 'date' as const, key: 'asOfDate', label: 'التاريخ' },
];

export function BalanceSheetPage() {
  const [filters, setFilters] = useState<{ asOfDate?: string }>({});

  const { data, isLoading, error, refetch } = useReport<BalanceSheetDto>({
    endpoint: '/balance-sheet',
    params: filters,
    enabled: !!filters.asOfDate,
  });

  return (
    <div className="space-y-6">
      <PageHeader
        title="الميزانية العمومية"
        description="عرض الميزانية العمومية حسب تاريخ محدد"
        actions={
          <div className="flex gap-2">
            {data && <ReportExportDropdown reportType="balance-sheet" params={filters} />}
            <Button variant="outline" size="sm" onClick={() => refetch()} disabled={isLoading}>
              <RefreshCw className={`h-4 w-4 ms-1 ${isLoading ? 'animate-spin' : ''}`} />
              تحديث
            </Button>
          </div>
        }
      />

      <ReportFilters fields={balanceSheetFields} onApply={setFilters} />

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

      {data && <BalanceSheetGrid data={data} />}

      {!data && !isLoading && !error && (
        <ReportEmptyState
          title="الميزانية العمومية"
          description="اختر تاريخاً لعرض الميزانية العمومية. ستظهر الأصول والخصوم وحقوق الملكية في تاريخ محدد."
        />
      )}
    </div>
  );
}
