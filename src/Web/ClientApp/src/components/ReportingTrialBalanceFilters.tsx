import { FilterBar } from '@/components/ui/FilterBar';
import { FilterSelect } from '@/components/ui/FilterSelect';
import type { FiscalYearDto, FiscalPeriodDto } from '@/web-api-client';
import type { TrialBalanceFilters } from '@/features/reporting/trial-balance-report/shared/types';

interface ReportingTrialBalanceFiltersProps {
  fiscalYears?: FiscalYearDto[];
  periods?: FiscalPeriodDto[];
  value: Partial<TrialBalanceFilters>;
  onChange: (filters: Partial<TrialBalanceFilters>) => void;
}

export function ReportingTrialBalanceFilters({
  fiscalYears,
  periods,
  value,
  onChange,
}: ReportingTrialBalanceFiltersProps) {
  const yearOptions = (fiscalYears ?? []).map((fy) => ({
    value: String(fy.id),
    label: fy.name ?? String(fy.yearNumber),
  }));

  const periodOptions = (periods ?? []).map((p) => ({
    value: String(p.id),
    label: p.name ?? `فترة ${p.periodNumber}`,
  }));

  const hasFilters = value.fiscalPeriodId;

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
        value={value.fiscalPeriodId ? String(value.fiscalPeriodId) : ''}
        onChange={(v) => onChange({ ...value, fiscalPeriodId: v ? Number(v) : undefined })}
        options={periodOptions}
        placeholder="الفترة"
        label="الفترة"
      />
    </FilterBar>
  );
}
