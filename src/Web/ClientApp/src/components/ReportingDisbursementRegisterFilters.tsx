import { FilterBar } from '@/components/ui/FilterBar';
import { FilterSelect } from '@/components/ui/FilterSelect';
import type { FiscalYearDto } from '@/web-api-client';
import type { DisbursementRegisterFilters } from '@/features/reporting/disbursement-register-report/shared/types';

interface ReportingDisbursementRegisterFiltersProps {
  fiscalYears?: FiscalYearDto[];
  value: Partial<DisbursementRegisterFilters>;
  onChange: (filters: Partial<DisbursementRegisterFilters>) => void;
}

const statusOptions = [
  { value: 'Draft', label: 'مسودة' },
  { value: 'Submitted', label: 'مُقدّم' },
  { value: 'Approved', label: 'معتمد' },
  { value: 'Paid', label: 'مدفوع' },
  { value: 'Rejected', label: 'مرفوض' },
];

export function ReportingDisbursementRegisterFilters({
  fiscalYears,
  value,
  onChange,
}: ReportingDisbursementRegisterFiltersProps) {
  const yearOptions = (fiscalYears ?? []).map((fy) => ({
    value: String(fy.id),
    label: fy.name ?? String(fy.yearNumber),
  }));

  const hasFilters = value.fiscalPeriodId || value.fundId || value.status;

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
        value={value.status ?? ''}
        onChange={(v) => onChange({ ...value, status: v || undefined })}
        options={statusOptions}
        placeholder="الحالة"
        label="الحالة"
      />
    </FilterBar>
  );
}
