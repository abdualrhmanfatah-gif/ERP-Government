import { useMutation } from '@tanstack/react-query';
import { toast } from 'sonner';

const API_BASE = '/api/Reports';

interface UseReportExportOptions {
  endpoint: string;
  format: 'excel' | 'pdf';
  params: Record<string, string | number | undefined>;
}

export function useReportExport() {
  return useMutation({
    mutationFn: async ({ endpoint, format, params }: UseReportExportOptions) => {
      const queryString = new URLSearchParams(
        Object.entries(params)
          .filter(([, v]) => v !== undefined && v !== '')
          .map(([k, v]) => [k, String(v)])
      ).toString();

      const sep = queryString ? '&' : '?';
      const url = `${API_BASE}${endpoint}/export?format=${format}${sep}${queryString}`;

      const response = await fetch(url);
      if (!response.ok) {
        throw new Error(`Export failed: ${response.statusText}`);
      }

      const blob = await response.blob();
      const ext = format === 'excel' ? 'xlsx' : 'pdf';
      const filename = `${endpoint.replace(/^\//, '')}-${new Date().toISOString().split('T')[0]}.${ext}`;

      const a = window.document.createElement('a');
      a.href = URL.createObjectURL(blob);
      a.download = filename;
      a.click();
      URL.revokeObjectURL(a.href);
    },
    onSuccess: () => {
      toast.success('تم التصدير بنجاح');
    },
    onError: (error: Error) => {
      console.error('Export failed:', error);
      toast.error('فشل التصدير. يرجى المحاولة مرة أخرى.');
    },
  });
}
