import { useState, useCallback, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { Page, Button } from '@/components/ui';
import { DisbursementRequestForm } from '@/components/DisbursementRequestForm';
import { AttachmentsPanel } from '@/components/DocumentsAttachmentsPanel';
import { FileUploadZone } from '@/components/FileUploadZone';
import { useUploadAttachment } from '@/features/documents/hooks/useDocuments';
import { documentsClient } from '@/features/documents/shared/client';
import { notify } from '@/features/notifications/notify';

export function DisbursementRequestCreatePage() {
  const navigate = useNavigate();
  const [formState, setFormState] = useState({ canSave: false, isSaving: false });
  const [createdId, setCreatedId] = useState<number | null>(null);
  const uploadMutation = useUploadAttachment('DisbursementRequest', createdId ?? 0);
  const pendingFiles = useRef<File[]>([]);

  const handleUpload = useCallback(async (file: File) => {
    if (!createdId) {
      pendingFiles.current.push(file);
      notify({ type: 'info', title: 'سيتم رفع الملف بعد إنشاء الطلب' });
      return;
    }
    try {
      await uploadMutation.mutateAsync({ file });
      notify({ type: 'success', title: 'تم رفع الملف بنجاح' });
    } catch (err: unknown) {
      const problem = err as { detail?: string; message?: string };
      notify({ type: 'error', title: problem.detail ?? problem.message ?? 'حدث خطأ أثناء رفع الملف' });
    }
  }, [createdId, uploadMutation]);

  const handleSaved = useCallback(async (id?: number) => {
    if (!id) {
      navigate('/payments/disbursement-requests');
      return;
    }
    setCreatedId(id);
    for (const file of pendingFiles.current) {
      try {
        await documentsClient.upload('DisbursementRequest', id, file);
        notify({ type: 'success', title: `تم رفع ${file.name}` });
      } catch {
        notify({ type: 'error', title: `فشل رفع ${file.name}` });
      }
    }
    pendingFiles.current = [];
  }, [navigate]);

  return (
    <Page
      title="طلب صرف جديد"
      actions={
        <>
          <Button variant="outline" size="sm" type="button" onClick={() => navigate(-1)} disabled={formState.isSaving}>
            إلغاء
          </Button>
          <Button
            variant="default"
            size="sm"
            type="submit"
            form="disbursement-request-form"
            disabled={!formState.canSave || formState.isSaving}
            loading={formState.isSaving}
          >
            إنشاء طلب الصرف
          </Button>
        </>
      }
    >
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        {/* العمود الأيسر: النموذج */}
        <div className="lg:col-span-8">
          <DisbursementRequestForm
            mode="create"
            onStateChange={setFormState}
            onSaved={handleSaved}
          />
        </div>

        {/* العمود الأيمن: المرفقات */}
        <div className="lg:col-span-4 space-y-6">
          {createdId ? (
            <AttachmentsPanel
              documentType="DisbursementRequest"
              documentId={createdId}
            />
          ) : (
            <div className="space-y-3">
              <p className="text-sm font-semibold text-[var(--color-on-surface)]">المرفقات</p>
              <FileUploadZone
                onUpload={handleUpload}
                isUploading={uploadMutation.isPending}
              />
            </div>
          )}
        </div>
      </div>
    </Page>
  );
}
