import { useNavigate } from 'react-router-dom';
import { PageHeader } from '@/components/ui/PageHeader';
import { TemplateForm } from '@/components/AccountingTemplateForm';
import { useCreateTemplate } from '../../hooks/useCreateTemplate';
import { useJournalsList } from '../../hooks/useJournalsList';
import { showToast } from '@/components/ui/Toast';

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
      showToast('success', 'تم إنشاء القالب بنجاح');
      navigate('/accounting/templates');
    } catch {
      showToast('error', 'فشل إنشاء القالب');
    }
  };

  return (
    <div>
      <PageHeader title="إنشاء قالب جديد" description="إضافة قالب قيود يومية" />
      <TemplateForm journals={journals} onSubmit={handleSubmit} loading={isPending} />
    </div>
  );
}
