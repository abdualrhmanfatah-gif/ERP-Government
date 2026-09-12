import { useNavigate, useSearchParams } from 'react-router-dom';
import { Page } from '@/components/ui';
import { useCreateEncumbrance, useReverseEncumbrance } from '../../hooks/useEncumbrances';
import { EncumbranceForm } from '../components/EncumbranceForm';

export default function EncumbranceCreatePage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const budgetItemIdParam = searchParams.get('budgetItemId');

  const createEncumbrance = useCreateEncumbrance();
  const reverseEncumbrance = useReverseEncumbrance();

  function onSubmit(data: Record<string, unknown>) {
    if (data.type === 'reversal') {
      reverseEncumbrance.mutate(
        { id: data.id as number, rowVersion: data.rowVersion as string, reason: data.reason as string },
        { onSuccess: () => navigate('/budgeting/encumbrances'), onError: () => {} }
      );
    } else {
      createEncumbrance.mutate(data as any, {
        onSuccess: () => navigate('/budgeting/encumbrances'),
        onError: () => {},
      });
    }
  }

  return (
    <Page title="التزام جديد" maxWidth="sm">
      <EncumbranceForm
        initialData={budgetItemIdParam ? { budgetItemId: Number(budgetItemIdParam) } : undefined}
        onSubmit={onSubmit}
        onCancel={() => navigate('/budgeting/encumbrances')}
        isPending={createEncumbrance.isPending || reverseEncumbrance.isPending}
      />
    </Page>
  );
}
