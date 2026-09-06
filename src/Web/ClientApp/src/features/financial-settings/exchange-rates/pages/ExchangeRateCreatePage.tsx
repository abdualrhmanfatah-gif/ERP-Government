import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Button } from '@/components/ui';
import { ArrowRight } from 'lucide-react';
import { notify } from '@/features/notifications/notify';
import { useCreateExchangeRate } from '../../hooks/useExchangeRates';
import { useCurrenciesList } from '../../hooks/useCurrencies';
import { exchangeRateTypeLabels } from '../../shared/types';

export default function ExchangeRateCreatePage() {
  const navigate = useNavigate();
  const createMutation = useCreateExchangeRate();
  const { data: currencies = [], isLoading: currenciesLoading } = useCurrenciesList(true);
  const [errors, setErrors] = useState<Record<string, string>>({});

  function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setErrors({});
    const form = new FormData(e.currentTarget);
    const baseCurrencyId = Number(form.get('baseCurrencyId'));
    const currencyId = Number(form.get('currencyId'));
    const rateDate = form.get('rateDate') as string;
    const rateType = Number(form.get('rateType'));
    const rate = Number(form.get('rate'));

    const newErrors: Record<string, string> = {};
    if (!baseCurrencyId) newErrors.baseCurrencyId = 'العملة الأساسية مطلوبة';
    if (!currencyId) newErrors.currencyId = 'العملة مطلوبة';
    if (baseCurrencyId === currencyId) newErrors.currencyId = 'يجب أن تكون العملات مختلفة';
    if (!rateDate) newErrors.rateDate = 'التاريخ مطلوب';
    if (!rate || rate <= 0) newErrors.rate = 'السعر يجب أن يكون أكبر من صفر';

    if (Object.keys(newErrors).length > 0) {
      setErrors(newErrors);
      return;
    }

    createMutation.mutate(
      { baseCurrencyId, currencyId, rateDate, rateType, rate },
      {
        onSuccess: () => {
          notify({ type: 'success', title: 'تم إنشاء سعر الصرف بنجاح' });
          navigate('/financial-settings/exchange-rates');
        },
        onError: () => notify({ type: 'error', title: 'حدث خطأ أثناء الإنشاء' }),
      },
    );
  }

  if (currenciesLoading) return <div className="text-center py-12 text-[var(--color-on-surface-variant)]">جاري التحميل...</div>;

  return (
    <div className="max-w-2xl mx-auto space-y-6">
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="icon" onClick={() => navigate('/financial-settings/exchange-rates')} className="cursor-pointer">
          <ArrowRight size={18} />
        </Button>
        <h1 className="text-headline-sm sm:text-headline-md font-bold text-[var(--color-on-surface)]">سعر صرف جديد</h1>
      </div>

      <div className="rounded-xl border border-[var(--color-border-container)] bg-[var(--color-surface-container-lowest)] p-6">
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label htmlFor="baseCurrencyId" className="block text-sm font-medium text-[var(--color-on-surface-variant)] mb-1">العملة الأساسية *</label>
              <select
                id="baseCurrencyId"
                name="baseCurrencyId"
                required
                className="w-full px-3 py-2 rounded-lg border border-[var(--color-border-container)] bg-[var(--color-surface-container)] text-[var(--color-on-surface)]"
              >
                <option value="">اختر...</option>
                {currencies.map((c) => (
                  <option key={c.id} value={c.id}>{c.code} — {c.name}</option>
                ))}
              </select>
              {errors.baseCurrencyId && <p className="text-xs text-[var(--color-error)] mt-1">{errors.baseCurrencyId}</p>}
            </div>
            <div>
              <label htmlFor="currencyId" className="block text-sm font-medium text-[var(--color-on-surface-variant)] mb-1">العملة *</label>
              <select
                id="currencyId"
                name="currencyId"
                required
                className="w-full px-3 py-2 rounded-lg border border-[var(--color-border-container)] bg-[var(--color-surface-container)] text-[var(--color-on-surface)]"
              >
                <option value="">اختر...</option>
                {currencies.map((c) => (
                  <option key={c.id} value={c.id}>{c.code} — {c.name}</option>
                ))}
              </select>
              {errors.currencyId && <p className="text-xs text-[var(--color-error)] mt-1">{errors.currencyId}</p>}
            </div>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label htmlFor="rateDate" className="block text-sm font-medium text-[var(--color-on-surface-variant)] mb-1">تاريخ السعر *</label>
              <input
                id="rateDate"
                name="rateDate"
                type="date"
                required
                className="w-full px-3 py-2 rounded-lg border border-[var(--color-border-container)] bg-[var(--color-surface-container)] text-[var(--color-on-surface)]"
              />
              {errors.rateDate && <p className="text-xs text-[var(--color-error)] mt-1">{errors.rateDate}</p>}
            </div>
            <div>
              <label htmlFor="rateType" className="block text-sm font-medium text-[var(--color-on-surface-variant)] mb-1">نوع السعر *</label>
              <select
                id="rateType"
                name="rateType"
                required
                className="w-full px-3 py-2 rounded-lg border border-[var(--color-border-container)] bg-[var(--color-surface-container)] text-[var(--color-on-surface)]"
              >
                {Object.entries(exchangeRateTypeLabels).map(([value, label]) => (
                  <option key={value} value={value}>{label}</option>
                ))}
              </select>
            </div>
          </div>
          <div>
            <label htmlFor="rate" className="block text-sm font-medium text-[var(--color-on-surface-variant)] mb-1">السعر *</label>
            <input
              id="rate"
              name="rate"
              type="number"
              step="0.000001"
              min="0"
              required
              className="w-full px-3 py-2 rounded-lg border border-[var(--color-border-container)] bg-[var(--color-surface-container)] text-[var(--color-on-surface)]"
            />
            {errors.rate && <p className="text-xs text-[var(--color-error)] mt-1">{errors.rate}</p>}
          </div>
          <div className="flex justify-end gap-2 pt-4">
            <Button type="button" variant="ghost" onClick={() => navigate('/financial-settings/exchange-rates')} className="cursor-pointer">
              إلغاء
            </Button>
            <Button type="submit" disabled={createMutation.isPending} className="cursor-pointer">
              {createMutation.isPending ? 'جاري الإنشاء...' : 'إنشاء'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
}
