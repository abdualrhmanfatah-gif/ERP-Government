import { useState } from 'react';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { RefreshCw } from 'lucide-react';
import { ReportFilters } from '../components/ReportFilters';
import { CashFlowStatementGrid } from '../components/CashFlowStatementGrid';
import { ReportExportDropdown } from '../components/ReportExportDropdown';
import { ReportEmptyState } from '../components/ReportEmptyState';
import { useReport } from '../hooks/useReport';
import type { CashFlowStatementDto } from '../types';

const cashFlowFields = [
  { type: 'date' as const, key: 'startDate', label: 'من' },
  { type: 'date' as const, key: 'endDate', label: 'إلى' },
];

export function CashFlowStatementPage() {
  const [filters, setFilters] = useState<{ startDate?: string; endDate?: string }>({});

  const { data, isLoading, error, refetch } = useReport<CashFlowStatementDto>({
    endpoint: '/cash-flow',
    params: filters,
    enabled: !!(filters.startDate && filters.endDate),
  });

  return (
    <div className="space-y-6">
      <PageHeader
        title="قائمة التدفقات النقدية"
        description="عرض التدفقات النقدية من الأنشطة التشغيلية والاستثمارية والتمويلية"
        actions={
          <div className="flex gap-2">
            {data && <ReportExportDropdown reportType="cash-flow" params={filters} />}
            <Button variant="outline" size="sm" onClick={() => refetch()} disabled={isLoading}>
              <RefreshCw className={`h-4 w-4 ms-1 ${isLoading ? 'animate-spin' : ''}`} />
              تحديث
            </Button>
          </div>
        }
      />

      <ReportFilters fields={cashFlowFields} onApply={setFilters} />

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

      {data && <CashFlowStatementGrid data={data} />}

      {!data && !isLoading && !error && (
        <ReportEmptyState
          title="قائمة التدفقات النقدية"
          description="اختر تاريخ البداية والنهاية لعرض قائمة التدفقات النقدية. ستظهر الأنشطة التشغيلية والاستثمارية والتمويلية."
        />
      )}
    </div>
  );
}
