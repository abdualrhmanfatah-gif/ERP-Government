import { FilterBar } from '@/components/ui/FilterBar';
import { FilterSelect } from '@/components/ui/FilterSelect';
import type { FiscalYearDto } from '@/web-api-client';
import type { AvailabilitySnapshotFilters } from '@/features/reporting/availability-snapshot-report/shared/types';

interface ReportingAvailabilitySnapshotFiltersProps {
  fiscalYears?: FiscalYearDto[];
  value: Partial<AvailabilitySnapshotFilters>;
  onChange: (filters: Partial<AvailabilitySnapshotFilters>) => void;
}

export function ReportingAvailabilitySnapshotFilters({
  fiscalYears,
  value,
  onChange,
}: ReportingAvailabilitySnapshotFiltersProps) {
  const yearOptions = (fiscalYears ?? []).map((fy) => ({
    value: String(fy.id),
    label: fy.name ?? String(fy.yearNumber),
  }));

  const hasFilters = value.budgetItemId || value.fundId;

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
    </FilterBar>
  );
}
