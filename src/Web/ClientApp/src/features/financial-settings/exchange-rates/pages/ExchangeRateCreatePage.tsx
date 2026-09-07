import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Button, Card, Input, Select, Loading } from '@/components/ui';
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

  if (currenciesLoading) return <Loading />;

  return (
    <div className="max-w-2xl mx-auto space-y-6">
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="icon" onClick={() => navigate('/financial-settings/exchange-rates')} className="cursor-pointer">
          <ArrowRight size={18} />
        </Button>
        <h1 className="text-headline-sm sm:text-headline-md font-bold text-[var(--color-on-surface)]">سعر صرف جديد</h1>
      </div>

      <Card className="bg-[var(--color-surface-container-lowest)]">
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <Select
                label="العملة الأساسية *"
                id="baseCurrencyId"
                name="baseCurrencyId"
                required
                value=""
                options={[
                  { value: '', label: 'اختر...' },
                  ...currencies.map((c) => ({ value: String(c.id), label: `${c.code} — ${c.name}` })),
                ]}
              />
              {errors.baseCurrencyId && <p className="text-xs text-[var(--color-error)] mt-1">{errors.baseCurrencyId}</p>}
            </div>
            <div>
              <Select
                label="العملة *"
                id="currencyId"
                name="currencyId"
                required
                options={[
                  { value: '', label: 'اختر...' },
                  ...currencies.map((c) => ({ value: String(c.id), label: `${c.code} — ${c.name}` })),
                ]}
              />
              {errors.currencyId && <p className="text-xs text-[var(--color-error)] mt-1">{errors.currencyId}</p>}
            </div>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <Input
                label="تاريخ السعر *"
                id="rateDate"
                name="rateDate"
                type="date"
                required
              />
              {errors.rateDate && <p className="text-xs text-[var(--color-error)] mt-1">{errors.rateDate}</p>}
            </div>
            <div>
              <Select
                label="نوع السعر *"
                id="rateType"
                name="rateType"
                required
                options={Object.entries(exchangeRateTypeLabels).map(([value, label]) => ({ value, label }))}
              />
            </div>
          </div>
          <div>
            <Input
              label="السعر *"
              id="rate"
              name="rate"
              type="number"
              step="0.000001"
              min="0"
              required
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
      </Card>
    </div>
  );
}
