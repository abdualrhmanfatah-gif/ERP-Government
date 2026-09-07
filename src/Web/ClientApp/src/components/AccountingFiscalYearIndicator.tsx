import { useFiscalYearByDate } from '@/features/accounting/hooks/useFiscalYearByDate';
import { Loading } from '@/components/ui/Loading';
import { ErrorState } from '@/components/ui/ErrorState';

interface FiscalYearIndicatorProps {
  date: string;
}

export function FiscalYearIndicator({ date }: FiscalYearIndicatorProps) {
  const { data: fiscalInfo, isLoading, error } = useFiscalYearByDate(date);

  if (isLoading) {
    return <Loading text="جاري التحقق من السنة المالية..." />;
  }

  if (error || !fiscalInfo) {
    return <ErrorState message="لا توجد سنة مالية مفتوحة لهذا التاريخ" />;
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
