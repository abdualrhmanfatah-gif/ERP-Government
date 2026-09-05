import { useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { PageHeader } from '@/components/ui/PageHeader';
import { DocumentSequenceForm } from '../components/DocumentSequenceForm';
import { useCreateDocumentSequence } from '../hooks/useCreateDocumentSequence';

export function DocumentSequenceCreatePage() {
  const navigate = useNavigate();
  const createMutation = useCreateDocumentSequence();

  const handleSubmit = async (data: Record<string, unknown>) => {
    try {
      await createMutation.mutateAsync(data as never);
      toast.success('تم إنشاء التسلسل بنجاح');
      navigate('/financial/document-sequences');
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'حدث خطأ أثناء إنشاء التسلسل';
      toast.error(message);
    }
  };

  return (
    <div>
      <PageHeader
        title="تسلسل جديد"
        description="إنشاء تسلسل مستندات جديد"
      />
      <DocumentSequenceForm
        onSubmit={handleSubmit}
        serverError={createMutation.error ? 'حدث خطأ أثناء إنشاء التسلسل' : undefined}
        loading={createMutation.isPending}
      />
    </div>
  );
}
