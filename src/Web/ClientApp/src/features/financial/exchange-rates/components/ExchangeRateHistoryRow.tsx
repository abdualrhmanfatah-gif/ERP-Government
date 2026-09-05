import type { ExchangeRateDto } from '../../../../web-api-client';

interface ExchangeRateHistoryRowProps {
  rates: ExchangeRateDto[];
  baseCurrencyId?: number;
  currencyId?: number;
}

/**
 * Inline expandable row showing mini-table of historical rates
 * for the same currency pair, sorted by date descending.
 */
export function ExchangeRateHistoryRow({
  rates,
  baseCurrencyId,
  currencyId,
}: ExchangeRateHistoryRowProps) {
  const filteredRates = rates
    .filter(
      (r) =>
        r.baseCurrencyId === baseCurrencyId &&
        r.currencyId === currencyId
    )
    .sort((a, b) => {
      const dateA = a.rateDate ? new Date(a.rateDate).getTime() : 0;
      const dateB = b.rateDate ? new Date(b.rateDate).getTime() : 0;
      return dateB - dateA; // Descending
    });

  const formatDate = (d: Date | string | undefined) =>
    d ? new Date(d).toLocaleDateString('ar-EG') : '—';

  const formatRateType = (t: string | number | undefined) => {
    if (t === 'official' || t === 'Official' || t === 0) return 'رسمي';
    return 'سوق';
  };

  if (filteredRates.length === 0) {
    return (
      <div className="p-4 text-center text-sm text-[var(--color-on-surface-variant)]">
        لا توجد سجلات أسعار صرف لهذه الزوج
      </div>
    );
  }

  return (
    <div className="overflow-x-auto">
      <table
        role="table"
        className="w-full border-collapse text-xs leading-relaxed"
        aria-label="سجل أسعار الصرف"
      >
        <thead>
          <tr>
            <th scope="col" className="px-3 py-2 text-start font-semibold text-[var(--color-primary)] bg-[var(--color-surface)] border-b border-[var(--color-border-container)]">
              التاريخ
            </th>
            <th scope="col" className="px-3 py-2 text-start font-semibold text-[var(--color-primary)] bg-[var(--color-surface)] border-b border-[var(--color-border-container)]">
              النوع
            </th>
            <th scope="col" className="px-3 py-2 text-start font-semibold text-[var(--color-primary)] bg-[var(--color-surface)] border-b border-[var(--color-border-container)]">
              السعر
            </th>
          </tr>
        </thead>
        <tbody>
          {filteredRates.map((rate) => (
            <tr
              key={rate.id}
              className="bg-[var(--color-surface-container-lowest)] hover:bg-[var(--color-surface-container-low)] transition-colors duration-100"
            >
              <td className="px-3 py-2 border-b border-[var(--color-border-container)]">
                {formatDate(rate.rateDate)}
              </td>
              <td className="px-3 py-2 border-b border-[var(--color-border-container)]">
                {formatRateType(rate.rateType)}
              </td>
              <td className="px-3 py-2 border-b border-[var(--color-border-container)] tabular-nums">
                {rate.rate?.toLocaleString('ar-EG') ?? '—'}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
