import { useFiscalYearByDate } from '../hooks/useFiscalYearByDate';

interface FiscalYearIndicatorProps {
  date: string;
}

export function FiscalYearIndicator({ date }: FiscalYearIndicatorProps) {
  const { data: fiscalInfo, isLoading, error } = useFiscalYearByDate(date);

  if (isLoading) {
    return (
      <div className="text-xs text-[var(--color-on-surface-variant)] bg-[var(--color-surface-container-low)] rounded-lg px-3 py-2">
        جاري التحقق من السنة المالية...
      </div>
    );
  }

  if (error || !fiscalInfo) {
    return (
      <div className="text-xs text-[var(--color-error)] bg-[var(--color-error-container)] rounded-lg px-3 py-2">
        لا توجد سنة مالية مفتوحة لهذا التاريخ
      </div>
    );
  }

  return (
    <div className="flex gap-4 text-xs text-[var(--color-on-surface-variant)] bg-[var(--color-surface-container-low)] rounded-lg px-3 py-2">
      <span>
        السنة المالية: <strong>{fiscalInfo.fiscalYearCode} — {fiscalInfo.fiscalYearName}</strong>
      </span>
      <span>
        الفترة: <strong>{fiscalInfo.fiscalPeriodName}</strong>
      </span>
    </div>
  );
}
