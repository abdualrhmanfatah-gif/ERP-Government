import { Button } from '@/components/ui/Button';
import { useReportExport } from '../hooks/useReportExport';
import { FileSpreadsheet, FileText, Printer } from 'lucide-react';

interface ReportExportDropdownProps {
  reportType: string;
  params: Record<string, string | number | undefined>;
}

export function ReportExportDropdown({ reportType, params }: ReportExportDropdownProps) {
  const exportMutation = useReportExport();

  const handleExport = (format: 'excel' | 'pdf') => {
    exportMutation.mutate({
      endpoint: `/${reportType}`,
      format,
      params,
    });
  };

  const handlePrint = () => {
    window.print();
  };

  return (
    <div className="flex gap-2">
      <Button
        variant="outline"
        size="sm"
        onClick={() => handleExport('excel')}
        disabled={exportMutation.isPending}
      >
        <FileSpreadsheet className="h-4 w-4 ms-1" />
        Excel
      </Button>
      <Button
        variant="outline"
        size="sm"
        onClick={() => handleExport('pdf')}
        disabled={exportMutation.isPending}
      >
        <FileText className="h-4 w-4 ms-1" />
        PDF
      </Button>
      <Button
        variant="outline"
        size="sm"
        onClick={handlePrint}
      >
        <Printer className="h-4 w-4 ms-1" />
        طباعة
      </Button>
    </div>
  );
}
