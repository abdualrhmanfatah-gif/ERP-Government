import { useNavigate } from 'react-router-dom';
import { DataGrid } from '@/components/ui/DataGrid';
import type { JournalEntryTemplateDto } from '../web-api-client';

const templateTypeLabels: Record<string, string> = {
  Standard: 'قياسية',
  Recurring: 'دورية',
  Adjustment: 'تسوية',
};

const columns = [
  { key: 'templateName', header: 'اسم القالب', accessorKey: 'templateName' as const, width: 200 },
  { key: 'journalName', header: 'الدفتر', accessorKey: 'journalName' as const, width: 150 },
  {
    key: 'templateType',
    header: 'النوع',
    accessorKey: 'templateType' as const,
    width: 120,
    cell: (row: JournalEntryTemplateDto) => templateTypeLabels[row.templateType ?? ''] ?? row.templateType ?? '—',
  },
  {
    key: 'isActive',
    header: 'نشط',
    accessorKey: 'isActive' as const,
    width: 80,
    cell: (row: JournalEntryTemplateDto) =>
      row.isActive ? (
        <span className="text-[var(--color-success)]">نشط</span>
      ) : (
        <span className="text-[var(--color-on-surface-variant)]">غير نشط</span>
      ),
  },
];

interface TemplateGridProps {
  data: JournalEntryTemplateDto[];
  loading?: boolean;
  error?: string;
  onRetry?: () => void;
}

export function TemplateGrid({ data, loading, error, onRetry }: TemplateGridProps) {
  const navigate = useNavigate();

  return (
    <DataGrid
      columns={columns}
      data={data}
      loading={loading}
      error={error}
      onRetry={onRetry}
      emptyMessage="لا توجد قوالب"
      rowKey={(row) => row.id ?? 0}
      onRowClick={(row) => navigate(`/accounting/templates/${row.id}`)}
    />
  );
}
