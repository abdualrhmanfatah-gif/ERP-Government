import { useLookupExchangeRate } from '@/features/financial-settings/hooks/useExchangeRates';
import { exchangeRateTypeLabels, ExchangeRateType } from '@/features/financial-settings/shared/types';
import { Loading } from '@/components/ui';

interface ExchangeRateLookupProps {
  baseCurrencyId: number;
  currencyId: number;
  date: string;
}

export function ExchangeRateLookup({ baseCurrencyId, currencyId, date }: ExchangeRateLookupProps) {
  const { data, isLoading, error } = useLookupExchangeRate(
    baseCurrencyId && currencyId && date ? { baseCurrencyId, currencyId, date } : null,
  );

  if (isLoading) return <Loading />;

  if (error || !data) {
    return (
      <div className="text-sm text-[var(--color-on-surface-variant)] p-3 rounded-lg bg-[var(--color-surface-container)]">
        لا يوجد سعر صرف لهذا التاريخ
      </div>
    );
  }

  const rateTypeName = exchangeRateTypeLabels[ExchangeRateType[data.rateType as keyof typeof ExchangeRateType] as ExchangeRateType] ?? data.rateType;

  return (
    <div className="rounded-lg border border-[var(--color-border-container)] bg-[var(--color-surface-container-lowest)] p-4 space-y-2">
      <div className="text-sm font-medium text-[var(--color-on-surface)]">سعر الصرف الفعلي</div>
      <div className="flex items-center gap-4">
        <div className="font-mono text-lg font-bold text-[var(--color-on-surface)]">
          {data.rate.toLocaleString('ar-EG', { minimumFractionDigits: 2, maximumFractionDigits: 6 })}
        </div>
        <span className="text-sm text-[var(--color-on-surface-variant)]">
          ({rateTypeName})
        </span>
      </div>
      <div className="text-xs text-[var(--color-on-surface-variant)]">
        تاريخ السعر: {new Intl.DateTimeFormat('ar-EG', { dateStyle: 'medium' }).format(new Date(data.rateDate))}
      </div>
    </div>
  );
}
