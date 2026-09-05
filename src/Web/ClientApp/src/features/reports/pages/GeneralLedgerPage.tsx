import { useState } from 'react';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button } from '@/components/ui/Button';
import { RefreshCw } from 'lucide-react';
import { ReportFilters } from '../components/ReportFilters';
import { GeneralLedgerGrid } from '../components/GeneralLedgerGrid';
import { ReportExportDropdown } from '../components/ReportExportDropdown';
import { ReportEmptyState } from '../components/ReportEmptyState';
import { useReport } from '../hooks/useReport';
import type { GeneralLedgerDto } from '../types';

const generalLedgerFields = [
  { type: 'search' as const, key: 'accountCode', label: 'رقم الحساب', placeholder: 'e.g. 1001' },
  { type: 'date' as const, key: 'startDate', label: 'من' },
  { type: 'date' as const, key: 'endDate', label: 'إلى' },
];

export function GeneralLedgerPage() {
  const [filters, setFilters] = useState<{
    accountCode?: string;
    startDate?: string;
    endDate?: string;
    page?: number;
    pageSize?: number;
  }>({});

  const { data, isLoading, error, refetch } = useReport<GeneralLedgerDto>({
    endpoint: '/general-ledger',
    params: filters,
    enabled: true,
  });

  return (
    <div className="space-y-6">
      <PageHeader
        title="دفتر الأستاذ"
        description="عرض القيود المحاسبية مرتبة حسب التاريخ"
        actions={
          <div className="flex gap-2">
            {data && <ReportExportDropdown reportType="general-ledger" params={filters} />}
            <Button variant="outline" size="sm" onClick={() => refetch()} disabled={isLoading}>
              <RefreshCw className={`h-4 w-4 ms-1 ${isLoading ? 'animate-spin' : ''}`} />
              تحديث
            </Button>
          </div>
        }
      />

      <ReportFilters fields={generalLedgerFields} onApply={setFilters} />

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

      {data && <GeneralLedgerGrid data={data} />}

      {!data && !isLoading && !error && (
        <ReportEmptyState
          title="دفتر الأستاذ"
          description="اختر معايير البحث لعرض دفتر الأستاذ. يمكنك التصفية برقم الحساب والفترة الزمنية."
        />
      )}
    </div>
  );
}
