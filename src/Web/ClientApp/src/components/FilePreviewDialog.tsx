import { Download } from 'lucide-react';
import { Dialog } from '@/components/ui/Dialog';
import { Button } from '@/components/ui/Button';
import { Loading } from '@/components/ui/Loading';
import { Alert } from '@/components/ui/Alert';
import { useFileBlob } from './file-preview/useFileBlob';
import { PdfViewer } from './file-preview/PdfViewer';
import { WordViewer } from './file-preview/WordViewer';
import { ExcelViewer } from './file-preview/ExcelViewer';
import type { AttachmentRecord } from '@/features/documents/shared/types';
import { formatFileSize } from '@/shared/utils/file-utils';

interface FilePreviewDialogProps {
  open: boolean;
  onClose: () => void;
  attachment: AttachmentRecord | null;
}

const PDF_TYPES = ['application/pdf'];
const WORD_TYPES = [
  'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
  'application/msword',
];
const EXCEL_TYPES = [
  'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
  'application/vnd.ms-excel',
];

function isPdf(mime: string) {
  return PDF_TYPES.includes(mime);
}
function isWord(mime: string) {
  return WORD_TYPES.includes(mime);
}
function isExcel(mime: string) {
  return EXCEL_TYPES.includes(mime);
}
function isImage(mime: string) {
  return mime.startsWith('image/');
}

export function FilePreviewDialog({ open, onClose, attachment }: FilePreviewDialogProps) {
  const { url, blob, isLoading, error } = useFileBlob(attachment?.id ?? null);

  if (!attachment) return null;

  const handleDownload = () => {
    if (!url) return;
    const a = document.createElement('a');
    a.href = url;
    a.download = attachment.fileName;
    a.click();
  };

  const renderContent = () => {
    if (isLoading) return <Loading />;
    if (error) return <Alert variant="error">{error}</Alert>;
    if (!url) return null;

    if (isPdf(attachment.mimeType)) return <PdfViewer url={url} />;
    if (isWord(attachment.mimeType) && blob) return <WordViewer blob={blob} />;
    if (isExcel(attachment.mimeType) && blob) return <ExcelViewer blob={blob} />;
    if (isImage(attachment.mimeType))
      return (
        <div className="flex justify-center overflow-auto max-h-[70vh]">
          <img src={url} alt={attachment.fileName} className="max-w-full object-contain" />
        </div>
      );

    return (
      <div className="text-center py-8">
        <p className="text-[var(--color-on-surface-variant)] mb-4">
          لا يمكن استعراض هذا النوع من الملفات
        </p>
        <Button variant="outline" onClick={handleDownload}>
          <Download size={16} className="ml-2" />
          تحميل الملف
        </Button>
      </div>
    );
  };

  return (
    <Dialog
      open={open}
      onClose={onClose}
      title={attachment.fileName}
      className="max-w-4xl"
      footer={
        <div className="flex gap-2">
          <Button variant="outline" onClick={handleDownload} disabled={!url}>
            <Download size={16} className="ml-2" />
            تحميل
          </Button>
          <Button variant="ghost" onClick={onClose}>
            إغلاق
          </Button>
        </div>
      }
    >
      <div className="text-xs text-[var(--color-on-surface-variant)] mb-3">
        {formatFileSize(attachment.sizeBytes)}
      </div>
      {renderContent()}
    </Dialog>
  );
}
