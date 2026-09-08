import { FilterSelect } from '@/components/ui/FilterSelect';
import type { FiscalYearDto, FiscalPeriodDto } from '@/web-api-client';
import type { FinancialStatementFilters } from '@/features/reporting/financial-statements/shared/types';

interface ReportingFinancialStatementFiltersProps {
  fiscalYears?: FiscalYearDto[];
  periods?: FiscalPeriodDto[];
  value: Partial<FinancialStatementFilters>;
  onChange: (filters: Partial<FinancialStatementFilters>) => void;
}

export function ReportingFinancialStatementFilters({
  fiscalYears,
  periods,
  value,
  onChange,
}: ReportingFinancialStatementFiltersProps) {
  const yearOptions = (fiscalYears ?? []).map((fy) => ({
    value: String(fy.id),
    label: fy.name ?? String(fy.yearNumber),
  }));

  const periodOptions = (periods ?? []).map((p) => ({
    value: String(p.id),
    label: p.name ?? `فترة ${p.periodNumber}`,
  }));

  return (
    <div className="flex gap-2 items-center flex-wrap">
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
        placeholder="من الفترة"
        label="من الفترة"
      />
      <FilterSelect
        value={value.fiscalPeriodEndId ? String(value.fiscalPeriodEndId) : ''}
        onChange={(v) => onChange({ ...value, fiscalPeriodEndId: v ? Number(v) : undefined })}
        options={periodOptions}
        placeholder="إلى الفترة"
        label="إلى الفترة"
      />
    </div>
  );
}
