import { useState, useRef, useCallback } from 'react';
import { Button } from '@/components/ui';
import { Upload, X } from 'lucide-react';
import { getFileIcon } from '@/shared/utils/file-icons';
import { formatFileSize } from '@/shared/utils/file-utils';

const MAX_FILE_SIZE = 10 * 1024 * 1024;

const ACCEPTED_TYPES: Record<string, string[]> = {
  'application/pdf': ['.pdf'],
  'image/jpeg': ['.jpg', '.jpeg'],
  'image/png': ['.png'],
  'image/webp': ['.webp'],
  'application/vnd.openxmlformats-officedocument.wordprocessingml.document': ['.docx'],
  'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet': ['.xlsx'],
  'text/plain': ['.txt'],
};

function getExtension(name: string) {
  return name.split('.').pop()?.toLowerCase() ?? '';
}

interface FileUploadZoneProps {
  onUpload: (file: File) => Promise<void>;
  isUploading?: boolean;
  disabled?: boolean;
}

export function FileUploadZone({ onUpload, isUploading = false, disabled = false }: FileUploadZoneProps) {
  const [isDragging, setIsDragging] = useState(false);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [error, setError] = useState<string | null>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  const validate = useCallback((file: File): string | null => {
    if (file.size > MAX_FILE_SIZE) return `حجم الملف يتجاوز ${formatFileSize(MAX_FILE_SIZE)}`;
    const allowedExtensions = Object.values(ACCEPTED_TYPES).flat();
    const extension = `.${getExtension(file.name)}`;
    if (file.type) {
      if (!ACCEPTED_TYPES[file.type]) return 'نوع الملف غير مدعوم';
    } else if (!allowedExtensions.includes(extension)) {
      return 'نوع الملف غير مدعوم';
    }
    return null;
  }, []);

  const handleFile = useCallback((file: File) => {
    const err = validate(file);
    if (err) { setError(err); setSelectedFile(null); return; }
    setError(null);
    setSelectedFile(file);
  }, [validate]);

  const handleDrop = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(false);
    if (disabled || isUploading) return;
    const file = e.dataTransfer.files[0];
    if (file) handleFile(file);
  }, [disabled, isUploading, handleFile]);

  const handleDragOver = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    if (!disabled && !isUploading) setIsDragging(true);
  }, [disabled, isUploading]);

  const handleDragLeave = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(false);
  }, []);

  const handleSubmit = async () => {
    if (!selectedFile) return;
    await onUpload(selectedFile);
    setSelectedFile(null);
    setError(null);
    if (inputRef.current) inputRef.current.value = '';
  };

  const handleClear = () => {
    setSelectedFile(null);
    setError(null);
    if (inputRef.current) inputRef.current.value = '';
  };

  return (
    <div className="space-y-3">

      {/* Drop Zone */}
      <div
        onDrop={handleDrop}
        onDragOver={handleDragOver}
        onDragLeave={handleDragLeave}
        onClick={() => !disabled && !isUploading && inputRef.current?.click()}
        className={`
          relative flex flex-col items-center justify-center gap-3 p-10 rounded-xl border-2 border-dashed
          transition-all duration-200
          ${disabled || isUploading
            ? 'opacity-40 cursor-not-allowed bg-[var(--color-surface)] border-[var(--color-on-surface)] border-2'
            : isDragging
              ? 'border-[var(--color-primary)] bg-[var(--color-primary-container)] cursor-copy border-2'
              : 'border-[var(--color-on-surface)] bg-[var(--color-surface)] hover:border-[var(--color-primary)] hover:bg-[var(--color-surface-container-low)] cursor-pointer border-2'
          }
        `}
      >
        <input
          ref={inputRef}
          type="file"
          className="hidden"
          accept={Object.values(ACCEPTED_TYPES).flat().join(',')}
          onChange={(e) => {
            const file = e.target.files?.[0];
            if (file) handleFile(file);
          }}
          disabled={disabled || isUploading}
        />
        <div className={`p-3 rounded-full ${isDragging ? 'bg-[var(--color-primary)]' : 'bg-[var(--color-surface-container)]'}`}>
          <Upload size={20} className={isDragging ? 'text-[var(--color-on-primary)]' : 'text-[var(--color-primary)]'} />
        </div>
        <div className="text-center">
          <p className="text-sm font-medium text-[var(--color-on-surface)]">
            {isDragging ? 'أفلت الملف هنا' : 'اسحب الملف هنا أو انقر للتصفح'}
          </p>
          <p className="text-xs text-[var(--color-on-surface-variant)] mt-1">
            {formatFileSize(MAX_FILE_SIZE)} كحد أقصى
          </p>
        </div>
      </div>

      {/* Error */}
      {error && (
        <p className="text-xs text-[var(--color-error)]">{error}</p>
      )}

      {/* Selected File Preview */}
      {selectedFile && (
        <div className="flex items-center gap-3 p-3 rounded-lg bg-[var(--color-surface-container)] border border-[var(--color-outline-variant)]">
          {getFileIcon(selectedFile.type)}
          <div className="flex-1 min-w-0">
            <p className="text-sm font-medium truncate">{selectedFile.name}</p>
            <p className="text-xs text-[var(--color-on-surface-variant)]">
              {formatFileSize(selectedFile.size)} · {getExtension(selectedFile.name).toUpperCase()}
            </p>
          </div>
          <div className="flex gap-2 shrink-0">
            <Button variant="primary" size="sm" onClick={handleSubmit} disabled={isUploading}>
              {isUploading ? 'جارٍ الرفع...' : 'رفع'}
            </Button>
            <Button variant="ghost" size="icon" onClick={handleClear} disabled={isUploading}>
              <X size={14} />
            </Button>
          </div>
        </div>
      )}
    </div>
  );
}

export default FileUploadZone;
