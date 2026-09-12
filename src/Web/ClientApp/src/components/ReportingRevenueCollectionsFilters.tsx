import { FilterBar } from '@/components/ui/FilterBar';
import { FilterSelect } from '@/components/ui/FilterSelect';
import type { FiscalYearDto } from '@/web-api-client';
import type { RevenueCollectionsFilters } from '@/features/reporting/revenue-collections-report/shared/types';

interface ReportingRevenueCollectionsFiltersProps {
  fiscalYears?: FiscalYearDto[];
  value: Partial<RevenueCollectionsFilters>;
  onChange: (filters: Partial<RevenueCollectionsFilters>) => void;
}

const paymentMethodOptions = [
  { value: 'Cash', label: 'نقدي' },
  { value: 'Check', label: 'شيك' },
  { value: 'Transfer', label: 'تحويل' },
];

export function ReportingRevenueCollectionsFilters({
  fiscalYears,
  value,
  onChange,
}: ReportingRevenueCollectionsFiltersProps) {
  const yearOptions = (fiscalYears ?? []).map((fy) => ({
    value: String(fy.id),
    label: fy.name ?? String(fy.yearNumber),
  }));

  const hasFilters = value.fiscalPeriodId || value.fundId || value.partyId || value.paymentMethod;

  return (
    <FilterBar
      hasFilters={hasFilters}
      onClear={() => onChange({ fiscalYearId: value.fiscalYearId })}
    >
      <FilterSelect
        value={value.fiscalYearId ? String(value.fiscalYearId) : ''}
        onChange={(v) => onChange({ ...value, fiscalYearId: v ? Number(v) : undefined })}
        options={yearOptions}
        placeholder="السنة المالية"
        label="السنة المالية"
      />
      <FilterSelect
        value={value.paymentMethod ?? ''}
        onChange={(v) => onChange({ ...value, paymentMethod: v || undefined })}
        options={paymentMethodOptions}
        placeholder="طريقة الدفع"
        label="طريقة الدفع"
      />
    </FilterBar>
  );
}
