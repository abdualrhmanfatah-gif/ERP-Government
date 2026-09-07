import { useParams, useNavigate } from 'react-router-dom';
import { PageHeader } from '@/components/ui/PageHeader';
import { Button, Loading, EmptyState } from '@/components/ui';
import { JournalForm } from '@/components/AccountingJournalForm';
import { useJournalById } from '../../hooks/useJournalById';
import { useUpdateJournal } from '../../hooks/useUpdateJournal';
import { showToast } from '@/components/ui/Toast';

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

  if (isLoading) {
    return <Loading />;
  }

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
      showToast('success', 'تم تعديل الدفتر بنجاح');
      navigate('/accounting/journals');
    } catch {
      showToast('error', 'فشل تعديل الدفتر — تحقق من صلاحية التعديل أو تعارض البيانات');
    }
  };

  return (
    <div>
      <PageHeader title={`تعديل الدفتر: ${journal.name}`} description={`الرمز: ${journal.code}`} />
      <JournalForm
        initialData={journal}
        onSubmit={handleSubmit}
        loading={isPending}
        lockedFields={lockedFieldsWhenUsed}
      />
    </div>
  );
}
