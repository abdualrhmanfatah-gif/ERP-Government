import { useNavigate } from 'react-router-dom';
import { Page } from '@/components/ui';
import { notify } from '@/features/notifications/notify';
import { useCreateCurrency } from '../../hooks/useCurrencies';
import { CurrencyForm } from '../components/CurrencyForm';

export default function CurrencyCreatePage() {
  const navigate = useNavigate();
  const createMutation = useCreateCurrency();

  function onSubmit(data: { code: string; name: string; symbol: string; decimalPlaces: number; roundingPrecision: number; isBase: boolean }) {
    createMutation.mutate(data, {
      onSuccess: () => {
        notify({ type: 'success', title: 'تم إنشاء العملة بنجاح' });
        navigate('/financial-settings/currencies');
      },
      onError: (err) => notify({ type: 'error', title: err instanceof Error ? err.message : 'حدث خطأ أثناء الإنشاء' }),
    });
  }

  return (
    <Page title="عملة جديدة" maxWidth="sm">
      <CurrencyForm
        onSubmit={onSubmit}
        onCancel={() => navigate('/financial-settings/currencies')}
        isPending={createMutation.isPending}
      />
    </Page>
  );
}
