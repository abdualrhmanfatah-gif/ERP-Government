import { useNavigate } from 'react-router-dom';
import { PageHeader } from '@/components/ui/PageHeader';
import { JournalForm } from '@/components/AccountingJournalForm';
import { useCreateJournal } from '../../hooks/useCreateJournal';
import { showToast } from '@/components/ui/Toast';

export function JournalCreatePage() {
  const navigate = useNavigate();
  const { mutateAsync, isPending } = useCreateJournal();

  const handleSubmit = async (data: Record<string, unknown>) => {
    try {
      await mutateAsync(data);
      showToast('success', 'تم إنشاء الدفتر بنجاح');
      navigate('/accounting/journals');
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'فشل إنشاء الدفتر';
      showToast('error', message);
    }
  };

  return (
    <div>
      <PageHeader title="إنشاء دفتر جديد" description="إضافة دفتر يومية جديد" />
      <JournalForm onSubmit={handleSubmit} loading={isPending} />
    </div>
  );
}
