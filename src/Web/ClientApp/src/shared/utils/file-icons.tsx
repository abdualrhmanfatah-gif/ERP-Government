import { FileText, Image, FileSpreadsheet, File } from 'lucide-react';
import type { ReactNode } from 'react';

export function getFileIcon(mimeType: string): ReactNode {
  if (mimeType.startsWith('image/')) return <Image size={16} className="text-blue-500" />;
  if (mimeType === 'application/pdf') return <FileText size={16} className="text-red-500" />;
  if (mimeType.includes('spreadsheet') || mimeType.includes('excel')) return <FileSpreadsheet size={16} className="text-green-600" />;
  if (mimeType.includes('wordprocessing') || mimeType.includes('document')) return <FileText size={16} className="text-blue-600" />;
  return <File size={16} className="text-[var(--color-on-surface-variant)]" />;
}
