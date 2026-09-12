import { useNavigate } from 'react-router-dom';
import { Page } from '@/components/ui';
import { useCreateRecurringEntry } from '../hooks/useRecurringEntries';
import { RecurringEntryForm } from '../components/RecurringEntryForm';
import type { CreateRecurringEntryCommand } from '../shared/types';

export default function RecurringEntryCreatePage() {
  const navigate = useNavigate();
  const createMutation = useCreateRecurringEntry();

  function onSubmit(data: CreateRecurringEntryCommand) {
    createMutation.mutate(data, {
      onSuccess: () => navigate('/accounting/recurring-entries'),
      onError: () => {},
    });
  }

  return (
    <Page title="إنشاء جدول دوري جديد" maxWidth="sm">
      <RecurringEntryForm
        onSubmit={onSubmit}
        onCancel={() => navigate('/accounting/recurring-entries')}
        isPending={createMutation.isPending}
      />
    </Page>
  );
}
