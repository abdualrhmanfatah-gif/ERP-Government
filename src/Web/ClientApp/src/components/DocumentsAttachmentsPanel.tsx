import { useState, useEffect } from 'react';
import { useAttachments, useAttachmentRequirements, useAttachmentGateCheck, useUploadAttachment, useDeleteAttachment } from '@/features/documents/hooks/useDocuments';
import type { AttachmentsPanelProps } from '@/features/documents/shared/types';
import { Button } from '@/components/ui';
import { ConfirmDialog } from '@/components/ui/ConfirmDialog';
import { Badge } from '@/components/ui/Badge';
import { FileUploadZone } from '@/components/FileUploadZone';
import { Trash2, FileText, Download, ChevronDown, ChevronUp } from 'lucide-react';
import { notify } from '@/features/notifications/notify';
import { getFileIcon } from '@/shared/utils/file-icons';
import { formatFileSize } from '@/shared/utils/file-utils';

export function AttachmentsPanel({ documentType, documentId, showGate = false, onGateChange }: AttachmentsPanelProps) {
  const { data: attachments, isLoading, isError, refetch } = useAttachments(documentType, documentId);
  const { data: requirements } = useAttachmentRequirements(documentType);
  const { data: gateCheck } = useAttachmentGateCheck(documentType, documentId);
  const uploadMutation = useUploadAttachment(documentType, documentId);
  const deleteMutation = useDeleteAttachment(documentType, documentId);

  const [isExpanded, setIsExpanded] = useState(true);
  const [deleteId, setDeleteId] = useState<number | null>(null);

  const missingTypes = gateCheck ?? [];
  const gateMet = showGate && missingTypes.length === 0;

  useEffect(() => {
    if (showGate && onGateChange) onGateChange(gateMet);
  }, [showGate, gateMet, onGateChange]);

  async function handleUpload(file: File) {
    const typeCode = requirements?.[0]?.attachmentTypeCode ?? 'GENERAL';
    await uploadMutation.mutateAsync({ file, attachmentTypeCode: typeCode });
    notify({ type: 'success', title: 'تم رفع الملف بنجاح' });
  }

  async function handleDelete() {
    if (deleteId === null) return;
    await deleteMutation.mutateAsync(deleteId);
    notify({ type: 'success', title: 'تم حذف المرفق' });
    setDeleteId(null);
  }

  function handleDownload(att: { id: number; fileName: string }) {
    const a = document.createElement('a');
    a.href = `/api/Documents/attachments/${att.id}/download`;
    a.download = att.fileName;
    a.click();
  }

  if (isLoading) {
    return (
      <div className="rounded-xl border-2 border-[var(--color-on-surface)] border-dashed bg-[var(--color-surface-container-lowest)] p-4">
        <div className="h-5 w-28 rounded bg-[var(--color-surface-container)] animate-pulse" />
      </div>
    );
  }

  if (isError) {
    return (
      <div className="rounded-xl border-2 border-[var(--color-on-surface)] border-dashed bg-[var(--color-surface-container-lowest)] p-4">
        <p className="text-sm text-[var(--color-error)]">حدث خطأ أثناء تحميل المرفقات</p>
        <Button variant="link" size="sm" onClick={() => refetch()} className="mt-2">إعادة المحاولة</Button>
      </div>
    );
  }

  const count = attachments?.length ?? 0;

  return (
    <div className="rounded-xl border-2 border-[var(--color-on-surface)] border-dashed bg-[var(--color-surface-container-lowest)] overflow-hidden">
      {/* Header */}
      <button
        type="button"
        onClick={() => setIsExpanded(!isExpanded)}
        className="w-full flex items-center justify-between px-4 py-3 hover:bg-[var(--color-surface-container-low)] transition-colors"
      >
        <div className="flex items-center gap-2">
          <span className="text-sm font-semibold text-[var(--color-on-surface)]">المرفقات</span>
          {count > 0 && (
            <Badge variant="secondary" className="text-xs px-1.5 py-0.5">{count}</Badge>
          )}
        </div>
        {isExpanded ? <ChevronUp size={16} /> : <ChevronDown size={16} />}
      </button>

      {isExpanded && (
        <div className="px-4 pb-4 space-y-4">
          {/* Gate Status */}
          {showGate && missingTypes.length > 0 && (
            <div className="p-3 rounded-lg bg-[var(--color-error-container)] text-[var(--color-on-error-container)] text-sm flex items-center gap-2">
              <span className="shrink-0 w-2 h-2 rounded-full bg-[var(--color-error)]" />
              مرفق مطلوب مفقود: {missingTypes.join('، ')}
            </div>
          )}
          {showGate && gateMet && (
            <div className="p-3 rounded-lg bg-[var(--color-success-container)] text-[var(--color-on-success-container)] text-sm flex items-center gap-2">
              <span className="shrink-0 w-2 h-2 rounded-full bg-green-500" />
              جميع المرفقات المطلوبة متوفرة
            </div>
          )}

          {/* Upload Zone */}
          <FileUploadZone
            onUpload={handleUpload}
            isUploading={uploadMutation.isPending}
          />

          {/* File List */}
          {!attachments || attachments.length === 0 ? (
            <div className="text-center py-6">
              <FileText size={32} className="mx-auto text-[var(--color-on-surface-variant)] opacity-40" />
              <p className="text-sm text-[var(--color-on-surface-variant)] mt-2">لا توجد مرفقات</p>
            </div>
          ) : (
            <div className="space-y-2">
              {attachments.map((att) => (
                <div
                  key={att.id}
                  className="flex items-center gap-3 p-3 rounded-lg bg-[var(--color-surface-container)] hover:bg-[var(--color-surface-container-high)] transition-colors group"
                >
                  {getFileIcon(att.mimeType)}
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-medium truncate">{att.fileName}</p>
                    <p className="text-xs text-[var(--color-on-surface-variant)]">
                      {att.attachmentTypeCode} · {formatFileSize(att.sizeBytes)} · {new Date(att.createdAt).toLocaleDateString('ar-YE')}
                    </p>
                  </div>
                  <div className="flex gap-1 opacity-0 group-hover:opacity-100 transition-opacity shrink-0">
                    <Button variant="ghost" size="icon" onClick={() => handleDownload(att)} title="تحميل">
                      <Download size={14} />
                    </Button>
                    <Button variant="ghost" size="icon" onClick={() => setDeleteId(att.id)} title="حذف">
                      <Trash2 size={14} className="text-[var(--color-error)]" />
                    </Button>
                  </div>
                </div>
              ))}
            </div>
          )}

          {/* Delete Confirm */}
          {deleteId !== null && (
            <ConfirmDialog
              open={deleteId !== null}
              onClose={() => setDeleteId(null)}
              onConfirm={handleDelete}
              title="حذف المرفق"
              message="هل أنت متأكد من حذف هذا المرفق؟"
              loading={deleteMutation.isPending}
            />
          )}
        </div>
      )}
    </div>
  );
}

export default AttachmentsPanel;
