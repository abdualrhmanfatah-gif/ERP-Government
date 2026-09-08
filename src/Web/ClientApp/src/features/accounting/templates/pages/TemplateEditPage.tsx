import { useParams, useNavigate } from 'react-router-dom';
import { Page, Button, Card, Loading, EmptyState } from '@/components/ui';
import { TemplateForm } from '@/components/AccountingTemplateForm';
import { TemplateLinesSection } from '@/components/AccountingTemplateLinesSection';
import { useTemplateById } from '../../hooks/useTemplateById';
import { useUpdateTemplate } from '../../hooks/useUpdateTemplate';
import { useJournalsList } from '../../hooks/useJournalsList';
import { showToast } from '@/components/ui/Toast';

export function TemplateEditPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const templateId = id ? Number(id) : null;
  const { data: template, isLoading } = useTemplateById(templateId);
  const { mutateAsync, isPending } = useUpdateTemplate();
  const { data: journals = [] } = useJournalsList();

  if (!template) {
    return (
      <EmptyState
        message="القالب غير موجود"
        action={
          <Button variant="outline" onClick={() => navigate('/accounting/templates')}>
            العودة للقائمة
          </Button>
        }
      />
    );
  }

  const handleSubmit = async (data: Record<string, unknown>) => {
    try {
      await mutateAsync({
        id: templateId!,
        data: {
          id: templateId!,
          templateName: data.templateName as string,
          description: data.description as string | undefined,
          journalId: data.journalId as number,
          templateType: data.templateType as string,
          rowVersion: template.rowVersion,
        },
      });
      showToast('success', 'تم تعديل القالب بنجاح');
      navigate('/accounting/templates');
    } catch {
      showToast('error', 'فشل تعديل القالب');
    }
  };

  return (
    <Page title={`تعديل القالب: ${template.templateName}`} description={`الدفتر: ${template.journalName}`} maxWidth="sm" loading={isLoading}>
      <TemplateForm
        initialData={template}
        journals={journals}
        onSubmit={handleSubmit}
        loading={isPending}
      />
      <Card variant="default">
        <div className="px-6 py-4 border-b border-[var(--color-outline-variant)] bg-[var(--color-surface-container-low)]">
          <h2 className="text-base font-bold text-[var(--color-on-surface)]">أسطر القالب</h2>
        </div>
        <div className="p-6">
          <TemplateLinesSection templateId={templateId!} lines={(template as unknown as { lines?: unknown[] }).lines ?? []} />
        </div>
      </Card>
    </Page>
  );
}
