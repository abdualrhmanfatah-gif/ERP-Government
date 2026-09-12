import { useNavigate } from 'react-router-dom';
import { Page } from '@/components/ui';
import { notify } from '@/features/notifications/notify';
import { useCreateFiscalYear } from '../../hooks/useFiscalYears';
import { FiscalYearForm } from '../components/FiscalYearForm';

export default function FiscalYearCreatePage() {
  const navigate = useNavigate();
  const createMutation = useCreateFiscalYear();

  function onSubmit(data: { name: string; startDate: string; endDate: string }) {
    createMutation.mutate(data, {
      onSuccess: () => {
        notify({ type: 'success', title: 'تم إنشاء السنة المالية بنجاح' });
        navigate('/financial-settings/fiscal-years');
      },
      onError: () => notify({ type: 'error', title: 'حدث خطأ أثناء الإنشاء' }),
    });
  }

  return (
    <Page title="سنة مالية جديدة" maxWidth="sm">
      <FiscalYearForm
        onSubmit={onSubmit}
        onCancel={() => navigate('/financial-settings/fiscal-years')}
        isPending={createMutation.isPending}
      />
    </Page>
  );
}
