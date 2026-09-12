import { useParams, useNavigate } from 'react-router-dom';
import { Page, Button, EmptyState } from '@/components/ui';
import { JournalForm } from '@/components/AccountingJournalForm';
import { useJournalById } from '../../hooks/useJournalById';
import { useUpdateJournal } from '../../hooks/useUpdateJournal';
import { notify } from '@/features/notifications/notify';

const lockedFieldsWhenUsed = [
  'code', 'name', 'type', 'accountId', 'suspenseAccountId',
  'allowForeignCurrency', 'sequenceId',
];

export function JournalEditPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const journalId = id ? Number(id) : null;
  const { data: journal, isLoading } = useJournalById(journalId);
  const { mutateAsync, isPending } = useUpdateJournal();

  if (!journal) {
    return (
      <EmptyState
        message="الدفتر غير موجود"
        action={
          <Button variant="outline" onClick={() => navigate('/accounting/journals')}>
            العودة للقائمة
          </Button>
        }
      />
    );
  }

  const handleSubmit = async (data: Record<string, unknown>) => {
    try {
      await mutateAsync({
        id: journalId!,
        data: {
          id: journalId!,
          name: data.name as string,
          type: data.type as string,
          accountId: data.accountId as number | undefined,
          suspenseAccountId: data.suspenseAccountId as number | undefined,
          allowForeignCurrency: data.allowForeignCurrency as boolean,
          requireApprovalBeforePosting: data.requireApprovalBeforePosting as boolean,
          rowVersion: journal.rowVersion,
        },
      });
      notify({ type: 'success', title: 'تم تعديل الدفتر بنجاح' });
      navigate('/accounting/journals');
    } catch {
      notify({ type: 'error', title: 'فشل تعديل الدفتر — تحقق من صلاحية التعديل أو تعارض البيانات' });
    }
  };

  return (
    <Page title={`تعديل الدفتر: ${journal.name}`} description={`الرمز: ${journal.code}`} maxWidth="sm" loading={isLoading}>
      <JournalForm
        initialData={journal}
        onSubmit={handleSubmit}
        loading={isPending}
        lockedFields={lockedFieldsWhenUsed}
      />
    </Page>
  );
}
