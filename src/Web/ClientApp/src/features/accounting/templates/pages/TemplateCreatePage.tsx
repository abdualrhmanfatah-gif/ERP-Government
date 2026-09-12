import { useNavigate } from 'react-router-dom';
import { Page } from '@/components/ui';
import { TemplateForm } from '@/components/AccountingTemplateForm';
import { useCreateTemplate } from '../../hooks/useCreateTemplate';
import { useJournalsList } from '../../hooks/useJournalsList';
import { notify } from '@/features/notifications/notify';

export function TemplateCreatePage() {
  const navigate = useNavigate();
  const { mutateAsync, isPending } = useCreateTemplate();
  const { data: journals = [] } = useJournalsList();

  const handleSubmit = async (data: Record<string, unknown>) => {
    try {
      await mutateAsync({
        ...data,
        isSystemTemplate: false,
      });
      notify({ type: 'success', title: 'تم إنشاء القالب بنجاح' });
      navigate('/accounting/templates');
    } catch {
      notify({ type: 'error', title: 'فشل إنشاء القالب' });
    }
  };

  return (
    <Page title="إنشاء قالب جديد" description="إضافة قالب قيود يومية" maxWidth="sm">
      <TemplateForm journals={journals} onSubmit={handleSubmit} loading={isPending} />
    </Page>
  );
}
