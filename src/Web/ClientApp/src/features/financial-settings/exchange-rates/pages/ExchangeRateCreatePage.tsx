import { useNavigate } from 'react-router-dom';
import { Page } from '@/components/ui';
import { notify } from '@/features/notifications/notify';
import { useCreateExchangeRate } from '../../hooks/useExchangeRates';
import { useCurrenciesList } from '../../hooks/useCurrencies';
import { ExchangeRateForm } from '../components/ExchangeRateForm';

export default function ExchangeRateCreatePage() {
  const navigate = useNavigate();
  const createMutation = useCreateExchangeRate();
  const { data: currencies = [], isLoading: currenciesLoading } = useCurrenciesList(true);

  const currencyOptions = currencies.map((c) => ({ value: String(c.id), label: `${c.code} — ${c.name}` }));

  function onSubmit(data: { baseCurrencyId: number; currencyId: number; rateDate: string; rateType: number; rate: number }) {
    createMutation.mutate(data, {
      onSuccess: () => {
        notify({ type: 'success', title: 'تم إنشاء سعر الصرف بنجاح' });
        navigate('/financial-settings/exchange-rates');
      },
      onError: () => notify({ type: 'error', title: 'حدث خطأ أثناء الإنشاء' }),
    });
  }

  return (
    <Page title="سعر صرف جديد" maxWidth="sm" loading={currenciesLoading}>
      <ExchangeRateForm
        onSubmit={onSubmit}
        onCancel={() => navigate('/financial-settings/exchange-rates')}
        isPending={createMutation.isPending}
        currencyOptions={currencyOptions}
      />
    </Page>
  );
}
