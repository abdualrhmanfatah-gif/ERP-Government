import { FilterSelect } from '@/components/ui/FilterSelect';
import { Button } from '@/components/ui/Button';
import type { FundDto, BudgetClassificationDto, FiscalYearDto } from '@/web-api-client';
import type { BudgetExecutionFilters } from '@/features/reporting/budget-execution-report/shared/types';

interface ReportingBudgetExecutionFiltersProps {
  fiscalYears?: FiscalYearDto[];
  funds?: FundDto[];
  classifications?: BudgetClassificationDto[];
  value: Partial<BudgetExecutionFilters>;
  onChange: (filters: Partial<BudgetExecutionFilters>) => void;
}

export function ReportingBudgetExecutionFilters({
  fiscalYears,
  funds,
  classifications,
  value,
  onChange,
}: ReportingBudgetExecutionFiltersProps) {
  const yearOptions = (fiscalYears ?? []).map((fy) => ({
    value: String(fy.id),
    label: fy.name ?? String(fy.yearNumber),
  }));
  const fundOptions = (funds ?? []).map((f) => ({
    value: String(f.id),
    label: `${f.fundNumber} — ${f.fundName}`,
  }));

  // Program options: classifications whose parent is a root node (level 2).
  const rootIds = new Set((classifications ?? []).filter((c) => !c.parentId).map((c) => c.id));
  const programOptions = (classifications ?? [])
    .filter((c) => c.parentId !== null && c.parentId !== undefined && rootIds.has(c.parentId))
    .map((c) => ({ value: String(c.id), label: `${c.code} — ${c.name}` }));
  const projectOptions = (classifications ?? [])
    .filter((c) => c.parentId !== null && c.parentId !== undefined && !rootIds.has(c.parentId))
    .map((c) => ({ value: String(c.id), label: `${c.code} — ${c.name}` }));

  const hasFilters = value.fundId || value.programId || value.projectId;

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
        value={value.fundId ? String(value.fundId) : ''}
        onChange={(v) => onChange({ ...value, fundId: v ? Number(v) : undefined })}
        options={fundOptions}
        placeholder="الصندوق"
        label="الصندوق"
      />
      <FilterSelect
        value={value.programId ? String(value.programId) : ''}
        onChange={(v) => onChange({ ...value, programId: v ? Number(v) : undefined })}
        options={programOptions}
        placeholder="البرنامج"
        label="البرنامج"
      />
      <FilterSelect
        value={value.projectId ? String(value.projectId) : ''}
        onChange={(v) => onChange({ ...value, projectId: v ? Number(v) : undefined })}
        options={projectOptions}
        placeholder="المشروع"
        label="المشروع"
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
