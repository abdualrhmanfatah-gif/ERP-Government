import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'sonner';
import { PageHeader } from '@/components/ui/PageHeader';
import { Loading } from '@/components/ui/Loading';
import { DocumentSequenceForm } from '../components/DocumentSequenceForm';
import { useDocumentSequence } from '../hooks/useDocumentSequence';
import { useUpdateDocumentSequence } from '../hooks/useUpdateDocumentSequence';

export function DocumentSequenceEditPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: sequence, isLoading, error } = useDocumentSequence(id ? Number(id) : null);
  const updateMutation = useUpdateDocumentSequence();

  const handleSubmit = async (data: Record<string, unknown>) => {
    if (!sequence) return;
    try {
      await updateMutation.mutateAsync({ id: sequence.id, ...data } as never);
      toast.success('تم تعديل التسلسل بنجاح');
      navigate('/financial/document-sequences');
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'حدث خطأ أثناء تعديل التسلسل';
      toast.error(message);
    }
  };

  if (isLoading) return <Loading />;
  if (error || !sequence) return <div className="text-center py-8 text-[var(--color-on-surface-variant)]">خطأ في تحميل البيانات</div>;

  return (
    <div>
      <PageHeader
        title="تعديل التسلسل"
        description={`تعديل تسلسل: ${sequence.name}`}
      />
      <DocumentSequenceForm
        initialData={sequence}
        isEdit
        onSubmit={handleSubmit}
        serverError={updateMutation.error ? 'حدث خطأ أثناء تعديل التسلسل' : undefined}
        loading={updateMutation.isPending}
      />
    </div>
  );
}
