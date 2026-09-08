import { FilterSelect } from '@/components/ui/FilterSelect';
import { Button } from '@/components/ui/Button';
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
    <div className="flex gap-2 items-center flex-wrap">
      <FilterSelect
        value={value.fiscalYearId ? String(value.fiscalYearId) : ''}
        onChange={(v) => onChange({ ...value, fiscalYearId: v ? Number(v) : undefined })}
        options={yearOptions}
        placeholder="السنة المالية"
        label="السنة المالية"
      />
      {hasFilters && (
        <Button
          variant="ghost"
          size="sm"
          onClick={() =>
            onChange({ fiscalYearId: value.fiscalYearId })
          }
        >
          مسح الفلاتر
        </Button>
      )}
    </div>
  );
}
