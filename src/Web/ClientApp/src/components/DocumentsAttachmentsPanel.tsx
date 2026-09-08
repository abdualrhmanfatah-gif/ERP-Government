import { useState, useRef, useEffect } from 'react';
import { useAttachments, useAttachmentRequirements, useAttachmentGateCheck, useUploadAttachment, useDeleteAttachment } from '@/features/documents/hooks/useDocuments';
import type { AttachmentsPanelProps } from '@/features/documents/shared/types';
import { Button } from '@/components/ui';
import { ConfirmDialog } from '@/components/ui/ConfirmDialog';
import { Select } from '@/components/ui/Select';
import { Input } from '@/components/ui/Input';
import { Upload, Trash2, FileText } from 'lucide-react';

const MAX_FILE_SIZE = 10 * 1024 * 1024; // 10 MB

export function AttachmentsPanel({ documentType, documentId, showGate = false, onGateChange }: AttachmentsPanelProps) {
  const { data: attachments, isLoading, isError, refetch } = useAttachments(documentType, documentId);
  const { data: requirements } = useAttachmentRequirements(documentType);
  const { data: gateCheck } = useAttachmentGateCheck(documentType, documentId);
  const uploadMutation = useUploadAttachment(documentType, documentId);
  const deleteMutation = useDeleteAttachment(documentType, documentId);

  const [isExpanded, setIsExpanded] = useState(true);
  const [showUpload, setShowUpload] = useState(false);
  const [selectedType, setSelectedType] = useState('');
  const [customType, setCustomType] = useState('');
  const [fileError, setFileError] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [deleteId, setDeleteId] = useState<number | null>(null);

  const missingTypes = gateCheck ?? [];
  const gateMet = showGate && missingTypes.length === 0;

  useEffect(() => {
    if (showGate && onGateChange) {
      onGateChange(gateMet);
    }
  }, [showGate, gateMet, onGateChange]);

  async function handleUpload() {
    const file = fileInputRef.current?.files?.[0];
    if (!file) return;

    if (file.size > MAX_FILE_SIZE) {
      setFileError('حجم الملف يتجاوز الحد الأقصى 10 ميجابايت');
      return;
    }

    setFileError(null);
    const typeCode = selectedType === 'OTHER' ? customType : selectedType || 'GENERAL';
    await uploadMutation.mutateAsync({ file, attachmentTypeCode: typeCode });
    setShowUpload(false);
    setSelectedType('');
    setCustomType('');
    if (fileInputRef.current) fileInputRef.current.value = '';
  }

  async function handleDelete() {
    if (deleteId === null) return;
    await deleteMutation.mutateAsync(deleteId);
    setDeleteId(null);
  }

  function formatSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }

  if (isLoading) {
    return (
      <div className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-4">
        <div className="h-6 w-32 rounded bg-[var(--color-surface-container)] animate-pulse" />
      </div>
    );
  }

  if (isError) {
    return (
      <div className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)] p-4">
        <p className="text-sm text-[var(--color-error)]">حدث خطأ أثناء تحميل المرفقات</p>
        <Button variant="link" size="sm" onClick={() => refetch()} className="mt-2">
          إعادة المحاولة
        </Button>
      </div>
    );
  }

  return (
    <div className="rounded-lg border border-[var(--color-outline-variant)] bg-[var(--color-surface-container-lowest)]">
      <Button
        variant="ghost"
        className="w-full flex items-center justify-between px-4 py-3 text-sm font-semibold text-[var(--color-on-surface)]"
        onClick={() => setIsExpanded(!isExpanded)}
      >
        <span>المرفقات</span>
        <span className="text-[var(--color-on-surface-variant)]">{isExpanded ? '▲' : '▼'}</span>
      </Button>
      {isExpanded && (
        <div className="px-4 pb-4">
          {showGate && missingTypes.length > 0 && (
            <div className="mb-3 p-2 rounded bg-[var(--color-warning-container)] text-[var(--color-on-warning-container)] text-sm">
              مرفق مطلوب مفقود: {missingTypes.join(', ')}
            </div>
          )}
          {showGate && gateMet && (
            <div className="mb-3 p-2 rounded bg-[var(--color-success-container)] text-[var(--color-on-success-container)] text-sm">
              جميع المرفقات المطلوبة متوفرة
            </div>
          )}

          <div className="flex items-center justify-between mb-3">
            <span className="text-xs text-[var(--color-on-surface-variant)]">
              {attachments?.length ?? 0} مرفق
            </span>
            <Button variant="ghost" size="sm" onClick={() => setShowUpload(!showUpload)}>
              <Upload size={14} className="ms-1" />
              رفع مرفق
            </Button>
          </div>

          {showUpload && (
            <div className="mb-4 p-3 rounded bg-[var(--color-surface-container)] space-y-3">
              <Select
                label="نوع المرفق"
                value={selectedType}
                onChange={(e) => setSelectedType(e.target.value)}
                options={[
                  { value: '', label: 'اختر النوع' },
                  ...(requirements?.map((r) => ({ value: r.attachmentTypeCode, label: `${r.titleAr} ${r.isMandatory ? '(مطلوب)' : ''}` })) ?? []),
                  { value: 'OTHER', label: 'أخرى' },
                ]}
              />
              {selectedType === 'OTHER' && (
                <Input
                  label="نوع مخصص"
                  type="text"
                  value={customType}
                  onChange={(e) => setCustomType(e.target.value)}
                />
              )}
              <div>
                <input
                  type="file"
                  ref={fileInputRef}
                  className="w-full text-sm text-[var(--color-on-surface-variant)]"
                />
              </div>
              {fileError && (
                <p className="text-xs text-[var(--color-error)]">{fileError}</p>
              )}
              <div className="flex gap-2">
                <Button variant="primary" size="sm" onClick={handleUpload} disabled={uploadMutation.isPending}>
                  رفع
                </Button>
                <Button variant="ghost" size="sm" onClick={() => { setShowUpload(false); setFileError(null); }}>
                  إلغاء
                </Button>
              </div>
            </div>
          )}

          {!attachments || attachments.length === 0 ? (
            <p className="text-sm text-[var(--color-on-surface-variant)]">لا توجد مرفقات</p>
          ) : (
            <div className="space-y-2">
              {attachments.map((att) => (
                <div key={att.id} className="flex items-center gap-3 p-3 rounded bg-[var(--color-surface-container)]">
                  <FileText size={16} className="text-[var(--color-on-surface-variant)]" />
                  <div className="flex-1">
                    <p className="text-sm font-medium">{att.fileName}</p>
                    <p className="text-xs text-[var(--color-on-surface-variant)]">
                      {att.attachmentTypeCode} · {formatSize(att.sizeBytes)} · {new Date(att.createdAt).toLocaleDateString('ar-EG')}
                    </p>
                  </div>
                  <Button variant="ghost" size="icon" onClick={() => setDeleteId(att.id)}>
                    <Trash2 size={14} />
                  </Button>
                </div>
              ))}
            </div>
          )}

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
